using ElShrine.EGraphic;
using ElShrine.Wpf;
using System.Runtime.Serialization;
using System.Windows;
using Color = System.Drawing.Color;

namespace ElShrine.Wpf.UITheme
{
    [DataContract]
    public class Theme : ViewModelBase
    {
        [DataMember] private string themeName = string.Empty;
        [IgnoreDataMember] public string ThemeName
        {
            get => themeName;
            set 
            {
                themeName = value;
                NoticePropertyChanged(nameof(ThemeName));
            }
        }

        #region old
        [DataMember] private ColorData foreColor = Color.LightSlateGray.ToColorData();
        [IgnoreDataMember]
        public ColorData ForeColor
        {
            get => foreColor;
            set
            {
                foreColor = value;
                NoticePropertyChanged(nameof(ForeColor));
            }
        }
        [DataMember] private ColorData borderColor = Color.LightSlateGray.ToColorData();
        [IgnoreDataMember]
        public ColorData BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                NoticePropertyChanged(nameof(BorderColor));
            }
        }
        [DataMember] private ColorData selectionColor = Color.Purple.ToColorData();
        [IgnoreDataMember]
        public ColorData SelectionColor
        {
            get => selectionColor;
            set
            {
                selectionColor = value;
                NoticePropertyChanged(nameof(SelectionColor));
            }
        }
        [DataMember] private ColorData clickColor = Color.DodgerBlue.Lerp(Color.White, 0.5, false).ToColorData();
        [IgnoreDataMember]
        public ColorData ClickColor
        {
            get => clickColor;
            set
            {
                clickColor = value;
                NoticePropertyChanged(nameof(ClickColor));
            }
        }

        [DataMember] private CornerRadius cornerRadius = new(2);
        [IgnoreDataMember]
        public CornerRadius CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = value;
                NoticePropertyChanged(nameof(CornerRadius));
            }
        }

        #endregion


        #region colors

        [DataMember] private ColorData primaryColor = Color.LightSlateGray.ToColorData();
        [IgnoreDataMember]
        public ColorData PrimaryColor
        {
            get => primaryColor;
            set
            {
                primaryColor = value;
                NoticePropertyChanged(nameof(PrimaryColor));
            }
        }


        [DataMember] private ColorData backColor = Color.White.ToColorData();
        [IgnoreDataMember]
        public ColorData BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                NoticePropertyChanged(nameof(BackColor));
            }
        }


        [DataMember] private ColorData secondaryColor = Color.Purple.ToColorData();
        [IgnoreDataMember]
        public ColorData SecondaryColor
        {
            get => secondaryColor;
            set
            {
                secondaryColor = value;
                NoticePropertyChanged(nameof(SecondaryColor));
            }
        }

        [DataMember] private ColorData fontColor = Color.Black.ToColorData();
        [IgnoreDataMember]
        public ColorData FontColor
        {
            get => fontColor;
            set
            {
                fontColor = value;
                NoticePropertyChanged(nameof(FontColor));
            }
        }
        #endregion

        #region font
        [DataMember] private int fontSizeLarge = 24;
        [IgnoreDataMember] public int FontSizeLarge
        {
            get => fontSizeLarge;
            set
            {
                fontSizeLarge = value;
                NoticePropertyChanged(nameof(FontSizeLarge));
            }
        }

        [DataMember] private int fontSizeMedium = 16;
        [IgnoreDataMember] public int FontSizeMedium
        {
            get => fontSizeMedium;
            set
            {
                fontSizeMedium = value;
                NoticePropertyChanged(nameof(FontSizeMedium));
            }
        }


        [DataMember] private int fontSizeNormal = 12; [IgnoreDataMember]
        public int FontSizeNormal
        {
            get => fontSizeNormal;
            set
            {
                fontSizeNormal = value;
                NoticePropertyChanged(nameof(FontSizeNormal));
            }
        }


        [DataMember] private int fontSizeSmall = 10;
        [IgnoreDataMember] public int FontSizeSmall
        {
            get => fontSizeSmall;
            set
            {
                fontSizeSmall = value;
                NoticePropertyChanged(nameof(FontSizeSmall));
            }
        }
        #endregion

        #region animations
        public const double ANIMA_DEFAULTDURA = 0.2d;  
        [DataMember] private double animDurationIn = ANIMA_DEFAULTDURA;
        [IgnoreDataMember]
        public double AnimDurationIn
        {
            get => animDurationIn;
            set
            {
                animDurationIn = value;
                NoticePropertyChanged(nameof(AnimDurationIn));
            }
        }


        [DataMember] private double animDurationOut = ANIMA_DEFAULTDURA;
        [IgnoreDataMember]
        public double AnimDurationOut
        {
            get => animDurationOut;
            set
            {
                animDurationOut = value;
                NoticePropertyChanged(nameof(AnimDurationOut));
            }
        }
        #endregion


        

        public readonly static Theme Default = new() { ThemeName = nameof(Default) };
    }
}
