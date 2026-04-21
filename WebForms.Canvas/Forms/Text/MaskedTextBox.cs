
namespace System.Windows.Forms;

/// <summary>
/// Represents a Windows Forms MaskedTextBox control that applies input formatting masks
/// </summary>
public class MaskedTextBox : TextBoxBase
{
    private string _mask = string.Empty;
    private char _promptChar = '_';
    private char _passwordChar = '\0';
    private bool _hidePromptOnLeave = false;
    private MaskFormat _cutCopyMaskFormat = MaskFormat.IncludeLiterals;
    private bool _useSystemPasswordChar = false;
    private bool _beepOnError = false;

    public event EventHandler? MaskChanged;
    public event MaskInputRejectedEventHandler? MaskInputRejected;

    public MaskedTextBox()
    {
        Width = 100;
        Height = 23;
    }

    public string Mask
    {
        get => _mask;
        set
        {
            var newMask = value ?? string.Empty;
            if (_mask != newMask)
            {
                _mask = newMask;
                Text = string.Empty;
                MaskChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public char PromptChar
    {
        get => _promptChar;
        set { _promptChar = value; Invalidate(); }
    }

    public char PasswordChar
    {
        get => _passwordChar;
        set { _passwordChar = value; Invalidate(); }
    }

    public bool HidePromptOnLeave
    {
        get => _hidePromptOnLeave;
        set { _hidePromptOnLeave = value; Invalidate(); }
    }

    public bool BeepOnError { get => _beepOnError; set => _beepOnError = value; }
    public MaskFormat CutCopyMaskFormat { get => _cutCopyMaskFormat; set => _cutCopyMaskFormat = value; }
    public bool UseSystemPasswordChar { get => _useSystemPasswordChar; set { _useSystemPasswordChar = value; Invalidate(); } }

    /// <summary>
    /// Returns true if the current value satisfies the mask
    /// </summary>
    public bool MaskCompleted
    {
        get
        {
            if (string.IsNullOrEmpty(_mask)) return true;
            var display = GetDisplayText();
            return !display.Contains(_promptChar);
        }
    }

    /// <summary>
    /// The displayed text with prompt characters inserted
    /// </summary>
    public string MaskedText => GetDisplayText();

    protected override string GetDisplayText()
    {
        if (string.IsNullOrEmpty(_mask))
            return _passwordChar != '\0' ? new string(_passwordChar, Text.Length) : Text;

        var result = new System.Text.StringBuilder();
        int dataIdx = 0;
        string rawText = Text;

        for (int i = 0; i < _mask.Length; i++)
        {
            char m = _mask[i];
            if (m == '0' || m == '9' || m == '#' || m == 'L' || m == '?' || m == 'A' || m == 'a' || m == '&' || m == 'C')
            {
                // Editable position
                bool hide = _hidePromptOnLeave && !Focused;
                if (dataIdx < rawText.Length)
                {
                    char d = rawText[dataIdx++];
                    result.Append(_passwordChar != '\0' ? _passwordChar : d);
                }
                else
                {
                    result.Append(hide ? ' ' : _promptChar);
                }
            }
            else
            {
                // Literal character
                result.Append(m);
            }
        }
        return result.ToString();
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (!string.IsNullOrEmpty(_mask) && !ValidateKey(e.KeyCode) && !e.Control && !IsNavigationKey(e.KeyCode))
        {
            e.Handled = true;
            MaskInputRejected?.Invoke(this, new MaskInputRejectedEventArgs(Text.Length, MaskedTextResultHint.DigitExpected));
            return;
        }
        base.OnKeyDown(e);
    }

    private bool ValidateKey(Keys key)
    {
        return key == Keys.Back || key == Keys.Delete
            || (key >= Keys.D0 && key <= Keys.D9)
            || (key >= Keys.A && key <= Keys.Z);
    }

    private bool IsNavigationKey(Keys key)
    {
        return key == Keys.Left || key == Keys.Right || key == Keys.Home || key == Keys.End;
    }
}

public enum MaskFormat { ExcludePromptAndLiterals, IncludeLiterals, IncludePrompt, IncludePromptAndLiterals }

public delegate void MaskInputRejectedEventHandler(object? sender, MaskInputRejectedEventArgs e);

public class MaskInputRejectedEventArgs : EventArgs
{
    public int Position { get; }
    public MaskedTextResultHint RejectionHint { get; }
    public MaskInputRejectedEventArgs(int position, MaskedTextResultHint hint) { Position = position; RejectionHint = hint; }
}

public enum MaskedTextResultHint
{
    Unknown = -1, CharacterEscaped = 1, NoEffect = 2, SideEffect = 3, Success = 4,
    AsciiCharacterExpected = -2, AlphanumericCharacterExpected = -3, DigitExpected = -4,
    LetterExpected = -5, SignedDigitExpected = -6, InvalidInput = -51, PromptCharNotAllowed = -52,
    UnavailableEditPosition = -53, PositionOutOfRange = -54
}
