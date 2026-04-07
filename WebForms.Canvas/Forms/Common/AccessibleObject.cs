namespace System.Windows.Forms;

public class AccessibleObject
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? DefaultAction { get; set; }
    public AccessibleRole Role { get; set; } = AccessibleRole.Default;
    public object? Value { get; set; }
}
