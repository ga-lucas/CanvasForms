namespace System.Windows.Forms;

public enum Keys
{
    None = 0,
    Back = 8,
    Tab = 9,
    Enter = 13,
    Shift = 16,
    Control = 17,
    Alt = 18,
    Escape = 27,
    Space = 32,
    PageUp = 33,
    PageDown = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Delete = 46,
    D0 = 48,
    D1 = 49,
    D2 = 50,
    D3 = 51,
    D4 = 52,
    D5 = 53,
    D6 = 54,
    D7 = 55,
    D8 = 56,
    D9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123
}

public class KeyEventArgs : EventArgs
{
    public Keys KeyCode { get; }
    public bool Alt { get; }
    public bool Control { get; }
    public bool Shift { get; }
    public bool Handled { get; set; }

    public KeyEventArgs(Keys keyCode, bool alt = false, bool control = false, bool shift = false)
    {
        KeyCode = keyCode;
        Alt = alt;
        Control = control;
        Shift = shift;
    }
}

public delegate void KeyEventHandler(object? sender, KeyEventArgs e);

public class KeyPressEventArgs : EventArgs
{
    public char KeyChar { get; }
    public bool Handled { get; set; }

    public KeyPressEventArgs(char keyChar)
    {
        KeyChar = keyChar;
    }
}

public delegate void KeyPressEventHandler(object? sender, KeyPressEventArgs e);
