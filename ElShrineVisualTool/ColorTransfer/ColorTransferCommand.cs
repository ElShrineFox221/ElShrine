using ElShrine.ColorTransfer.Model;
using ElShrine.ECommand;
using System.Drawing;
using System.IO;

namespace ElShrine.ColorTransfer
{
    [CommandCarrier(Name = "CT")]
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
