namespace ElShrine.Old
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public readonly record struct Coordinate2D(double X, double Y) : IComparable<Coordinate2D>
    {
        public double Distance => Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        public readonly static Coordinate2D Base = new(0, 0);
        public Coordinate2D Offset(Coordinate2D cor)
            => new(X + cor.X, Y + cor.Y);
        public Coordinate2D Factor(double factor)
            => new(X * factor, Y * factor);
        public override string ToString() => $"({X},{Y})[{Distance}]";
        public int CompareTo(Coordinate2D other)
            => (X * Y).CompareTo(other.X * other.Y);
        public static Coordinate2D Max(params Coordinate2D[] cors)
        {
            Coordinate2D result = Base;
            foreach (Coordinate2D cor in cors) if (cor.CompareTo(result) == 1) result = cor;
            return result;
        }
        public (double x, double y) ToTuple() => (X, Y);

        public Coordinate2D Absoluted => new(Math.Abs(X), Math.Abs(Y));
        public static Coordinate2D operator +(Coordinate2D cor1, Coordinate2D cor2)
            => new(cor1.X + cor2.X, cor1.Y + cor2.Y);
        public static Coordinate2D operator -(Coordinate2D cor1, Coordinate2D cor2)
            => new(cor1.X - cor2.X, cor1.Y - cor2.Y);
        public static Coordinate2D operator /(Coordinate2D cor1, Coordinate2D cor2)
            => new(cor1.X / cor2.X, cor1.Y / cor2.Y);
        public static Coordinate2D operator *(Coordinate2D cor1, Coordinate2D cor2)
            => new(cor1.X * cor2.X, cor1.Y * cor2.Y);
        public static bool operator <(Coordinate2D cor1, Coordinate2D cor2)
            => cor1.CompareTo(cor2) < 0;
        public static bool operator >(Coordinate2D cor1, Coordinate2D cor2)
            => cor1.CompareTo(cor2) > 0;
    }
}
