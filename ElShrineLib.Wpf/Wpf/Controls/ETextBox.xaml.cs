using ElShrine.Common;
using ElShrine.Modules;
using ElShrine.Wpf.UITheme;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ElShrine.Wpf.Controls
{
    [GenerateDPCli]
    public partial class ETextBox : TextBox, IThemeControlBase, IHeaderControlBase
    {
        #region DPs
        public string NoticeInfo
        {
            get => (string)GetValue(NoticeInfoProperty);
            set => SetValue(NoticeInfoProperty, value);
        }
        public IEnumerable<object> SuggestionsSource
        {
            get => (IEnumerable<object>)GetValue(SuggestionsSourceProperty);
            set => SetValue(SuggestionsSourceProperty, value);
        }
        public bool IsPasswordBox
        {
            get => (bool)GetValue(IsPasswordBoxProperty);
            set => SetValue(IsPasswordBoxProperty, value);
        }

        public static readonly DependencyProperty NoticeInfoProperty = DependencyProperty.Register(nameof(NoticeInfo),  typeof(string), typeof(ETextBox), new(string.Empty));
        public static readonly DependencyProperty SuggestionsSourceProperty = DependencyProperty.Register(nameof(SuggestionsSource),  typeof(IEnumerable<object>), typeof(ETextBox), new(null, OnSuggestionsSourceChanged));
        public static readonly DependencyProperty IsPasswordBoxProperty = DependencyProperty.Register(nameof(IsPasswordBox),  typeof(bool), typeof(ETextBox), new(false));

        public event ValueChangedHandler<string>? AppliedSuggestion;
        #endregion

        #region Private Properties
        private Popup? PART_SuggestionPopup;
        private ListBox? PART_SuggestionListBox;
        private ScrollViewer? PART_ContentHost;
        private bool isUpdatingFromSuggestion;
        private INotifyCollectionChanged? currentCollection;
        #endregion

        #region Implements
        static ETextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ETextBox), new FrameworkPropertyMetadata(typeof(ETextBox)));
        public ETextBox() => UIThemesManager.RegisterCoerceThemeDPs(this);
        public void GlobalThemeChanged(object? sender, ValueChangedEventArgs<Theme> e) => UIThemesManager.CoerceValue(this);
        public void LocalThemePorpertyChanged(DependencyPropertyChangedEventArgs e) => StateListenersManager.Instance.RedoSetterTransitions(this);
        #endregion

        #region Initialize
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_SuggestionPopup = GetTemplateChild(nameof(PART_SuggestionPopup)) as Popup;
            PART_SuggestionListBox = GetTemplateChild(nameof(PART_SuggestionListBox)) as ListBox;
            PART_ContentHost = GetTemplateChild(nameof(PART_ContentHost)) as ScrollViewer;
            if (PART_SuggestionListBox != null)
            {
                PART_SuggestionListBox.PreviewKeyDown += OnSuggestionListKeyDown;
                PART_SuggestionListBox.MouseUp += OnSuggestionListMouseUp;
            }
            UpdateSuggestionsSourceListener();
        }
        private void UpdateSuggestionsSourceListener()
        {
            if (currentCollection != null) currentCollection.CollectionChanged -= OnSuggestionCollectionChanged;
            if (SuggestionsSource is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnSuggestionCollectionChanged;
                currentCollection = newCollection;
            }
        }
        private void OnSuggestionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateSuggestions();
        #endregion

        #region Auto complete suggestion
        private static void OnSuggestionsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ETextBox etb)
            {
                etb.UpdateSuggestionsSourceListener();
                etb.UpdateSuggestions();
            }
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (!isUpdatingFromSuggestion) UpdateSuggestions();
        }
        private void UpdateSuggestions()
        {
            if (PART_SuggestionPopup is null || PART_SuggestionListBox is null) return;
            if (SuggestionsSource is null || !SuggestionsSource.Any())
            {
                PART_SuggestionPopup.IsOpen = false;
                return;
            }
            var suggestions = new List<string>();
            var pattern = Text;
            foreach (var item in SuggestionsSource)
            {
                string str = item?.ToString() ?? string.Empty;
                if (pattern.IsSubSequenceOf(str)) suggestions.Add(str);
            }

            suggestions = [.. suggestions.OrderBy(s => s.GetDeletionDistance(pattern)).ThenBy(s => s)];
            PART_SuggestionListBox.ItemsSource = suggestions;
            PART_SuggestionListBox.SelectedIndex = suggestions.Count != 0 ? 0 : -1;

            if (suggestions.Count != 0)
            {
                PART_SuggestionPopup.PlacementTarget = PART_ContentHost;
                PART_SuggestionPopup.Placement = PlacementMode.Bottom;
                PART_SuggestionPopup.IsOpen = true;
            }
            else PART_SuggestionPopup.IsOpen = false;
        }
        #endregion

        #region Key handling
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (PART_SuggestionPopup?.IsOpen == true)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        moveSuggestionListSelection(-1);
                        e.Handled = true;
                        return;
                    case Key.Down:
                        moveSuggestionListSelection(1);
                        e.Handled = true;
                        return;
                    case Key.Enter:
                        ApplySelectedSuggestion();
                        e.Handled = true;
                        return;
                    case Key.Escape:
                        PART_SuggestionPopup.IsOpen = false;
                        e.Handled = true;
                        return;
                }
            }
            else if (e.Key == Key.Enter)
            {
                GetBindingExpression(TextProperty)?.UpdateSource();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
            void moveSuggestionListSelection(int mount)
            {
                if (PART_SuggestionListBox?.Items.Count > 0)
                {
                    int newIndex = PART_SuggestionListBox.SelectedIndex + mount;
                    if (newIndex >= 0 && newIndex < PART_SuggestionListBox.Items.Count)
                    {
                        PART_SuggestionListBox.SelectedIndex = newIndex;
                        PART_SuggestionListBox.ScrollIntoView(PART_SuggestionListBox.SelectedItem);
                    }
                }
            }
        }
        private void ApplySelectedSuggestion()
        {
            if (PART_SuggestionListBox?.SelectedItem != null)
            {
                isUpdatingFromSuggestion = true;
                var newText = PART_SuggestionListBox.SelectedItem.ToString() ?? string.Empty;
                AppliedSuggestion?.Invoke(this, new(Text, newText));
                Text = newText;
                CaretIndex = Text?.Length ?? 0;
                isUpdatingFromSuggestion = false;
                
            }
            PART_SuggestionPopup?.IsOpen = false;
        }
        private void OnSuggestionListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ApplySelectedSuggestion();
                e.Handled = true;
            }
        }
        private void OnSuggestionListMouseUp(object sender, MouseButtonEventArgs e) => ApplySelectedSuggestion();
        #endregion

    }
}
