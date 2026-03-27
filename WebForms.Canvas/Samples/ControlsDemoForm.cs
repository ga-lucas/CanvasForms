using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

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

    public ControlsDemoForm()
    {
        Text = "Controls Demo";
        Width = 400;
        Height = 500;
        BackColor = Color.FromArgb(240, 240, 240);

        InitializeControls();
    }

    private void InitializeControls()
    {
        // Title Label
        _titleLabel = new Label
        {
            Text = "Windows Forms Controls Demo",
            Left = 10,
            Top = 10,
            Width = 380,
            Height = 20,
            ForeColor = Color.FromArgb(0, 0, 128),
            TextAlign = ContentAlignment.TopCenter
        };
        Controls.Add(_titleLabel);

        // TextBox
        _textBox = new TextBox
        {
            Left = 10,
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
            Left = 10,
            Top = 70,
            Width = 380,
            Height = 20,
            ForeColor = Color.FromArgb(64, 64, 64)
        };
        Controls.Add(_outputLabel);

        // Buttons
        _button1 = new Button
        {
            Text = "Click Me!",
            Left = 10,
            Top = 100,
            Width = 100,
            Height = 30
        };
        _button1.Click += OnButton1Click;
        Controls.Add(_button1);

        _button2 = new Button
        {
            Text = "Disabled",
            Left = 120,
            Top = 100,
            Width = 100,
            Height = 30,
            Enabled = false
        };
        Controls.Add(_button2);

        // CheckBoxes Section
        var checkBoxLabel = new Label
        {
            Text = "CheckBoxes:",
            Left = 10,
            Top = 145,
            Width = 100,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(checkBoxLabel);

        _checkBox1 = new CheckBox
        {
            Text = "Option 1",
            Left = 10,
            Top = 170,
            Width = 150,
            Height = 20,
            Checked = true
        };
        _checkBox1.CheckedChanged += OnCheckBox1Changed;
        Controls.Add(_checkBox1);

        _checkBox2 = new CheckBox
        {
            Text = "Option 2",
            Left = 10,
            Top = 195,
            Width = 150,
            Height = 20
        };
        _checkBox2.CheckedChanged += OnCheckBox2Changed;
        Controls.Add(_checkBox2);

        // RadioButtons Section
        var radioLabel = new Label
        {
            Text = "RadioButtons (Select One):",
            Left = 10,
            Top = 230,
            Width = 200,
            Height = 20,
            ForeColor = Color.Black
        };
        Controls.Add(radioLabel);

        _radio1 = new RadioButton
        {
            Text = "Choice A",
            Left = 10,
            Top = 255,
            Width = 150,
            Height = 20,
            Checked = true
        };
        _radio1.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio1);

        _radio2 = new RadioButton
        {
            Text = "Choice B",
            Left = 10,
            Top = 280,
            Width = 150,
            Height = 20
        };
        _radio2.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio2);

        _radio3 = new RadioButton
        {
            Text = "Choice C",
            Left = 10,
            Top = 305,
            Width = 150,
            Height = 20
        };
        _radio3.CheckedChanged += OnRadioChanged;
        Controls.Add(_radio3);

        // Status Label
        var statusLabel = new Label
        {
            Text = "Status: Ready",
            Left = 10,
            Top = 340,
            Width = 380,
            Height = 20,
            ForeColor = Color.FromArgb(0, 128, 0)
        };
        Controls.Add(statusLabel);
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
}
