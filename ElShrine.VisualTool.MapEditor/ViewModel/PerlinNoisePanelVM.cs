using ElShrine.Old.Wpf.ViewModel;
using ElShrine.VisualTool.MapEditor.Common.PerlinNoise;
using ElShrine.VisualTool.MapEditor.Model;
using ElShrine.Wpf;
using System.ComponentModel;
using System.Drawing;

namespace ElShrine.VisualTool.MapEditor.ViewModel
{
    public sealed class PerlinNoisePanelVM(int seed, Size size, List<Rank> items) : CollectionViewModelBase<Rank, RankVM>(items)
    {
        protected override RankVM Construct(Rank model)
        {
            RankVM vm = new(model);
            vm.PropertyChanged += (_, _) => NoticePropertyChanged(nameof(Regeneratable));
            return vm;
        }
        protected override Rank Construct(RankVM viewModel) => new(viewModel.Frequency, viewModel.Amplitude);

        protected override Rank GetAddItem(object? commandParam, out bool confrimAdd)
        {
            int length = (int)Math.Pow(2, ViewModels.Count);
            Rank rank;
            if (commandParam is Rank r) rank = new(r.Frequency, r.Amplitude);
            else rank = new(length, 1d / length);
            confrimAdd = true;
            return rank;
        }
        protected override int[] GetClearExceptIndexes(object? commandParam) => [];
        protected override int[] GetRemoveItemIndexes(object? commandParam)
        {
            int[] result;
            if (commandParam is IList<RankVM> list) result = [.. list.Select(ViewModels.IndexOf)];
            else if (commandParam is RankVM rvm) result = [ViewModels.IndexOf(rvm)];
            else if (commandParam is int i) result = [i];
            else if (commandParam is int[] ints) result = ints;
            else result = [];
            return result;
        }
        protected override bool ReorderItems(object? commandParam) => false;
        protected override void CollectionChanged(CollectionChangeAction change, IEnumerable<Rank> models)
        {
            RankCount = ViewModels.Count;
            base.CollectionChanged(change, models);
        }

        private int seed = seed;
        public int Seed
        {
            get => seed;
            set
            {
                if (seed != value)
                {
                    seed = value;
                    LocalDirtied = true;
                    NoticePropertyChanged(nameof(Seed), nameof(Regeneratable));
                }
            }
        }
        public VMCommand ChangeSeed => new(o =>
        {
            if (o is int i) Seed = i;
            else if (o is string s && int.TryParse(s, out int i1)) Seed = i1;
        });
        public VMCommand RandomSeed => new(o =>
        {
            Seed = new Random().Next(int.MaxValue);
        });

        private int rankCount = items.Count;
        public int RankCount 
        { 
            get => rankCount; 
            set
            {
                rankCount = value;
                NoticePropertyChanged(nameof(RankCount), nameof(Regeneratable));
            } 
        }
        public int RankLengthFactor { get; set; } = 2;
        public VMCommand RankCountChange => new(o =>
        {
            if (int.TryParse(o?.ToString(), out int r)) RankCount = Math.Max(0, RankCount + r);
        });
        public VMCommand GenerateRanks => new(o =>
        {
            int sub = RankCount - ViewModels.Count;
            if (sub > 0) for (; sub > 0; sub--) Add.Execute(null);
            else for (; sub < 0; sub++) Remove.Execute(ViewModels.Count - 1);
        });
        //

        private Size LocalSize = size;
        public void UpdateSize(Size size)
        {
            if (LocalSize != size)
            {
                LocalSize = size;
                NoticePropertyChanged(nameof(LocalSize), nameof(Regeneratable));
            }
        }

        private Perlin2D perlin2D = new(seed, [.. items.Select(i => new Rank(i.Frequency, i.Amplitude))], size);
        public Perlin2D Perlin2D
        {
            get
            {
                if (Regeneratable) RegenPerlin();
                return perlin2D;
            }
        }
        private bool PerlinChangedCheck()
        {
            bool unchanged = ViewModels.Count == 0;
            if (!unchanged)
            {
                unchanged = perlin2D.Seed == Seed && perlin2D.Ranks.Length == ViewModels.Count && perlin2D.NoiseMatrixSize == LocalSize;
                for (int i = 0; i < ViewModels.Count && unchanged; i++)
                {
                    RankVM rankvm = new(perlin2D.Ranks[i]), vmRankvm = ViewModels[i];
                    unchanged = rankvm.DataEqual(vmRankvm);
                }
            }
            return !unchanged;
        }
        public Perlin2D RegenPerlin()
        {
            Perlin2D p2d = new(Seed, [.. ViewModels.Select(vm => new Rank(vm.Frequency, vm.Amplitude))], LocalSize);
            p2d.RegeneratePerlinNoiseMatrix();
            perlin2D = p2d;
            NoticePropertyChanged(nameof(Perlin2D), nameof(Regeneratable));
            return p2d;
        }
        public bool Regeneratable => PerlinChangedCheck();
        public VMCommand RegenerateNoise => new(o =>
        {
            if (Regeneratable) RegenPerlin();
        });
    }
}
