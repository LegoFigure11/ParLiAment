using ParLiAment.Core.Interfaces;
using SysBot.Base;

namespace ParLiAment.WinForms;

public class ClientConfig : ISeedResetConfig
{
    // Connection
    public string IP { get; set; } = "192.168.0.0";
    public int UsbPort { get; set; } = 0;
    public SwitchProtocol Protocol { get; set; } = SwitchProtocol.WiFi;

    // Fields
    public int TID { get; set; } = 1337;
    public int SID { get; set; } = 1390;

    // Seed Reset
    public int ExtraTimeReturnHome { get; set; } = 0;
    public int ExtraTimeCloseGame { get; set; } = 0;

    public int ExtraTimeLoadProfile { get; set; } = 0;
    public bool AvoidSystemUpdate { get; set; } = false;
    public int ExtraTimeLoadGame { get; set; } = 0;

    public bool ScreenOff { get; set; } = false;
}
