using WebForms.Canvas.Drawing;
using WebForms.Canvas.Forms;

namespace WebForms.Canvas.Samples;

public class ListBoxDemoForm : Form
{
    private ListBox listBox1;
    private ListBox listBox2;
    private Button btnAdd;
    private Button btnRemove;
    private Button btnClear;
    private TextBox txtNewItem;
    private Label lblSelected;
    private Label lblMultiSelect;
    private CheckBox chkMultiSelect;

    public ListBoxDemoForm()
    {
        Text = "ListBox Demo";
        Width = 600;
        Height = 450;
        BackColor = Color.FromArgb(240, 240, 240);

        // Title Label
        var lblTitle = new Label
        {
            Text = "ListBox Control Demo",
            Left = 20,
            Top = 20,
            Width = 560,
            Height = 30,
            Font = new Font("Arial", 16),
            ForeColor = Color.FromArgb(0, 51, 153)
        };
        Controls.Add(lblTitle);

        // ListBox 1 - Single Selection
        var lblList1 = new Label
        {
            Text = "Single Selection:",
            Left = 20,
            Top = 60,
            Width = 150,
            Height = 20
        };
        Controls.Add(lblList1);

        listBox1 = new ListBox
        {
            Left = 20,
            Top = 85,
            Width = 200,
            Height = 200,
            SelectionMode = SelectionMode.One
        };

        // Add some sample items
        listBox1.Items.Add("Apple");
        listBox1.Items.Add("Banana");
        listBox1.Items.Add("Cherry");
        listBox1.Items.Add("Date");
        listBox1.Items.Add("Elderberry");
        listBox1.Items.Add("Fig");
        listBox1.Items.Add("Grape");
        listBox1.Items.Add("Honeydew");
        listBox1.Items.Add("Ice Cream Bean");
        listBox1.Items.Add("Jackfruit");
        listBox1.Items.Add("Kiwi");
        listBox1.Items.Add("Lemon");
        listBox1.Items.Add("Mango");

        listBox1.SelectedIndexChanged += (s, e) =>
        {
            if (listBox1.SelectedIndex >= 0)
            {
                lblSelected?.Text = $"Selected: {listBox1.SelectedItem}";
            }
        };

        Controls.Add(listBox1);

        // ListBox 2 - Multi Selection
        lblMultiSelect = new Label
        {
            Text = "Multi Selection (Ctrl/Shift):",
            Left = 240,
            Top = 60,
            Width = 200,
            Height = 20
        };
        Controls.Add(lblMultiSelect);

        listBox2 = new ListBox
        {
            Left = 240,
            Top = 85,
            Width = 200,
            Height = 200,
            SelectionMode = SelectionMode.MultiExtended
        };

        listBox2.Items.Add("C#");
        listBox2.Items.Add("JavaScript");
        listBox2.Items.Add("Python");
        listBox2.Items.Add("Java");
        listBox2.Items.Add("C++");
        listBox2.Items.Add("TypeScript");
        listBox2.Items.Add("Go");
        listBox2.Items.Add("Rust");

        listBox2.SelectedIndexChanged += (s, e) =>
        {
            var count = listBox2.SelectedIndices.Count;
            lblMultiSelect.Text = $"Multi Selection ({count} selected):";
        };

        Controls.Add(listBox2);

        // Add item controls
        var lblAddItem = new Label
        {
            Text = "Add Item:",
            Left = 20,
            Top = 300,
            Width = 80,
            Height = 20
        };
        Controls.Add(lblAddItem);

        txtNewItem = new TextBox
        {
            Left = 100,
            Top = 298,
            Width = 120,
            Height = 23
        };
        Controls.Add(txtNewItem);

        btnAdd = new Button
        {
            Text = "Add",
            Left = 225,
            Top = 297,
            Width = 60,
            Height = 25
        };
        btnAdd.Click += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(txtNewItem.Text))
            {
                listBox1.Items.Add(txtNewItem.Text);
                txtNewItem.Text = "";
            }
        };
        Controls.Add(btnAdd);

        btnRemove = new Button
        {
            Text = "Remove",
            Left = 290,
            Top = 297,
            Width = 70,
            Height = 25
        };
        btnRemove.Click += (s, e) =>
        {
            if (listBox1.SelectedIndex >= 0)
            {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        };
        Controls.Add(btnRemove);

        btnClear = new Button
        {
            Text = "Clear All",
            Left = 365,
            Top = 297,
            Width = 75,
            Height = 25
        };
        btnClear.Click += (s, e) =>
        {
            listBox1.Items.Clear();
        };
        Controls.Add(btnClear);

        // Multi-select mode toggle
        chkMultiSelect = new CheckBox
        {
            Text = "Enable Multi-Select for List 1",
            Left = 20,
            Top = 335,
            Width = 250,
            Height = 20
        };
        chkMultiSelect.CheckedChanged += (s, e) =>
        {
            listBox1.SelectionMode = chkMultiSelect.Checked 
                ? SelectionMode.MultiExtended 
                : SelectionMode.One;
        };
        Controls.Add(chkMultiSelect);

        // Selected item label
        lblSelected = new Label
        {
            Text = "Selected: (none)",
            Left = 20,
            Top = 365,
            Width = 560,
            Height = 20,
            ForeColor = Color.FromArgb(0, 120, 215)
        };
        Controls.Add(lblSelected);

        // Instructions
        var lblInstructions = new Label
        {
            Text = "Instructions: Use arrow keys, Home, End, Page Up/Down. " +
                   "In multi-select mode: Ctrl+Click to toggle, Shift+Click for range.",
            Left = 20,
            Top = 390,
            Width = 560,
            Height = 40,
            ForeColor = Color.FromArgb(100, 100, 100)
        };
        Controls.Add(lblInstructions);
    }
}
