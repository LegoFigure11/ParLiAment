using System.Text.Json;

namespace ParLiAment.WinForms.Subforms;

public partial class ResetSettings : Form
{
    private readonly ClientConfig _config;
    private readonly MainWindow _mainWindow;

    public ResetSettings(ref ClientConfig cfg, MainWindow m)
    {
        _config = cfg;
        _mainWindow = m;
        InitializeComponent();
    }

    private void ResetSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
        string output = JsonSerializer.Serialize(_config);
        using StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
        sw.Write(output);

        _mainWindow.Config = _config;
        _mainWindow.ResetSettingsFormOpen = false;
    }

    private void ResetSettings_Load(object sender, EventArgs e)
    {
        TB_ExtraTimeReturnHome.Text = $"{_config.ExtraTimeReturnHome}";
        TB_ExtraTimeLoadProfile.Text = $"{_config.ExtraTimeLoadProfile}";
        TB_ExtraTimeCloseGame.Text = $"{_config.ExtraTimeCloseGame}";
        TB_ExtraTimeLoadGame.Text = $"{_config.ExtraTimeLoadGame}";
        CB_AvoidUpdate.SelectedIndex = _config.AvoidSystemUpdate ? 0 : 1;
        CB_ScreenOff.SelectedIndex = _config.ScreenOff ? 0 : 1;

        TB_ExtraTimeReturnHome.KeyPress += _mainWindow.AllowOnlyNumerical_KeyPress!;
        TB_ExtraTimeReturnHome.KeyDown += _mainWindow.Dec_HandlePaste!;
        TB_ExtraTimeLoadProfile.KeyPress += _mainWindow.AllowOnlyNumerical_KeyPress!;
        TB_ExtraTimeLoadProfile.KeyDown += _mainWindow.Dec_HandlePaste!;
        TB_ExtraTimeLoadGame.KeyPress += _mainWindow.AllowOnlyNumerical_KeyPress!;
        TB_ExtraTimeLoadGame.KeyDown += _mainWindow.Dec_HandlePaste!;
        TB_ExtraTimeCloseGame.KeyPress += _mainWindow.AllowOnlyNumerical_KeyPress!;
        TB_ExtraTimeCloseGame.KeyDown += _mainWindow.Dec_HandlePaste!;
    }

    private void TB_ExtraTimeReturnHome_TextChanged(object sender, EventArgs e)
    {
        _config.ExtraTimeReturnHome = int.Parse(TB_ExtraTimeReturnHome.Text);
    }

    private void TB_ExtraTimeCloseGame_TextChanged(object sender, EventArgs e)
    {
        _config.ExtraTimeCloseGame = int.Parse(TB_ExtraTimeCloseGame.Text);
    }

    private void TB_ExtraTimeLoadProfile_TextChanged(object sender, EventArgs e)
    {
        _config.ExtraTimeLoadProfile = int.Parse(TB_ExtraTimeLoadProfile.Text);
    }

    private void TB_ExtraTimeLoadGame_TextChanged(object sender, EventArgs e)
    {
        _config.ExtraTimeLoadGame = int.Parse(TB_ExtraTimeLoadGame.Text);
    }

    private void CB_AvoidUpdate_SelectedIndexChanged(object sender, EventArgs e)
    {
        _config.AvoidSystemUpdate = CB_AvoidUpdate.GetSelectedIndex() == 0;
    }

    private void CB_ScreenOff_SelectedIndexChanged(object sender, EventArgs e)
    {
        _config.ScreenOff = CB_ScreenOff.GetSelectedIndex() == 0;
    }
}
