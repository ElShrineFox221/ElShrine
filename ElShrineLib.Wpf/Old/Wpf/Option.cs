using ElShrine.Wpf;
using ElShrine.Wpf.ViewModel;
using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace ElShrine.Old.Wpf
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [DataContract]
    public partial class Option : ViewModelBase, IEName
    {
        protected Option() { }
        public static Option GetInstance() => Instance;
        
        protected readonly static Option Instance = new();

        private int layTimeSpanMS = 300;
        [DataMember]
        public int LayTimeSpanMS
        {
            get => layTimeSpanMS; set
            {
                layTimeSpanMS = value;
                NoticePropertyChanged(this, nameof(LayTimeSpanMS));
            }
        }

        private double layTimeSpanContinueFactor = 0.7;
        [DataMember]
        public double LayTimeSpanContinueFactor
        {
            get => layTimeSpanContinueFactor; set
            {
                layTimeSpanContinueFactor = value;
                NoticePropertyChanged(this, nameof(LayTimeSpanContinueFactor));
            }
        }

        private int layTimeSpanFactorInPixel = 100;
        [DataMember]
        public int LayTimeSpanFactorInPixel
        {
            get => layTimeSpanFactorInPixel; set
            {
                layTimeSpanFactorInPixel = value;
                NoticePropertyChanged(this, nameof(LayTimeSpanFactorInPixel));
            }
        }

        private double arrowRollingTimeSpanFactor = 2.5;
        [DataMember]
        public double ArrowRollingTimeSpanFactor
        {
            get => arrowRollingTimeSpanFactor; set
            {
                arrowRollingTimeSpanFactor = value;
                NoticePropertyChanged(this, nameof(ArrowRollingTimeSpanFactor));
            }
        }

        private double layRate = -0.4;
        [DataMember]
        public double LayRate
        {
            get => layRate; set
            {
                layRate = value;
                NoticePropertyChanged(this, nameof(LayRate));
            }
        }

        private double layRateFactor = 0.5;
        [DataMember]
        public double LayRateFactor
        {
            get => layRateFactor; set
            {
                layRateFactor = value;
                NoticePropertyChanged(this, nameof(LayRateFactor));
            }
        }


        #region Thickness
        [DataMember]
        private Thickness option_WindowBorderThickness = new(4);
        [DataMember]
        public Thickness Option_WindowBorderThickness
        {
            get => option_WindowBorderThickness; set
            {
                option_WindowBorderThickness = value;
                NoticePropertyChanged(this, nameof(Option_WindowBorderThickness));
            }
        }
        #endregion


        private double windowHeaderWidth = 110;
        [DataMember]
        public double WindowHeaderWidth
        {
            get => windowHeaderWidth; set
            {
                windowHeaderWidth = value;
                NoticePropertyChanged(this, nameof(WindowHeaderWidth));
            }
        }

        private double scrollBarOpacity = 0.6;
        [DataMember]
        public double ScrollBarOpacity
        {
            get => scrollBarOpacity; set
            {
                scrollBarOpacity = value;
                NoticePropertyChanged(this, nameof(ScrollBarOpacity));
            }
        }

        private bool scrollToBottom = true;
        [DataMember]
        public bool ScrollToBottom
        {
            get => scrollToBottom; set
            {
                scrollToBottom = value;
                NoticePropertyChanged(this, nameof(ScrollToBottom));
            }
        }


        #region Color
        [DataMember]
        public Color Option_ForeColorSource { get => option_ForeColor.Color; set => Option_ForeColor = new(value); }
        [DataMember]
        public Color Option_BackColorSource { get => option_BackColor.Color; set => Option_BackColor = new(value); }
        [DataMember]
        public Color Option_FontColorSource { get => option_FontColor.Color; set => Option_FontColor = new(value); }

        private SolidColorBrush option_ForeColor = new(System.Drawing.Color.PaleVioletRed.ToMediaColor());
        public SolidColorBrush Option_ForeColor
        {
            get => option_ForeColor; set
            {
                option_ForeColor = value;
                Instance.NoticePropertyChanged(this, nameof(Option_ForeColor));
            }
        }

        private SolidColorBrush option_BackColor = new(System.Drawing.Color.WhiteSmoke.ToMediaColor());
        public SolidColorBrush Option_BackColor
        {
            get => option_BackColor; set
            {
                option_BackColor = value;
                Instance.NoticePropertyChanged(this, nameof(Option_BackColor));
            }
        }

        private SolidColorBrush option_FontColor = new(System.Drawing.Color.Black.ToMediaColor());
        public SolidColorBrush Option_FontColor
        {
            get => option_FontColor; set
            {
                option_FontColor = value;
                Instance.NoticePropertyChanged(this, nameof(Option_FontColor));
            }
        }
        #endregion


        #region Opacity
        private double option_Opacity = 0.96;
        [DataMember]
        public double Option_Opacity
        {
            get => option_Opacity; set
            {
                option_Opacity = value;
                NoticePropertyChanged(this, nameof(Option_Opacity));
            }
        }

        private double option_Opacity_Min = 0.8;
        [DataMember]
        public double Option_Opacity_Min
        {
            get => option_Opacity_Min; set
            {
                option_Opacity_Min = value;
                NoticePropertyChanged(this, nameof(Option_Opacity_Min));
            }
        }

        private double option_Opacity_Disenabled = 0.57;
        [DataMember]
        public double Option_Opacity_Disenabled
        {
            get => option_Opacity_Disenabled; set
            {
                option_Opacity_Disenabled = value;
                NoticePropertyChanged(this, nameof(Option_Opacity_Disenabled));
            }
        }

        private double option_Opacity_Enabled = 1;
        [DataMember]
        public double Option_Opacity_Enabled
        {
            get => option_Opacity_Enabled; set
            {
                option_Opacity_Enabled = value;
                NoticePropertyChanged(this, nameof(Option_Opacity_Enabled));
            }
        }
        #endregion


        private int option_MaxColorCollectionNum = 6;
        [DataMember]
        public int Option_MaxColorCollectionNum
        {
            get => option_MaxColorCollectionNum; set
            {
                option_MaxColorCollectionNum = value;
                NoticePropertyChanged(this, nameof(Option_MaxColorCollectionNum));
            }
        }

        public string Name { get; } = nameof(Option);
    }
}
