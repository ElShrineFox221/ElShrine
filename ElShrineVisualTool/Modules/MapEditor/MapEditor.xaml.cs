using ElShrine.Modules.MapEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElShrine.Modules.MapEditor
{
    public partial class MapEditor
    {
        private void MapEditorRoot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tbc && tbc.DataContext is MapEditorVM mevm)
            {

                switch (tbc.SelectedIndex)
                {
                    case 0:
                        mevm.RefreshMapDrited.Execute(null);
                        break;
                    case 1:
                        if (mevm.SelectedMap is not null)
                        {
                            mevm.SelectedMap.ResenderingPanel.MapIsVisible = true;
                            mevm.SelectedMap.DrawingPanel.MapIsVisible = false;
                        } 
                        break;
                    case 2:
                        if (mevm.SelectedMap is not null)
                        {
                            mevm.SelectedMap.ResenderingPanel.MapIsVisible = false;
                            mevm.SelectedMap.DrawingPanel.MapIsVisible = true;
                        }
                        break;
                }
            }
        }

        private FrameworkElement? ImageContainer { get; set; } = null;
        private MapVM? Map { get; set; } = null;
        private void ResetInfos(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is MapVM mvm)
            {
                Map = mvm;
                ImageContainer = fe;
            }
        }
        private void ImageDirectContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            ResetInfos(sender, e);
        }
        private void ImageDirectContainer_MouseMove(object sender, MouseEventArgs e)
        {
            ResetInfos(sender, e);
            if (Map is not null && ImageContainer is not null)
            {
                var loc = e.GetPosition(ImageContainer);
                Map.DrawingPanel.CusorMapPosition = loc;
                Map.DrawingPanel.Move(loc);
            }
        }
        private void ImageDirectContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            ResetInfos(sender, e);
            if (Map is not null && ImageContainer is not null)
            {
                Map.DrawingPanel.CusorMapPosition = null;
            }
        }

        private void ImageDirectContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ResetInfos(sender, e);
            if (Map is not null && ImageContainer is not null)
            {
                var pos = e.GetPosition(ImageContainer);
                Map.DrawingPanel.LeftClick(pos);
            }
        }

        private void ImageDirectContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
