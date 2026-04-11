using System.Net.Sockets;
using PKHeX.Core;
using ParLiAment.Core.Structures;
using SysBot.Base;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchCommand;

namespace ParLiAment.Core.Connection;

public class ConnectionWrapperAsync(SwitchConnectionConfig Config, Action<string> StatusUpdate) : Offsets
{
    public readonly ISwitchConnectionAsync Connection = Config.Protocol switch
    {
        SwitchProtocol.USB => new SwitchUSBAsync(Config.Port),
        _ => new SwitchSocketAsync(Config),
    };

    public bool Connected => IsConnected;
    private bool IsConnected { get; set; }
    private readonly bool CRLF = Config.Protocol is SwitchProtocol.WiFi;

    private string title { get; set; } = string.Empty;
    private readonly SAV8LA sav = new();


    public async Task<(bool, string)> Connect(CancellationToken token)
    {
        if (Connected) return (true, "");

        try
        {
            StatusUpdate("Connecting...");
            Connection.Connect();
            IsConnected = true;

            StatusUpdate("Detecting Game Version");
            title = await Connection.GetTitleID(token).ConfigureAwait(false);
            if (title != TitleID)
            {
                IsConnected = false;
                return (false, $"{title} is not Pokémon Legends: Arceus.");
            }

            StatusUpdate("Configuring sysmodule...");
            var cmd = Configure(SwitchConfigureParameter.mainLoopSleepTime, 15, CRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            StatusUpdate("Reading SAV...");
            await ReadMyStatusAsync(token).ConfigureAwait(false);

            StatusUpdate("Connected!");
            return (true, "");
        }
        catch (SocketException e)
        {
            IsConnected = false;
            return (false, e.Message);
        }
    }

    public async Task<(bool, string)> DisconnectAsync(CancellationToken token)
    {
        if (!Connected) return (true, "");

        try
        {
            StatusUpdate("Disconnecting controller");
            await DetachController(token).ConfigureAwait(false);

            StatusUpdate("Disconnecting...");
            Connection.Disconnect();
            IsConnected = false;
            StatusUpdate("Disconnected!");
            return (true, "");
        }
        catch (SocketException e)
        {
            IsConnected = false;
            return (false, e.Message);
        }
    }

    public (ushort TID, ushort SID) GetIDs()
    {
        var myStatus = sav.MyStatus;
        return (myStatus.TID16, myStatus.SID16);
    }

    private ulong _currentSeedOffset = 0;
    public async Task<(ulong s0, ulong s1)> GetCurrentRNGState(CancellationToken token)
    {
        if (_currentSeedOffset == 0)
            _currentSeedOffset = await Connection.PointerAll(MainRNGPointer, token).ConfigureAwait(false);

        var data = await Connection.ReadBytesAsync(_currentSeedOffset, 16, token).ConfigureAwait(false);
        return (BitConverter.ToUInt64(data, 0), BitConverter.ToUInt64(data, 8));
    }

    public async Task SetCurrentRNGState(ulong _s0, ulong _s1, CancellationToken token)
    {
        if (_currentSeedOffset == 0)
            _currentSeedOffset = await Connection.PointerAll(MainRNGPointer, token).ConfigureAwait(false);

        var s0 = BitConverter.GetBytes(_s0);
        var s1 = BitConverter.GetBytes(_s1);
        await Connection.WriteBytesAsync(s0, _currentSeedOffset, token).ConfigureAwait(false);
        await Connection.WriteBytesAsync(s1, _currentSeedOffset + 8, token).ConfigureAwait(false);
    }

    private ulong _myStatusOffset = 0;
    private async Task ReadMyStatusAsync(CancellationToken token)
    {
        if (_myStatusOffset == 0)
            _myStatusOffset = await Connection.PointerAll(MyStatusPointer, token).ConfigureAwait(false);

        var data = await Connection.ReadBytesAsync(_myStatusOffset, 0x50, token).ConfigureAwait(false);
        data.AsSpan().CopyTo(sav.MyStatus.Data);
    }


    private ulong _wildPokemonOffset = 0;
    public async Task<PA8> ReadEncounter(CancellationToken token)
    {
        if (_wildPokemonOffset == 0)
            _wildPokemonOffset = await Connection.PointerAll(WildPokemonPointer, token).ConfigureAwait(false);

        var data = await Connection.ReadBytesAsync(_wildPokemonOffset, BoxFormatSlotSize, token).ConfigureAwait(false);
        return new PA8(data);
    }

    private ulong _boxPokemonOffset = 0;
    public async Task<PA8> ReadBoxPokemon(CancellationToken token)
    {
        if (_boxPokemonOffset == 0)
            _boxPokemonOffset = await Connection.PointerAll(BoxStartPokemonPointer, token).ConfigureAwait(false);

        var data = await Connection.ReadBytesAsync(_boxPokemonOffset, BoxFormatSlotSize, token).ConfigureAwait(false);
        return new PA8(data);
    }

    public async Task PressAndHold(IEnumerable<SwitchButton> b, int hold, int delay, CancellationToken token)
    {
        foreach (var key in b)
            await Connection.SendAsync(Hold(key, CRLF), token).ConfigureAwait(false);
        await Task.Delay(hold, token).ConfigureAwait(false);
        foreach (var key in b)
            await Connection.SendAsync(Release(key, CRLF), token).ConfigureAwait(false);
        await Task.Delay(delay, token).ConfigureAwait(false);
    }

    public async Task PressHOME(int sleep, CancellationToken token)
    {
        await Connection.SendAsync(Click(HOME, CRLF), token).ConfigureAwait(false);
        await Task.Delay(sleep, token).ConfigureAwait(false);
    }

    public async Task DetachController(CancellationToken token)
    {
        await Connection.SendAsync(SwitchCommand.DetachController(CRLF), token).ConfigureAwait(false);
    }

    public async Task PressButton(SwitchButton btn, int delay, CancellationToken token)
    {
        await Connection.SendAsync(Click(btn, CRLF), token).ConfigureAwait(false);
        await Task.Delay(delay, token).ConfigureAwait(false);
    }

    public async Task DoTurboCommand(string command, CancellationToken token)
    {
        switch (command)
        {
            case "Wait (100ms)":
                await Task.Delay(100, token).ConfigureAwait(false);
                break;
            case "Wait (500ms)":
                await Task.Delay(500, token).ConfigureAwait(false);
                break;
            case "Wait (1000ms)":
                await Task.Delay(1_000, token).ConfigureAwait(false);
                break;
            default:
                _ = command switch
                {
                    "A" => await Connection.SendAsync(Click(A, CRLF), token).ConfigureAwait(false),
                    "B" => await Connection.SendAsync(Click(B, CRLF), token).ConfigureAwait(false),
                    "X" => await Connection.SendAsync(Click(X, CRLF), token).ConfigureAwait(false),
                    "Y" => await Connection.SendAsync(Click(Y, CRLF), token).ConfigureAwait(false),

                    "Left Stick (L3)" => await Connection.SendAsync(Click(LSTICK, CRLF), token).ConfigureAwait(false),
                    "Right Stick (R3)" => await Connection.SendAsync(Click(RSTICK, CRLF), token).ConfigureAwait(false),

                    "L" => await Connection.SendAsync(Click(L, CRLF), token).ConfigureAwait(false),
                    "R" => await Connection.SendAsync(Click(R, CRLF), token).ConfigureAwait(false),
                    "ZL" => await Connection.SendAsync(Click(ZL, CRLF), token).ConfigureAwait(false),
                    "ZR" => await Connection.SendAsync(Click(ZR, CRLF), token).ConfigureAwait(false),

                    "+" => await Connection.SendAsync(Click(PLUS, CRLF), token).ConfigureAwait(false),
                    "-" => await Connection.SendAsync(Click(MINUS, CRLF), token).ConfigureAwait(false),

                    "Up (Hold)" => await Connection.SendAsync(SetStick(SwitchStick.LEFT, 0, 30000, CRLF), token).ConfigureAwait(false),
                    "Down (Hold)" => await Connection.SendAsync(SetStick(SwitchStick.LEFT, 0, -30000, CRLF), token).ConfigureAwait(false),
                    "Left (Hold)" => await Connection.SendAsync(SetStick(SwitchStick.LEFT, -30000, 0, CRLF), token).ConfigureAwait(false),
                    "Right (Hold)" => await Connection.SendAsync(SetStick(SwitchStick.LEFT, 30000, 0, CRLF), token).ConfigureAwait(false),
                    "Release Stick" => await Connection.SendAsync(ResetStick(SwitchStick.LEFT, CRLF), token).ConfigureAwait(false),

                    "D-Pad Up" => await Connection.SendAsync(Click(DUP, CRLF), token).ConfigureAwait(false),
                    "D-Pad Down" => await Connection.SendAsync(Click(DDOWN, CRLF), token).ConfigureAwait(false),
                    "D-Pad Left" => await Connection.SendAsync(Click(DLEFT, CRLF), token).ConfigureAwait(false),
                    "D-Pad Right" => await Connection.SendAsync(Click(DRIGHT, CRLF), token).ConfigureAwait(false),

                    "HOME" => await Connection.SendAsync(Click(HOME, CRLF), token).ConfigureAwait(false),
                    "Screenshot" => await Connection.SendAsync(Click(CAPTURE, CRLF), token).ConfigureAwait(false),

                    _ => throw new NotImplementedException(),
                };
                break;
        }
    }
}
