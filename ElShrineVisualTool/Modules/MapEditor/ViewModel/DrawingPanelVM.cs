using ElShrine.Common;
using ElShrine.Modules.MapEditor.Model;
using ElShrine.Wpf.ViewModel;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using PointD = System.Windows.Point;
using VMCommand = ElShrine.Wpf.VMCommand;


namespace ElShrine.Modules.MapEditor.ViewModel
{
    public enum DrawingActionType
    {
        Normal, Point, Line, Curve, Circle, Rectangle, Poly, PolyCurve
    }
    
    public sealed class DrawingPanelVM(DrawingElement[] eles, Size size, double scale) : ViewModelBase
    {
        public bool DrawingPanelEnabled { get; set; } = false;

        #region Prop Action
        public DrawingActionType CurrentAction { get; set; }
        public string CurrentActionInfo => CurrentAction.ToString();
        public VMCommand ChangeAction => new(o =>
        {
            if (o is string s)
            {
                DrawingActionType dType = s switch
                {
                    nameof(DrawingActionType.Point) => DrawingActionType.Point,
                    nameof(DrawingActionType.Line) => DrawingActionType.Line,
                    nameof(DrawingActionType.Curve) => DrawingActionType.Curve,
                    nameof(DrawingActionType.Circle) => DrawingActionType.Circle,
                    nameof(DrawingActionType.Rectangle) => DrawingActionType.Rectangle,
                    nameof(DrawingActionType.Poly) => DrawingActionType.Poly,
                    nameof(DrawingActionType.PolyCurve) => DrawingActionType.PolyCurve,
                    _ => DrawingActionType.Normal,
                };
                CurrentAction = dType;
                NoticePropertyChanged(nameof(CurrentAction), nameof(CurrentActionInfo));
            }
        });
        #endregion


        #region Maps
        public bool mapIsVisible = false;
        public bool MapIsVisible
        {
            get => mapIsVisible;
            set
            {
                mapIsVisible = value;
                NoticePropertyChanged(nameof(MapIsVisible));
            }
        }


        private readonly List<DrawingElement> DrawingElements = [.. eles];
        private readonly List<DrawingElement> SelectedElements = [];
        private readonly List<DrawingElement> PreviewElements = [];
        public WriteableBitmap CurrentEleImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        public WriteableBitmap DrawingEleImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        public WriteableBitmap PreviewEleImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        private readonly ResenderProcessVM ResenderProcess = new(Dispatcher.CurrentDispatcher);
        private async void UpdateCurrentElePainter()
        {
            var result = await ResenderProcessVM.Begin(CurrentEleImage, m =>
            {
                DrawingElePainter painter = new(m, [.. SelectedElements]) { Pen = new(Color.Black, ActualPenWidth) };
                painter.Draw();
            });
            result.CopyToWriteableBitmap(CurrentEleImage);
            NoticePropertyChanged(nameof(CurrentEleImage));
        }
        private async void UpdateDrawingMapImage()
        {
            var result = await ResenderProcessVM.Begin(DrawingEleImage, m =>
            {
                DrawingElePainter painter = new(m, [.. DrawingElements]) { Pen = new(Color.Black, ActualPenWidth) };
                painter.Draw();
            });
            result.CopyToWriteableBitmap(DrawingEleImage);
            NoticePropertyChanged(nameof(DrawingEleImage));
        }
        private async void UpdatePreviewEleImage()
        {
            var result = await ResenderProcessVM.Begin(PreviewEleImage, m =>
            {
                DrawingElePainter painter = new(m, [.. PreviewElements]) { Pen = new(Color.Red, ActualPenWidth) };
                painter.Draw();
            });
            result.CopyToWriteableBitmap(PreviewEleImage);
            NoticePropertyChanged(nameof(PreviewEleImage));
        }
        #endregion

        private int penWidth = 2;
        public int PenWidth
        {
            get => penWidth;
            set
            {
                
                if(penWidth != value)
                {
                    penWidth = value;
                    NoticePropertyChanged(nameof(PenWidth));
                }
            }
        }
        public float ActualPenWidth => (float)(PenWidth / 1000d / Scale * Math.Max(Size.Width, Size.Height));

        #region Download;
        private Size Size = size;
        private double Scale = scale;

        public void UpdateSize(Size size)
        {
            Size = size;
            CurrentEleImage = BitmapHelper.NewWriteableBitMap(size); UpdateCurrentElePainter();
            DrawingEleImage = BitmapHelper.NewWriteableBitMap(size); UpdateDrawingMapImage();
            PreviewEleImage = BitmapHelper.NewWriteableBitMap(size); UpdatePreviewEleImage();
        }
        public void UpdateScale(double scale)
        {
            Scale = scale;
            UpdateCurrentElePainter();
            UpdateDrawingMapImage();
            UpdatePreviewEleImage();
        }
        #endregion


        #region Mouse Status
        public bool MouseIn = false;
        private PointD? cursorMapPosition = null;
        public PointD? CusorMapPosition
        {
            get => cursorMapPosition;
            set
            {
                cursorMapPosition = value;
                NoticePropertyChanged(nameof(CusorMapPositionX), nameof(CusorMapPositionY), nameof(CusorMapPositionInfo));
            }
        }
        public int CusorMapPositionX => CusorMapPosition is null ? -1 : (int)(Size.Width * CusorMapPosition.Value.X);
        public int CusorMapPositionY => CusorMapPosition is null ? -1 : (int)(Size.Height * CusorMapPosition.Value.Y);
        public string CusorMapPositionInfo => CusorMapPosition is null ? "Mouse is out" : $"{(int)CusorMapPosition.Value.X},{(int)CusorMapPosition.Value.Y}";

        #endregion

        private PointD ToStandardPoint(PointD point)
            => new(point.X / Size.Width, point.Y / Size.Height);
        private PointD ToMapPoint(PointD point)
            => new(point.X * Size.Width, point.Y * Size.Height);
        private static PointD ToPointD(Point point)
           => new(point.X, point.Y);


        
        private readonly Queue<PointD> TempPoints = [];
        private readonly List<PointD> TempPrePoints = [];
        private void ConstructEle(DrawingActionType action, int pointCount)
        {
            if (TempPoints.Count >= pointCount)
            {
                TempPrePoints.Clear();
                PointD[] points = new PointD[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    points[i] = TempPoints.Dequeue();
                }
                DrawingElement ele = new(action, points);
                DrawingElements.AddRange(SelectedElements);
                SelectedElements.Clear();
                SelectedElements.Add(ele);
                //
                UpdateDrawingMapImage();
            }
        }

        private int GetActionPointsNeed()
        {
            var count = -1;
            switch (CurrentAction)
            {
                default:
                case DrawingActionType.Normal:
                    break;
                case DrawingActionType.Point:
                    count = 1;
                    break;
                case DrawingActionType.Line:
                case DrawingActionType.Circle:
                case DrawingActionType.Rectangle:
                    count = 2;
                    break;
                case DrawingActionType.Curve:
                    count = 3;
                    break;
                case DrawingActionType.Poly:
                case DrawingActionType.PolyCurve:
                    count = 0;
                    break;
            }
            return count;
        }
        public void Move(PointD mapLoc)
        {
            PreviewElements.Clear();
            var p = ToStandardPoint(mapLoc);
            var act = CurrentAction;
            var count = GetActionPointsNeed();
            var length = TempPoints.Count;
            PointD[] points;
            if (length < count)
            {
                points = [.. TempPrePoints, p];
                if (length == count - 1)
                {
                    var ele = new DrawingElement(act, points);
                    PreviewElements.Add(ele);
                }
                else
                {
                    
                }
                UpdatePreviewEleImage();
            }
        }
        public void LeftClick(PointD mapLoc)
        {
            var point = ToStandardPoint(mapLoc);
            TempPoints.Enqueue(point);
            TempPrePoints.Add(point);
            var act = CurrentAction;
            var count = GetActionPointsNeed();
            if (count > 0) ConstructEle(act, count);
            UpdateCurrentElePainter();
        }
        public void RightClick(Point mapLoc)
        {
            TempPoints.Clear();
        }

    }
}
