using ElShrine.EGraphic;
using ElShrine.Wpf.UITheme;
using ElShrine.Wpf.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VMC = ElShrine.Wpf.VMCommand;

namespace ElShrine.Wpf.Controls
{
    public partial class EColorPalette : UserControl, IThemeControl
    {
        public ColorData ResultColor
        {
            get => (ColorData)GetValue(ResultColorProperty);
            set => SetValue(ResultColorProperty, value);
        }
        public static readonly DependencyProperty ResultColorProperty = DependencyProperty.Register(nameof(ResultColor), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public bool ConstantUpdateResultColor
        {
            get => (bool)GetValue(ConstantUpdateResultColorProperty);
            set => SetValue(ConstantUpdateResultColorProperty, value);
        }
        public static readonly DependencyProperty ConstantUpdateResultColorProperty = DependencyProperty.Register(nameof(ConstantUpdateResultColor), typeof(bool), typeof(EColorPalette), new(false));

        public EColorPalette()
        {
            InitializeComponent();
            alphaSlider.ValueChanged += OnSliderChanged;
            redSlider.ValueChanged += OnSliderChanged;
            greenSlider.ValueChanged += OnSliderChanged;
            blueSlider.ValueChanged += OnSliderChanged;
            hSlider.ValueChanged += OnSliderChanged;
            sSlider.ValueChanged += OnSliderChanged;
            vSlider.ValueChanged += OnSliderChanged;
            hslHSlider.ValueChanged += OnSliderChanged;
            hslSSlider.ValueChanged += OnSliderChanged;
            hslLSlider.ValueChanged += OnSliderChanged;
            confrimBtn.Command = ConfrimCommand;
            cancelBtn.Command = CancelCommand;
            Loaded += (_, _) =>
            {
                var colorData = new ColorData();
                MainColorData = colorData;
                UpdateSliders(colorData);
            };
        }

        #region UI

        #region DPs

        #region Theme
        public CornerRadius BorderCornerRadius
        {
            get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
            set => SetValue(BorderCornerRadiusProperty, value);
        }
        public Brush FontBrush
        {
            get => (Brush)GetValue(FontBrushProperty);
            set => SetValue(FontBrushProperty, value);
        }
        public Brush SelectionBrush
        {
            get => (Brush)GetValue(SelectionBrushProperty);
            set => SetValue(SelectionBrushProperty, value);
        }
        public Brush ClickBrush
        {
            get => (Brush)GetValue(ClickBrushProperty);
            set => SetValue(ClickBrushProperty, value);
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(EColorPalette), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush), typeof(Brush), typeof(EColorPalette), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(nameof(SelectionBrush), typeof(Brush), typeof(EColorPalette), new(Theme.Default.SelectionColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush), typeof(Brush), typeof(EColorPalette), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        #endregion

        #endregion

        #region HistroyColor
        public bool HistorColorsShown
        {
            get => (bool)GetValue(HistorColorsShownProperty);
            set => SetValue(HistorColorsShownProperty, value);
        }
        public int HistoryColorsMaxCount
        {
            get => (int)GetValue(HistoryColorsMaxCountProperty); 
            set => SetValue(HistoryColorsMaxCountProperty, value); 
        }
        public bool AddToHistoryColors
        {
            get => (bool) GetValue(AddToHistoryColorsProperty);
            set => SetValue(AddToHistoryColorsProperty, value);
        }
       
        public static readonly DependencyProperty HistorColorsShownProperty =
            DependencyProperty.Register(nameof(HistorColorsShown), typeof(bool), typeof(EColorPalette), new(true));
        public static readonly DependencyProperty HistoryColorsMaxCountProperty =
            DependencyProperty.Register(nameof(HistoryColorsMaxCount), typeof(int), typeof(EColorPalette), new(20));
        public static readonly DependencyProperty AddToHistoryColorsProperty =
            DependencyProperty.Register(nameof(AddToHistoryColors), typeof(bool), typeof(EColorPalette), new(true));

        public static ObservableCollection<HistroyColor> HistoryColors { get; protected set; } = [];

        public class HistroyColor(ColorData model) : ViewModelBase<ColorData>(model)
        {
            private bool selected = false;
            public bool Selected
            {
                get => selected;
                set
                {
                    selected = value;
                    NoticePropertyChanged(nameof(Selected));
                }
            }

            private bool marked = false;
            public bool Marked
            {
                get => marked;
                set
                {
                    marked = value;
                    NoticePropertyChanged(nameof(Marked));
                }
            }
        }

        public void AddOrSelectHistoryColor(ColorData colorData, bool addCheck, bool reorder)
        {
            var index = HistoryColors.ToList().FindIndex(d => d.Model.DataEqual(colorData));
            var addChecked = !addCheck || (HistorColorsShown && AddToHistoryColors);
            if (addCheck) index = AddHistoryColor(colorData, reorder, addChecked); 
            UpdateHistoryColorsSelection(colorData);
        }
        protected static void UpdateHistoryColorsSelection(ColorData colorData)
        {
            for (int i = 0; i < HistoryColors.Count; i++)
            {
                var historyColor = HistoryColors[i];
                historyColor.Selected = colorData.DataEqual(historyColor.Model);
            }
        } 
        public static int AddHistoryColor(ColorData colorData, bool reorder, bool confrimAdd)
        {
            var index = HistoryColors.ToList().FindIndex(d => d.Model.DataEqual(colorData));
            if (index == -1)
            {
                //add
                if (confrimAdd)
                {
                    HistoryColors.Insert(0, new(colorData));
                    index = 0;
                }
            }
            else
            {
                //select or reorder
                if (reorder)
                {
                    HistoryColors.RemoveAt(index);
                    HistoryColors.Insert(0, new(colorData));
                    index = 0;
                }
            }
            return index;
        }
        public static void RemoveHistoryColor(ColorData colorData)
        {
            var index = HistoryColors.ToList().FindIndex(d => d.Model.DataEqual(colorData));
            if (index != -1) HistoryColors.RemoveAt(index);
        }
        public static void MarkHistoryColor(ColorData colorData, bool? marked)
        {
            var index = HistoryColors.ToList().FindIndex(d => d.Model.DataEqual(colorData));
            if (index == -1) HistoryColors[AddHistoryColor(colorData, true, true)].Marked = marked ?? true;
            else HistoryColors[index].Marked = marked ?? !HistoryColors[index].Marked;
        }
        public static void ClearHistoryColors() => HistoryColors.Clear();

        public static VMC AddHistoryColorCommand => new(parameter =>
        {
            ColorData? colorData = null;
            if (parameter is int i) colorData = i.ToColorData();
            else if (parameter is ColorData cd) colorData = cd.Clone();
            if (colorData is not null) AddHistoryColor(colorData, false, true);
        });
        public static VMC RemoveHistoryColorCommand => new(parameter =>
        {
            ColorData? colorData = null;
            if (parameter is int i) colorData = i.ToColorData();
            else if (parameter is ColorData cd) colorData = cd.Clone();
            if (colorData is not null) RemoveHistoryColor(colorData);
        });
        public static VMC ClearHistoryColorsCommand => new(parameter => ClearHistoryColors());
        public static VMC MarkHistoryColorCommand => new(parameter =>
        {
            ColorData? colorData = null;
            if (parameter is int i) colorData = i.ToColorData();
            else if (parameter is ColorData cd) colorData = cd.Clone();
            if (colorData is not null) MarkHistoryColor(colorData, null);
        });
        public void ConfrimResultColor()
        {
            OnResultColorUpdated?.Invoke(CachedColorData.Clone(), MainColorData.Clone(), ControlDataUpdates.Confrim);
            CachedColorData = MainColorData.Clone();
            ResultColor = MainColorData.Clone();
            AddOrSelectHistoryColor(MainColorData.Clone(), false, true);
        }
        public void CancelResultColor()
        {
            OnResultColorUpdated?.Invoke(MainColorData.Clone(), CachedColorData.Clone(), ControlDataUpdates.Cancel);
            MainColorData = CachedColorData.Clone();
        }

        public delegate void ResultColorUpdatedEventHandler(ColorData oldData, ColorData newData, ControlDataUpdates updates);
        public event ResultColorUpdatedEventHandler? OnResultColorUpdated;
        #endregion

        #region HSV/HSL
        public ColorData MaxHsvSColorData
        {
            get => (ColorData)GetValue(MaxHsvSColorDataProperty);
            protected set => SetValue(MaxHsvSColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MaxHsvSColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MaxHsvSColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MaxHsvSColorDataProperty = MaxHsvSColorDataPropertyKey.DependencyProperty;

        public ColorData MinHsvSColorData
        {
            get => (ColorData)GetValue(MinHsvSColorDataProperty);
            protected set => SetValue(MinHsvSColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MinHsvSColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MinHsvSColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MinHsvSColorDataProperty = MinHsvSColorDataPropertyKey.DependencyProperty;

        public ColorData MaxVColorData
        {
            get => (ColorData)GetValue(MaxVColorDataProperty);
            protected set => SetValue(MaxVColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MaxVColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MaxVColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MaxVColorDataProperty = MaxVColorDataPropertyKey.DependencyProperty;

        public ColorData MinVColorData
        {
            get => (ColorData)GetValue(MinVColorDataProperty);
            protected set => SetValue(MinVColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MinVColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MinVColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MinVColorDataProperty = MinVColorDataPropertyKey.DependencyProperty;

        public ColorData MaxHslSColorData
        {
            get => (ColorData)GetValue(MaxHslSColorDataProperty);
            protected set => SetValue(MaxHslSColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MaxHslSColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MaxHslSColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MaxHslSColorDataProperty = MaxHslSColorDataPropertyKey.DependencyProperty;

        public ColorData MinHslSColorData
        {
            get => (ColorData)GetValue(MinHslSColorDataProperty);
            protected set => SetValue(MinHslSColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MinHslSColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MinHslSColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MinHslSColorDataProperty = MinHslSColorDataPropertyKey.DependencyProperty;

        public ColorData CentreLColorData
        {
            get => (ColorData)GetValue(CentreLColorDataProperty);
            protected set => SetValue(CentreLColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey CentreLColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CentreLColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty CentreLColorDataProperty = CentreLColorDataPropertyKey.DependencyProperty;

        public ColorData MaxAlphaColorData
        {
            get => (ColorData)GetValue(MaxAlphaColorDataProperty);
            protected set => SetValue(MaxAlphaColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey MaxAlphaColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MaxAlphaColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty MaxAlphaColorDataProperty = MaxAlphaColorDataPropertyKey.DependencyProperty;

        #endregion

        #region Back Colors
        public ColorData GrayColorData
        {
            get => (ColorData)GetValue(GrayColorDataProperty);
            protected set => SetValue(GrayColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey GrayColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(GrayColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty GrayColorDataProperty = GrayColorDataPropertyKey.DependencyProperty;

        public ColorData OppositeColorData
        {
            get => (ColorData)GetValue(OppositeColorDataProperty);
            protected set => SetValue(OppositeColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey OppositeColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(OppositeColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty OppositeColorDataProperty = OppositeColorDataPropertyKey.DependencyProperty;

        public ColorData CachedColorData
        {
            get => (ColorData)GetValue(CachedColorDataProperty);
            protected set => SetValue(CachedColorDataPropertyKey, value);
        }
        private static readonly DependencyPropertyKey CachedColorDataPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CachedColorData), typeof(ColorData), typeof(EColorPalette), new(new ColorData()));
        public static readonly DependencyProperty CachedColorDataProperty = CachedColorDataPropertyKey.DependencyProperty;
        #endregion

        #region Palette - color modify

        public ColorData MainColorData
        {
            get => (ColorData)GetValue(MainColorDataProperty);
            set => SetValue(MainColorDataProperty, value);
        }
        public static readonly DependencyProperty MainColorDataProperty =
            DependencyProperty.Register(nameof(MainColorData), typeof(ColorData), typeof(EColorPalette), new FrameworkPropertyMetadata(new ColorData(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMainColorChanged));

        public string MainColorHex8
        {
            get => (string)GetValue(MainColorHex8Property);
            set => SetValue(MainColorHex8Property, value);
        }
        public static readonly DependencyProperty MainColorHex8Property =
            DependencyProperty.Register(nameof(MainColorHex8), typeof(string), typeof(EColorPalette), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHexChanged));

        protected bool Updating = false;
        private static void OnMainColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cp = (EColorPalette)d;
            if (cp.Updating) return;
            cp.Updating = true;
            cp.OnMainColorChanged(((ColorData)e.NewValue).Clone(), ((ColorData)e.OldValue).Clone());
            cp.Updating = false;
        }
        private static void OnHexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cp = (EColorPalette)d;
            if (cp.Updating) return;
            cp.Updating = true;
            var oldHex8 = (string)e.OldValue;
            var newHex8 = (string)e.NewValue;
            if (!oldHex8.EqualIgnoreCase(newHex8))
            {
                var colorData = new ColorData
                {
                    Hex8 = newHex8
                };
                cp.OnMainColorChanged(colorData, cp.MainColorData.Clone());
            }
            cp.Updating = false;
        }
        protected virtual void OnMainColorChanged(ColorData newColor, ColorData oldColor)
        {
            MainColorHex8 = newColor.Hex8;
            UpdateSliders(newColor, paletteSpaceTabControl.SelectedIndex);
            UpdateBackColors(newColor);
            UpdateHistoryColorsSelection(newColor);
            if (ConstantUpdateResultColor)
            {
                ResultColor = newColor.Clone();
                OnResultColorUpdated?.Invoke(oldColor.Clone(), newColor.Clone(), ControlDataUpdates.Update);
            }
            Updating = false;
        }
        protected virtual void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Updating) return;
            if (sender == alphaSlider) MainColorData = ((byte)e.NewValue, MainColorData.V1, MainColorData.V2, MainColorData.V3).ToColorData(MainColorData.Space);
            else
            {
                var newColor  = paletteSpaceTabControl.SelectedIndex switch
                {
                    1 => (MainColorData.A, (byte)hSlider.Value, (byte)sSlider.Value, (byte)vSlider.Value).ToColorData(ColorSpace.AHSV),
                    2 => (MainColorData.A, (byte)hslHSlider.Value, (byte)hslSSlider.Value, (byte)hslLSlider.Value).ToColorData(ColorSpace.AHSL),
                    _ => (MainColorData.A, (byte)redSlider.Value, (byte)greenSlider.Value, (byte)blueSlider.Value).ToColorData(ColorSpace.ARGB),
                };
                MainColorData = newColor;
            }
        }
        protected void UpdateBackColors(ColorData newColorData)
        {
            var maxAlphaData = (int)(newColorData.Data | 0xFF000000);
            var maxAlphaColorData = new ColorData(maxAlphaData, newColorData.Space);
            var hsvData = maxAlphaColorData.ToAHSV();
            MaxHsvSColorData = (hsvData.Data | 0x0000FF00).ToColorData(ColorSpace.AHSV);
            MinHsvSColorData = ((int)(hsvData.Data & 0xFFFF00FF)).ToColorData(ColorSpace.AHSV);
            MaxVColorData = (hsvData.Data | 0x000000FF).ToColorData(ColorSpace.AHSV);
            MinVColorData = ((int)(hsvData.Data & 0xFFFFFF00)).ToColorData(ColorSpace.AHSV);
            //
            var hslData = maxAlphaColorData.ToAHSL();
            MaxHslSColorData = (hslData.Data | 0x0000FF00).ToColorData(ColorSpace.AHSL);
            MinHslSColorData = ((int)(hslData.Data & 0xFFFF00FF)).ToColorData(ColorSpace.AHSL);
            CentreLColorData = ((int)((hslData.Data & 0xFFFFFF00) | 0x0000007F)).ToColorData(ColorSpace.AHSL);
            //
            MaxAlphaColorData = maxAlphaColorData;
            GrayColorData = maxAlphaColorData.ToGray();
            OppositeColorData = maxAlphaColorData.ToOpposite();
        }
        protected void UpdateSliders(ColorData colorData, int targetPannelIndex = -1)
        {
            switch (targetPannelIndex)
            {
                case 0:
                    updateRgbSliders(colorData);
                    
                    break;
                case 1:
                    updateHsvSliders(colorData);
                    break;
                case 2:
                    updateHslSliders(colorData);
                    break;
                default:
                    updateRgbSliders(colorData);
                    updateHsvSliders(colorData);
                    updateHslSliders(colorData);
                    break;
            }
            alphaSlider.Value = colorData.A;
            void updateRgbSliders(ColorData colorData)
            {
                colorData = colorData.ToARGB();
                redSlider.Value = colorData.V1;
                greenSlider.Value = colorData.V2;
                blueSlider.Value = colorData.V3;
            }
            void updateHsvSliders(ColorData colorData)
            {
                colorData = colorData.ToAHSV();
                hSlider.Value = colorData.V1;
                sSlider.Value = colorData.V2;
                vSlider.Value = colorData.V3;
            }
            void updateHslSliders(ColorData colorData)
            {
                colorData = colorData.ToAHSL();
                hslHSlider.Value = colorData.V1;
                hslSSlider.Value = colorData.V2;
                hslLSlider.Value = colorData.V3;
            }
        }

        public void SetInitialColor(ColorData colorData)
        {
            MainColorData = colorData.Clone();
            ResultColor = colorData.Clone();
            CachedColorData = colorData.Clone();
        }
        #endregion

        public bool ConfrimButtonShown
        {
            get => (bool)GetValue(ConfrimButtonShownProperty);
            set => SetValue(ConfrimButtonShownProperty, value);
        }
        public static readonly DependencyProperty ConfrimButtonShownProperty =
            DependencyProperty.Register(nameof(ConfrimButtonShown), typeof(bool), typeof(EColorPalette), new(true));

        private void ColorCarrierBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is Border bd && bd.Background is SolidColorBrush scb)
            {
                var colorData = scb.Color.ToColorData();
                if (e.LeftButton == MouseButtonState.Pressed) AddOrSelectHistoryColor(colorData, true, false);
                else if(e.RightButton == MouseButtonState.Pressed) MainColorData = colorData;
            }
        }
        private void HistoryCarrierBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bd && bd.Background is SolidColorBrush scb)
            {
                var colorData = scb.Color.ToColorData();
                if (e.LeftButton == MouseButtonState.Pressed) MainColorData = colorData;
                else if (e.RightButton == MouseButtonState.Pressed) RemoveHistoryColor(colorData);
                else if (e.MiddleButton == MouseButtonState.Pressed) MarkHistoryColor(colorData, null);
            }
        }
        private VMC ConfrimCommand => new(o => ConfrimResultColor());
        private VMC CancelCommand => new(o => CancelResultColor());

        private void PaletteSpaceTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl tbc) UpdateSliders(MainColorData, tbc.SelectedIndex);
        }
    }
}
