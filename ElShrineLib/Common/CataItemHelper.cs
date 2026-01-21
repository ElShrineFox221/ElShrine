using ElShrine.Common.DataStructure;

namespace ElShrine.Common
{
    public static class CataItemHelper
    {
        public static string ToCataName(this ICataItem item, bool showActualName = false)
            => showActualName ? $"{item.VirtualCataName}({item.ActualCataName})" : item.VirtualCataName;
        public static string ToItemName(this ICataItem item, bool showActualName = false)
            => (!showActualName || item.VirtualCataName == item.ActualCataName) ? item.VirtualCataName : $"{item.VirtualItemName}({item.ActualItemName})";
        public static string ToFullName(this ICataItem item)
            => $"{item.ActualCataName}.{item.ActualItemName}";
        public static string ToSearchName(this ICataItem item, bool showActualName = false)
            => showActualName ?
            $"{item.VirtualCataName}.{item.VirtualItemName}({item.ToFullName()})" :
            $"{item.VirtualCataName}.{item.VirtualItemName}";
    }
}
