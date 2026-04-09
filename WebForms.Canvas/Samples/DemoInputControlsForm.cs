using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates input controls: TextBox, MaskedTextBox, RichTextBox, NumericUpDown, DateTimePicker, MonthCalendar, LinkLabel
/// </summary>
public class DemoInputControlsForm : Form
{
    private Label? _textBoxOutput;
    private Label? _numericOutput;
    private Label? _dateOutput;
    private Label? _maskedOutput;

    public DemoInputControlsForm()
    {
        Text = "Input Controls Demo";
        Width = 750;
        Height = 650;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        const int leftCol = 20;
        const int rightCol = 385;
        int y = 20;

        // ═══ Title ═══
        var title = new Label
        {
            Text = "Input Controls",
            Left = 20, Top = y, Width = 710, Height = 30,
            Font = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(title);
        y += 40;

        // ═══ LEFT COLUMN ═══

        // TextBox
        var lblTextBox = new Label { Text = "TextBox (plain):", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblTextBox);
        y += 25;

        var textBox = new TextBox { Left = leftCol, Top = y, Width = 340, Height = 23, Text = "Type here..." };
        textBox.TextChanged += (s, e) => { if (_textBoxOutput != null) _textBoxOutput.Text = $"Text: {textBox.Text}"; };
        Controls.Add(textBox);
        y += 30;

        _textBoxOutput = new Label { Text = "Text: Type here...", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_textBoxOutput);
        y += 30;

        // AutoComplete TextBox
        var lblAutoComplete = new Label { Text = "TextBox with AutoComplete:", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblAutoComplete);
        y += 25;

        var autoCompleteTextBox = new TextBox
        {
            Left = leftCol, Top = y, Width = 340, Height = 23,
            AutoCompleteMode = AutoCompleteMode.Suggest,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            AutoCompleteCustomSource = new[] { "apple", "apricot", "banana", "blueberry", "cherry", "coconut", "date", "fig", "grape", "kiwi", "lemon", "mango", "orange", "peach", "pear", "plum", "strawberry" }
        };
        Controls.Add(autoCompleteTextBox);
        y += 30;

        var lblAutoHelp = new Label { Text = "(try: apple, banana, cherry...)", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.Gray };
        Controls.Add(lblAutoHelp);
        y += 30;

        // MaskedTextBox
        var lblMasked = new Label { Text = "MaskedTextBox (Phone):", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblMasked);
        y += 25;

        var maskedTextBox = new MaskedTextBox { Left = leftCol, Top = y, Width = 200, Height = 23, Mask = "(000) 000-0000" };
        maskedTextBox.TextChanged += (s, e) => { if (_maskedOutput != null) _maskedOutput.Text = $"Phone: {maskedTextBox.Text}"; };
        Controls.Add(maskedTextBox);
        y += 30;

        _maskedOutput = new Label { Text = "Phone: ", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_maskedOutput);
        y += 30;

        // NumericUpDown
        var lblNumeric = new Label { Text = "NumericUpDown:", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblNumeric);
        y += 25;

        var numericUpDown = new NumericUpDown { Left = leftCol, Top = y, Width = 120, Minimum = 0, Maximum = 100, Value = 50, DecimalPlaces = 1 };
        numericUpDown.ValueChanged += (s, e) => { if (_numericOutput != null) _numericOutput.Text = $"Value: {numericUpDown.Value:F1}"; };
        Controls.Add(numericUpDown);
        y += 30;

        _numericOutput = new Label { Text = "Value: 50.0", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_numericOutput);
        y += 30;

        // LinkLabel
        var lblLink = new Label { Text = "LinkLabel:", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblLink);
        y += 25;

        var linkLabel = new LinkLabel { Text = "Visit Microsoft Learn", Left = leftCol, Top = y, Width = 200, Height = 20 };
        linkLabel.LinkClicked += (s, e) => { linkLabel.LinkVisited = true; linkLabel.Text = "Link clicked!"; };
        Controls.Add(linkLabel);
        y += 35;

        // RichTextBox
        var lblRich = new Label { Text = "RichTextBox:", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblRich);
        y += 25;

        var richTextBox = new RichTextBox { Left = leftCol, Top = y, Width = 340, Height = 80, Text = "This is a RichTextBox.\nIt supports multiple lines.\nTry editing this text!" };
        Controls.Add(richTextBox);

        // ═══ RIGHT COLUMN ═══
        y = 60;

        // DateTimePicker
        var lblDate = new Label { Text = "DateTimePicker:", Left = rightCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblDate);
        y += 25;

        var dateTimePicker = new DateTimePicker
        {
            Left = rightCol, Top = y, Width = 200,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };
        dateTimePicker.ValueChanged += (s, e) => { if (_dateOutput != null) _dateOutput.Text = $"Selected: {dateTimePicker.Value:d}"; };
        Controls.Add(dateTimePicker);
        y += 30;

        _dateOutput = new Label { Text = $"Selected: {DateTime.Today:d}", Left = rightCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_dateOutput);
        y += 30;

        // MonthCalendar
        var lblCalendar = new Label { Text = "MonthCalendar:", Left = rightCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblCalendar);
        y += 25;

        var monthCalendar = new MonthCalendar { Left = rightCol, Top = y, MaxSelectionCount = 7 };
        Controls.Add(monthCalendar);
        y += 180;

        // Info
        var info = new Label
        {
            Text = "Use arrow keys, Home/End, Page Up/Down to navigate calendar.",
            Left = rightCol, Top = y, Width = 340, Height = 40,
            ForeColor = Color.Gray
        };
        Controls.Add(info);
    }
}
