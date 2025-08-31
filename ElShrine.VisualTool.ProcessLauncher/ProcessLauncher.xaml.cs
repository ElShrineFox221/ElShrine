using ElShrine.VisualTool.ProcessLauncher.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ElShrine.VisualTool.ProcessLauncher
{
    public partial class ProcessLauncher
    {
        private void ProcessLauncher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Selector selector && selector.DataContext is ProcessLauncherVM plvm)
            {
                if (selector is ListView listView) plvm.SelectedItems = listView.SelectedItems;
                else if (selector is DataGrid dataGrid) plvm.SelectedItems = dataGrid.SelectedItems;
                else plvm.SelectedItems = e.AddedItems;
            }
        }
        private void PermissionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.DataContext is ExeInfoVM eivm)
            {
                eivm.PermissionIndex = cb.SelectedIndex;
                ResetParentSelectedIndex(cb, eivm);
            }
        }
        private void LaunchModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cb && cb.DataContext is ExeInfoVM eivm)
            {
                eivm.LaunchModeIndex = cb.SelectedIndex;
                ResetParentSelectedIndex(cb, eivm);
            }
        }
        private static void ResetParentSelectedIndex(FrameworkElement fe, ExeInfoVM eivm)
        {
            var p = VisualTreeHelper.GetParent(fe);
            while (p is not null && p is not Selector) p = VisualTreeHelper.GetParent(p);
            if (p is not null && p is Selector sel && sel.DataContext is ProcessLauncherVM plvm)
            {
                sel.SelectedIndex = plvm.ExeInfos.IndexOf(eivm);
            }
        }
    }
}
