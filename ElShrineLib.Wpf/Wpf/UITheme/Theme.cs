using ElShrine.Graphics;
using System.Runtime.Serialization;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.UITheme
{
    [DataContract]
    public class Theme(string themeName) : ViewModelBase
    {
        [DataMember] public string ThemeName { get; init; } = themeName;

        #region colors
        [DataMember] public ColorData PrimaryColor
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(PrimaryColor));
            }
        } = Color.LightSlateGray.ToColorData();


        [DataMember] public ColorData BackColor
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(BackColor));
            }
        } = Color.White.ToColorData();


        [DataMember] public ColorData SecondaryColor
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(SecondaryColor));
            }
        } = Color.Purple.ToColorData();

        [DataMember] public ColorData FontColor
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(FontColor));
            }
        } = Color.Black.ToColorData();
        #endregion
        
        #region font
        [DataMember] public int FontSizeLarge
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(FontSizeLarge));
            }
        } = 24;
        [DataMember] public int FontSizeMedium
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(FontSizeMedium));
            }
        } = 16;
        [DataMember] public int FontSizeNormal
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(FontSizeNormal));
            }
        } = 12;
        [DataMember] public int FontSizeSmall
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(FontSizeSmall));
            }
        } = 10;
        #endregion
        
        #region animations
        public const double ANIMA_DEFAULTDURA = 0.2d;
        [DataMember] public double AnimDurationIn
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(AnimDurationIn));
            }
        } = ANIMA_DEFAULTDURA;
        [DataMember] public double AnimDurationOut
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged(nameof(AnimDurationOut));
            }
        } = ANIMA_DEFAULTDURA;
        #endregion


        public readonly static Theme Default = new(nameof(Default));
    }
}
