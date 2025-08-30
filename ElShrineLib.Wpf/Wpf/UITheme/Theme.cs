using ElShrine.EGraphic;
using ElShrine.Wpf.ViewModel;
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


        [DataMember] private ColorData foreColor = Color.LightSlateGray.ToColorData();
        [IgnoreDataMember] public ColorData ForeColor
        {
            get => foreColor;
            set
            {
                foreColor = value;
                NoticePropertyChanged(nameof(ForeColor));
            }
        }


        [DataMember] private ColorData backColor = Color.White.ToColorData();
        [IgnoreDataMember] public ColorData BackColor
        {
            get => backColor;
            set
            {
                backColor = value;
                NoticePropertyChanged(nameof(BackColor));
            }
        }


        [DataMember] private ColorData borderColor = Color.LightSlateGray.ToColorData();
        [IgnoreDataMember] public ColorData BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                NoticePropertyChanged(nameof(BorderColor));
            }
        }


        [DataMember] private ColorData selectionColor = Color.DodgerBlue.ToColorData();
        [IgnoreDataMember] public ColorData SelectionColor
        {
            get => selectionColor;
            set
            {
                selectionColor = value;
                NoticePropertyChanged(nameof(SelectionColor));
            }
        }


        [DataMember] private ColorData clickColor = Color.DodgerBlue.Lerp(Color.White, 0.5, false).ToColorData();
        [IgnoreDataMember] public ColorData ClickColor
        {
            get => clickColor;
            set
            {
                clickColor = value;
                NoticePropertyChanged(nameof(ClickColor));
            }
        }

        [DataMember] private ColorData fontColor = Color.Black.ToColorData();
        [IgnoreDataMember] public ColorData FontColor
        {
            get => fontColor;
            set
            {
                fontColor = value;
                NoticePropertyChanged(nameof(FontColor));
            }
        }


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


        [DataMember] private CornerRadius cornerRadius = new(2);
        [IgnoreDataMember] public CornerRadius CornerRadius
        {
            get => cornerRadius;
            set
            {
                cornerRadius = value;
                NoticePropertyChanged(nameof(CornerRadius));
            }
        }


        public readonly static Theme Default = new() { ThemeName = nameof(Default) };
    }
}
