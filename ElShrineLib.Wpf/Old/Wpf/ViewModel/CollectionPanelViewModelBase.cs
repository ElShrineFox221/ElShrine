using ElShrine.Common.Interface;
using ElShrine.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ElShrine.Old.Wpf.ViewModel
{
    [Obsolete(ObsoleteMsg.OldNamespaceMsg)]
    public abstract class CollectionViewModelBase<ItemModel, ItemViewModel>(List<ItemModel> items) : ViewModelBase, IEVMCollection, IEDirtable where ItemViewModel : ViewModelBase<ItemModel>, IEVMEquatabe<ItemViewModel> where ItemModel : class
    {
        protected override void Initialize() => ReconstructViewModels();

        protected void ReconstructViewModels()
        {
            ViewModels = [.. Models.Select(model => {
                var vm = Construct(model);
                vm.PropertyChanged+=(o,e) =>{
                    NoticePropertyChanged(nameof(Dirtied));
                };
                return vm;
            })];
            NoticePropertyChanged(nameof(ViewModels));
        }

        protected List<ItemModel> Models = items;
        public ObservableCollection<ItemViewModel> ViewModels { get; set; } = [];

        public VMCommand Add => new(o =>
        {
            var item = GetAddItem(o, out bool confrimed);
            if (confrimed)
            {
                var itemvm = Construct(item);
                Models.Add(item);
                ViewModels.Add(itemvm);
                CollectionChanged(CollectionChangeAction.Add, [item]);
            }
        });
        public VMCommand Remove => new(o =>
        {
            var indexes = GetRemoveItemIndexes(o);
            ItemModel[] models = new ItemModel[indexes.Length];
            int count = 0;
            foreach (var index in indexes)
            {
                models[count++] = Models[index];
                Models.RemoveAt(index);
                ViewModels.RemoveAt(index);
            }
            CollectionChanged(CollectionChangeAction.Remove, models);
        });
        public VMCommand Clear => new(o =>
        {
            var exceptIndexes = GetClearExceptIndexes(o);
            ItemModel[] models = new ItemModel[Models.Count - exceptIndexes.Length];
            for (int i = Models.Count - 1, count = 0; i >= 0; i--)
            {
                if (!exceptIndexes.Contains(i))
                {
                    models[count++] = Models[i];
                    Models.RemoveAt(i);
                    ViewModels.RemoveAt(i);
                }
            }
            CollectionChanged(CollectionChangeAction.Remove, models);
        });
        public VMCommand Reorder => new(o =>
        {
            bool result = ReorderItems(o);
            if (result)
            {
                ReconstructViewModels();
                CollectionChanged(CollectionChangeAction.Refresh, []);
            }
        });

        protected bool LocalDirtied = false;
        public bool Dirtied
        {
            get
            {
                bool result = LocalDirtied;
                if (!LocalDirtied)
                {
                    bool countEqual = ViewModels.Count == Models.Count, itemsEqual = true;
                    if (countEqual)
                    {
                        for (int i = 0; i < ViewModels.Count && itemsEqual; i++)
                        {
                            if (ViewModels[i] is IEDirtable ied) itemsEqual = !ied.Dirtied;
                            if (itemsEqual) itemsEqual = ViewModels[i].DataEqual(Construct(Models[i]));
                        }
                    }
                    result = !countEqual || !itemsEqual;
                } 
                return result;
            }
            set
            {
                LocalDirtied = value;
                if (!value)
                {
                    foreach (var viewModel in ViewModels)
                    {
                        if (viewModel is IEDirtable ied) ied.Dirtied = false;
                        else break;
                    }
                }
                NoticePropertyChanged(nameof(Dirtied));
            }
        }
        protected abstract ItemViewModel Construct(ItemModel model);
        protected abstract ItemModel Construct(ItemViewModel viewModel);
        protected abstract ItemModel GetAddItem(object? commandParam, out bool confrimAdd);
        protected abstract int[] GetRemoveItemIndexes(object? commandParam);
        protected abstract int[] GetClearExceptIndexes(object? commandParam);
        protected abstract bool ReorderItems(object? commandParam);

        protected virtual void CollectionChanged(CollectionChangeAction change, IEnumerable<ItemModel> models)
        {
            Dirtied = true;
        }
    }
}
