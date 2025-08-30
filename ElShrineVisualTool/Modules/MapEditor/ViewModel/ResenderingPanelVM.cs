using ElShrine.Common;
using ElShrine.Modules.MapEditor.Model;
using ElShrine.Wpf.ViewModel;
using MathNet.Numerics.LinearAlgebra;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Int32Size = System.Drawing.Size;
using VMCommand = ElShrine.Wpf.VMCommand;

namespace ElShrine.Modules.MapEditor.ViewModel
{
    public sealed class ResenderingPanelVM(Int32Size size, Func<Matrix<double>> matrixFunc, FillColor[] fillColors) : ViewModelBase
    {
        protected override void Initialize()
        {
            SizeChanged += (_, s) =>
            {
                UpdateMapSize(s);
            };
        }
        //upload = size
        //download = matrix, fillcolors

        #region Upload > Size
        private Int32Size pixelImageSize = size;
        private Int32Size PixelImageSize
        {
            get => pixelImageSize;
            set
            {
                if (pixelImageSize != value)
                {
                    pixelImageSize = value;
                    SizeChanged?.Invoke(this, value);
                    UpdateSizeInfo();
                    UpdateMapSize(value);
                }
            }
        }
        public int Width
        {
            get => PixelImageSize.Width;
            set => PixelImageSize = new(value, PixelImageSize.Height);
        }
        public int Height
        {
            get => PixelImageSize.Height;
            set => PixelImageSize = new(PixelImageSize.Width, value);
        }
        public bool RewidenEnabled { get; set; } = false;
        public bool ReheightenEnabled { get; set; } = false;
        private void UpdateSizeInfo()
            => NoticePropertyChanged(nameof(Width), nameof(Height), nameof(PixelImageSize));

        public VMCommand Rewiden => new(o =>
        {
            if (RewidenEnabled)
            {
                if (o is int i) Width = i;
                else if (o is string s && int.TryParse(s, out int i1)) Width = i1;
            }
            RewidenEnabled = !RewidenEnabled;
            NoticePropertyChanged(nameof(RewidenEnabled));
        });
        public VMCommand Reheighten => new(o =>
        {
            if (ReheightenEnabled)
            {
                if (o is int i) Height = i;
                else if (o is string s && int.TryParse(s, out int i1)) Height = i1;
            }
            ReheightenEnabled = !ReheightenEnabled;
            NoticePropertyChanged(nameof(ReheightenEnabled));
        });

        public event EventHandler<Int32Size>? SizeChanged;

        #endregion

        #region Dowload > Matrix, FillColors
        public void ConfrimMatrixChanged() => Regeneratable = true;
        private Func<Matrix<double>> FuncToGetMatrix = matrixFunc;
        private Matrix<double> AltitudeMatrix => FuncToGetMatrix.Invoke();
        public void UpdateMatrix(Func<Matrix<double>> matrixFunc)
        {
            FuncToGetMatrix = matrixFunc;
            GrayRegenerateable = true;
            ColoredRegenerateable = true;
            ContourRegenerateable = true;
            UpdateRegeneratableInfos();
        }

        private FillColor[] FillColors = fillColors;
        public void UpdateFillColors(FillColor[] fillColors)
        {
            if (FillColors != fillColors) FillColors = fillColors;
            ColoredRegenerateable = true;
            UpdateRegeneratableInfos();
        }
        #endregion

        #region View Type
        private ComboBoxItem? selectedMapViewType;
        public ComboBoxItem? SelectedMapViewType
        {
            get => selectedMapViewType;
            set
            {
                selectedMapViewType = value;
                if (selectedMapViewType is not null)
                {
                    switch (selectedMapViewType.Content.ToString())
                    {
                        case "Colored":
                            ColoredMapVisible = true;
                            GrayMapVisible = false;
                            break;
                        default:
                            ColoredMapVisible = false;
                            GrayMapVisible = true;
                            break;
                    }
                    NoticePropertyChanged(nameof(ColoredMapVisible), nameof(GrayMapVisible));
                }
            }
        }
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

        #region Gray Map
        public WriteableBitmap GrayMapImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        private bool grayMapVisible = true;
        public bool GrayMapVisible
        {
            get => grayMapVisible;
            set
            {
                grayMapVisible = value;
                NoticePropertyChanged(nameof(GrayMapVisible));
            }
        }
        //
        private bool GrayRegenerateable = true;
        #endregion

        #region Colored Map
        public WriteableBitmap ColoredMapImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        private bool coloredMapVisible = true;
        public bool ColoredMapVisible
        {
            get => coloredMapVisible;
            set
            {
                coloredMapVisible = value;
                NoticePropertyChanged(nameof(ColoredMapVisible));
            }
        }
        //
        private bool ColoredRegenerateable = true;
        #endregion

        #region Contour Map
        public WriteableBitmap ContourMapImage { get; set; } = BitmapHelper.NewWriteableBitMap(size);
        private bool contourMapVisible = true;
        public bool ContourMapVisible
        {
            get => contourMapVisible;
            set
            {
                contourMapVisible = value;
                NoticePropertyChanged(nameof(ContourMapVisible));
            }
        }
        //
        private int contours = 10;
        public int Contours
        {
            get => contours;
            set
            {
                if(contours != value)
                {
                    contours = value;
                    ContourRegenerateable = true;
                    UpdateRegeneratableInfos();
                    NoticePropertyChanged(nameof(Contours));
                }
            }
        }
        public VMCommand ContoursChange => new(o =>
        {
            if (o is int i) Contours = Math.Max(Contours + i, 0);
            else if (o is string s && int.TryParse(s, out int i1)) Contours = i1;
        });

        private double contourLineWidth = 1;
        public double ContourLineWidth
        {
            get => Math.Round(contourLineWidth, 1);
            set
            {
                if(contourLineWidth != value)
                {
                    contourLineWidth = value;
                    ContourRegenerateable = true;
                    UpdateRegeneratableInfos();
                    NoticePropertyChanged(nameof(ContourLineWidth));
                }
            }
        }
        public VMCommand ContourLineWidthChange => new(o =>
        {
            if (o is double d) ContourLineWidth = Math.Max(ContourLineWidth + d, 0);
            else if (o is string s && double.TryParse(s, out double d1)) ContourLineWidth = d1;
        });
        //
        private bool ContourRegenerateable = true;
        #endregion
        #endregion

        private void UpdateRegeneratableInfos()
            => NoticePropertyChanged(nameof(Regeneratable), nameof(ContourRegenerateable), nameof(GrayRegenerateable), nameof(ColoredRegenerateable));
        public bool Regeneratable
        {
            get => ContourRegenerateable || GrayRegenerateable || ColoredRegenerateable;
            set
            {
                ContourRegenerateable = value;
                GrayRegenerateable = value;
                ColoredRegenerateable = value;
                UpdateRegeneratableInfos();
            }
        }
        private void UpdateMapSize(Int32Size size)
        {
            GrayMapImage = BitmapHelper.NewWriteableBitMap(size);
            ColoredMapImage = BitmapHelper.NewWriteableBitMap(size);
            ContourMapImage = BitmapHelper.NewWriteableBitMap(size);
            Regeneratable = true;
        }


        public ResenderProcessVM ResenderProcess { get; private set; } = new(Dispatcher.CurrentDispatcher);
        public VMCommand RegenerateNoiseImage => new(async o =>
        {
            if (ContourRegenerateable)
            {
                var result = await ResenderProcessVM.Begin(ContourMapImage, m =>
                {
                    ContourPainter paint = new(m, AltitudeMatrix, Contours)
                    {
                        Scale = 1,
                        LineWidth = (float)ContourLineWidth,
                    };
                    paint.Draw();
                });
                result.CopyToWriteableBitmap(ContourMapImage);
                NoticePropertyChanged(nameof(ContourMapImage));
                ContourRegenerateable = false;
            }
            if (ColoredRegenerateable)
            {
                var result = await ResenderProcessVM.Begin(ColoredMapImage, m =>
                {
                    PixelPainter paint = new(m, AltitudeMatrix)
                    {
                        FillColors = [..FillColors]
                    };
                    paint.Draw();
                });
                result.CopyToWriteableBitmap(ColoredMapImage);
                NoticePropertyChanged(nameof(ColoredMapImage));
                ColoredRegenerateable = false;
            }
            if (GrayRegenerateable)
            {
                var result = await ResenderProcessVM.Begin(GrayMapImage, m =>
                {
                    PixelPainter paint = new(m, AltitudeMatrix)
                    {
                        ColorMatrix = PixelPainter.GrayPaintStyle
                    };
                    paint.Draw();
                });
                result.CopyToWriteableBitmap(GrayMapImage);
                NoticePropertyChanged(nameof(GrayMapImage));
                GrayRegenerateable = false;
            }
            //
            UpdateRegeneratableInfos();
            NoticePropertyChanged(nameof(GrayMapImage), nameof(ColoredMapImage), nameof(ContourMapImage));
        });
    }
}
