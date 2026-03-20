using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.Controls.Extensions;
using ElShrine.Wpf.Converters;
using ElShrine.Wpf.UITheme;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls;

[GenerateDPCli]
public partial class ENumInput : Control, IThemeControlBase, IHeaderControlBase
{
    #region DPs
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }
    public bool IsStepRepeatBtnVisible
    {
        get => (bool)GetValue(IsStepRepeatBtnVisibleProperty);
        set => SetValue(IsStepRepeatBtnVisibleProperty, value);
    }
    public double StepIncrement
    {
        get => (double)GetValue(StepIncrementProperty);
        set => SetValue(StepIncrementProperty, value);
    }
    public double StepDecrement
    {
        get => (double)GetValue(StepDecrementProperty);
        set => SetValue(StepDecrementProperty, value);
    }
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: 0.0,
            propertyChangedCallback: OnValueChanged, coerceValueCallback: (s, e) =>
        {
            if (e is not double d) return double.NaN;
            if (s is not ENumInput input) return d;
            var min_set = input.Minimum;
            var max_set = input.Maximum;
            var max = Math.Max(min_set, max_set);
            var min = Math.Min(max_set, min_set);
            return Math.Clamp(d, min, max);
        }));
    public static readonly DependencyProperty ValueFormatProperty = DependencyProperty.Register(
        nameof(ValueFormat),
        typeof(string),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: string.Empty));
    public static readonly DependencyProperty IsStepRepeatBtnVisibleProperty = DependencyProperty.Register(
        nameof(IsStepRepeatBtnVisible),
        typeof(bool),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: true));
    public static readonly DependencyProperty StepIncrementProperty = DependencyProperty.Register(
        nameof(StepIncrement),
        typeof(double),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: 1.0));
    public static readonly DependencyProperty StepDecrementProperty = DependencyProperty.Register(
        nameof(StepDecrement),
        typeof(double),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: 1.0));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: double.PositiveInfinity));
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: double.NegativeInfinity));

    #region private
    public bool CanIncrese
    {
        get => (bool) GetValue(CanIncreseProperty);
        protected set => SetCurrentValue(CanIncreseProperty, value);
    }
    protected static readonly DependencyProperty CanIncreseProperty = DependencyProperty.Register(
        nameof(CanIncrese),
        typeof(bool),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: true));
    public bool CanDecrease
    {
        get => (bool) GetValue(CanDecreaseProperty);
        protected set => SetCurrentValue(CanDecreaseProperty, value);
    }
    protected static readonly DependencyProperty CanDecreaseProperty = DependencyProperty.Register(
        nameof(CanDecrease),
        typeof(bool),
        typeof(ENumInput),
        new FrameworkPropertyMetadata(defaultValue: true));
    #endregion
    #endregion

    #region VMCs
    public static readonly VMCommand StepIncreaseCommand = new(parameter =>
    {
        if (parameter is not ENumInput input) return;
        input.SetCurrentValue(ValueProperty, input.Value + input.StepIncrement);
    }, parameter =>
    {
        if (parameter is not ENumInput input) return false;
        return input.Value < input.Maximum;
    });
    public static readonly VMCommand StepDecreaseCommand = new(parameter =>
    {
        if (parameter is not ENumInput input) return;
        input.SetCurrentValue(ValueProperty, input.Value - input.StepDecrement);
    }, parameter =>
    {
        if (parameter is not ENumInput input) return false;
        return input.Value > input.Minimum;
    });
    public static readonly VMCommand ConfrimInputCommand = new(parameter =>
    {
        if (parameter is not ENumInput input || input.PART_Input is null) return;
        var suc = false;
        try
        {
            var num = CommonConverter.ToNumber(input.PART_Input.Text);
            input.SetCurrentValue(ValueProperty, num);
            suc = true;
        }
        catch { }
        if (!suc) input.PART_Input.Text = input.Value.ToString(input.ValueFormat);
    });
    #endregion

    #region Implements
    static ENumInput() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ENumInput), new FrameworkPropertyMetadata(typeof(ENumInput)));
    public ENumInput() => WpfModuleAccessor.UITheme.RegisterCoerceThemeDPs(this);
    public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => TransHelper.CoerceValue(this);
    public void LocalThemePropertyChanged(DependencyPropertyChangedEventArgs e) => WpfModuleAccessor.StateListener.RedoSetterTransitions(this);
    #endregion

    #region Value relas
    public event ValueChangedHandler<double>? ValueChanged;
    #endregion

    private TextBox? PART_Input;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        PART_Input = GetTemplateChild(nameof(PART_Input)) as TextBox;
        if(PART_Input is not null)
        {
            PART_Input.LostMouseCapture += (s, e) => ConfrimInputCommand.InvokeSync(this);
            PART_Input.LostStylusCapture += (s, e) => ConfrimInputCommand.InvokeSync(this);
            PART_Input.LostTouchCapture += (s, e) => ConfrimInputCommand.InvokeSync(this);
        }
    }
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ENumInput input || input.PART_Input is null) return;
        try
        {
            var valueText = input.Value.ToString(input.ValueFormat);
            if (input.PART_Input.Text != valueText) input.PART_Input.SetCurrentValue(TextBox.TextProperty, valueText);
            input.ValueChanged?.Invoke(input, new ValueChangedEventArgs<double>(CommonConverter.ToNumber(e.OldValue), CommonConverter.ToNumber(e.NewValue)));
        }
        catch
        {
            return;
        }
        bool canIncrease = input.Value < input.Maximum, canDecrease = input.Value > input.Minimum;
        if (canIncrease ^ input.CanIncrese) input.CanIncrese = canIncrease;
        if (canDecrease ^ input.CanDecrease) input.CanDecrease = canDecrease;
    }
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        ConfrimInputCommand.InvokeSync(this);
    }
    protected override void OnLostTouchCapture(TouchEventArgs e)
    {
        base.OnLostTouchCapture(e);
        ConfrimInputCommand.InvokeSync(this);
    }
    protected override void OnLostStylusCapture(StylusEventArgs e)
    {
        base.OnLostStylusCapture(e);
        ConfrimInputCommand.InvokeSync(this);
    }
}
