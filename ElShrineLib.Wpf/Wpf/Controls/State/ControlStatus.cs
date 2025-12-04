using System;

namespace ElShrine.Wpf.Controls.State
{
    /// <summary>
    /// 定义控件的各种活动状态。
    /// 位值越高的状态，其优先级越高（如 Disabled > MouseDown）。
    /// </summary>
    [Flags]
    public enum ControlStatus
    {
        Normal = 0,
        Default = Normal,
        
        MIn = 1,
        MouseIn = MIn,

        Focused = 2,

        Checked = 4,
        Selected = Checked,

        MDown = 8,
        MouseDown = MDown,

        Disabled = 16,
        Disable = Disabled
    }
}
