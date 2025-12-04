using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

namespace ElShrine.Wpf
{
    public abstract class EWindowViewModelBase(Window ownerWindow) : ViewModelBase
    {
        #region Static | Windows Manager
        protected static readonly List<Window> ownerWindows = [];
        public static ReadOnlyCollection<Window> OwnerWindows => ownerWindows.AsReadOnly();
        public static TWindowVM SetWindowViewModel<TWindowVM>(Window ownerWindow) where TWindowVM : EWindowViewModelBase
        {
            #region ViewModel Instance Building
            TWindowVM viewmodel = (TWindowVM?)Activator.CreateInstance(typeof(TWindowVM), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, [ownerWindow], null) ?? throw new("Cannot build a instance of the viewmodel.");
            ownerWindow.DataContext = viewmodel;
            ownerWindows.Add(ownerWindow);
            #endregion
            return viewmodel;
        }
        #endregion

        public Window OwnerWindow { get; protected set; } = ownerWindow;
    }
}
