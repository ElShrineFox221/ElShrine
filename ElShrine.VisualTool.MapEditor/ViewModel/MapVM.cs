using ElShrine.Common.Interface;
using Int32Size = System.Drawing.Size;
using VMCommand = ElShrine.Wpf.VMCommand;
using ElShrine.VisualTool.MapEditor.Model;
using ElShrine.Wpf;

namespace ElShrine.VisualTool.MapEditor.ViewModel
{
    public sealed class MapVM : ViewModelBase<Map>, IEDirtable
    {
        public MapVM(Map model, bool loadedFromFile) : base(model)
        {
            #region Panels
            //PerlinNoisePanel: download>size; upload>matrix, @seed
            //to ResenderingPanel, @Seed
            PerlinNoisePanel = new(model.Seed, model.ImageSize, model.BasicNoiseCollection);
            PerlinNoisePanel.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PerlinNoisePanelVM.Seed)) Seed = PerlinNoisePanel.Seed;
                if (e.PropertyName == nameof(PerlinNoisePanelVM.Regeneratable) && PerlinNoisePanel.Regeneratable) ResenderingPanel?.ConfrimMatrixChanged();
            };

            //ResenderPanel: download>fillcolors, matrix; upload>@size
            //to PerlinNoisePanel @Int32Size
            ResenderingPanel = new(model.ImageSize, () => PerlinNoisePanel.Perlin2D.MergedNoiseMatrix, [.. model.FillColors]);
            ResenderingPanel.SizeChanged += (_, s) =>
            {
                ImageSize = s;
                PerlinNoisePanel.UpdateSize(s);
            };

            //DrawingPanel: download>size, scale; upload>drawingeles
            DrawingPanel = new([], model.ImageSize, MapScale);

            //ColorPaintPanel: download>null; upload>fillcolors
            ColorPaintPanel = new(model.FillColors);
            ColorPaintPanel.ColorChanged += (_, _) =>
            {
                ResenderingPanel.UpdateFillColors([..model.FillColors]);
            };
            #endregion

            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(MapScale) || e.PropertyName == nameof(ImageSize)) DrawingPanel.UpdateScale(MapScale);
            };

            localDirtied = !loadedFromFile;
        }



        //
        public PerlinNoisePanelVM PerlinNoisePanel { get; }
        //
        public ResenderingPanelVM ResenderingPanel { get; set; }
        //
        public DrawingPanelVM DrawingPanel { get; set; }
        //
        
        public ColorPaintPanelVM ColorPaintPanel { get; set; }
        //

        #region Previewer

        #region Scale
        private int mapPercentScale = 50;
        public int MapPercentScale
        {
            get => mapPercentScale;
            set
            {
                mapPercentScale = value;
                NoticePropertyChanged(nameof(MapPercentScale), nameof(MapScale));
            }
        }
        public double MapScale
        {
            get => MapPercentScale / 100d;
            set => MapPercentScale = (int)Math.Round(value * 100, 0);
        }
        public VMCommand ChangeMapScale => new(o =>
        {
            if (o is double d) MapScale += d;
            else if (o is int i) MapPercentScale += i * 10;
            else if (o is string s && double.TryParse(s, out double r)) MapScale = r;
        });
        #endregion

        #endregion

        #region Map Infos
        //Name
        public string PublicName
        {
            get => Model.Name;
            set
            {
                Model.Name = value;
                LocalDirtied = true;
                NoticePropertyChanged(nameof(PublicName), nameof(IdenticalName), nameof(Directory), nameof(FileName));
            }
        }
        public string IdenticalName => Model.GetIdentifiedPath();
        public string Directory => Model.GetDirectory();
        public string FileName => $"{Model.GetFileName()}.{nameof(Map)}";

        public void UpdateResenderingInfos()
            => NoticePropertyChanged(nameof(Height), nameof(Width), nameof(ImageSize));
        //CommonImageData
        public int Width => Model.ImageSize.Width;
        public int Height => Model.ImageSize.Height;
        public Int32Size ImageSize
        {
            get => Model.ImageSize;
            set
            {
                Model.ImageSize = value;
                LocalDirtied = true;
                UpdateResenderingInfos();
            }
        }
        //Noise
        public int Seed
        {
            get => Model.Seed;
            set
            {
                Model.Seed = value;
                LocalDirtied = true;
                NoticePropertyChanged(nameof(Seed));
            }
        }
        #endregion

        public Map GetMap() => Model;
        

        

        private bool localDirtied;
        private bool LocalDirtied
        {
            get => localDirtied;
            set
            {
                localDirtied = value;
                NoticePropertyChanged(nameof(Dirtied));
            }
        }
        public bool Dirtied
        {
            get => PerlinNoisePanel.Dirtied || ColorPaintPanel.Dirtied || LocalDirtied;
            set
            {
                PerlinNoisePanel.Dirtied = value;
                ColorPaintPanel.Dirtied = value;
                LocalDirtied = value; 
            }
        }
    }
}
