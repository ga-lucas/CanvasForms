using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

/// <summary>
/// Demonstrates selection controls: Button, CheckBox, RadioButton, ComboBox, ListBox, CheckedListBox, ProgressBar
/// </summary>
public class DemoSelectionControlsForm : Form
{
    private Label? _comboOutput;
    private Label? _listBoxOutput;
    private Label? _checkedListOutput;
    private ProgressBar? _progressBar;
    private Label? _progressLabel;
    private int _progressValue = 0;

    public DemoSelectionControlsForm()
    {
        Text = "Selection Controls Demo";
        Width = 750;
        Height = 680;
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
            Text = "Selection Controls",
            Left = 20, Top = y, Width = 710, Height = 30,
            Font = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(title);
        y += 40;

        // ═══ LEFT COLUMN ═══

        // Buttons
        var lblButtons = new Label { Text = "Buttons:", Left = leftCol, Top = y, Width = 340, Height = 20, Font = new Font("Arial", 12) };
        Controls.Add(lblButtons);
        y += 25;

        var button1 = new Button { Text = "Click Me!", Left = leftCol, Top = y, Width = 100, Height = 30 };
        button1.Click += (s, e) => button1.Text = $"Clicked!";
        Controls.Add(button1);

        var button2 = new Button { Text = "Disabled", Left = leftCol + 110, Top = y, Width = 100, Height = 30, Enabled = false };
        Controls.Add(button2);
        y += 40;

        // CheckBoxes
        var lblCheckBoxes = new Label { Text = "CheckBoxes:", Left = leftCol, Top = y, Width = 340, Height = 20, Font = new Font("Arial", 12) };
        Controls.Add(lblCheckBoxes);
        y += 25;

        var checkBox1 = new CheckBox { Text = "Option 1", Left = leftCol, Top = y, Width = 150, Height = 20, Checked = true };
        Controls.Add(checkBox1);
        y += 25;

        var checkBox2 = new CheckBox { Text = "Option 2", Left = leftCol, Top = y, Width = 150, Height = 20 };
        Controls.Add(checkBox2);
        y += 35;

        // RadioButtons
        var lblRadio = new Label { Text = "RadioButtons:", Left = leftCol, Top = y, Width = 340, Height = 20, Font = new Font("Arial", 12) };
        Controls.Add(lblRadio);
        y += 25;

        var radio1 = new RadioButton { Text = "Choice A", Left = leftCol, Top = y, Width = 150, Height = 20, Checked = true };
        Controls.Add(radio1);
        y += 25;

        var radio2 = new RadioButton { Text = "Choice B", Left = leftCol, Top = y, Width = 150, Height = 20 };
        Controls.Add(radio2);
        y += 25;

        var radio3 = new RadioButton { Text = "Choice C", Left = leftCol, Top = y, Width = 150, Height = 20 };
        Controls.Add(radio3);
        y += 35;

        // ComboBox
        var lblCombo = new Label { Text = "ComboBox (DropDownList):", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblCombo);
        y += 25;

        var comboBox = new ComboBox { Left = leftCol, Top = y, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        comboBox.Items.Add("Red"); comboBox.Items.Add("Green"); comboBox.Items.Add("Blue"); comboBox.Items.Add("Yellow");
        comboBox.SelectedIndex = 0;
        comboBox.SelectedIndexChanged += (s, e) => { if (_comboOutput != null) _comboOutput.Text = $"Selected: {comboBox.Text}"; };
        Controls.Add(comboBox);
        y += 30;

        _comboOutput = new Label { Text = "Selected: Red", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_comboOutput);
        y += 35;

        // CheckedListBox
        var lblChecked = new Label { Text = "CheckedListBox:", Left = leftCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblChecked);
        y += 25;

        var checkedListBox = new CheckedListBox { Left = leftCol, Top = y, Width = 200, Height = 100, CheckOnClick = true };
        checkedListBox.Items.Add("Feature A"); checkedListBox.Items.Add("Feature B"); checkedListBox.Items.Add("Feature C");
        checkedListBox.SetItemChecked(0, true);
        checkedListBox.ItemCheck += (s, e) =>
        {
            if (_checkedListOutput == null) return;
            var count = checkedListBox.CheckedIndices.Count;
            if (e.NewValue == CheckState.Checked && e.CurrentValue != CheckState.Checked) count++;
            else if (e.NewValue != CheckState.Checked && e.CurrentValue == CheckState.Checked) count--;
            _checkedListOutput.Text = $"Checked: {count} item{(count != 1 ? "s" : "")}";
        };
        Controls.Add(checkedListBox);
        y += 105;

        _checkedListOutput = new Label { Text = "Checked: 1 item", Left = leftCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_checkedListOutput);

        // ═══ RIGHT COLUMN ═══
        y = 60;

        // ListBox (Single Selection)
        var lblListBox = new Label { Text = "ListBox (Single Selection):", Left = rightCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblListBox);
        y += 25;

        var listBox = new ListBox { Left = rightCol, Top = y, Width = 200, Height = 150, SelectionMode = SelectionMode.One };
        listBox.Items.Add("Apple"); listBox.Items.Add("Banana"); listBox.Items.Add("Cherry"); listBox.Items.Add("Date"); listBox.Items.Add("Elderberry"); listBox.Items.Add("Fig"); listBox.Items.Add("Grape");
        listBox.SelectedIndexChanged += (s, e) =>
        {
            if (_listBoxOutput != null && listBox.SelectedIndex >= 0)
                _listBoxOutput.Text = $"Selected: {listBox.SelectedItem}";
        };
        Controls.Add(listBox);
        y += 155;

        _listBoxOutput = new Label { Text = "Selected: (none)", Left = rightCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_listBoxOutput);
        y += 30;

        // ProgressBar
        var lblProgress = new Label { Text = "ProgressBar:", Left = rightCol, Top = y, Width = 340, Height = 20 };
        Controls.Add(lblProgress);
        y += 25;

        _progressBar = new ProgressBar { Left = rightCol, Top = y, Width = 300, Height = 23, Minimum = 0, Maximum = 100, Value = 0 };
        Controls.Add(_progressBar);
        y += 30;

        _progressLabel = new Label { Text = "0%", Left = rightCol, Top = y, Width = 340, Height = 20, ForeColor = Color.FromArgb(64, 64, 64) };
        Controls.Add(_progressLabel);
        y += 30;

        var btnIncrement = new Button { Text = "+10", Left = rightCol, Top = y, Width = 60, Height = 25 };
        btnIncrement.Click += (s, e) =>
        {
            _progressValue = Math.Min(100, _progressValue + 10);
            if (_progressBar != null) _progressBar.Value = _progressValue;
            if (_progressLabel != null) _progressLabel.Text = $"{_progressValue}%";
        };
        Controls.Add(btnIncrement);

        var btnReset = new Button { Text = "Reset", Left = rightCol + 70, Top = y, Width = 60, Height = 25 };
        btnReset.Click += (s, e) =>
        {
            _progressValue = 0;
            if (_progressBar != null) _progressBar.Value = 0;
            if (_progressLabel != null) _progressLabel.Text = "0%";
        };
        Controls.Add(btnReset);
        y += 35;

        // Info
        var info = new Label
        {
            Text = "Use arrow keys, Home/End, Page Up/Down to navigate ListBox.",
            Left = rightCol, Top = y, Width = 340, Height = 40,
            ForeColor = Color.Gray
        };
        Controls.Add(info);
    }
}
