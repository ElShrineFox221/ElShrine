using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ElShrine.Old.Wpf.Model
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [DataContract]
    public abstract class ElShrineResourceBase : IEName, IEInformationChanged, IECloneable<ElShrineResourceBase>, IEOrderlyComparable<ElShrineResourceBase>
    {
        protected ElShrineResourceBase() { }
        protected ElShrineResourceBase(ElShrineResourceBase erb, string? name = null)
        {
            IsCopy = true;
            Name = name ?? erb.Name;
        }
        [DataMember]
        public bool IsCopy { get; init; } = false;

        private string name = Const.Sign_Void;
        [DataMember]
        public string Name
        {
            get => name;
            set {
                name = value;
                OnInformationChanged(this, nameof(Name));
            }
        }
        public Func<ElShrineResourceBase, string?, ElShrineResourceBase>? CloneMethod { get; init; } = null;

        public override string ToString() => Name;

        public event IEInformationChanged.InformationChangedHandler? InformationChanged;
        public virtual void OnInformationChanged(object sender, string valueName) => InformationChanged?.Invoke(sender, valueName);

        public virtual int CompareTo(ElShrineResourceBase? other) => 0;
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [DataContract]
    public class EBasicList<T> : ElShrineResourceBase, IEList<T>, IECloneable<EBasicList<T>>
        where T : class
    {
        public EBasicList() : this(false) { }
        public EBasicList(bool RepeatCheck) : base() { this.RepeatCheck = RepeatCheck; }
        public EBasicList(EBasicList<T> listSource, string? name = null) : base(listSource, name) { 
            RepeatCheck = listSource.RepeatCheck;
            IsReadOnly = listSource.IsReadOnly;
        }
        public T this[int index] { get => Items[index]; }
        [DataMember]
        public List<T> Items { get; init; } = [];
        [DataMember]
        public bool RepeatCheck { get; init; } = false;
        public int Count { get => Items.Count; }
        [DataMember]
        public bool IsReadOnly { get; init; } = false;
        Func<EBasicList<T>, string?, EBasicList<T>>? IECloneable<EBasicList<T>>.CloneMethod { get; init; } = (name, listSource) => new(name, listSource);

        public int IndexOf(T element) => this.IndexOf<EBasicList<T>, T>(element);

        public void Add(T element) => this.Add<EBasicList<T>, T>(element);
        public void Remove(T? element = null) => this.Remove<EBasicList<T>, T>(element);
        public void RemoveAt(int index) => this.RemoveAt<EBasicList<T>, T>(index);
        public void Clear() => this.Clear<EBasicList<T>, T>();
        public void Reverse() => this.Reverse<EBasicList<T>, T>();
        public bool Contains(T element) => this.Contains<EBasicList<T>, T>(element);
        public T? Find(Predicate<T> match) => this.Find<EBasicList<T>, T>(match);
        public List<T> FindAll(Predicate<T> match) => this.FindAll<EBasicList<T>, T>(match);

        public IEnumerator GetEnumerator() => this.GetEnumerator<EBasicList<T>, T>();
    }
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    [DataContract]
    public class EList<T> : EBasicList<T>, IEOrderlySortable<T>, IECloneable<EList<T>>
        where T : class, IEName, IEOrderlyComparable<T>, IECloneable<T>
    {
        public EList() : this(false) { }
        public EList(bool RepeatCheck) : base(RepeatCheck) { }
        public EList(EList<T> listSource, string? name = null) : base(listSource, name)
        {
            foreach(var item in listSource.Items) Add(item.Clone());
        }

        Func<EList<T>, string?, EList<T>>? IECloneable<EList<T>>.CloneMethod { get; init; } = (listSource, name) => new(listSource, name);
    }
}
