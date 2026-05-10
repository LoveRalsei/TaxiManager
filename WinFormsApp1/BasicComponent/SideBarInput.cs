namespace TaxiManager.BasicComponent;

public class SideBarInput : SideBarItem
{
    
    private TextBox _inputBox;
    private string _value;
    
    public SideBarInput(string? text, string defaultValue) : base(text)
    {
        _inputBox.Text = defaultValue;
    }
    
    public override void InitComponents()
    {
        base.InitComponents();

        // 创建输入框
        _inputBox = new TextBox
        {
            Width = 150,
            Height = 28,
            //Margin = new Padding(0, 8, 0, 8),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            BorderStyle = BorderStyle.FixedSingle
        };
        _inputBox.TextChanged += OnInputBoxTextChanged;

        AddControlComponent( _inputBox );
    }
    
    private void OnInputBoxTextChanged(object? sender, EventArgs e)
    {
        _value = _inputBox.Text;
    }

    public override object? GetValue()
    {
        return _value;
    }
    
    public override void SetValue(object? value)
    {
        _inputBox.Text = (string)value!;
    }
}