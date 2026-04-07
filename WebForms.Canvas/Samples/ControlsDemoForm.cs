using Canvas.Windows.Forms.Drawing;
using System.Windows.Forms;

namespace Canvas.Windows.Forms.Samples;

public class ControlsDemoForm : Form
{
    private Label? _titleLabel;
    private Button? _button1;
    private Button? _button2;
    private TextBox? _textBox;
    private Label? _outputLabel;
    private CheckBox? _checkBox1;
    private CheckBox? _checkBox2;
    private RadioButton? _radio1;
    private RadioButton? _radio2;
    private RadioButton? _radio3;
    private PictureBox? _pictureBox;
    private ComboBox? _comboBox1;
    private ComboBox? _comboBox2;
    private Label? _comboBoxOutput;
    private CheckedListBox? _checkedListBox;
    private Label? _checkedListBoxOutput;
    private DateTimePicker? _dateTimePicker;
    private Label? _dateTimePickerOutput;

    public ControlsDemoForm()
    {
        Text = "Controls Demo";
        Width = 820;
        Height = 720;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        const int leftColX = 10;
        const int rightColX = 420;
        const int colWidth = 380;

        // Title Label
        _titleLabel = new Label
        {
            Text = "Windows Forms Controls Demo",
            Left = 10,
            Top = 10,
            Width = 780,
            Height = 20,
            ForeColor = Color.FromArgb(0, 0, 128),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(_titleLabel);

        // TextBox
        _textBox = new TextBox
        {
            Left = leftColX,
            Top = 40,
            Width = 200,
            Height = 20,
            Text = "Type here..."
        };
        _textBox.TextChanged += OnTextBoxChanged;
        Controls.Add(_textBox);

        // Output Label
        _outputLabel = new Label
        {
            Text = "Text: Type here...",
            Left = leftColX,
            Top = 70,
            Width = colWidth,
            Height = 20,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_outputLabel);

        // AutoComplete TextBox
        var autoCompleteLabel = new Label
        {
            Text = "AutoComplete TextBox (try: apple, banana, cherry):",
            Left = leftColX,
            Top = 100,
            Width = colWidth,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(autoCompleteLabel);

        var autoCompleteTextBox = new TextBox
        {
            Left = leftColX,
            Top = 125,
            Width = 200,
            Height = 23,
            AutoCompleteMode = AutoCompleteMode.Suggest,
            AutoCompleteSource = AutoCompleteSource.CustomSource,
            AutoCompleteCustomSource = new[]
            {
                "apple", "apricot", "avocado",
                "banana", "blueberry", "blackberry",
                "cherry", "cranberry", "coconut",
                "date", "dragonfruit", "durian",
                "elderberry", "fig", "grape",
                "grapefruit", "guava", "honeydew",
                "kiwi", "lemon", "lime",
                "mango", "melon", "nectarine",
                "orange", "papaya", "peach",
                "pear", "pineapple", "plum",
                "raspberry", "strawberry", "tangerine",
                "watermelon"
            }
        };
        Controls.Add(autoCompleteTextBox);

        // Buttons
        _button1 = new Button
        {
            Text = "Click Me!",
            Left = leftColX,
            Top = 160,
            Width = 100,
            Height = 30
        };
        _button1.Click += OnButton1Click;
        Controls.Add(_button1);

        _button2 = new Button
        {
            Text = "Disabled",
            Left = leftColX + 110,
            Top = 160,
            Width = 100,
            Height = 30,
            Enabled = false
        };
        Controls.Add(_button2);

        // CheckBoxes Section
        var checkBoxLabel = new Label
        {
            Text = "CheckBoxes:",
            Left = leftColX,
            Top = 205,
            Width = 100,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(checkBoxLabel);

        _checkBox1 = new CheckBox
        {
            Text = "Option 1",
            Left = leftColX,
            Top = 230,
            Width = 150,
            Height = 20,
            Checked = true
        };
        _checkBox1.CheckedChanged += OnCheckBox1Changed;
        Controls.Add(_checkBox1);

        _checkBox2 = new CheckBox
        {
            Text = "Option 2",
            Left = leftColX,
            Top = 255,
            Width = 150,
            Height = 20
        };
        _checkBox2.CheckedChanged += OnCheckBox2Changed;
        Controls.Add(_checkBox2);

        // RadioButtons Section
        var radioLabel = new Label
        {
            Text = "RadioButtons (Select One):",
            Left = leftColX,
            Top = 290,
            Width = 200,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(radioLabel);

        _radio1 = new RadioButton
        {
            Text = "Choice A",
            Left = leftColX,
            Top = 315,
            Width = 150,
            Height = 20,
            Checked = true
        };
        _radio1.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio1);

        _radio2 = new RadioButton
        {
            Text = "Choice B",
            Left = leftColX,
            Top = 340,
            Width = 150,
            Height = 20
        };
        _radio2.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio2);

        _radio3 = new RadioButton
        {
            Text = "Choice C",
            Left = leftColX,
            Top = 365,
            Width = 150,
            Height = 20
        };
        _radio3.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio3);

        // Status Label
        var statusLabel = new Label
        {
            Text = "Status: Ready",
            Left = leftColX,
            Top = 395,
            Width = colWidth,
            Height = 20,
            ForeColor = Color.FromArgb(0, 128, 0)
        };
        Controls.Add(statusLabel);

        // DateTimePicker Section (right column)
        var dateLabel = new Label
        {
            Text = "DateTimePicker:",
            Left = rightColX,
            Top = 40,
            Width = 150,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(dateLabel);

        _dateTimePicker = new DateTimePicker
        {
            Left = rightColX,
            Top = 65,
            Width = 180,
            Format = DateTimePickerFormat.Short,
            MinDate = DateTime.Today.AddDays(-7),
            MaxDate = DateTime.Today.AddDays(30),
            Value = DateTime.Today
        };
        _dateTimePicker.ValueChanged += OnDateTimePickerValueChanged;
        Controls.Add(_dateTimePicker);

        _dateTimePickerOutput = new Label
        {
            Text = $"Selected: {DateTime.Today:d}",
            Left = rightColX,
            Top = 95,
            Width = colWidth,
            Height = 20,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_dateTimePickerOutput);

        // ComboBox Section
        var comboLabel = new Label
        {
            Text = "ComboBoxes:",
            Left = rightColX,
            Top = 140,
            Width = 100,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(comboLabel);

        // DropDownList style (non-editable)
        _comboBox1 = new ComboBox
        {
            Left = rightColX,
            Top = 165,
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _comboBox1.Items.Add("Red");
        _comboBox1.Items.Add("Green");
        _comboBox1.Items.Add("Blue");
        _comboBox1.Items.Add("Yellow");
        _comboBox1.Items.Add("Orange");
        _comboBox1.Items.Add("Purple");
        _comboBox1.Items.Add("Cyan");
        _comboBox1.Items.Add("Magenta");
        _comboBox1.SelectedIndex = 0;
        _comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        Controls.Add(_comboBox1);

        var combo1Label = new Label
        {
            Text = "(DropDownList)",
            Left = rightColX + 160,
            Top = 165,
            Width = 100,
            Height = 20,
            ForeColor = Color.FromArgb(128, 128, 128)
        };
        Controls.Add(combo1Label);

        // DropDown style (editable)
        _comboBox2 = new ComboBox
        {
            Left = rightColX,
            Top = 195,
            Width = 150,
            DropDownStyle = ComboBoxStyle.DropDown
        };
        _comboBox2.Items.Add("Apple");
        _comboBox2.Items.Add("Banana");
        _comboBox2.Items.Add("Cherry");
        _comboBox2.Items.Add("Date");
        _comboBox2.Items.Add("Elderberry");
        _comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
        Controls.Add(_comboBox2);

        var combo2Label = new Label
        {
            Text = "(DropDown)",
            Left = rightColX + 160,
            Top = 195,
            Width = 100,
            Height = 20,
            ForeColor = Color.FromArgb(128, 128, 128)
        };
        Controls.Add(combo2Label);

        _comboBoxOutput = new Label
        {
            Text = "ComboBox selection: Red",
            Left = rightColX,
            Top = 225,
            Width = colWidth,
            Height = 20,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_comboBoxOutput);

        // CheckedListBox Section
        var checkedListBoxLabel = new Label
        {
            Text = "CheckedListBox:",
            Left = leftColX,
            Top = 425,
            Width = 150,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(checkedListBoxLabel);

        _checkedListBox = new CheckedListBox
        {
            Left = leftColX,
            Top = 450,
            Width = 180,
            Height = 100,
            CheckOnClick = true
        };
        _checkedListBox.Items.Add("Feature A");
        _checkedListBox.Items.Add("Feature B");
        _checkedListBox.Items.Add("Feature C");
        _checkedListBox.Items.Add("Feature D");
        _checkedListBox.Items.Add("Feature E");
        _checkedListBox.SetItemChecked(0, true);
        _checkedListBox.SetItemChecked(2, true);
        _checkedListBox.ItemCheck += CheckedListBox_ItemCheck;
        Controls.Add(_checkedListBox);

        _checkedListBoxOutput = new Label
        {
            Text = "Checked: 2 items",
            Left = leftColX + 190,
            Top = 450,
            Width = 200,
            Height = 40,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_checkedListBoxOutput);

        // PictureBox Section
        var pictureLabel = new Label
        {
            Text = "PictureBox:",
            Left = rightColX,
            Top = 270,
            Width = 100,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(pictureLabel);

        _pictureBox = new PictureBox
        {
            Left = rightColX,
            Top = 295,
            Width = 150,
            Height = 60,
            ImageUrl = "https://via.placeholder.com/150x60", // Placeholder image
            SizeMode = PictureBoxSizeMode.StretchImage
        };
        Controls.Add(_pictureBox);

        var pictureInfo = new Label
        {
            Text = "Image loaded from URL",
            Left = rightColX + 160,
            Top = 295,
            Width = 210,
            Height = 40,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(pictureInfo);
    }

    private void OnDateTimePickerValueChanged(object? sender, EventArgs e)
    {
        if (_dateTimePicker != null && _dateTimePickerOutput != null)
        {
            _dateTimePickerOutput.Text = $"Selected: {_dateTimePicker.Value:d}";
        }
    }

    private void CheckedListBox_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
        if (_checkedListBox != null && _checkedListBoxOutput != null)
        {
            // Calculate new count (accounting for the change)
            var newCount = _checkedListBox.CheckedIndices.Count;
            if (e.NewValue == CheckState.Checked && e.CurrentValue != CheckState.Checked)
                newCount++;
            else if (e.NewValue != CheckState.Checked && e.CurrentValue == CheckState.Checked)
                newCount--;

            _checkedListBoxOutput.Text = $"Checked: {newCount} item{(newCount != 1 ? "s" : "")}";
        }
    }

    private void OnTextBoxChanged(object? sender, EventArgs e)
    {
        if (_outputLabel != null && _textBox != null)
        {
            _outputLabel.Text = $"Text: {_textBox.Text}";
        }
    }

    private void OnButton1Click(object? sender, EventArgs e)
    {
        if (_button1 != null)
        {
            _button1.Text = $"Clicked! ({DateTime.Now:HH:mm:ss})";
        }

        if (_button2 != null)
        {
            _button2.Enabled = !_button2.Enabled;
            _button2.Text = _button2.Enabled ? "Enabled" : "Disabled";
        }
    }

    private void OnCheckBox1Changed(object? sender, EventArgs e)
    {
        if (_checkBox1 != null)
        {
            System.Diagnostics.Debug.WriteLine($"CheckBox 1: {_checkBox1.Checked}");
        }
    }

    private void OnCheckBox2Changed(object? sender, EventArgs e)
    {
        if (_checkBox2 != null)
        {
            System.Diagnostics.Debug.WriteLine($"CheckBox 2: {_checkBox2.Checked}");
        }
    }

    private void OnRadioChanged(object? sender, EventArgs e)
    {
        if (sender is RadioButton rb && rb.Checked)
        {
            System.Diagnostics.Debug.WriteLine($"Selected: {rb.Text}");
        }
    }

    private void ComboBox1_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_comboBoxOutput != null && _comboBox1 != null)
        {
            _comboBoxOutput.Text = $"ComboBox selection: {_comboBox1.Text}";
        }
    }

    private void ComboBox2_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_comboBoxOutput != null && _comboBox2 != null)
        {
            _comboBoxOutput.Text = $"ComboBox selection: {_comboBox2.Text}";
        }
    }
}

