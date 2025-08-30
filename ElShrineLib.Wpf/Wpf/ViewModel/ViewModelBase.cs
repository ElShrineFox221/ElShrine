using System.ComponentModel;
using System.Runtime.Serialization;

namespace ElShrine.Wpf.ViewModel
{
    [DataContract]
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void NoticePropertyChanged(object sender, string memberName) => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(memberName));
        public void NoticePropertyChanged(params string[] memberNames)
        {
            foreach (var member in memberNames) NoticePropertyChanged(this, member);
        }

        public ViewModelBase()
        {
            Initialize();
        }
        protected virtual void Initialize() { }
    }
    public abstract class ViewModelBase<TModel>(TModel model) : ViewModelBase
    {
        public TModel Model { get; init; } = model;
    }
}
