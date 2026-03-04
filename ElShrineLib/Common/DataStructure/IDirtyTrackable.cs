using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ElShrine.Common.DataStructure
{
    public class IsDirtyChangedEventArgs(bool isDirty, bool isFromChild, string propertyName) : EventArgs
    {
        public bool IsDirty { get; } = isDirty;
        public bool IsFromChild { get; } = isFromChild;
        public string PropertyName { get; } = propertyName;
    }
    public delegate void IsDirtyChangedHandler(object? sender, IsDirtyChangedEventArgs e);
    public interface IDirtyTrackable
    {
        event IsDirtyChangedHandler IsDirtyChanged;
        bool IsDirty { get; }
        void ApplyChanges();
        void CancelChanges();
    }
    public abstract class DirtyTrackableObject : IDirtyTrackable
    {
        private bool applyingChanges;
        private bool cancelingChanges;
        private readonly Dictionary<string, object?> savedProps = [];
        private readonly HashSet<object?> dirtySourceChildren = [];
        private readonly Dictionary<IDirtyTrackable, int> trackedChildren = [];

        public event IsDirtyChangedHandler? IsDirtyChanged;

        public bool IsDirty { get; private set; }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            //Handle old property
            if (!savedProps.TryGetValue(propertyName, out var oldProp)) 
                oldProp = savedProps[propertyName] = field;
            if (field is IDirtyTrackable oldTrackable)
            {
                if (!trackedChildren.TryGetValue(oldTrackable, out var count)) 
                    count = 0;
                if (count > 0) 
                    trackedChildren[oldTrackable] = --count;
                if (count == 0)
                {
                    oldTrackable.IsDirtyChanged -= OnChildDirtyChangedInternal;
                    trackedChildren.Remove(oldTrackable);
                    dirtySourceChildren.Remove(oldTrackable);
                }
            }
            //Handle new property
            var newProp = value;
            field = value;
            if (value is IDirtyTrackable newTrackable)
            {
                if (!trackedChildren.TryGetValue(newTrackable, out var count)) 
                    count = 0;
                trackedChildren[newTrackable] = ++count;
                if (count == 1)
                {
                    newTrackable.IsDirtyChanged += OnChildDirtyChangedInternal;
                    if (newTrackable.IsDirty)
                        dirtySourceChildren.Add(newTrackable);
                }
            }
            //Refresh dirty
            if (Equals(oldProp, newProp)) 
                savedProps.Remove(propertyName);
            if (!applyingChanges && !cancelingChanges) 
                RefreshIsDirty(false, propertyName, true);
            return true;
        }
        private void OnChildDirtyChangedInternal(object? sender, IsDirtyChangedEventArgs e)
        {
            if (applyingChanges || cancelingChanges) return;

            if (e.IsDirty) dirtySourceChildren.Add(sender);
            else dirtySourceChildren.Remove(sender);
            RefreshIsDirty(true, e.PropertyName, false);
            OnChildDirtyChanged(sender, e);
        }
        public virtual void OnChildDirtyChanged(object? sender, IsDirtyChangedEventArgs e) { }

        #region interface required
        public void ApplyChanges()
        {
            if (!IsDirty || applyingChanges) return;
            applyingChanges = true;
            try
            {

                foreach (var child in trackedChildren.Keys)
                    child.ApplyChanges();
                dirtySourceChildren.Clear();
                savedProps.Clear();

                RefreshIsDirty(false, string.Empty, false);
            }
            finally
            {
                applyingChanges = false;
            }
        }
        public void CancelChanges()
        {
            if (!IsDirty || cancelingChanges) return;
            cancelingChanges = true;
            try
            {
                var savedPropsCopy = savedProps.ToList();
                foreach (var kvp in savedPropsCopy)
                {
                    string propName = kvp.Key;
                    object? originalValue = kvp.Value;

                    var propInfo = GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (propInfo is not null && propInfo.CanWrite)
                        propInfo.SetValue(this, originalValue);
                }
                savedProps.Clear();

                foreach (var child in trackedChildren.Keys)
                    child.CancelChanges();
                dirtySourceChildren.Clear();

                RefreshIsDirty(false, string.Empty, true);
            }
            finally
            {
                cancelingChanges = false;
            }
        }
        #endregion

        #region local helpers
        private void RefreshIsDirty(bool isFromChild, string propertyName, bool forceToNotify)
        {
            var isDirty = savedProps.Count > 0 || dirtySourceChildren.Count > 0;
            var changed = IsDirty ^ isDirty;
            if (changed) IsDirty = isDirty;
            if (changed || forceToNotify) IsDirtyChanged?.Invoke(this, new(IsDirty, isFromChild, propertyName));
        }
        #endregion
    }
    public class DirtyTrackableCollection<T> : Collection<T>, IDirtyTrackable
    {
        private bool temporaryPreventDirtyNotification;
        private List<T>? cachedOriginalItems;
        private readonly HashSet<IDirtyTrackable> dirtySourceChildren = [];
        private readonly Dictionary<IDirtyTrackable, int> trackedChildren = [];

        public event IsDirtyChangedHandler? IsDirtyChanged;

        private bool IsStructureChanged => cachedOriginalItems is not null;
        public bool IsDirty { get; private set; }

        #region .ctor
        public DirtyTrackableCollection() => ValidateType(typeof(T));
        public DirtyTrackableCollection(IList<T> collection) : base(collection)
        {
            ValidateType(typeof(T));
            foreach (var item in collection) HandleItemAdded(item);
        }
        private void ValidateType(Type t)
        {
            if (t.IsValueType && t.IsImplementOf(typeof(IDirtyTrackable)))
                throw new InvalidOperationException($"{t.FullName} is not a valid type for {nameof(DirtyTrackableCollection<>)}, the dirtyTrackable struct is not supported.");
        }
        #endregion

        #region collection operations
        protected sealed override void InsertItem(int index, T item)
        {
            RebuildCachedOriginalItems();
            base.InsertItem(index, item);
            HandleItemAdded(item);
            RefreshIsDirty(false, nameof(Items));
        }
        protected sealed override void RemoveItem(int index)
        {
            RebuildCachedOriginalItems();
            T item = Items[index];
            base.RemoveItem(index);
            HandleItemRemoved(item);
            RefreshIsDirty(false, nameof(Items));
        }
        protected sealed override void SetItem(int index, T item)
        {
            RebuildCachedOriginalItems();
            T oldItem = Items[index];
            base.SetItem(index, item);
            HandleItemRemoved(oldItem);
            HandleItemAdded(item);
            RefreshIsDirty(false, nameof(Items));
        }
        protected sealed override void ClearItems()
        {
            RebuildCachedOriginalItems();
            var oldItems = Items.ToList();
            base.ClearItems();
            foreach (var item in oldItems)
                HandleItemRemoved(item, blockedStructureChangedHandler: true);
            HandleStructuralChanged();
            RefreshIsDirty(false, nameof(Items));
        }
        #endregion

        #region local helpers

        #region structural
        private void RebuildCachedOriginalItems(bool forceToRebuild = false)
        {
            if (IsStructureChanged && !forceToRebuild) return;
            cachedOriginalItems = new List<T>(Count);
            cachedOriginalItems.AddRange(this);
        }
        private void HandleStructuralChanged()
        {
            if (temporaryPreventDirtyNotification) return;
            var isNewStructureDirty = IsStructureChanged && !this.SequenceEqual(cachedOriginalItems!);
            if (!isNewStructureDirty) cachedOriginalItems = null;
        }
        private void HandleItemAdded(T item, bool blockedStructureChangedHandler = false)
        {
            if (item is IDirtyTrackable trackable)
            {
                ValidateType(trackable.GetType());
                if (!trackedChildren.TryGetValue(trackable, out var count)) count = 0;
                trackedChildren[trackable] = ++count;
                if (count == 1)
                {
                    trackable.IsDirtyChanged += OnChildDirtyChangedInternal;
                    if (trackable.IsDirty)
                        dirtySourceChildren.Add(trackable);
                }
                if (!blockedStructureChangedHandler) HandleStructuralChanged();
            }
        }
        private void HandleItemRemoved(T item, bool blockedStructureChangedHandler = false)
        {
            if (item is IDirtyTrackable trackable)
            {
                ValidateType(trackable.GetType());
                if (!trackedChildren.TryGetValue(trackable, out var count)) count = 0;
                if (count > 0) trackedChildren[trackable] = --count;
                if (count == 0)
                {
                    trackedChildren.Remove(trackable);
                    trackable.IsDirtyChanged -= OnChildDirtyChangedInternal;
                    dirtySourceChildren.Remove(trackable);
                }
                if (!blockedStructureChangedHandler) HandleStructuralChanged();
            }
        }
        #endregion

        #region child notification
        private void OnChildDirtyChangedInternal(object? sender, IsDirtyChangedEventArgs e)
        {
            if (sender is IDirtyTrackable trackable)
            {
                if (e.IsDirty)
                    dirtySourceChildren.Add(trackable);
                else
                    dirtySourceChildren.Remove(trackable);

                RefreshIsDirty(true, e.PropertyName);
                OnChildDirtyChanged(sender, e);
            }
        }
        #endregion

        private void RefreshIsDirty(bool isFromChild, string propertyName)
        {
            if (temporaryPreventDirtyNotification) return;
            var isDirty = dirtySourceChildren.Count > 0 || IsStructureChanged;

            bool changed = IsDirty ^ isDirty;
            if (changed)
            {
                IsDirty = isDirty;
                IsDirtyChanged?.Invoke(this, new IsDirtyChangedEventArgs(IsDirty, isFromChild, propertyName));
            }
        }
        #endregion

        public virtual void OnChildDirtyChanged(object? sender, IsDirtyChangedEventArgs e) { }

        #region interface implementations
        public void ApplyChanges()
        {
            if (!IsDirty) return;
            temporaryPreventDirtyNotification = true;
            try
            {
                foreach (var child in trackedChildren.Keys)
                    child.ApplyChanges();
                cachedOriginalItems = null;
            }
            finally
            {
                temporaryPreventDirtyNotification = false;
                RefreshIsDirty(false, nameof(Items));
            }
        }
        public void CancelChanges()
        {
            if (!IsDirty) return;
            temporaryPreventDirtyNotification = true;
            try
            {
                if (IsStructureChanged)
                {
                    var list = Items.ToList();
                    Items.Clear();
                    foreach (var item in list) 
                        HandleItemRemoved(item, true);
                    foreach(var item in cachedOriginalItems!)
                    {
                        Items.Add(item);
                        HandleItemAdded(item, true);
                    }
                    cachedOriginalItems = null;
                }
                foreach (var item in trackedChildren.Keys)
                    item.CancelChanges();
            }
            finally
            {
                temporaryPreventDirtyNotification = false;
                HandleStructuralChanged();
                RefreshIsDirty(false, nameof(Items));
            }
        }
        #endregion
    }
}