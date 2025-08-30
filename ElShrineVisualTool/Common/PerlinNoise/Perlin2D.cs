using ElShrine.Modules.MapEditor.Model;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using Vector = System.Windows.Vector;

namespace ElShrine.Common.PerlinNoise
{
    public sealed class Perlin2D(int? seed, Rank[] ranks, Size noiseMatrixSize)
    {
        #region Member
        public const int DefaultSeed = 22181;
        public readonly int Seed = seed ?? DefaultSeed;
        public readonly Rank[] Ranks = ranks;
        public Size NoiseMatrixSize = noiseMatrixSize;
        #endregion

        public readonly List<Matrix<double>> NoiseMatrixes = [];
        private Matrix<double>? mergedNoiseMatrix = null;
        public Matrix<double> MergedNoiseMatrix
        {
            get
            {
                mergedNoiseMatrix ??= RegeneratePerlinNoiseMatrix();
                return mergedNoiseMatrix;
            }
        }

        private static Vector[,] GenerateGradMatrix(Rank rank, int seed)
        {
            Random rand = new(seed); 
            int r = rank.Frequency + 1;
            Vector[,] gradVecs = new Vector[r, r];
            for (int x = 0; x < r; x++)
            {
                for (int y = 0; y < r; y++)
                {
                    gradVecs[y, x] = getRandomGradVec(rand);
                }
            }
            return gradVecs;

            static Vector getRandomGradVec(Random rand)
            {
                Vector vec = new(doubleScale(rand.NextDouble()), doubleScale(rand.NextDouble()));
                vec.Normalize();
                return vec;
                static double doubleScale(double d) => 2d * (d - 0.5d);
            }
        }
        private static double GetPerlin(double x, double y, ref Vector[,] vectors)
        {
            //x, y => center
            //
            int x0 = (int)Math.Floor(x),
                y0 = (int)Math.Floor(y),
                x1 = x0 + 1,
                y1 = y0 + 1;
            double deltaX0 = x - x0,
                deltaY0 = y - y0,
                deltaX1 = x - x1,
                deltaY1 = y - y1;
            Vector
                v1d = new(deltaX0, deltaY0),
                v2d = new(deltaX1, deltaY0),
                v3d = new(deltaX1, deltaY1),
                v4d = new(deltaX0, deltaY1);
            Vector
                v1g = vectors[x0, y0],
                v2g = vectors[x1, y0],
                v3g = vectors[x1, y1],
                v4g = vectors[x0, y1];
            double
                dot1 = v1d * v1g,
                dot2 = v2d * v2g,
                dot3 = v3d * v3g,
                dot4 = v4d * v4g;
            double
                lerp12 = lerp(dot1, dot2, x - x0),
                lerp34 = lerp(dot4, dot3, x - x0),
                lerpResult = lerp(lerp12, lerp34, y - y0);
            return lerpResult;
            static double lerp(double d0, double d1, double rate)
                //=> (d1 - d0) * rate + d0;
                => CommonHelper.SinEaseLerp(d0, d1, rate);
        }
        public Matrix<double> RegeneratePerlinNoiseMatrix()
        {
            Random subSeedSource = new(Seed);
            int w = NoiseMatrixSize.Width, h = NoiseMatrixSize.Height;
            Vector[,] gradMatrix;
            double cellWidth = NoiseMatrixSize.Width;
            NoiseMatrixes.Clear();
            mergedNoiseMatrix = null;
            foreach (var rank in Ranks)
            {
                int subSeed = subSeedSource.Next(int.MaxValue);
                cellWidth = Math.Max(w, h) / (double)rank.Frequency;
                gradMatrix = GenerateGradMatrix(rank, subSeed);
                Matrix<double> noiseMartix = rank.Amplitude * Matrix<double>.Build.Dense(h, w, noiseMartixFunc);
                NoiseMatrixes.Add(noiseMartix);
                if (mergedNoiseMatrix is null) mergedNoiseMatrix = noiseMartix;
                else mergedNoiseMatrix += noiseMartix;
            }
            mergedNoiseMatrix = mergedNoiseMatrix?.Normalize();
            return mergedNoiseMatrix ?? Matrix<double>.Build.Dense(1, 1, 0);
            double noiseMartixFunc(int y, int x) => GetPerlin(x / cellWidth, y / cellWidth, ref gradMatrix);
        }

        public class Perlin2DNoise(Matrix<double> matrix)
        {
            public double this[double xRate, double yRate]
            {
                get
                {
                    double x = xRate * _size.Width, y = yRate * _size.Height;
                    int intX = (int)Math.Floor(x), intY = (int)Math.Floor(y),
                        intX1 = intX + 1, intY1 = intY + 1;
                    double d0 = CommonHelper.SinEaseLerp(_matrix[intY, intX], _matrix[intY, intX1], x - intX);
                    double d1 = CommonHelper.SinEaseLerp(_matrix[intY1, intX], _matrix[intY1, intX1], x - intX);
                    double d = CommonHelper.SinEaseLerp(d0, d1, y - intY);
                    return d;
                }
            }
            private readonly Matrix<double> _matrix = matrix;
            private readonly Size _size = new(matrix.ColumnCount, matrix.RowCount);
        }
    }
}
