using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class EPopup : Popup
    {
        #region DPs
        public FrameworkElement MouseDownOpenSource
        {
            get => (FrameworkElement)GetValue(MouseDownOpenSourceProperty);
            set => SetValue(MouseDownOpenSourceProperty, value);
        }
        public bool InterceptOpenMouse
        {
            get => (bool)GetValue(InterceptOpenMouseDownProperty);
            set => SetValue(InterceptOpenMouseDownProperty, value);
        }
        public bool InterceptCloseMouse
        {
            get => (bool)GetValue(InterceptCloseMouseDownProperty);
            set => SetValue(InterceptCloseMouseDownProperty, value);
        }

        public static readonly DependencyProperty MouseDownOpenSourceProperty = DependencyProperty.Register(
            nameof(MouseDownOpenSource),
            typeof(FrameworkElement),
            typeof(EPopup),
            new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: MouseDownOpenSourceChangedCallback));
        public static readonly DependencyProperty InterceptOpenMouseDownProperty = DependencyProperty.Register(
            nameof(InterceptOpenMouse),
            typeof(bool),
            typeof(EPopup),
            new FrameworkPropertyMetadata(defaultValue: true));
        public static readonly DependencyProperty InterceptCloseMouseDownProperty = DependencyProperty.Register(
            nameof(InterceptCloseMouse),
            typeof(bool),
            typeof(EPopup),
            new FrameworkPropertyMetadata(defaultValue: true));
        #endregion

        #region Implements
        static EPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EPopup), new FrameworkPropertyMetadata(typeof(EPopup)));
            IsOpenProperty.OverrideMetadata(typeof(EPopup), new FrameworkPropertyMetadata(defaultValue: false, coerceValueCallback: (s, e) =>
            {
                if(s is not EPopup popup) return e;
                if(popup.cached_preventsClose && e is false)
                {
                    popup.cached_preventsClose = false;
                    return true;
                }
                return e;
            }, propertyChangedCallback: (s, e) =>
            {

            }));
        }
        #endregion

        private static void MouseDownOpenSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not EPopup popup) return;
            var newValue = e.NewValue as FrameworkElement;
            var oldValue = e.OldValue as FrameworkElement;
            if (newValue == oldValue) return;
            if (oldValue is not null)
            {
                oldValue.PreviewMouseDown -= popup.MouseSourceCallback_PreDown;
                oldValue.PreviewMouseUp -= popup.MouseSourceCallback_PreUpClose;
            }
            if(newValue is not null)
            {
                newValue.PreviewMouseDown += popup.MouseSourceCallback_PreDown;
                newValue.PreviewMouseUp += popup.MouseSourceCallback_PreUpClose;

            }
        }
        private bool cached_preventsClose;
        private bool cachedIsOpen;
        private void MouseSourceCallback_PreDown(object s, MouseButtonEventArgs e)
        {
            cachedIsOpen = IsOpen;
            if (!IsOpen)
            {
                IsOpen = true;
                cached_preventsClose = true;
                if (InterceptOpenMouse) e.Handled = true;
            }
            else if(!StaysOpen)
            {
                IsOpen = false;
                if (InterceptCloseMouse) e.Handled = true;
            }
        }
        private void MouseSourceCallback_PreUpClose(object s, MouseButtonEventArgs e)
        { 
            if (!InterceptCloseMouse) return;
            if(cachedIsOpen && !StaysOpen)
            {
                IsOpen = false;
                e.Handled = true;
            }
        }
    }
}
