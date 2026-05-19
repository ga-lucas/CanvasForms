
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
    /// When true, only ASCII characters are accepted as input.
    /// Non-ASCII characters are rejected and <see cref="MaskInputRejected"/> is raised.
    /// </summary>
    public bool AsciiOnly { get; set; } = false;

    /// <summary>
    /// Gets or sets the data type used to validate the value of the masked text box.
    /// When non-null, <see cref="TypeValidationCompleted"/> is raised on Validating
    /// with the parse result, matching WinForms behaviour.
    /// </summary>
    public Type? ValidatingType { get; set; }

    /// <summary>
    /// Raised when the control has finished validating the current text against
    /// <see cref="ValidatingType"/>. Equivalent to WinForms <c>MaskedTextBox.TypeValidationCompleted</c>.
    /// </summary>
    public event TypeValidationEventHandler? TypeValidationCompleted;

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

    // ── Mask helpers ─────────────────────────────────────────────────────────

    /// <summary>Returns a list of every editable position in the mask as (maskIndex, maskToken) pairs.</summary>
    private List<(int MaskIndex, char Token)> GetEditPositions()
    {
        var list = new List<(int, char)>();
        for (int i = 0; i < _mask.Length; i++)
        {
            char m = _mask[i];
            if (IsEditToken(m))
                list.Add((i, m));
        }
        return list;
    }

    private static bool IsEditToken(char m) =>
        m == '0' || m == '9' || m == '#' ||
        m == 'L' || m == '?' ||
        m == 'A' || m == 'a' ||
        m == '&' || m == 'C';

    /// <summary>
    /// Maps a raw-text index (character stored in <see cref="TextBoxBase.Text"/>) to the
    /// corresponding 0-based edit-position index within <see cref="GetEditPositions"/>.
    /// </summary>
    private int GetEditPositionIndexForRawIndex(int rawIndex) => rawIndex; // 1-to-1: raw text only stores editable chars

    /// <summary>
    /// Maps the current display caret (position in <see cref="MaskedText"/>) to the
    /// raw-text (editable-only) index that the next typed character would land on.
    /// </summary>
    private int DisplayCaretToRawIndex(int displayCaret)
    {
        if (string.IsNullOrEmpty(_mask)) return displayCaret;

        var editPositions = GetEditPositions();
        // Count how many editable positions are before or at displayCaret in the masked string.
        int editCount = 0;
        int maskPos = 0;
        for (int d = 0; d < displayCaret && maskPos < _mask.Length; d++, maskPos++)
        {
            if (IsEditToken(_mask[maskPos]))
                editCount++;
        }
        return Math.Min(editCount, editPositions.Count);
    }

    /// <summary>
    /// Validates <paramref name="c"/> against the mask token at the given edit-position index.
    /// </summary>
    private bool CharMatchesToken(char c, char token)
    {
        return token switch
        {
            '0'       => char.IsDigit(c),                        // required digit
            '9'       => char.IsDigit(c) || c == ' ',            // optional digit or space
            '#'       => char.IsDigit(c) || c == '+' || c == '-' || c == ' ', // digit, sign, or space
            'L'       => char.IsLetter(c),                       // required letter
            '?'       => char.IsLetter(c) || c == ' ',           // optional letter or space
            'A'       => char.IsLetterOrDigit(c),                // required alphanumeric
            'a'       => char.IsLetterOrDigit(c) || c == ' ',    // optional alphanumeric
            '&'       => c != '\0',                              // any non-null
            'C'       => true,                                   // any character
            _         => false
        };
    }

    // ── Input overrides ───────────────────────────────────────────────────────

    protected internal override void OnKeyPress(KeyPressEventArgs e)
    {
        if (ReadOnly || !Enabled || string.IsNullOrEmpty(_mask))
        {
            base.OnKeyPress(e);
            return;
        }

        var c = e.KeyChar;
        if (char.IsControl(c))
        {
            base.OnKeyPress(e);
            return;
        }

        // AsciiOnly: reject non-ASCII characters
        if (AsciiOnly && c > '\x7F')
        {
            e.Handled = true;
            MaskInputRejected?.Invoke(this, new MaskInputRejectedEventArgs(
                DisplayCaretToRawIndex(_selectionStart), MaskedTextResultHint.AsciiCharacterExpected));
            return;
        }

        var editPositions = GetEditPositions();
        // The caret within the raw text (editable-only chars)
        var rawCaret = DisplayCaretToRawIndex(_selectionStart);

        // If past the last editable position the mask is full — reject
        if (rawCaret >= editPositions.Count)
        {
            e.Handled = true;
            MaskInputRejected?.Invoke(this, new MaskInputRejectedEventArgs(rawCaret, MaskedTextResultHint.UnavailableEditPosition));
            return;
        }

        var token = editPositions[rawCaret].Token;
        if (!CharMatchesToken(c, token))
        {
            e.Handled = true;
            var hint = token is '0' or '9' or '#'
                ? MaskedTextResultHint.DigitExpected
                : token is 'L' or '?'
                    ? MaskedTextResultHint.LetterExpected
                    : MaskedTextResultHint.InvalidInput;
            MaskInputRejected?.Invoke(this, new MaskInputRejectedEventArgs(rawCaret, hint));
            return;
        }

        // Accept — let base insert the raw character into Text
        base.OnKeyPress(e);
    }

    protected override void OnValidating(CancelEventArgs e)
    {
        base.OnValidating(e);

        if (ValidatingType != null)
        {
            object? parsedValue = null;
            bool isValid = false;
            string message = string.Empty;

            try
            {
                parsedValue = Convert.ChangeType(Text, ValidatingType);
                isValid = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var args = new TypeValidationEventArgs(ValidatingType, isValid, parsedValue, message);
            TypeValidationCompleted?.Invoke(this, args);

            if (args.Cancel)
                e.Cancel = true;
        }
    }

    protected internal override void OnKeyDown(KeyEventArgs e)
    {
        if (string.IsNullOrEmpty(_mask) || e.Control || IsNavigationKey(e.KeyCode))
        {
            base.OnKeyDown(e);
            return;
        }

        if (e.KeyCode == Keys.Back)
        {
            // Erase last editable position before caret, leave literals intact
            if (_selectionLength > 0)
            {
                // Selection: clear all editable chars in the selection range
                var rawStart = DisplayCaretToRawIndex(_selectionStart);
                var rawEnd   = DisplayCaretToRawIndex(_selectionStart + _selectionLength);
                if (rawStart < rawEnd)
                {
                    var t = Text;
                    Text = t.Remove(rawStart, rawEnd - rawStart);
                    _selectionStart = rawStart;
                    _selectionLength = 0;
                }
                e.Handled = true;
                return;
            }

            var rawCaret = DisplayCaretToRawIndex(_selectionStart);
            if (rawCaret > 0)
            {
                var t = Text;
                Text = t.Remove(rawCaret - 1, 1);
                _selectionStart = Math.Max(0, _selectionStart - 1);
                _selectionLength = 0;
            }
            e.Handled = true;
            return;
        }

        if (e.KeyCode == Keys.Delete)
        {
            var rawCaret = DisplayCaretToRawIndex(_selectionStart);
            if (_selectionLength > 0)
            {
                var rawEnd = DisplayCaretToRawIndex(_selectionStart + _selectionLength);
                if (rawCaret < rawEnd)
                {
                    var t = Text;
                    Text = t.Remove(rawCaret, rawEnd - rawCaret);
                    _selectionLength = 0;
                }
                e.Handled = true;
                return;
            }
            if (rawCaret < Text.Length)
            {
                Text = Text.Remove(rawCaret, 1);
                e.Handled = true;
            }
            return;
        }

        base.OnKeyDown(e);
    }

    private bool IsNavigationKey(Keys key) =>
        key == Keys.Left || key == Keys.Right || key == Keys.Home || key == Keys.End ||
        key == Keys.Up   || key == Keys.Down  || key == Keys.Tab;

    // ── Additional WinForms properties ────────────────────────────────────────

    /// <summary>
    /// Gets a value indicating whether all required editable positions in the mask have been filled.
    /// </summary>
    public bool MaskFull
    {
        get
        {
            if (string.IsNullOrEmpty(_mask)) return true;
            var editPositions = GetEditPositions();
            // Required positions: '0', 'L', 'A', '&' (no space/blank allowed)
            int required = editPositions.Count(ep =>
                ep.Token == '0' || ep.Token == 'L' || ep.Token == 'A' || ep.Token == '&');
            return Text.Length >= required && !MaskedText.Contains(_promptChar);
        }
    }

    /// <summary>
    /// Returns the raw (unmasked) text — only the editable characters the user typed,
    /// without any literal characters from the mask.
    /// </summary>
    public string UnmaskedText => Text;

    private MaskFormat _textMaskFormat = MaskFormat.IncludeLiterals;

    /// <summary>
    /// Controls how the <see cref="Text"/> property returns its value — whether it
    /// includes literal characters and/or prompt characters from the mask.
    /// Matches the WinForms <c>MaskedTextBox.TextMaskFormat</c> property.
    /// </summary>
    public MaskFormat TextMaskFormat
    {
        get => _textMaskFormat;
        set { _textMaskFormat = value; Invalidate(); }
    }

    /// <summary>
    /// Returns the formatted text according to the current <see cref="TextMaskFormat"/>.
    /// </summary>
    public string FormattedText
    {
        get
        {
            if (string.IsNullOrEmpty(_mask)) return Text;
            return _textMaskFormat switch
            {
                MaskFormat.ExcludePromptAndLiterals => Text, // raw editable chars only
                MaskFormat.IncludeLiterals           => BuildFormattedText(includePrompts: false),
                MaskFormat.IncludePrompt             => Text, // prompts without literals is unusual; raw
                MaskFormat.IncludePromptAndLiterals  => GetDisplayText(),
                _                                    => Text
            };
        }
    }

    private string BuildFormattedText(bool includePrompts)
    {
        var result = new System.Text.StringBuilder();
        int dataIdx = 0;
        string rawText = Text;
        for (int i = 0; i < _mask.Length; i++)
        {
            char m = _mask[i];
            if (IsEditToken(m))
            {
                if (dataIdx < rawText.Length)
                    result.Append(rawText[dataIdx++]);
                else if (includePrompts)
                    result.Append(_promptChar);
                // else skip (ExcludePromptAndLiterals variant with literals)
            }
            else
            {
                result.Append(m); // literal
            }
        }
        return result.ToString();
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

public delegate void TypeValidationEventHandler(object? sender, TypeValidationEventArgs e);

/// <summary>
/// Provides data for the <see cref="MaskedTextBox.TypeValidationCompleted"/> event.
/// Matches the WinForms <c>TypeValidationEventArgs</c> signature.
/// </summary>
public class TypeValidationEventArgs : EventArgs
{
    public TypeValidationEventArgs(Type returnType, bool isValidInput, object? returnValue, string message)
    {
        ReturnType   = returnType;
        IsValidInput = isValidInput;
        ReturnValue  = returnValue;
        Message      = message;
    }

    /// <summary>The type that the control attempted to validate against.</summary>
    public Type ReturnType { get; }

    /// <summary>True if the text successfully converted to <see cref="ReturnType"/>.</summary>
    public bool IsValidInput { get; }

    /// <summary>The converted value when <see cref="IsValidInput"/> is true; otherwise null.</summary>
    public object? ReturnValue { get; }

    /// <summary>Describes the failure when <see cref="IsValidInput"/> is false.</summary>
    public string Message { get; }

    /// <summary>
    /// Set to true in the event handler to cancel the Validating event
    /// (prevent focus leaving the control on validation failure).
    /// </summary>
    public bool Cancel { get; set; }
}
