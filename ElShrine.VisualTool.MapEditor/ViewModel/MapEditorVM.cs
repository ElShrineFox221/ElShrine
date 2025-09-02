using ElShrine.ECommand;
using ElShrine.EOption;
using ElShrine.VisualTool;
using ElShrine.VisualTool.MapEditor.Model;
using ElShrine.Wpf.ViewModel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Command = ElShrine.ECommand.Command;
using VMCommand = ElShrine.Wpf.VMCommand;

namespace ElShrine.VisualTool.MapEditor.ViewModel
{
    [WpfPageRootVM(Name = "Map Editor", Version = "1.0", Tags = ["Common", "Graph", "Map"], Description = "Map editor with 2d perlin noises and drawing pannel.", DataTemplateUri = "/ElShrine.VisualTool.MapEditor;component/MapEditor.xaml", DataTemplateName = "MapEditorTemplate")]
    [StartupClass]
    public class MapEditorVM : ViewModelBase, ISingleton<MapEditorVM>
    {
        private MapEditorVM()
        {
            MapManager.LoadedMapsChanged += (actionType, map, fromFile) =>
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    switch (actionType)
                    {
                        case CollectionChangeAction.Add:
                            if (map is not null) LoadedMaps.Add(new(map, fromFile));
                            break;
                        case CollectionChangeAction.Remove:
                            if (map is not null)
                            {
                                var mapvm = LoadedMaps.ToList().Find(lm => lm.GetMap() == map);
                                if (mapvm is not null) LoadedMaps.Remove(mapvm);
                            }
                            break;
                        default:
                            NoticePropertyChanged(nameof(LoadedMaps));
                            break;
                    }
                });
            };

        }
        private static MapEditorVM? Instance = null;
        public static MapEditorVM GetInstance() => Instance ??= new();
        #region Common
        public ObservableCollection<MapVM> LoadedMaps { get; set; } = [];
        public MapVM? SelectedMap => SelectedMapIndex >= 0 && SelectedMapIndex < LoadedMaps.Count ? LoadedMaps[SelectedMapIndex] : null;
        #endregion

        #region Global Panel VM

        #region Import Map
        private string mapInputPath = $"{Environment.CurrentDirectory}\\{nameof(Map)}";
        public string MapImportPath
        {
            get => mapInputPath; set
            {
                mapInputPath = value;
                NoticePropertyChanged(nameof(MapImportPath));
            }
        }
        public VMCommand OpenMapImportDialog => new(o =>
        {
            string defaultPath = $"{Environment.CurrentDirectory}\\{nameof(Map)}";
            if (!Directory.Exists(defaultPath)) Directory.CreateDirectory(defaultPath);

            var dialog = new OpenFolderDialog() { DefaultDirectory = defaultPath };
            
            var result = dialog.ShowDialog();
            if (result is not null && result.Value)
            {
                var path = dialog.FolderName;
                MapImportPath = path;
            }
        });
        public VMCommand MapImportConfrim => new(o =>
        {
            Command.ParseAndExcute($"{nameof(MapManager)}.{nameof(MapManager.Load)} \"{MapImportPath}\"");
        });
        #endregion

        public VMCommand SaveLoadedMaps => new(o =>
        {
            foreach ( var map in LoadedMaps )
            {
                if (map.Dirtied)
                {
                    map.Dirtied = false;
                }
            }
            Command.ParseAndExcute($"{nameof(MapManager)}.{nameof(MapManager.Save)}");
        });

        #region New Map
        public string MapNameInputBar { get; set; } = Const.EmptyStr;
        public VMCommand CreateNewMap => new(o =>
        {
            string? name = MapNameInputBar.IsEmpty() ? "null" : MapNameInputBar;
            Command.ParseAndExcute($"{nameof(MapManager)}.{nameof(MapManager.CreateMap)} {name}");
        });
        #endregion

        #region Selected Map
        public int SelectedMapIndex { get; set; } = -1;
        public bool MapEnable => SelectedMap is not null;
        public VMCommand SelectedMapChange => new(o =>
        {
            if (o is MapVM m)
            {
                var index = LoadedMaps.IndexOf(m);
                SelectedMapIndex = index == SelectedMapIndex ? -1 : index;
                NoticePropertyChanged(nameof(SelectedMapIndex), nameof(SelectedMap), nameof(MapEnable));
            }
        });
        #endregion

        #region Selected Map Panel
        public VMCommand DeleteMap => new(o =>
        {
            if (SelectedMap is not null)
            {
                Command.ParseAndExcute($"{nameof(MapManager)}.{nameof(MapManager.DeleteMap)} \"{SelectedMap.PublicName}\" \"{SelectedMap.Directory}\"");
            }
        });
        #region Map Rename
        public bool RenameEnabled { get; set; } = false;
        public VMCommand RenameMap => new(o =>
        {
            if (o is string s && SelectedMap is not null)
            {
                if (RenameEnabled)
                {
                    SelectedMap.PublicName = s.IsEmpty() ? SelectedMap.PublicName : s;
                    RenameEnabled = false;
                    SelectedMap.NoticePropertyChanged(nameof(SelectedMap.PublicName));
                }
                else RenameEnabled = true;
                NoticePropertyChanged(nameof(RenameEnabled));
            }
        });
        public VMCommand SaveImageFile => new(o =>
        {
            if(SelectedMap is not null)
            {
                var dia = new OpenFolderDialog() { DefaultDirectory = Environment.CurrentDirectory };
                var suc = dia.ShowDialog();
                if (suc is bool b && b) Command.ParseAndExcute($"{nameof(MapManager)}.{nameof(MapManager.ExportPNG)} \"{SelectedMap.IdenticalName}\" \"{dia.FolderName}\"");
            }
        });
        #endregion
        #endregion

        #endregion

        public VMCommand OpenDirectory => new(o =>
        {
            if (o is string path) Command.ParseAndExcute($"{nameof(GlobalCommands.Open)} \"{path}\"");
        });
        public VMCommand RefreshMapDrited => new(o =>
        {
            foreach(var map in LoadedMaps)
            {
                map.NoticePropertyChanged(nameof(MapVM.Dirtied));
            }
        });

        #region DrawingPanel




        #endregion
    }
}
