using ElShrine.Wpf;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class ElShrineTempFieldUnit : ContentControl, INotifyPropertyChanged
    {
        public ElShrineTempFieldUnit(ElShrineTempField parent)
        {
            ETFParent = parent;
            MouseDoubleClick += delegate
            {
                Visible = !Visible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Distance)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cost)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartSign)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TagSign)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
            };
        }
        public int X
        {
            get { return (int)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));
        public int Y
        {
            get { return (int)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));
        public ElShrineTempField? ETFParent = null;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Distance
        {
            get => ETFParent is not null ? (Math.Abs(ETFParent.TagX - X) + Math.Abs(ETFParent.TagY - Y)).ToString() : "0";
        }
        public string Cost
        {
            get => ETFParent is not null ? (Math.Abs(ETFParent.StartX - X) + Math.Abs(ETFParent.StartY - Y)).ToString() : "0";
        }
        public string Value
        {
            get => ETFParent is not null ? (Math.Abs(ETFParent.StartX - X) + Math.Abs(ETFParent.StartY - Y) + Math.Abs(ETFParent.TagX - X) + Math.Abs(ETFParent.TagY - Y)).ToString() : "0";
        }
        public string StartSign
        {
            get => ETFParent is not null && Cost == "0" ? "Start" : string.Empty;
        }
        public string TagSign
        {
            get => ETFParent is not null && Distance == "0" ? "Tag" : string.Empty;
        }
        public bool Visible { get; set; } = false;
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class ElShrineTempField : ContentControl
    {
        public void Initialize(int tagx = 5, int tagy = 3, int fieldx = 5, int fieldy = 5, int startx = 1, int starty = 3)
        {
            TagX = tagx; TagY = tagy; FieldX = fieldx; FieldY = fieldy;
            StartX = startx; StartY = starty;
            Canvas? canvas = (Canvas?)this.FindChild("TempFieldContainer_Canvas");
            Style? style = (Style?)new Window().TryFindResource("ElShrineTempFieldUnit_Style");
            if (canvas is not null)
            {
                double width = canvas.ActualWidth / FieldX;
                double height = canvas.ActualHeight / FieldY;
                canvas.Children.Clear();
                for (int x = 0; x < FieldX; x++)
                {
                    for (int y = 0; y < FieldY; y++)
                    {
                        ElShrineTempFieldUnit unit = new(this)
                        {
                            X = x,
                            Y = y,
                            Width = width,
                            Height = height,
                            Visibility = Visibility.Visible,
                            Opacity = 1,
                        };
                        if (style is not null) unit.Style = style;
                        canvas.Children.Add(unit);
                        Canvas.SetLeft(unit, x * canvas.ActualWidth / FieldX);
                        Canvas.SetTop(unit, y * canvas.ActualHeight / FieldY);
                    }
                }
            }
        }
        public static void InitializeInstance(FrameworkElement sender, string name,
            int tagx = 4, int tagy = 2, int fieldx = 5, int fieldy = 5, int startx = 0, int starty = 2)
        {
            object? o = sender.FindChild(name);
            if (o is ElShrineTempField etf) etf.Initialize(tagx, tagy, fieldx, fieldy, startx, starty);
        }

        public int StartX
        {
            get { return (int)GetValue(StartXProperty); }
            set { SetValue(StartXProperty, value); }
        }
        public static readonly DependencyProperty StartXProperty =
            DependencyProperty.Register("StartX", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));

        public int StartY
        {
            get { return (int)GetValue(StartYProperty); }
            set { SetValue(StartYProperty, value); }
        }
        public static readonly DependencyProperty StartYProperty =
            DependencyProperty.Register("StartY", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));


        public int TagX
        {
            get { return (int)GetValue(TagXProperty); }
            set { SetValue(TagXProperty, value); }
        }
        public static readonly DependencyProperty TagXProperty =
            DependencyProperty.Register("TagX", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));

        public int TagY
        {
            get { return (int)GetValue(TagYProperty); }
            set { SetValue(TagYProperty, value); }
        }
        public static readonly DependencyProperty TagYProperty =
            DependencyProperty.Register("TagY", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));


        public int FieldX
        {
            get { return (int)GetValue(FieldXProperty); }
            set { SetValue(FieldXProperty, value); }
        }
        public static readonly DependencyProperty FieldXProperty =
            DependencyProperty.Register("FieldX", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));

        public int FieldY
        {
            get { return (int)GetValue(FieldYProperty); }
            set { SetValue(FieldYProperty, value); }
        }
        public static readonly DependencyProperty FieldYProperty =
            DependencyProperty.Register("FieldY", typeof(int), typeof(ElShrineTempField), new PropertyMetadata(0));
    }
}
