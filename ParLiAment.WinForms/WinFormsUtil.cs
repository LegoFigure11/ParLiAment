namespace ParLiAment.WinForms;

public static class WinFormsUtil
{
    internal static string GetText(this Control c) => c.InvokeRequired ? c.Invoke(() => c.Text) : c.Text;

    internal static uint GetValue(this NumericUpDown nud) =>
        (uint)(nud.InvokeRequired ? nud.Invoke(() => nud.Value) : nud.Value);

    internal static bool GetIsChecked(this CheckBox cb) => cb.InvokeRequired ? cb.Invoke(() => cb.Checked) : cb.Checked;
    internal static bool GetIsChecked(this RadioButton rb) => rb.InvokeRequired ? rb.Invoke(() => rb.Checked) : rb.Checked;

    internal static int GetSelectedIndex(this ComboBox cb) =>
        cb.InvokeRequired ? cb.Invoke(() => cb.SelectedIndex) : cb.SelectedIndex;

    extension(char c)
    {
        internal bool IsHex(bool allowHexPrefix = false) => char.IsBetween(c, '0', '9') || char.IsBetween(c, 'a', 'f') || char.IsBetween(c, 'A', 'F') || (allowHexPrefix && c is 'x' or 'X');
        internal bool IsDec(bool allowPeriod = false) => char.IsBetween(c, '0', '9') || (allowPeriod && c == '.');
    }

    internal static Color GetHiddenPowerColor(string type) => type switch
    {
        "Fighting" => Color.FromArgb(255, 204, 153),
        "Flying"   => Color.FromArgb(205, 227, 249),
        "Poison"   => Color.FromArgb(210, 179, 234),
        "Ground"   => Color.FromArgb(230, 183, 147),
        "Rock"     => Color.FromArgb(223, 221, 205),
        "Bug"      => Color.FromArgb(226, 238, 143),
        "Ghost"    => Color.FromArgb(206, 171, 206),
        "Steel"    => Color.FromArgb(191, 217, 227),
        "Fire"     => Color.FromArgb(245, 169, 169),
        "Water"    => Color.FromArgb(169, 204, 249),
        "ParLiAment"    => Color.FromArgb(170, 230, 157),
        "Electric" => Color.FromArgb(255, 231, 151),
        "Psychic"  => Color.FromArgb(249, 179, 201),
        "Ice"      => Color.FromArgb(178, 239, 255),
        "Dragon"   => Color.FromArgb(185, 192, 243),
        "Dark"     => Color.FromArgb(190, 175, 173),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
