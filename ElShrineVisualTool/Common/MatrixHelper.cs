using MathNet.Numerics.LinearAlgebra;

namespace ElShrine.Common
{
    public static class MatrixHelper
    {
        public static void GetLimit(this Matrix<double> matrix, out double max, out double min)
        {
            max = matrix.Enumerate().Max();
            min = matrix.Enumerate().Min();
        }
        public static void Normalize(ref Matrix<double> martix)
        {
            martix.GetLimit(out double max, out double min);
            martix = (max == min) ? martix : (martix - min) / (max - min);
        }
        public static Matrix<double> Normalize(this Matrix<double> martix)
        {
            martix.GetLimit(out double max, out double min);
            return (max == min) ? martix : (martix - min) / (max - min); ;
        }
    }
}
