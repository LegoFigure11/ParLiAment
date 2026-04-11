using ParLiAment.Core;
using ParLiAment.Core.Connection;
using ParLiAment.Core.Enums;
using ParLiAment.Core.Interfaces;
using ParLiAment.WinForms.Subforms;
using PKHeX.Core;
using SysBot.Base;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using static ParLiAment.Core.Utils;
using static ParLiAment.Core.Encounters;

namespace ParLiAment.WinForms;

public partial class MainWindow : Form
{
    private static CancellationTokenSource Source = new();
    private static CancellationTokenSource GameResetSource = new();

    private static readonly Lock _connectLock = new();

    public ClientConfig Config;
    private ConnectionWrapperAsync ConnectionWrapper = default!;
    private readonly SwitchConnectionConfig ConnectionConfig;

    private static PA8 first = new();
    private static PA8 second = new();
    private static bool selectedFirst;
    private static bool selectedSecond;

    private bool stop;
    private bool reset;
    private bool readPause;
    private uint total;

    private bool babyMode;
    private bool babyModePrimed;
    private uint babyModeTarget = 0;
    private SwitchButton babyModeButton = SwitchButton.A;

    private PA8 _enc = new();

    internal List<object> Frames = [];

    private readonly Version CurrentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
    public static readonly Font BoldFont = new("Microsoft Sans Serif", 8, FontStyle.Bold);

    public MainWindow()
    {
        Config = new ClientConfig();
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        if (File.Exists(configPath))
        {
            var text = File.ReadAllText(configPath);
            Config = JsonSerializer.Deserialize<ClientConfig>(text)!;
        }
        else
        {
            Config = new();
        }

        ConnectionConfig = new()
        {
            IP = Config.IP,
            Port = Config.Protocol is SwitchProtocol.WiFi ? 6000 : Config.UsbPort,
            Protocol = Config.Protocol,
        };

        var v = CurrentVersion;
#if DEBUG
        var build = "";

        var asm = System.Reflection.Assembly.GetEntryAssembly();
        var gitVersionInformationType = asm?.GetType("GitVersionInformation");
        var sha = gitVersionInformationType?.GetField("ShortSha");

        if (sha is not null) build += $"#{sha.GetValue(null)}";

        var date = File.GetLastWriteTime(AppContext.BaseDirectory);
        build += $" (dev-{date:yyyyMMdd})";

#else
        var build = "";
#endif

        Text = $"ParLiAment v{v.Major}.{v.Minor}.{v.Build}{build}";

        InitializeComponent();
    }

    private void MainWindow_Load(object sender, EventArgs e)
    {
        CenterToScreen();

        if (Config.Protocol is SwitchProtocol.WiFi)
        {
            TB_SwitchIP.Text = Config.IP;
        }
        else
        {
            L_SwitchIP.Text = "USB Port:";
            TB_SwitchIP.Text = $"{Config.UsbPort}";
        }

        TB_TID.Text = $"{Config.TID:D5}";
        TB_SID.Text = $"{Config.SID:D5}";

        CB_Static_Species.Items.Clear();
        var main = GetMainEncounters();
        foreach (var item in main) CB_Static_Species.Items.Add(item);

        CB_Spawner_Species.Items.Clear();
        var spawner = GetSpawnerEncounters();
        foreach (var item in spawner) CB_Spawner_Species.Items.Add(item);

        SetComboBoxSelectedIndex(0, CB_BabyMode_Action, CB_Static_Nature, CB_Static_Species, CB_Spawner_Species);

        SetControlText("0", TB_InitialSeed0, TB_InitialSeed1);
        SetControlText(string.Empty, TB_CurrentAdvances, TB_AdvancesIncrease, TB_CurrentSeed0, TB_CurrentSeed1);

        TB_Status.Text = "Not Connected.";
        TB_Wild.Text = string.Empty;

        CheckForUpdates();
    }

    private void Connect(CancellationToken token)
    {
        Task.Run(
            async () =>
            {
                SetControlEnabledState(false, B_Connect);
                try
                {
                    ConnectionConfig.IP = TB_SwitchIP.Text;
                    (bool success, string err) = await ConnectionWrapper
                        .Connect(token)
                        .ConfigureAwait(false);
                    if (!success)
                    {
                        SetControlEnabledState(true, B_Connect);
                        this.DisplayMessageBox(err);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    SetControlEnabledState(true, B_Connect);
                    this.DisplayMessageBox(ex.Message);
                    return;
                }

                UpdateStatus("Detecting game version...");

                var (tid, sid) = ConnectionWrapper.GetIDs();
                SetControlText($"{tid:D5}", TB_TID);
                SetControlText($"{sid:D5}", TB_SID);

                UpdateStatus("Reading RNG State...");

                ulong _s0, _s1;
                try
                {
                    (_s0, _s1) = await ConnectionWrapper.GetCurrentRNGState(token).ConfigureAwait(false);
                    SetControlText($"{_s0:X16}", TB_InitialSeed0, TB_CurrentSeed0);
                    SetControlText($"{_s1:X16}", TB_InitialSeed1, TB_CurrentSeed1);
                    SetControlText("0", TB_CurrentAdvances, TB_AdvancesIncrease);
                }
                catch (Exception ex)
                {
                    this.DisplayMessageBox($"Error occurred while reading initial RNG state: {ex.Message}");
                    return;
                }

                SetControlEnabledState(true, B_Disconnect);

                UpdateStatus("Monitoring RNG State...");
                try
                {
                    total = 0;
                    stop = false;
                    while (!stop)
                    {
                        if (ConnectionWrapper.Connected && !readPause)
                        {
                            if (babyMode && !babyModePrimed)
                            {
                                babyModePrimed = true;
                                await ConnectionWrapper.DoTurboCommand("Release Stick", token).ConfigureAwait(false);
                            }

                            var (s0, s1) = await ConnectionWrapper.GetCurrentRNGState(token).ConfigureAwait(false);
                            var adv = Core.RNG.RNGUtil.GetAdvancesPassed(_s0, _s1, s0, s1);
                            if (reset || adv > 0)
                            {
                                if (reset || adv == 50_000)
                                {
                                    total = 0;
                                    reset = false;
                                    adv = 0;
                                }
                                else
                                {
                                    total += adv;
                                }

                                if (babyMode && total >= babyModeTarget)
                                {
                                    await ConnectionWrapper.PressButton(babyModeButton, 0, token).ConfigureAwait(false);
                                    await ConnectionWrapper.DetachController(token).ConfigureAwait(false);
                                    babyMode = false;
                                    babyModePrimed = false;
                                    UpdateStatus("Monitoring RNG State...");
                                }

                                _s0 = s0;
                                _s1 = s1;

                                SetControlText($"{_s0:X16}", TB_CurrentSeed0);
                                SetControlText($"{_s1:X16}", TB_CurrentSeed1);
                                SetControlText($"{total:N0}", TB_CurrentAdvances);
                                SetControlText($"{adv:N0}", TB_AdvancesIncrease);
                            }
                        }
                    }
                }
                catch
                {
                    // Ignored
                }
            },
            token
        );
    }

    private void Disconnect(CancellationToken token)
    {
        Task.Run(
            async () =>
            {
                SetControlEnabledState(false, B_Disconnect);
                stop = true;
                try
                {
                    var (success, err) = await ConnectionWrapper.DisconnectAsync(token).ConfigureAwait(false);
                    if (!success) this.DisplayMessageBox(err);
                }
                catch (Exception ex)
                {
                    this.DisplayMessageBox(ex.Message);
                }
                await Source.CancelAsync().ConfigureAwait(false);
                Source = new();
                await GameResetSource.CancelAsync().ConfigureAwait(false);
                GameResetSource = new();
                SetControlEnabledState(true, B_Connect);
            },
            token
        );
    }

    private void UpdateStatus(string status)
    {
        SetControlText(status, TB_Status);
    }

    public void SetControlText(string text, params object[] obj)
    {
        foreach (object o in obj)
        {
            if (o is not Control c)
                continue;

            if (InvokeRequired)
                Invoke(() => c.Text = text);
            else
                c.Text = text;
        }
    }

    public void SetControlEnabledState(bool state, params object[] obj)
    {
        foreach (object o in obj)
        {
            if (o is Control c)
            {
                if (InvokeRequired)
                    Invoke(() => c.Enabled = state);
                else
                    c.Enabled = state;
            }

            if (o is ToolStripMenuItem d)
            {
                if (InvokeRequired)
                    Invoke(() => d.Enabled = state);
                else
                    d.Enabled = state;
            }
        }
    }

    public void SetControlVisibleState(bool state, params object[] obj)
    {
        foreach (object o in obj)
        {
            if (o is Control c)
            {
                if (InvokeRequired)
                    Invoke(() => c.Visible = state);
                else
                    c.Visible = state;
            }

            if (o is DataGridViewColumn d)
            {
                if (InvokeRequired)
                    Invoke(() => d.Visible = state);
                else
                    d.Visible = state;
            }
        }
    }

    public void SetBindingSourceDataSource(object source, params object[] obj)
    {
        foreach (object o in obj)
        {
            if (o is not BindingSource b)
                continue;

            if (InvokeRequired)
                Invoke(() => b.DataSource = source);
            else
                b.DataSource = source;
        }
    }

    public void SetDataGridViewDataSource(object source, params object[] obj)
    {
        foreach (object o in obj)
        {
            if (o is not DataGridView d)
                continue;

            if (InvokeRequired)
            {
                Invoke(() =>
                {
                    d.AutoGenerateColumns = true;
                    d.DataSource = source;

                    d.Columns["Seed"]?.DisplayIndex = d.Columns.Count - 1;
                    d.Columns["HP"]?.Width = 50;
                    d.Columns["Atk"]?.Width = 50;
                    d.Columns["Def"]?.Width = 50;
                    d.Columns["SpA"]?.Width = 50;
                    d.Columns["SpD"]?.Width = 50;
                    d.Columns["Spe"]?.Width = 50;


                });
            }
            else
            {
                d.AutoGenerateColumns = true;
                d.DataSource = source;

                d.Columns["Seed"]?.DisplayIndex = d.Columns.Count - 1;
                d.Columns["HP"]?.Width = 50;
                d.Columns["Atk"]?.Width = 50;
                d.Columns["Def"]?.Width = 50;
                d.Columns["SpA"]?.Width = 50;
                d.Columns["SpD"]?.Width = 50;
                d.Columns["Spe"]?.Width = 50;
            }
        }
    }

    public void SetNUDValue(decimal val, params NumericUpDown[] nuds)
    {
        foreach (var nud in nuds)
        {
            if (InvokeRequired) Invoke(() => nud.Value = val);
            else nud.Value = val;
        }
    }

    public void SetComboBoxOption(string opt, params ComboBox[] cbs)
    {
        foreach (var cb in cbs)
        {
            if (InvokeRequired) Invoke(() => cb.SelectedIndex = cb.Items.IndexOf(opt));
            else cb.SelectedIndex = cb.Items.IndexOf(opt);
        }
    }

    public void SetComboBoxSelectedIndex(int idx, params ComboBox[] cbs)
    {
        foreach (var cb in cbs)
        {
            if (InvokeRequired) Invoke(() => cb.SelectedIndex = idx);
            else cb.SelectedIndex = idx;
        }
    }

    private void B_Connect_Click(object sender, EventArgs e)
    {
        lock (_connectLock)
        {
            if (ConnectionWrapper is { Connected: true })
                return;

            ConnectionWrapper = new(ConnectionConfig, UpdateStatus);
            Connect(Source.Token);
        }
    }

    private void B_Disconnect_Click(object sender, EventArgs e)
    {
        lock (_connectLock)
        {
            if (ConnectionWrapper is not { Connected: true })
                return;

            Disconnect(Source.Token);
        }
    }


    private static ShinyType GetFilterShinyType(int selected) => selected switch
    {
        1 => ShinyType.Either,
        2 => ShinyType.Square,
        3 => ShinyType.Star,
        4 => ShinyType.None,
        _ => ShinyType.Any,
    };

    private static Nature GetFilterNatureType(int selected) => selected switch
    {
        0 => Nature.Random,
        _ => (Nature)(selected - 1),
    };

    private void B_IV_Max_Click(object sender, EventArgs e)
    {
        var st = ((Button)sender).Name.Replace("B_", string.Empty).Replace("_Max", string.Empty);
        var underscore = st.IndexOf('_');
        var page = st[..underscore];
        var skill = st[(underscore + 1)..];
        List<string> stats = ModifierKeys == Keys.Shift ? ["HP", "Atk", "Def", "SpA", "SpD", "Spe"] : [skill];
        var val = ModifierKeys == Keys.Control ? 30 : 31;
        foreach (var stat in stats)
        {
            var min = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Min", true).FirstOrDefault()!;
            var max = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Max", true).FirstOrDefault()!;
            min.Value = val;
            max.Value = val;
        }
    }

    private void B_IV_Min_Click(object sender, EventArgs e)
    {
        var st = ((Button)sender).Name.Replace("B_", string.Empty).Replace("_Min", string.Empty);
        var underscore = st.IndexOf('_');
        var page = st[..underscore];
        var skill = st[(underscore + 1)..];
        List<string> stats = ModifierKeys == Keys.Shift ? ["HP", "Atk", "Def", "SpA", "SpD", "Spe"] : [skill];
        var val = ModifierKeys == Keys.Control ? 1 : 0;
        foreach (var stat in stats)
        {
            var min = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Min", true).FirstOrDefault()!;
            var max = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Max", true).FirstOrDefault()!;
            min.Value = val;
            max.Value = val;
        }
    }

    private void IV_Label_Click(object sender, EventArgs e)
    {
        var st = ((Label)sender).Name.Replace("L_", string.Empty);
        var underscore = st.IndexOf('_');
        var page = st[..underscore];
        var skill = st[(underscore + 1)..];
        List<string> stats = ModifierKeys == Keys.Shift ? ["HP", "Atk", "Def", "SpA", "SpD", "Spe"] : [skill];
        foreach (var stat in stats)
        {
            var min = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Min", true).FirstOrDefault()!;
            var max = (NumericUpDown)Controls.Find($"NUD_{page}_{stat}_Max", true).FirstOrDefault()!;
            var lab = (Label)Controls.Find($"L_{page}_{stat}Spacer", true).FirstOrDefault()!;
            min.Value = 0;
            max.Value = 31;
            if (lab.Text == "||")
            {
                lab.Text = "~";
                lab.Location = lab.Location with { X = lab.Location.X - 1 };
            }
        }
    }

    private void IV_Spacer_Click(object sender, EventArgs e)
    {
        var l = (Label)sender;
        if (l.Text == "~")
        {
            l.Text = "||";
            l.Location = l.Location with { X = l.Location.X + 1 };
        }
        else
        {
            l.Text = "~";
            l.Location = l.Location with { X = l.Location.X - 1 };
        }
    }

    private void TB_TID_TextChanged(object sender, EventArgs e)
    {
        if (TB_TID.Text.Length > 0)
        {
            var tid = int.Parse(TB_TID.Text);
            if (tid > 0xFFFF)
            {
                tid = 0xFFFF;
                SetControlText($"{tid}", TB_TID);
            }
            Config.TID = tid;
        }
    }

    private void TB_SID_TextChanged(object sender, EventArgs e)
    {
        if (TB_SID.Text.Length > 0)
        {
            var sid = int.Parse(TB_SID.Text);
            if (sid > 0xFFFF)
            {
                sid = 0xFFFF;
                SetControlText($"{sid}", TB_SID);
            }
            Config.SID = sid;
        }
    }

    private void TB_SwitchIP_TextChanged(object sender, EventArgs e)
    {
        if (Config.Protocol is SwitchProtocol.WiFi)
        {
            Config.IP = TB_SwitchIP.Text;
            ConnectionConfig.IP = TB_SwitchIP.Text;
        }
        else
        {
            if (int.TryParse(TB_SwitchIP.Text, out var port) && port >= 0)
            {
                Config.UsbPort = port;
                ConnectionConfig.Port = port;
                return;
            }

            MessageBox.Show("Please enter a valid numerical USB port.");
        }
    }

    private readonly JsonSerializerOptions options = new() { WriteIndented = true };
    private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        var configpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        string output = JsonSerializer.Serialize(Config, options);
        using StreamWriter sw = new(configpath);
        sw.Write(output);

        if (ConnectionWrapper is { Connected: true })
        {
            try
            {
                _ = ConnectionWrapper.DisconnectAsync(Source.Token).ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }

        Source.Cancel();
        Source = new();
    }

    private void B_ReadWildPokemon_Click(object sender, EventArgs e)
    {
        if (ConnectionWrapper.Connected)
        {
            Task.Run(async () =>
            {
                try
                {
                    readPause = true;
                    await Task.Delay(100, Source.Token).ConfigureAwait(false);
                    var pk = await ConnectionWrapper.ReadEncounter(Source.Token).ConfigureAwait(false);
                    readPause = false;
                    if (pk is { Valid: true, Species: > 0 })
                    {
                        _enc = pk;
                        SetControlText(ParsePA8(pk), TB_Wild);
                    }
                    else
                    {
                        SetControlText("Not found!", TB_Wild);
                    }
                }
                catch (Exception ex)
                {
                    this.DisplayMessageBox(ex.Message);
                }
            });
        }
    }

    private void B_ReadB1S1_Click(object sender, EventArgs e)
    {
        if (ConnectionWrapper.Connected)
        {
            Task.Run(async () =>
            {
                try
                {
                    readPause = true;
                    await Task.Delay(100, Source.Token).ConfigureAwait(false);
                    var pk = await ConnectionWrapper.ReadBoxPokemon(Source.Token).ConfigureAwait(false);
                    readPause = false;
                    if (pk is { Valid: true, Species: > 0 })
                    {
                        _enc = pk;
                        SetControlText(ParsePA8(pk), TB_Wild);
                    }
                    else
                    {
                        SetControlText("Not found!", TB_Wild);
                    }
                }
                catch (Exception ex)
                {
                    this.DisplayMessageBox(ex.Message);
                }
            });
        }
    }


    private void AllowOnlyHex_KeyPress(object sender, KeyPressEventArgs e)
    {
        var c = e.KeyChar;
        if (c != (char)Keys.Back && !char.IsControl(c))
        {
            if (!c.IsHex())
            {
                System.Media.SystemSounds.Asterisk.Play();
                e.Handled = true;
            }
        }
    }

    public void AllowOnlyNumerical_KeyPress(object sender, KeyPressEventArgs e)
    {
        var c = e.KeyChar;
        if (c != (char)Keys.Back && !char.IsControl(c))
        {
            if (!c.IsDec())
            {
                System.Media.SystemSounds.Asterisk.Play();
                e.Handled = true;
            }
        }
    }

    public void AllowOnlyIP_KeyPress(object sender, KeyPressEventArgs e)
    {
        var c = e.KeyChar;
        if (c == (char)Keys.Return)
        {
            B_Connect_Click(sender, EventArgs.Empty);
        }
        else if (c != (char)Keys.Back && !char.IsControl(c))
        {
            if (!c.IsDec(true))
            {
                System.Media.SystemSounds.Asterisk.Play();
                e.Handled = true;
            }
        }
    }

    public void State_HandlePaste(object sender, KeyEventArgs e)
    {
        if (e is not { Modifiers: Keys.Control, KeyCode: Keys.V } && e is not { Modifiers: Keys.Shift, KeyCode: Keys.Insert }) return;
        var n = string.Empty;
        foreach (char c in Clipboard.GetText())
        {
            if (c.IsHex()) n += c;
        }

        var l = n.Length;
        if (l == 0)
        {
            Clipboard.Clear();
            return;
        }
        if (l > 8)
        {
            ((TextBox)sender).Text = n[..8];
        }
        else
        {
            Clipboard.SetText(n);
        }
    }

    public void Dec_HandlePaste(object sender, KeyEventArgs e)
    {
        if (e is not { Modifiers: Keys.Control, KeyCode: Keys.V } && e is not { Modifiers: Keys.Shift, KeyCode: Keys.Insert }) return;
        var n = string.Empty;

        foreach (char c in Clipboard.GetText())
        {
            if (c.IsDec()) n += c;
        }

        var l = n.Length;
        var tb = (TextBox)sender;
        var max = tb.MaxLength;
        if (l == 0)
        {
            Clipboard.Clear();
        }
        else if (l > max)
        {
            tb.Text = n[..max];
        }
        else
        {
            Clipboard.SetText(n);
        }
    }

    private void IP_HandlePaste(object sender, KeyEventArgs e)
    {
        if (e is not { Modifiers: Keys.Control, KeyCode: Keys.V } && e is not { Modifiers: Keys.Shift, KeyCode: Keys.Insert }) return;
        var n = string.Empty;

        foreach (char c in Clipboard.GetText())
        {
            if (c.IsDec(true)) n += c;
        }

        var l = n.Length;
        var tb = (TextBox)sender;
        var max = tb.MaxLength;
        if (l == 0)
        {
            Clipboard.Clear();
        }
        else if (l > max)
        {
            tb.Text = n[..max];
        }
        else
        {
            Clipboard.SetText(n);
        }
    }

    private void ValidateInputs()
    {
        // Initial
        var initial = (TextBox)Controls.Find($"TB_Initial", true).FirstOrDefault()!;
        if (string.IsNullOrEmpty(initial.GetText())) SetControlText("0", initial);
        var mon = (TextBox)Controls.Find($"TB_MonInitial", true).FirstOrDefault()!;
        if (string.IsNullOrEmpty(mon.GetText())) SetControlText("0", mon);

        // Advances
        var advances = (TextBox)Controls.Find($"TB_Advances", true).FirstOrDefault()!;
        var adv = advances.GetText();
        if (string.IsNullOrEmpty(adv) || adv is "0") SetControlText("1", advances);

        var monadvances = (TextBox)Controls.Find($"TB_MonAdvances", true).FirstOrDefault()!;
        var monadv = advances.GetText();
        if (string.IsNullOrEmpty(monadv) || monadv is "0") SetControlText("1", monadvances);

        // Seed
        if (string.IsNullOrEmpty(TB_InitialSeed0.GetText())) SetControlText("0", TB_InitialSeed0);

        if (TB_InitialSeed0.GetText() is "0")
        {
            SetControlText("1337", TB_InitialSeed0);
        }
        SetControlText(TB_InitialSeed0.GetText().PadLeft(16, '0'), TB_InitialSeed0);

        // IDs
        if (string.IsNullOrEmpty(TB_TID.GetText())) SetControlText("0", TB_TID);
        if (string.IsNullOrEmpty(TB_SID.GetText())) SetControlText("0", TB_SID);
        SetControlText(TB_TID.GetText().PadLeft(5, '0'), TB_TID);
        SetControlText(TB_SID.GetText().PadLeft(5, '0'), TB_SID);
    }

    private void CheckForUpdates()
    {
        Task.Run(async () =>
        {
            Version? latestVersion;
            try { latestVersion = GetLatestVersion(); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception while checking for latest version: {ex}");
                return;
            }

            if (latestVersion is null || latestVersion <= CurrentVersion)
                return;

            while (!IsHandleCreated) // Wait for form to be ready
                await Task.Delay(2_000).ConfigureAwait(false);
            await InvokeAsync(() => NotifyNewVersionAvailable(latestVersion));
        });
    }

    private void NotifyNewVersionAvailable(Version version)
    {
        Text += $" - Update v{version.Major}.{version.Minor}.{version.Build} available!";

#if !DEBUG
        using Subforms.UpdateNotifPopup nup = new(CurrentVersion, version);
        if (nup.ShowDialog() == DialogResult.OK)
        {
            Process.Start(new ProcessStartInfo("https://github.com/LegoFigure11/AutomaticRadParLiAmentedExtrapolator/releases/")
            {
                UseShellExecute = true
            });
        }
#endif
    }

    internal bool ResetSettingsFormOpen = false;
    private ResetSettings? ResetSettingsForm;
    private void B_ResetSettings_Click(object sender, EventArgs e)
    {
        if (!ResetSettingsFormOpen)
        {
            ResetSettingsFormOpen = true;
            ResetSettingsForm = new ResetSettings(ref Config, this);
            ResetSettingsForm.Show();
        }
        else
        {
            ResetSettingsForm?.Focus();
        }
    }

    private void B_Reset_Cancel_Click(object sender, EventArgs e)
    {
        GameResetSource.Cancel();
        GameResetSource = new();
    }

    private void B_Static_Search_Click(object sender, EventArgs e)
    {
        SetControlEnabledState(false, B_Static_Search);
        Task.Run(async () =>
        {
            var s0 = ulong.Parse(TB_InitialSeed0.GetText(), NumberStyles.AllowHexSpecifier);
            var s1 = ulong.Parse(TB_InitialSeed1.GetText(), NumberStyles.AllowHexSpecifier);
            var start = ulong.Parse(TB_Static_Initial.GetText());
            var end = ulong.Parse(TB_Static_Advances.GetText());

            var cfg = new StaticConfig()
            {
                SID = ushort.Parse(TB_SID.GetText()),
                TID = ushort.Parse(TB_TID.GetText()),

                UseDelay = CB_Static_Delay.GetIsChecked(),
                Delay = NUD_Static_Delay.GetValue(),

                TargetNature = GetFilterNatureType(CB_Static_Nature.GetSelectedIndex()),

                TargetMinIVs = [NUD_Static_HP_Min.GetValue(), NUD_Static_Atk_Min.GetValue(), NUD_Static_Def_Min.GetValue(), NUD_Static_SpA_Min.GetValue(), NUD_Static_SpD_Min.GetValue(), NUD_Static_Spe_Min.GetValue()],
                TargetMaxIVs = [NUD_Static_HP_Max.GetValue(), NUD_Static_Atk_Max.GetValue(), NUD_Static_Def_Max.GetValue(), NUD_Static_SpA_Max.GetValue(), NUD_Static_SpD_Max.GetValue(), NUD_Static_Spe_Max.GetValue()],
                SearchTypes = [GetIVSearchType(L_Static_HPSpacer.GetText()), GetIVSearchType(L_Static_AtkSpacer.GetText()), GetIVSearchType(L_Static_DefSpacer.GetText()), GetIVSearchType(L_Static_SpASpacer.GetText()), GetIVSearchType(L_Static_SpDSpacer.GetText()), GetIVSearchType(L_Static_SpeSpacer.GetText())],

                _pk = GetMainEncounter(CB_Static_Species.GetSelectedIndex()),

                FiltersEnabled = CB_Static_FiltersEnabled.GetIsChecked(),
            };
            var staticFrames = await Core.RNG.Static.Generate(s0, s1, start, end, cfg);

            SetBindingSourceDataSource(staticFrames, BS_Results);
            SetDataGridViewDataSource(BS_Results, DGV_Results);
            SetControlEnabledState(true, B_Static_Search);
            Frames = [.. staticFrames.Cast<object>()];
        });
    }

    private void CB_Static_Delay_CheckedChanged(object sender, EventArgs e)
    {
        SetControlEnabledState(CB_Static_Delay.GetIsChecked(), NUD_Static_Delay);
    }

    private void B_BabyMode_Go_Click(object sender, EventArgs e)
    {
        babyModeTarget = uint.Parse(TB_BabyMode.GetText());
        var delay = CB_BabyModeDelay.GetIsChecked() ? NUD_BabyModeDelay.GetValue() : 0u;
        babyModeTarget -= delay;
        babyMode = true;
        babyModePrimed = false;
        UpdateStatus($"Primed: {babyModeTarget:N0}");
    }

    private void B_BabyMode_Cancel_Click(object sender, EventArgs e)
    {
        babyMode = false;
        babyModePrimed = false;
        UpdateStatus("Monitoring RNG State...");
        try
        {
            Task.Run(async () =>
            {
                await ConnectionWrapper.DetachController(Source.Token).ConfigureAwait(false);
            });
        }
        catch
        {
            // Ignored
        }
    }

    private void CB_BabyModeDelay_CheckedChanged(object sender, EventArgs e)
    {
        SetControlEnabledState(CB_BabyModeDelay.GetIsChecked(), NUD_BabyModeDelay);
    }

    private void DGV_Results_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        var index = e.RowIndex;
        if (Frames.Count <= index) return;
        var row = DGV_Results.Rows[index];
        var result = Frames[index];

        // IVs
        if (result is IIVFrame iv)
        {
            string[] stats = ["HP", "Atk", "Def", "SpA", "SpD", "Spe"];
            byte[] ivs = [iv.HP, iv.Atk, iv.Def, iv.SpA, iv.SpD, iv.Spe];
            for (var i = 0; i < stats.Length; i++)
            {
                var col = DGV_Results.Columns[stats[i]]!.Index;
                if (ivs[i] == 0)
                {
                    row.Cells[col].Style.Font = BoldFont;
                    row.Cells[col].Style.ForeColor = Color.OrangeRed;
                }
                else if (ivs[i] == 31)
                {
                    row.Cells[col].Style.Font = BoldFont;
                    row.Cells[col].Style.ForeColor = Color.SeaGreen;
                }
                else
                {
                    row.Cells[col].Style.ForeColor = row.DefaultCellStyle.ForeColor;
                    row.Cells[col].Style.Font = row.DefaultCellStyle.Font;
                }
            }
        }
    }

    private void B_CopyIVs_Click(object sender, EventArgs e)
    {
        foreach (Control ctrl in TC_Main.SelectedTab!.Controls)
        {
            if (ctrl is GroupBox gb)
            {
                if (gb.Name.EndsWith("_Filters"))
                {
                    foreach (Control child in gb.Controls)
                    {
                        if (child is NumericUpDown nud)
                        {
                            if (nud.Name.IndexOf("HP") > 0) nud.Value = _enc.IV_HP;
                            else if (nud.Name.IndexOf("Atk") > 0) nud.Value = _enc.IV_ATK;
                            else if (nud.Name.IndexOf("Def") > 0) nud.Value = _enc.IV_DEF;
                            else if (nud.Name.IndexOf("SpA") > 0) nud.Value = _enc.IV_SPA;
                            else if (nud.Name.IndexOf("SpD") > 0) nud.Value = _enc.IV_SPD;
                            else if (nud.Name.IndexOf("Spe") > 0) nud.Value = _enc.IV_SPE;
                        }
                    }
                }
            }
        }
    }

    private void CB_BabyMode_Action_SelectedIndexChanged(object sender, EventArgs e)
    {
        babyModeButton = CB_BabyMode_Action.GetSelectedIndex() == 0 ? SwitchButton.A : SwitchButton.HOME;
    }

    private void B_File1_Click(object sender, EventArgs e)
    {
        OpenFileDialog Open = new()
        {
            Title = "Select a File",
            Filter = "Legends: Arceus Pokémon File|*.PA8",
            FilterIndex = 1,
            RestoreDirectory = true,
            Multiselect = false
        };
        if (Open.ShowDialog() == DialogResult.OK)
        {
            var file = Open.FileName;
            try
            {
                var bytes = File.ReadAllBytes(file);
                var pk = new PA8(bytes);
                first = pk;
                TB_File1.Text = file;
                selectedFirst = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void B_File2_Click(object sender, EventArgs e)
    {
        OpenFileDialog Open = new()
        {
            Title = "Select a File",
            Filter = "Legends: Arceus Pokémon File|*.PA8",
            FilterIndex = 1,
            RestoreDirectory = true,
            Multiselect = false
        };
        if (Open.ShowDialog() == DialogResult.OK)
        {
            var file = Open.FileName;
            try
            {
                var bytes = File.ReadAllBytes(file);
                var pk = new PA8(bytes);
                second = pk;
                TB_File2.Text = file;
                selectedSecond = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void B_CalculateGroupSeed_Click(object sender, EventArgs e)
    {
        if (!selectedFirst || !selectedSecond)
        {
            this.DisplayMessageBox("Please select both your files.");
            return;
        }

        PA8[] entities = [first, second];
        var ECs = entities.Select(m => m.EncryptionConstant).ToArray();

        if (ECs.Distinct().Count() < 2)
        {
            this.DisplayMessageBox("Selected files are the same. Please select two different files.");
            return;
        }

        foreach (var entity in entities)
        {
            Debug.WriteLine($"Checking {entity.FileName}");

            var matches = Solver.GetAllSeeds(entity);
            foreach (var match in matches)
            {
                Debug.WriteLine($"Pokemon Seed: {match:x16}\n");
                var gen_seed_matches = Solver.FindPotentialGenSeeds(match);
                foreach (var gen_match in gen_seed_matches)
                {
                    Debug.WriteLine($"-Generator Seed: {gen_match:x16}");
                    var groupSeed = Solver.GetGroupSeed(gen_match);
                    Debug.WriteLine($"-Group Seed: {groupSeed:x16}\n");
                    if (!Solver.IsValidGroupSeed(groupSeed, ECs))
                        continue;

                    Debug.WriteLine($"Found a matching group seed: {groupSeed:x16}");
                    SetControlText($"{groupSeed:X16}", TB_GroupSeedResult);
                    this.DisplayMessageBox($"Found a matching group seed: {groupSeed:x16}", "Seed result");
                    return;
                }
            }
        }

        Debug.WriteLine($"No matching group seed found.");
        SetControlText("not found", TB_GroupSeedResult);
        this.DisplayMessageBox($"No matching group seed found.");
    }

    private void LL_SeedSolverAttribution_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        var page = new ProcessStartInfo
        {
            FileName = "https://gist.github.com/Lusamine/1a7f9e4418b618daa75f7c9e9c2a9e91",
            UseShellExecute = true
        };

        Process.Start(page);
    }

    private void B_CopyToInitial_Click(object sender, EventArgs e)
    {
#if DEBUG
        if (((Button)sender).Name == "B_CopyToInitial" && ModifierKeys == Keys.Shift)
        {
            Task.Run(
                async () =>
                {
                    try
                    {
                        ulong s0 = ulong.Parse(TB_InitialSeed0.Text, NumberStyles.AllowHexSpecifier);
                        ulong s1 = ulong.Parse(TB_InitialSeed1.Text, NumberStyles.AllowHexSpecifier);
                        await ConnectionWrapper.SetCurrentRNGState(s0, s1, Source.Token).ConfigureAwait(false);
                        reset = true;
                    }
                    catch (Exception ex)
                    {
                        this.DisplayMessageBox($"Something went wrong when writing the RNG state: {ex.Message}");
                    }
                }
            );
        }
        else
        {
#endif
            if (TB_CurrentSeed0.Text != string.Empty && TB_CurrentSeed1.Text != string.Empty)
            {
                var s0 = TB_CurrentSeed0.Text;
                var s1 = TB_CurrentSeed1.Text;

                SetControlText(s0, TB_InitialSeed0);
                SetControlText(s1, TB_InitialSeed1);

                reset = true;
            }
#if DEBUG
        }
#endif
    }
}

