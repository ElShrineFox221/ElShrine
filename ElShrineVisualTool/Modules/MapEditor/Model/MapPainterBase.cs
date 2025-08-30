using MathNet.Numerics.LinearAlgebra;
using System.Drawing;

namespace ElShrine.Modules.MapEditor.Model
{
    public abstract class MapPainterBase(Bitmap image, Matrix<double> altitudeMatrix) : BitmapPainterBase(image)
    {
        public readonly Matrix<double> AltitudeMatrix = altitudeMatrix;
    }
}
