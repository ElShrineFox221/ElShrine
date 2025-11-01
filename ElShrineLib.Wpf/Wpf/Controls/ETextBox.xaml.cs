using ElShrine.Common;
using ElShrine.Wpf.UITheme;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ElShrine.Wpf.Controls
{
    public class ETextBox : TextBox, IThemeControl
    {
        #region DPs

        #region Theme
        public CornerRadius BorderCornerRadius
        {
            get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
            set => SetValue(BorderCornerRadiusProperty, value);
        }
        public Brush FontBrush
        {
            get => (Brush)GetValue(FontBrushProperty);
            set => SetValue(FontBrushProperty, value);
        }
        public Brush ClickBrush
        {
            get => (Brush)GetValue(ClickBrushProperty);
            set => SetValue(ClickBrushProperty, value);
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty = DependencyProperty.Register(nameof(BorderCornerRadius),  typeof(CornerRadius), typeof(ETextBox), new(Theme.Default.CornerRadius));
        public static readonly DependencyProperty FontBrushProperty = DependencyProperty.Register(nameof(FontBrush),  typeof(Brush), typeof(ETextBox), new(Theme.Default.FontColor.ToSolidBrush()));
        public static readonly DependencyProperty ClickBrushProperty = DependencyProperty.Register(nameof(ClickBrush),  typeof(Brush), typeof(ETextBox), new(Theme.Default.ClickColor.ToSolidBrush()));
        #endregion

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }
        public ExpandDirection HeaderPlacement
        {
            get => (ExpandDirection)GetValue(HeaderPlacementProperty);
            set => SetValue(HeaderPlacementProperty, value);
        }
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
       
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header),  typeof(object), typeof(ETextBox), new(null));
        public static readonly DependencyProperty HeaderPlacementProperty = DependencyProperty.Register(nameof(HeaderPlacement),  typeof(ExpandDirection), typeof(ETextBox), new(ExpandDirection.Left));
        public static readonly DependencyProperty NoticeInfoProperty = DependencyProperty.Register(nameof(NoticeInfo),  typeof(string), typeof(ETextBox), new(string.Empty));
        public static readonly DependencyProperty SuggestionsSourceProperty = DependencyProperty.Register(nameof(SuggestionsSource),  typeof(IEnumerable<object>), typeof(ETextBox), new(null, OnSuggestionsSourceChanged));

        public event ValueChangedHandler<string>? AppliedSuggestion;
        #endregion

        #region Private Properties
        private Popup? autoCompletePopup;
        private ListBox? suggestionListBox;
        private ScrollViewer? contentHostScrollViewer;
        private bool isUpdatingFromSuggestion;
        private INotifyCollectionChanged? currentCollection;
        #endregion

        static ETextBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ETextBox), new FrameworkPropertyMetadata(typeof(ETextBox)));

        #region Initialize
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            autoCompletePopup = GetTemplateChild("PART_SuggestionPopup") as Popup;
            suggestionListBox = GetTemplateChild("PART_SuggestionListBox") as ListBox;
            contentHostScrollViewer = GetTemplateChild("PART_ContentHost") as ScrollViewer;
            if (suggestionListBox != null)
            {
                suggestionListBox.PreviewKeyDown += OnSuggestionListKeyDown;
                suggestionListBox.MouseUp += OnSuggestionListMouseUp;
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
            if (autoCompletePopup is null || suggestionListBox is null) return;
            if (SuggestionsSource is null || !SuggestionsSource.Any())
            {
                autoCompletePopup.IsOpen = false;
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
            suggestionListBox.ItemsSource = suggestions;
            suggestionListBox.SelectedIndex = suggestions.Count != 0 ? 0 : -1;

            if (suggestions.Count != 0)
            {
                autoCompletePopup.PlacementTarget = contentHostScrollViewer;
                autoCompletePopup.Placement = PlacementMode.Bottom;
                autoCompletePopup.IsOpen = true;
            }
            else autoCompletePopup.IsOpen = false;
        }
        #endregion

        #region Key handling
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (autoCompletePopup?.IsOpen == true)
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
                        autoCompletePopup.IsOpen = false;
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
                if (suggestionListBox?.Items.Count > 0)
                {
                    int newIndex = suggestionListBox.SelectedIndex + mount;
                    if (newIndex >= 0 && newIndex < suggestionListBox.Items.Count)
                    {
                        suggestionListBox.SelectedIndex = newIndex;
                        suggestionListBox.ScrollIntoView(suggestionListBox.SelectedItem);
                    }
                }
            }
        }
        private void ApplySelectedSuggestion()
        {
            if (suggestionListBox?.SelectedItem != null)
            {
                isUpdatingFromSuggestion = true;
                var newText = suggestionListBox.SelectedItem.ToString() ?? string.Empty;
                AppliedSuggestion?.Invoke(this, new(Text, newText));
                Text = newText;
                CaretIndex = Text?.Length ?? 0;
                isUpdatingFromSuggestion = false;
                
            }
            if (autoCompletePopup is not null) autoCompletePopup.IsOpen = false;
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
