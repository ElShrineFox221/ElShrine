using System;
using System.Collections;
using System.Collections.Generic;

namespace ElShrine.Old.Wpf.Model
{
    #region Inform Changes
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IEInformationChanged
    {
        public delegate void InformationChangedHandler(object sender, string valueName);
        public event InformationChangedHandler? InformationChanged;
        public void OnInformationChanged(object sender, string valueName);
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class IEInformationChangedExtension
    {
        public static void OnInformationChanged<T>(this T TInstance, object sender, params string[] valueNames) where T : IEInformationChanged
        {
            foreach (string name in valueNames) TInstance.OnInformationChanged(sender, name);
        }
    }

    #endregion

    #region Clone Instance
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IECloneable<T>
    {
        public Func<T, string?, T>? CloneMethod { get; init; }
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class IECloneableExtension
    {
        public static T Clone<T>(this T tInstance, string? newName = null) where T : IECloneable<T>
        {
            static T clone(in T tInstance, string? name = null) => tInstance.CloneMethod == null ? tInstance : tInstance.CloneMethod(tInstance, name);
            try
            {
                return (T)(Activator.CreateInstance(typeof(T), tInstance, newName) ?? clone(tInstance, newName));
            }
            catch
            {
                return clone(tInstance, newName);
            }
        }
    }

    #endregion

    #region EList Interface

    //IEOrderlyComparable & IEOrderlyComparableExtension
    #region Element Comparer Achieve
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IEOrderlyComparable<T> : IComparable<T> where T : IEName { };
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class IEOrderlyComparableExtension
    {
        public static int CompareByName<T>(T ins0, T ins1) where T : IEName => ins0.Name.CompareTo(ins1.Name);
        public static int CompareMethod<T>(T t0, T t1) where T : IEOrderlyComparable<T>, IEName => t0.CompareTo(t1);
    }

    public static class PartialComparer
    {
        public static int? Compare<T>(T t0, T t1) => Compare(Comparer<T>.Default, t0, t1);

        public static int? Compare<T>(IComparer<T> comparer, T t0, T t1)
        {
            int result = comparer.Compare(t0, t1);
            return result == 0 ? null : result;
        }

        public static int? ReferenceCompare<T>(T t0, T t1) where T : class 
            => t0 == t1 ? 0 : t0 == null ? -1 : t1 == null ? 1 : null;
    }

    #endregion
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IEOrderlySortable<TElement> : IEList<TElement> where TElement : class { }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public interface IEList<TElement> : IEnumerable where TElement : class
    {
        public List<TElement> Items { get; init; }
        public bool RepeatCheck { get; init; }
        public int Count { get; }
        public bool IsReadOnly { get; init; }

        public int IndexOf(TElement element);
        public void Add(TElement element);
        public void RemoveAt(int index);
        public void Remove(TElement? element = null);
        public void Clear();
        public bool Contains(TElement element);
        public TElement? Find(Predicate<TElement> match);
        public List<TElement> FindAll(Predicate<TElement> match);
        public void Reverse();
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public static class IEListExtension
    {
        public static TElement? FindByName<TList, TElement>
            (this TList list, TElement element)
            where TList : class, IEList<TElement>
            where TElement : class, IEName
        => list.Items.Find((_element) => _element.Name == element.Name);
        private static void Inform<TList, TElement>(this TList list) 
            where TList : class, IEList<TElement>
            where TElement : class
        {
            if (list is IEInformationChanged ieic) ieic.OnInformationChanged(list, nameof(list.Items), nameof(list.Count));
        }

        #region Sort

        #region Sort Basic Mehtods
        public static void Sort<TList, TElement>
            (this TList list, Comparison<TElement> comparison)
            where TList : class, IEList<TElement>
            where TElement : class
        {
            list.Items.Sort(comparison);
            list.Inform<TList, TElement>();
        }
        public static void Sort<TList, TElement>
            (this TList list)
            where TList : class, IEList<TElement>
            where TElement : class
        {
            list.Items.Sort(Comparer<TElement>.Default);
            list.Inform<TList, TElement>();
        }
        #endregion

        #region Sort Extend Methods
        public static void Sort<TList, TElement>
            (this TList list, bool DefaultSort = true) 
            where TList : class, IEOrderlySortable<TElement> 
            where TElement : class, IEName, IEOrderlyComparable<TElement>
        {
            if (DefaultSort) list.Sort<IEOrderlySortable<TElement>, TElement>(IEOrderlyComparableExtension.CompareByName);
            else list.Sort<IEOrderlySortable<TElement>, TElement>(IEOrderlyComparableExtension.CompareMethod);
        }
        #endregion

        #endregion

        #region Add | Remove | Reverse | Clear | Contains | Find FindAll
        public static void Add<TList, TElement>
            (this TList list, TElement element) 
            where TList : class, IEList<TElement>
            where TElement : class
        {
            if (list.IsReadOnly) throw new Exception("Cannot Correct the ReadOnly list");
            if (!list.RepeatCheck || element is IEName ieElement && list.Find((e) => ((IEName)e).Name == ieElement.Name) is null) 
            {
                list.Items.Add(element);
                list.Inform<TList, TElement>();
            }
        }

        public static void Remove<TList, TElement>
            (this TList list, TElement? element = null)
            where TList : class, IEList<TElement>
            where TElement : class
        {
            if (list.IsReadOnly) throw new Exception("Cannot Correct the ReadOnly list");
            try
            {
                TElement element1 = element ?? list.Items[^1];
                if (list.Items.Remove(element1))
                {
                    list.Inform<TList, TElement>();
                }
            }
            catch { }
        }
        public static void RemoveAt<TList, TElement>
            (this TList list, int index)
            where TList : class, IEList<TElement>
            where TElement : class
            => list.Remove(list.Items[index]);

        public static void Reverse<TList, TElement>
            (this TList list)
            where TList : class, IEList<TElement>
            where TElement : class
        {
            if (list.IsReadOnly) throw new Exception("Cannot Correct the ReadOnly list");
            list.Items.Reverse();
            list.Inform<TList, TElement>();
        }

        public static void Clear<TList, TElement>
            (this TList list)
            where TList : class, IEList<TElement>
            where TElement : class
        {
            if (list.IsReadOnly) throw new Exception("Cannot Correct the ReadOnly list");
            list.Items.Clear();
            list.Inform<TList, TElement>();
        }

        public static bool Contains<TList, TElement>
            (this TList list, TElement element)
            where TList : class, IEList<TElement>
            where TElement : class
        => list.Items.Contains(element);

        public static TElement? Find<TList, TElement>
            (this TList list, Predicate<TElement> match)
            where TList : class, IEList<TElement>
            where TElement : class
        => list.Items.Find(match);
        public static List<TElement> FindAll<TList, TElement>
            (this TList list, Predicate<TElement> match)
            where TList : class, IEList<TElement>
            where TElement : class
        => list.Items.FindAll(match);
        #endregion

        #region IndexOf
        public static int IndexOf<TList, TElement>
            (this TList list, TElement element)
            where TList : class, IEList<TElement>
            where TElement : class
            => list.Items.IndexOf(element);
        #endregion

        #region GetEnumerator
        public static IEnumerator GetEnumerator<TList, TElement>
            (this TList list)
            where TList : class, IEList<TElement>
            where TElement : class
        => list.Items.GetEnumerator();
        #endregion
    }
    #endregion
}
