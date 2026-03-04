using ElShrine.Modules;
using ElShrine.VisualTool.ColorTransfer.Model;
using System.Drawing;
using System.IO;

namespace ElShrine.VisualTool.ColorTransfer
{
    [CommandCarrier(OverrideName = "CT")]
    public static class ColorTransferCommand
    {
        public static void Trans(int tolerence)
        {
            string[] filepaths = Directory.GetFiles(Environment.CurrentDirectory);
            foreach (string filepath in filepaths)
            {
                FileInfo fileInfo = new(filepath);
                if (fileInfo.Extension.Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                {
                    Image image = Image.FromFile(filepath);
                    ColorGreyTransfer colorGreyTransfer = new(new Bitmap(image));
                    colorGreyTransfer.Transfer(tolerence);
                    
                    colorGreyTransfer.Save(filepath+".png");
                }
            }
        }
    }
}
