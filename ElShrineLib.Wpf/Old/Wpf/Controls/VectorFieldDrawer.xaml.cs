using ElShrine.Wpf;
using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static ElShrine.Wpf.Methods;

namespace ElShrine.Old.Wpf.Controls
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct Coordinate2D(double X, double Y)
    {
        public double Distance => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        public readonly static Coordinate2D Base = new(0, 0);
        public Coordinate2D Offset(Coordinate2D cor)
            => new(X + cor.X, Y + cor.Y);
        public Coordinate2D Factor(double factor)
            => new(X * factor, Y * factor);
        public Complex[] ToComplex() => [X, Y,];
        public Point ToPoint() => new(X, Y);
        public override string ToString() => $"({X},{Y})[{Distance}]";
    };
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct Coordinate3D(double X, double Y, double Z)
    {
        public double Distance => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        public readonly static Coordinate3D Base = new(0, 0, 0);
        public Coordinate3D Offset(Coordinate3D cor)
            => new(X + cor.X, Y + cor.Y, Z + cor.Z);
        public Coordinate3D Factor(double factor)
            => new(X * factor, Y * factor, Z * factor);
        public Complex[] ToComplex() => [X, Y, Z,];
        public override string ToString() => $"({X},{Y},{Z})[{Distance}]";
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public class ElShrineVectorFieldDrawer : ContentControl
    {
        public ElShrineVectorFieldDrawer()
        {
            DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100), };
            timer.Tick += delegate
            {
                if (ObserverVetor != Get3DObserveVector()) Redraw3D();
            };
            timer.Start();
        }

        public Func<Coordinate2D, Coordinate2D>? Vetor2DField = null;
        public Coordinate2D Vetor2DFieldFunc(Coordinate2D coordinate)
            => Vetor2DField?.Invoke(coordinate) ?? Coordinate2D.Base;
        public Func<Coordinate3D, Coordinate3D>? Vetor3DField = null;
        public Coordinate3D Vetor3DFieldFunc(Coordinate3D coordinate)
            => Vetor3DField?.Invoke(coordinate) ?? Coordinate3D.Base;

        public int MinimumSampleCount { get; set; } = 10;
        public double ArrowLengthRate { get; set; } = 0.6;
        public double Scale { get; set; } = 1;
        public Coordinate2D BaseCor { get; set; } = new(0.5, 0.5);

        public double ActualScale => Scale * Math.Min(ActualHeight, ActualWidth);
        public double ArrowLength2D => Sample2DSpace * ArrowLengthRate;
        public (int Horizional, int Vertical) Samples2D
            => ((int)(ActualWidth / Sample2DSpace) - 1, (int)(ActualHeight / Sample2DSpace) - 1);
        public int Sample2DCount
            => Samples2D.Vertical * Samples2D.Horizional;
        public double Sample2DSpace
            => Math.Min(ActualHeight, ActualWidth) / (MinimumSampleCount + 1);
        public Coordinate2D Base2DActualCor { get; set; } = new(0, 0);
        public Coordinate2D Field2DWidth
            => new(
                   ActualWidth / Math.Min(ActualHeight, ActualWidth) * Scale,
                   ActualHeight / Math.Min(ActualHeight, ActualWidth) * Scale
                   );
        public Coordinate2D Field2DBottom
            => new(-Field2DWidth.X * BaseCor.X + Base2DActualCor.X, -Field2DWidth.Y * BaseCor.Y + Base2DActualCor.Y);

        public int MaximumSample3DCount => Math.Max(Math.Max(Samples3D.X, Samples3D.Y), Samples3D.Z);
        public Coordinate3D ObserverVetor = new(1, 1, 1);
        public double Sample3DSpace
            => Scale / MaximumSample3DCount;
        public (int X, int Y, int Z) Samples3D { get; set; } = new(10, 10, 10);
        public int Sample3DCount
            => Samples3D.X * Samples3D.Y * Samples3D.Z;
        public Coordinate3D Base3DActualCor { get; set; } = new(0, 0, 0);
        public Coordinate3D Field3DWidth
            => new(
                   Scale,
                   Scale,
                   Scale
                   );
        public Coordinate3D Field3DBottom
            => new(-Field3DWidth.X * 0.5 + Base3DActualCor.X, -Field3DWidth.Y * 0.5 + Base3DActualCor.Y, -Field3DWidth.Z * 0.5 + Base3DActualCor.Z);



        public double SitaAngle
        {
            get { return (double)GetValue(SitaAngleProperty); }
            set { SetValue(SitaAngleProperty, value); Redraw3D(); }
        }
        public static readonly DependencyProperty SitaAngleProperty =
            DependencyProperty.Register("SitaAngle", typeof(double), typeof(ElShrineVectorFieldDrawer), new PropertyMetadata(Math.PI / 4d));


        public double PaiAngle
        {
            get { return (double)GetValue(PaiAngleProperty); }
            set { SetValue(PaiAngleProperty, value); Redraw3D(); }
        }
        public static readonly DependencyProperty PaiAngleProperty =
            DependencyProperty.Register("PaiAngle", typeof(double), typeof(ElShrineVectorFieldDrawer), new PropertyMetadata(Math.PI / 4d));
        public Coordinate3D Get3DObserveVector()
        {
            double z = Math.Sin(PaiAngle);
            double y = Math.Sin(SitaAngle) * Math.Abs(Math.Cos(PaiAngle));
            double x = Math.Cos(SitaAngle) * Math.Abs(Math.Cos(PaiAngle));

            Coordinate3D ov = new(x, y, z);
            return ov;
        }
        private void Redraw3D()
        {
            if (Object3D is not null && Vetor3DField is not null)
            {
                Coordinate3D ov = Get3DObserveVector();
                Draw3D(Object3D, ObjectName, Vetor3DField, ObserverVetor = ov);
            }
        }
        public object? Object3D = null;
        public string ObjectName = string.Empty;

        public static void Draw2D(object sender, string name, Func<Coordinate2D, Coordinate2D> Vetor2DField, Coordinate2D? BaseActualCor = null, Coordinate2D? BaseCorOffsetRate = null,
            double Scale = 1, int MinimumSampleCount = 10, double ArrowLengthRate = 0.6, double ColorParam = 1)
        {
            #region Step 1 - Confrim Elements
            ElShrineVectorFieldDrawer? elShrineVectorFieldDrawer = (ElShrineVectorFieldDrawer?)((FrameworkElement)sender).FindChild(name);

            Canvas? canvas = (Canvas?)elShrineVectorFieldDrawer?.FindChild("VectorFieldDrawerContainer_Canvas");
            if (canvas is null) return;
            ElShrineVectorFieldDrawer VF = elShrineVectorFieldDrawer ?? new();
            #endregion

            #region Step 2 - Confrim Child Elements
            VF.Vetor2DField = Vetor2DField;
            canvas.Children.Clear();
            #endregion

            #region Step 3 - Draw Coordinates
            VF.Scale = Scale;
            VF.MinimumSampleCount = MinimumSampleCount;
            VF.ArrowLengthRate = ArrowLengthRate;
            VF.Base2DActualCor = BaseActualCor is not null ? BaseActualCor.Value : VF.Base2DActualCor;
            VF.BaseCor = BaseCorOffsetRate is not null ? BaseCorOffsetRate.Value : VF.BaseCor;
            DrawLine(
                canvas,
                new(0, VF.BaseCor.Y * VF.ActualHeight),
                new(VF.ActualWidth, VF.BaseCor.Y * VF.ActualHeight)
                );//x
            DrawLine(
                canvas,
                new(VF.BaseCor.X * VF.ActualWidth, 0),
                new(VF.BaseCor.X * VF.ActualWidth, VF.ActualHeight)
                );//y
            #endregion

            #region Step 4 - Draw Vectors
            for (int v = 0; v < VF.Samples2D.Vertical; v++)
            {
                for (int h = 0; h < VF.Samples2D.Horizional; h++)
                {
                    Coordinate2D fieldInput = new(h / (double)VF.Samples2D.Horizional, v / (double)VF.Samples2D.Vertical);
                    (Coordinate2D cor1, Coordinate2D cor2, double length) = GetLinePoints(GetActualLinePoints(fieldInput));
                    if (double.IsNaN(cor2.Distance)) continue;
                    DrawLine(canvas, cor1, cor2, length, ColorParam);

                }
            }
            #endregion

            (Coordinate2D inCor, Coordinate2D vectorCor) GetActualLinePoints(Coordinate2D input)
            {
                Coordinate2D inCor = new(
                    input.X * VF.Field2DWidth.X + VF.Field2DBottom.X,
                    input.Y * VF.Field2DWidth.Y + VF.Field2DBottom.Y);
                Coordinate2D vectorCor = VF.Vetor2DFieldFunc(inCor);
                return (inCor, vectorCor);
            }
            (Coordinate2D cor1, Coordinate2D cor2, double length) GetLinePoints((Coordinate2D inCor, Coordinate2D vectorCor) actualCor)
            {
                double length = actualCor.vectorCor.Distance;
                Coordinate2D cor1 = new(
                    (VF.BaseCor.X + (actualCor.inCor.X - VF.Base2DActualCor.X) / VF.Field2DWidth.X) * VF.ActualWidth,
                    (VF.BaseCor.Y + (actualCor.inCor.Y - VF.Base2DActualCor.Y) / VF.Field2DWidth.Y) * VF.ActualHeight
                    );
                Coordinate2D cor2Delta = new(
                    actualCor.vectorCor.X / actualCor.vectorCor.Distance * VF.ArrowLength2D,
                    actualCor.vectorCor.Y / actualCor.vectorCor.Distance * VF.ArrowLength2D
                    );
                Coordinate2D cor2 = new(
                    cor1.X + cor2Delta.X,
                    cor1.Y + cor2Delta.Y
                    );
                return (cor1, cor2, length);
            }
        }
        public static void Draw3D(object sender, string name, Func<Coordinate3D, Coordinate3D> Vetor3DField,
            Coordinate3D? ObserverVector = null,
            Coordinate3D? BaseActualCor = null,
            double Scale = 1, int MinimumSampleCount = 10, double ArrowLengthRate = 0.6, double ColorParam = 1,
            double ViewScale = 300)
        {
            #region Step 1 - Confrim Elements
            ElShrineVectorFieldDrawer? elShrineVectorFieldDrawer = (ElShrineVectorFieldDrawer?)((FrameworkElement)sender).FindChild(name);

            Canvas? canvas = (Canvas?)elShrineVectorFieldDrawer?.FindChild("VectorFieldDrawerContainer_Canvas");
            if (canvas is null) return;
            ElShrineVectorFieldDrawer VF = elShrineVectorFieldDrawer ?? new();
            #endregion

            #region Step 2 - Confrim Child Elements
            VF.ObserverVetor = ObserverVector ?? VF.Get3DObserveVector();
            VF.Scale = Scale;
            VF.MinimumSampleCount = MinimumSampleCount;
            VF.ArrowLengthRate = ArrowLengthRate;
            VF.Base3DActualCor = BaseActualCor is not null ? BaseActualCor.Value : VF.Base3DActualCor;
            VF.Vetor3DField = Vetor3DField;
            canvas.Children.Clear();
            VF.Object3D = sender;
            VF.ObjectName = name;
            #endregion
            double factor = Math.Min(VF.ActualHeight, VF.ActualWidth) / VF.Scale;
            #region Step 3 - Draw Coordinates
            //ingnore yz
            VF.Base3DActualCor = BaseActualCor is not null ? BaseActualCor.Value : VF.Base3DActualCor;
            Coordinate2D baseCor2D = new(
                0.5 * VF.ActualWidth + (VF.BaseCor.X - 0.5) * Math.Min(VF.ActualWidth, VF.ActualHeight),
                0.5 * VF.ActualHeight + (VF.BaseCor.Y - 0.5) * Math.Min(VF.ActualWidth, VF.ActualHeight)
                );
            Coordinate2D corx = GetProjectiveResult(new(1, 0, 0), VF.ObserverVetor, ViewScale, VF.Base3DActualCor).Factor(5).Offset(baseCor2D);
            Coordinate2D cory = GetProjectiveResult(new(0, 1, 0), VF.ObserverVetor, ViewScale, VF.Base3DActualCor).Factor(5).Offset(baseCor2D);
            Coordinate2D corz = GetProjectiveResult(new(0, 0, 1), VF.ObserverVetor, ViewScale, VF.Base3DActualCor).Factor(5).Offset(baseCor2D);
            DrawLine(canvas, baseCor2D, corx, drawindex: 0);//x
            DrawLine(canvas, baseCor2D, cory, drawindex: 1);//y
            DrawLine(canvas, baseCor2D, corz, drawindex: 2);//z
            #endregion

            #region Step 4 - Draw Vectors
            for (int x = 0; x < VF.Samples3D.X; x++)
            {
                for (int y = 0; y < VF.Samples3D.Y; y++)
                {
                    for (int z = 0; z < VF.Samples3D.Z; z++)
                    {
                        Coordinate3D fieldInput = new(x / (double)VF.Samples3D.X, y / (double)VF.Samples3D.Y, z / (double)VF.Samples3D.Z);
                        (Coordinate2D cor1, Coordinate2D cor2, double length) = GetLinePoints(GetActualLinePoints(fieldInput), viewScale: ViewScale);
                        if (double.IsNaN(cor2.Distance)) continue;
                        DrawLine(canvas, cor1, cor2, length, ColorParam, x * 100 + y * 10 + z + 3);
                    }
                }
            }
            #endregion

            (Coordinate3D inCor, Coordinate3D vectorCor) GetActualLinePoints(Coordinate3D input)
            {
                Coordinate3D inCor = new(
                    input.X * VF.Field3DWidth.X + VF.Field3DBottom.X,
                    input.Y * VF.Field3DWidth.Y + VF.Field3DBottom.Y,
                    input.Z * VF.Field3DWidth.Z + VF.Field3DBottom.Z);
                Coordinate3D vectorCor = VF.Vetor3DFieldFunc(inCor);
                return (inCor, vectorCor);
            }
            (Coordinate2D cor1, Coordinate2D cor2, double length) GetLinePoints((Coordinate3D inCor, Coordinate3D vectorCor) actualCor, double viewScale = 300)
            {
                double length = actualCor.vectorCor.Distance;
                Coordinate3D cor3d2 = new(
                    actualCor.inCor.X + actualCor.vectorCor.X / actualCor.vectorCor.Distance * VF.Sample3DSpace - VF.Base3DActualCor.X,
                    actualCor.inCor.Y + actualCor.vectorCor.Y / actualCor.vectorCor.Distance * VF.Sample3DSpace - VF.Base3DActualCor.Y,
                    actualCor.inCor.Z + actualCor.vectorCor.Z / actualCor.vectorCor.Distance * VF.Sample3DSpace - VF.Base3DActualCor.Z
                    );
                Coordinate3D cor3d1 = new(
                    actualCor.inCor.X - VF.Base3DActualCor.X,
                    actualCor.inCor.Y - VF.Base3DActualCor.Y,
                    actualCor.inCor.Z - VF.Base3DActualCor.Z
                    );
                Coordinate2D cor2 = GetProjectiveResult(cor3d2, VF.ObserverVetor, viewScale);
                Coordinate2D cor1 = GetProjectiveResult(cor3d1, VF.ObserverVetor, viewScale);
                return (cor1.Offset(baseCor2D), cor2.Offset(baseCor2D), length);
            }

        }
        private static void DrawLine(Canvas? canvas, Coordinate2D cor1, Coordinate2D cor2, double? length = null, double ColorParam = 1, int drawindex = -1)
        {
            if (canvas is null) return;
            if (drawindex == -1 || canvas.Children.Count <= drawindex)
            {
                Line line = new()
                {
                    X1 = cor1.X,
                    Y1 = canvas.ActualHeight - cor1.Y,
                    X2 = cor2.X,
                    Y2 = canvas.ActualHeight - cor2.Y,
                    Stroke = new SolidColorBrush(ColorFromLength(length ?? new Coordinate2D(cor1.X - cor2.X, cor1.Y - cor2.Y).Distance, ColorParam)),
                    StrokeThickness = 2,
                    Visibility = Visibility.Visible,
                    Opacity = 1,
                };
                canvas.Children.Add(line);
            }
            else
            {
                Line line = new()
                {
                    X1 = cor1.X,
                    Y1 = canvas.ActualHeight - cor1.Y,
                    X2 = cor2.X,
                    Y2 = canvas.ActualHeight - cor2.Y,
                    Stroke = new SolidColorBrush(ColorFromLength(length ?? new Coordinate2D(cor1.X - cor2.X, cor1.Y - cor2.Y).Distance, ColorParam)),
                    StrokeThickness = 2,
                    Visibility = Visibility.Visible,
                    Opacity = 1,
                };
                canvas.Children.Add(line);
                /*Line line = (Line)canvas.Children[drawindex];
                line.BeginAnimation(Line.X1Property, new DoubleAnimation() { To = cor1.X, Duration = new(new(0)) }, HandoffBehavior.SnapshotAndReplace);
                line.BeginAnimation(Line.Y1Property, new DoubleAnimation() { To = canvas.ActualHeight - cor1.Y, Duration = new(new(0)) }, HandoffBehavior.SnapshotAndReplace);
                line.BeginAnimation(Line.X2Property, new DoubleAnimation() { To = cor2.X, Duration = new(new(0)) }, HandoffBehavior.SnapshotAndReplace);
                line.BeginAnimation(Line.Y2Property, new DoubleAnimation() { To = canvas.ActualHeight - cor2.Y, Duration = new(new(0)) }, HandoffBehavior.SnapshotAndReplace);*/
            }


        }

        public static Coordinate2D GetProjectiveResult(Coordinate3D coordinate, Coordinate3D observeVector, double viewScale = 10, Coordinate3D? cor = null)
        {
            double A = observeVector.X, B = observeVector.Y, C = observeVector.Z;
            coordinate = coordinate with { X = coordinate.X * viewScale, Y = coordinate.Y * viewScale, Z = coordinate.Z * viewScale, };
            //if (cor is not null) coordinate.Offset(((Coordinate3D)cor).Factor(1 - viewScale));

            Coordinate3D ObserveVector = new(A, B, C);
            Coordinate3D planeX = new(-B, A, 0), planeY = new(-A * C, -B * C, Math.Pow(A, 2) + Math.Pow(B, 2));
            Coordinate3D
                PlaneX = new(planeX.X / planeX.Distance, planeX.Y / planeX.Distance, 0),
                PlaneY = new(planeY.X / planeY.Distance, planeY.Y / planeY.Distance, planeY.Z / planeY.Distance);
            var matrix = DenseMatrix.OfArray(new Complex[,] {
                    { PlaneX.X,PlaneY.X,ObserveVector.X },
                    { PlaneX.Y,PlaneY.Y,ObserveVector.Y },
                    { PlaneX.Z,PlaneY.Z,ObserveVector.Z },
                });
            var vector = new DenseVector(coordinate.ToComplex());
            var result = matrix.LU().Solve(vector);
            return new(result[0].Real, result[1].Real);
        }
        private readonly static Color Red = System.Drawing.Color.Red.ToMediaColor();
        private readonly static Color Blue = System.Drawing.Color.Blue.ToMediaColor();
        public static Color ColorFromLength(double length, double param = 1)
            => ColorFromLength(Blue, Red, length, param);
        public static Color ColorFromLength(Color color, Color targetColor, double length, double param = 1)
        {
            double rate = length / (length + param);
            return ColorFactor(color, targetColor, rate);
        }
    }
}
