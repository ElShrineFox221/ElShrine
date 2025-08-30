using System.Drawing;


namespace ElShrine.Common.Input
{
    public record class MouseStatus(bool LeftDown, bool RightDown, bool MiddleDown, Point CusorLoc)
    {
    }
}
