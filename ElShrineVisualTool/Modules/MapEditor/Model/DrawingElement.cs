using ElShrine.Modules.MapEditor.ViewModel;
using PointD = System.Windows.Point;

namespace ElShrine.Modules.MapEditor.Model
{
    public record class DrawingElement(DrawingActionType Action, PointD[] Points);
}
