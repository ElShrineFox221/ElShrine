using ElShrine.EGraphic;
using ElShrine.Modules.MapEditor.Model;
using ElShrine.Wpf;
using ElShrine.Wpf.ViewModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ElShrine.Modules.MapEditor.ViewModel
{
    public sealed class ColorPaintPanelVM(List<FillColor> items) : CollectionViewModelBase<FillColor, FillColorVM>(items)
    {
        private void Sort() => Reorder.Execute(null);
        protected override FillColorVM Construct(FillColor model)
        {
            FillColorVM fcvm = new(model);
            fcvm.PropertyChanged += (o, e) =>
            {
                if (e.PropertyName == nameof(FillColorVM.Value)) Sort();
                UpdateBrushInfo();
            };
            return fcvm;
        }
        protected override FillColor Construct(FillColorVM viewModel) => new(viewModel.ColorCode, viewModel.Value);

        public event EventHandler? ColorChanged;
        public void UpdateBrushInfo()
        {
            NoticePropertyChanged(nameof(HorizontalGradientFillColorBrush), nameof(VerticalGradientFillColorBrush));
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }
        public GradientBrush HorizontalGradientFillColorBrush
            => new LinearGradientBrush([.. Models.Select(m => new GradientStop(m.Color.ToMediaColor(), m.Value))], 0);
        public GradientBrush VerticalGradientFillColorBrush
            => new LinearGradientBrush([.. Models.Select(m => new GradientStop(m.Color.ToMediaColor(), m.Value))], 90);
        protected override FillColor GetAddItem(object? commandParam, out bool confrimAdd)
        {
            confrimAdd = true;
            return new FillColor(System.Drawing.Color.PaleVioletRed, 0.5);
        }
        protected override int[] GetClearExceptIndexes(object? commandParam) => [];
        protected override int[] GetRemoveItemIndexes(object? commandParam)
        {
            int[] indexes = [];
            if (commandParam is not null)
            {
                if (commandParam is IList<FillColorVM> fcvms) indexes = [.. fcvms.Select(ViewModels.IndexOf).Where(i => i != -1)];
                else if (commandParam is FillColorVM fcvm) indexes = [ViewModels.IndexOf(fcvm)];
            }
            return indexes;
        }
        protected override bool ReorderItems(object? commandParam)
        {
            Models.Sort((fc0, fc1) => Math.Sign(fc0.Value - fc1.Value));
            return true;
        }

        protected override void CollectionChanged(CollectionChangeAction change, IEnumerable<FillColor> models)
        {
            base.CollectionChanged(change, models);
            if (change != CollectionChangeAction.Refresh) Reorder.Execute(null);
            UpdateBrushInfo();
        }


        #region Palette
        public ColorData PaletteColor { get; set; } = System.Drawing.Color.PaleGoldenrod.ToColorData();
        private bool paletteOpened = false;
        public bool PaletteOpened
        {
            get => paletteOpened;
            set
            {
                paletteOpened = value;
                NoticePropertyChanged(nameof(PaletteOpened));
            }
        }
        public VMCommand OpenPalette => new(o =>
        {
            if (o is FrameworkElement fe && fe.DataContext is FillColorVM fcvm) SelectedItem = fe;
            FillColorVM? vm = SelectedItem?.DataContext as FillColorVM;
            if (vm is not null) PaletteColor = vm.Color.ToColorData();

            PaletteOpened = true;
        });
        public VMCommand ClosePalette => new(o =>
        {
            PaletteOpened = false;
        });
        public VMCommand ConfrimPalette => new(o =>
        {
            PaletteOpened = false;
            if (o is FrameworkElement fe && fe.DataContext is FillColorVM fcvm) SelectedItem = fe;
            FillColorVM? vm = SelectedItem?.DataContext as FillColorVM;
            if (vm is not null) vm.Color = PaletteColor.ToMediaColor();
        });

        private FrameworkElement? selectedItem  = null;
        public FrameworkElement? SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                NoticePropertyChanged(nameof(SelectedItem));
            }
        }
        #endregion

        private bool panelFolded = false;
        public bool PanelFolded
        {
            get => panelFolded;
            set
            {
                panelFolded = value;
                NoticePropertyChanged(nameof(PanelFolded));
            }
        }
        public VMCommand ChangePanelFolded => new(o =>
        {
            if (o is bool b) PanelFolded = b;
        });
    }
}
