using ElShrine.Common.PerlinNoise;
using ElShrine.ECommand;
using ElShrine.EConsole;
using ElShrine.EFile;
using ElShrine.Modules.MapEditor.Model;
using MathNet.Numerics.LinearAlgebra;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using static ElShrine.Common.Interface.IENameExtension;

namespace ElShrine.Modules.MapEditor
{
    [CommandCarrier(Name ="Map",ItemMode = LoadMode.None)]
    public static class MapManager
    {
        public const string NewMapDefaultName = "NewMap";
        public readonly static List<Map> LoadedMaps = [];
        public delegate void LoadedMapsChangedHandler(CollectionChangeAction action, Map? map, bool fromFile);
        public static event LoadedMapsChangedHandler? LoadedMapsChanged; 

        private static bool Exist(Map map)
        {
            bool nameEqual = LoadedMaps.Any(m => m.GetIdentifiedPath() == map.GetIdentifiedPath());
            return nameEqual;
        }
        [Command]
        public static void Save()
        {
            foreach (var map in LoadedMaps)
            {
                DataHandler.Write(map);
            }
        }
        [Command]
        public static void ExportPNG(string mapIdName, string targetPath)
        {
            var map = LoadedMaps.Find(m => m.GetIdentifiedPath() == mapIdName);
            if(map is not null)
            {
                targetPath += $"\\{map.Name}";
                if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

                Perlin2D p2d = new(map.Seed, [.. map.BasicNoiseCollection], map.ImageSize);
                Matrix<double> matrix = p2d.RegeneratePerlinNoiseMatrix();

                /*var m0 = BitmapHelper.NewWriteableBitMap(map.ImageSize);
                new PixelPainter(m0, matrix) { ColorMatrix = PixelPainter.GrayPaintStyle }.Draw(m0.GetImageFullRect(), true);
                var bm0 = m0.CreateBitmap();
                bm0.Save($"{targetPath}\\Gray.Png");
                new ContourPainter(m0, matrix) { ClearWhenGraphicsDraw = false }.Draw(m0.GetImageFullRect(), true);
                bm0 = m0 .CreateBitmap();
                bm0.Save($"{targetPath}\\GrayContour.Png");

                var m1 = BitmapHelper.NewWriteableBitMap(map.ImageSize);
                PixelPainter paint = new(m1, matrix) { FillColors = map.FillColors };
                paint.ColorMatrix = paint.GradientPaintStyle;
                paint.Draw(m1.GetImageFullRect(), true);
                var bm1 = m1.CreateBitmap();
                bm1.Save($"{targetPath}\\Colored.Png");
                new ContourPainter(m1, matrix) { ClearWhenGraphicsDraw = false }.Draw(m1.GetImageFullRect(), true);
                bm1 = m1.CreateBitmap();
                bm1.Save($"{targetPath}\\ColoredContour.Png");*/
            }
        }
        [Command]
        public static void Load(string? directory)
        {
            if (Directory.Exists(directory))
            {
                int count = 0;
                FileInfo[] fileInfos = new DirectoryInfo(directory).GetFiles();
                var mapfis = fileInfos.Where(fi =>
                {
                    return fi.Extension.Equals($".{nameof(Map)}", StringComparison.CurrentCultureIgnoreCase);
                });
                string[] names = [.. mapfis.Select(fi => fi.Name.Replace(fi.Extension, Const.EmptyStr))];
                foreach ( string name in names )
                {
                    var fid = new FileDetail($"{directory}\\{name}");
                    var result = DataHandler.Read<Map>(fileDetail: fid);
                    //out Map map, name, directory, Serialization.Xml, nameof(Map), false
                    if(result.Success && result.Data is not null)
                    {
                        var map = result.Data;
                        if (!LoadedMaps.Any(lm => lm.GetIdentifiedPath() == map.GetIdentifiedPath()))
                        {
                            LoadedMaps.Add(map);
                            LoadedMapsChanged?.Invoke(CollectionChangeAction.Add, map, true);
                            count++;
                        }
                    }
                }
                ConsoleManager.ListInfo(new($"{count} new maps has been loaded"));
            }
        }
        [Command]
        public static void CreateMap(string? name)
        {
            Map map = new(name ?? $"{NewMapDefaultName}{LoadedMaps.Count + 1}");
            if (Exist(map)) map.Name += "_Copy";
            map.FillColors.Add(new(Color.Black, 0));
            map.FillColors.Add(new(Color.White, 1));
            map.BasicNoiseCollection.Add(new(1, 1));
            LoadedMaps.Add(map);
            LoadedMapsChanged?.Invoke(CollectionChangeAction.Add, map, false);
        }
        [Command]
        public static void DeleteMap(string name, string directory)
        {            
            bool hasFile = FileHelper.ExistFile(directory, name, nameof(Map), true);
            if (hasFile)
            {
                FileInfo fileInfo = new DirectoryInfo(directory).GetFiles().Where(fi => fi.Name.Replace(fi.Extension, Const.EmptyStr) == name && fi.Extension.Equals(nameof(Map),StringComparison.CurrentCultureIgnoreCase)).First();
                fileInfo.Delete();
            }
            var maps = LoadedMaps.Where(m => m.GetIdentifiedPath() == $"{directory}\\{name}");
            foreach (var map in maps)
            {
                LoadedMaps.Remove(map);
                LoadedMapsChanged?.Invoke(CollectionChangeAction.Remove, map, false);
            }
        }
    }
}
