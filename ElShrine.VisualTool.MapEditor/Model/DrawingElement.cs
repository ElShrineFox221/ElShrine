using ElShrine.VisualTool.MapEditor.ViewModel;
using PointD = System.Windows.Point;

namespace ElShrine.VisualTool.MapEditor.Model
{
    public record class DrawingElement(DrawingActionType Action, PointD[] Points);
}
