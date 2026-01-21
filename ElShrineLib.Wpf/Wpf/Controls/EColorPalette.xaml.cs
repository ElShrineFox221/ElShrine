using ElShrine.Common;
using ElShrine.Graphics;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Wpf.Controls
{
    public sealed class HistoryColor(uint colorData, bool favorite) : ViewModelBase
    {
        public ColorData Color => Graphics.ColorData.FromData(ColorData);
        public uint ColorData
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    NotifyPropertyChanged(nameof(ColorData), nameof(Color));
                }
            }
        } = colorData;
        public bool Favorite
        {
            get => field;
            set
            {
                if (field != value)
                {
                    field = value;
                    NotifyPropertyChanged(nameof(Favorite));
                }
            }
        } = favorite;
    }
    [GenerateDPCli]
    public partial class EColorPalette : Control, IThemeControlBase, INotifyPropertyChanged
    {
        #region Implements
        static EColorPalette() => DefaultStyleKeyProperty.OverrideMetadata(typeof(EColorPalette), new FrameworkPropertyMetadata(typeof(EColorPalette)));
        public EColorPalette()
        {
            PropertyChanged += OnPropertyChanged;
            Loaded += (s, e) =>
            {
                RefreshCachedColors();
                NotifyPropertyChanged(nameof(Hex),
                   nameof(MaxAlphaColor), nameof(GrayColor),
                   nameof(MaxHSVSaturationColor), nameof(MaxHSLSaturationColor), nameof(MinSaturationColor),
                   nameof(MaxHSVValueColor), nameof(CentreHSLLightnessColor));
            };
            UIThemesManager.RegisterCoerceThemeDPs(this);
        }
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }
        public void NotifyPropertyChanged(params IEnumerable<string> propNames)
        {
            foreach (var name in propNames)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion


        #region DPs
        public readonly static DependencyProperty ResultColorProperty = DependencyProperty.Register(nameof(ResultColor), typeof(ColorData), typeof(EColorPalette), new(defaultValue: ColorDataExtensions.GetAccentColor(), propertyChangedCallback: ResultColorChanged));
        public readonly static DependencyProperty CurrentColorProperty = DependencyProperty.Register(nameof(CurrentColor), typeof(ColorData), typeof(EColorPalette), new(defaultValue: ColorDataExtensions.GetAccentColor(), propertyChangedCallback: CurrentColorChanged));
        public readonly static DependencyProperty IsPreviewModeProperty = DependencyProperty.Register(nameof(IsPreviewMode), typeof(bool), typeof(EColorPalette), new(defaultValue: true, propertyChangedCallback: IsPreviewModeChanged));
        public readonly static DependencyProperty IsHistoryColorsVisibleProperty = DependencyProperty.Register(nameof(IsHistoryColorsVisible), typeof(bool), typeof(EColorPalette), new(defaultValue: true));
        public readonly static DependencyProperty HistoryColorsLimitProperty = DependencyProperty.Register(nameof(HistoryColorsLimit), typeof(int), typeof(EColorPalette), new(defaultValue: 20));
        public ColorData ResultColor
        {
            get => (ColorData)GetValue(ResultColorProperty);
            protected set => SetValue(ResultColorProperty, value);
        }
        public ColorData CurrentColor
        {
            get => (ColorData)GetValue(CurrentColorProperty);
            protected set
            {
                if (CurrentColor.ToSpace(value.Space).Data != value.Data) SetValue(CurrentColorProperty, value.Clone());
            }
        }
        public bool IsPreviewMode
        {
            get => (bool)GetValue(IsPreviewModeProperty);
            set => SetValue(IsPreviewModeProperty, value);
        }
        public bool IsHistoryColorsVisible
        {
            get => (bool)GetValue(IsHistoryColorsVisibleProperty);
            set => SetValue(IsHistoryColorsVisibleProperty, value);
        }
        public int HistoryColorsLimit
        {
            get => (int)GetValue(HistoryColorsLimitProperty);
            set => SetValue(HistoryColorsLimitProperty, value);
        }
        private static void ResultColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }
        private static void CurrentColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EColorPalette palette)
            {
                var newColorClone = ((ColorData)e.NewValue).ToARGB();
                if (palette.IsPreviewMode && palette.ResultColor.ToARGB().Data != newColorClone.Data) palette.ResultColor = newColorClone;
                palette.RefreshCachedColors();
                palette.NotifyPropertyChanged(nameof(Hex), 
                    nameof(MaxAlphaColor), nameof(GrayColor),
                    nameof(MaxHSVSaturationColor), nameof(MaxHSLSaturationColor), nameof(MinSaturationColor),
                    nameof(MaxHSVValueColor), nameof(CentreHSLLightnessColor));
                
            }
        }
        private static void IsPreviewModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EColorPalette palette)
            {
                if(e.NewValue is bool nb && e.OldValue is bool ob && nb ^ ob)
                {
                    palette.ResultColor = nb ? palette.CurrentColor : palette.InitialColor;
                }
            }
        }
        #endregion

        #region Properties
        private ColorSpace currentSpace = ColorSpace.ARGB;
        private ColorData GetTargetData() => currentSpace switch
        {
            ColorSpace.ARGB => ARGBColor,
            ColorSpace.AHSV => AHSVColor,
            ColorSpace.AHSL => AHSLColor,
            _ => ARGBColor,
        };
        private const double angleFactor = 360d / 255d;
        public double A
        {
            get => CurrentColor.A;
            set
            {
                var tar = GetTargetData();
                tar.A = (byte)Math.Clamp(Math.Round(value), 0, 255);
                CurrentColor = tar;
            }
        }
        public double V1
        {
            get => Math.Round(GetTargetData().V1 * (currentSpace == ColorSpace.ARGB ? 1 : angleFactor));
            set
            {
                var tar = GetTargetData();
                tar.V1 = (byte)Math.Clamp(Math.Round(value / (currentSpace == ColorSpace.ARGB ? 1 : angleFactor)), 0, 255);
                CurrentColor = tar;
                var name = currentSpace switch
                {
                    ColorSpace.AHSV => nameof(MaxHSVSaturationColor),
                    ColorSpace.AHSL => nameof(MaxHSLSaturationColor),
                    _ => string.Empty,
                };
                if(name != string.Empty) NotifyPropertyChanged(name);
            }
        }
        public double V2
        {
            get => GetTargetData().V2;
            set
            {
                var tar = GetTargetData();
                tar.V2 = (byte)Math.Clamp(Math.Round(value), 0, 255);
                CurrentColor = tar;
            }
        }
        public double V3
        {
            get => GetTargetData().V3;
            set
            {
                var tar = GetTargetData();
                tar.V3 = (byte)Math.Clamp(Math.Round(value), 0, 255);
                CurrentColor = tar;
            }
        }
        public string Hex
        {
            get => CurrentColor.ToARGB().Hex8;
            set
            {
                if (value.StartsWith('#')) value = value[1..];
                if (value.Length != 8) value = value.PadLeft(8, 'F')[0..8];
                var clone = CurrentColor.ToARGB();
                if(clone.Hex8 != value)
                {
                    clone.Hex8 = value;
                    CurrentColor = clone;
                }
            }
        }

        #region Cached colors
        public ColorData ARGBColor { get; set; } = ColorData.FromData();
        public ColorData AHSVColor { get; set; } = ColorData.FromData();
        public ColorData AHSLColor { get; set; } = ColorData.FromData();
        private void RefreshCachedColors()
        {
            var argbClone = CurrentColor.ToARGB();
            if (ARGBColor.ToARGB().Data != argbClone.Data) ARGBColor = argbClone;
            if (AHSVColor.ToARGB().Data != argbClone.Data) AHSVColor = CurrentColor.ToAHSV();
            if (AHSLColor.ToARGB().Data != argbClone.Data) AHSLColor = CurrentColor.ToAHSL();
            NotifyPropertyChanged(nameof(A), nameof(V1), nameof(V2), nameof(V3));
        }
        #endregion
        private void SpaceChanged(object sender, SelectionChangedEventArgs e)
        {
            var newSpaceItemIndex = e.AddedItems.Count > 0 ? PART_SlidersTabControl?.Items?.IndexOf(e.AddedItems[0]) ?? -1 : -1;
            currentSpace = newSpaceItemIndex switch
            {
                0 => ColorSpace.ARGB,
                1 => ColorSpace.AHSV,
                2 => ColorSpace.AHSL,
                _ => ColorSpace.ARGB
            };
            RefreshCachedColors();
        }
        #endregion

        #region Calucated properties
        public ColorData MaxAlphaColor => CurrentColor.WithAlpha(255);
        public ColorData GrayColor => MaxAlphaColor.ToGrayColor();
        public ColorData MaxHSVSaturationColor => AHSVColor.WithAlpha(255).WithHsvSaturation(1);
        public ColorData MaxHSLSaturationColor => AHSLColor.WithAlpha(255).WithHslSaturation(1);
        public ColorData MinSaturationColor => MaxAlphaColor.WithHsvSaturation(0);
        public ColorData MaxHSVValueColor => AHSVColor.WithAlpha(255).WithHsvValue(1);
        public ColorData CentreHSLLightnessColor => AHSLColor.WithAlpha(255).WithHslLightness(0.5);
        #endregion

        private ETabControl? PART_SlidersTabControl;
        private Border? PART_ResultColorPreview;
        private Border? PART_OriginalColorPreview;
        private Border? PART_GreyColorPreview;
        private ListBox? PART_HistoryColorsBox;
        private Button? PART_ConfrimBtn;
        private Button? PART_CancelBtn;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_SlidersTabControl = GetTemplateChild(nameof(PART_SlidersTabControl)) as ETabControl;
            PART_ResultColorPreview = GetTemplateChild(nameof(PART_ResultColorPreview)) as Border;
            PART_OriginalColorPreview = GetTemplateChild(nameof(PART_OriginalColorPreview)) as Border;
            PART_GreyColorPreview = GetTemplateChild(nameof(PART_GreyColorPreview)) as Border;
            PART_HistoryColorsBox = GetTemplateChild(nameof(PART_HistoryColorsBox)) as ListBox;
            PART_ConfrimBtn = GetTemplateChild(nameof(PART_ConfrimBtn)) as Button;
            PART_CancelBtn = GetTemplateChild(nameof(PART_CancelBtn)) as Button;
            //
            if (PART_SlidersTabControl is not null) PART_SlidersTabControl.SelectionChanged += SpaceChanged;
            if (PART_ResultColorPreview is not null) PART_ResultColorPreview.MouseDown += (s, e) =>
            {
                ResultColor = CurrentColor;
                AddHistoryColor(ResultColor);
            };
            if (PART_OriginalColorPreview is not null) PART_OriginalColorPreview.MouseDown += (s, e) => CurrentColor = InitialColor;
            if (PART_GreyColorPreview is not null) PART_GreyColorPreview.MouseDown += (s, e) => CurrentColor = GrayColor;
            if (PART_HistoryColorsBox is not null)
            {
                PART_HistoryColorsBox.SelectionChanged += (s, e) =>
                {
                    if (e.AddedItems.Count > 0 && e.AddedItems[0] is HistoryColor hc) CurrentColor = hc.Color;
                };
            }
            if (PART_ConfrimBtn is not null) PART_ConfrimBtn.Click += (s, e) => Confrim();
            if (PART_CancelBtn is not null) PART_CancelBtn.Click += (s, e) => CancelClose();
        }

        #region History colors
        
        public static ObservableCollection<HistoryColor> HistoryColors { get; } = [];
        public static void AddHistoryColor(ColorData color, bool? favorite = null)
        {
            var colorData = color.ToARGB();
            var hc = HistoryColors.ToList().Find(hc => hc.ColorData == colorData.Data);
            hc ??= new HistoryColor(colorData.Data, favorite ?? false);
            if (favorite is false) hc.Favorite = false;
            if (favorite is true) hc.Favorite = true;
            HistoryColors.Remove(hc);
            HistoryColors.Add(hc);
        }
        public static void RemoveHistoryColor(ColorData color)
        {
            var colorData = color.ToARGB();
            var hc = new HistoryColor(colorData.Data, false);
            HistoryColors.Remove(hc);
        }
        public static void ClearHistoryColors()
        {
            HistoryColors.Clear();
        }
        #endregion

        #region Actions
        public ColorData InitialColor { get; private set; } = ColorDataExtensions.GetAccentColor();
        public void Open(ColorData? initialColor = null)
        {
            initialColor ??= ResultColor ?? CurrentColor ?? ColorDataExtensions.GetAccentColor();
            CurrentColor = initialColor;
            InitialColor = initialColor;
            NotifyPropertyChanged(nameof(InitialColor));
            //Selector
        }
        public void Confrim()
        {
            ResultColor = CurrentColor;
            AddHistoryColor(CurrentColor);
        }
        public void CancelClose()
        {
            return;
        }
        #endregion

    }
}
