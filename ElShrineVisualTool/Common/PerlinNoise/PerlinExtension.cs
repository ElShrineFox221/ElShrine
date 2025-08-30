using ElShrine.Modules.MapEditor.Model;
using System.Drawing;

namespace ElShrine.Common.PerlinNoise
{
    public static class PerlinExtension
    {
        public static Perlin2D GetPerlin2D(this IEnumerable<Rank> ranks, Size size, int? seed = null)
        {
            var p2d = new Perlin2D(seed, [.. ranks], size);
            p2d.RegeneratePerlinNoiseMatrix();
            return p2d;
        }
    }
}
