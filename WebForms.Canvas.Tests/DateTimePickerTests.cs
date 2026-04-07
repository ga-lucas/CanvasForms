using System;
using Xunit;

namespace Canvas.Windows.Forms.Tests;

public class DateTimePickerTests
{
    [Fact]
    public void Value_Set_RaisesValueChanged_And_TextChanged()
    {
        var dtp = new System.Windows.Forms.DateTimePicker();

        var valueChanged = 0;
        var textChanged = 0;

        dtp.ValueChanged += (_, _) => valueChanged++;
        dtp.TextChanged += (_, _) => textChanged++;

        dtp.Value = dtp.Value.AddDays(1);

        Assert.Equal(1, valueChanged);
        Assert.Equal(1, textChanged);
    }

    [Fact]
    public void MinDate_Clamps_Value()
    {
        var dtp = new System.Windows.Forms.DateTimePicker();
        dtp.Value = new DateTime(2020, 1, 10);

        dtp.MinDate = new DateTime(2020, 1, 15);

        Assert.Equal(new DateTime(2020, 1, 15), dtp.Value.Date);
    }

    [Fact]
    public void MaxDate_Clamps_Value()
    {
        var dtp = new System.Windows.Forms.DateTimePicker();
        dtp.Value = new DateTime(2020, 1, 20);

        dtp.MaxDate = new DateTime(2020, 1, 15);

        Assert.Equal(new DateTime(2020, 1, 15), dtp.Value.Date);
    }

    [Fact]
    public void ShowUpDown_Forces_DroppedDown_False()
    {
        var dtp = new System.Windows.Forms.DateTimePicker();
        dtp.DroppedDown = true;

        dtp.ShowUpDown = true;
        dtp.DroppedDown = true;

        Assert.False(dtp.DroppedDown);
    }

    [Fact]
    public void Checked_False_Makes_Text_Empty_When_ShowCheckBox()
    {
        var dtp = new System.Windows.Forms.DateTimePicker
        {
            ShowCheckBox = true,
            Checked = false
        };

        Assert.Equal(string.Empty, dtp.Text);
    }
}
