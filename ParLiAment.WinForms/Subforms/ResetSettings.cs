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
        /*TB_OT.Text = _config.OT_Name;
        TB_ButtonPressDelay.Text = $"{_config.NameEntryButtonPressDelay}";
        TB_PageChangeDelay.Text = $"{_config.NameEntryPageChangeDelay}";
        TB_NameRejectDelay.Text = $"{_config.NameEntryRejectDelay}";
        TB_ReloadNameDelay.Text = $"{_config.NameEntryReloadNameScreenDelay}";*/
    }

    private void TB_OT_TextChanged(object sender, EventArgs e)
    {
        //_config.OT_Name = TB_OT.Text;
    }

    private void TB_ButtonPressDelay_TextChanged(object sender, EventArgs e)
    {
        //_config.NameEntryButtonPressDelay = int.Parse(TB_ButtonPressDelay.Text);
    }

    private void TB_PageChangeDelay_TextChanged(object sender, EventArgs e)
    {
        //_config.NameEntryPageChangeDelay = int.Parse(TB_PageChangeDelay.Text);
    }

    private void TB_NameRejectDelay_TextChanged(object sender, EventArgs e)
    {
        //_config.NameEntryRejectDelay = int.Parse(TB_NameRejectDelay.Text);
    }

    private void TB_LoadNameDelay_TextChanged(object sender, EventArgs e)
    {
        //_config.NameEntryReloadNameScreenDelay = int.Parse(TB_ReloadNameDelay.Text);
    }
}
