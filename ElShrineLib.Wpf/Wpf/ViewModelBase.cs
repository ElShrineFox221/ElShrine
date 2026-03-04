using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ElShrine.Wpf
{
    [DataContract]
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void NotifyPropertyChanged(object sender, string memberName) => PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(memberName));
        public void NotifyPropertiesChanged(params string[] memberNames)
        {
            foreach (var member in memberNames) NotifyPropertyChanged(this, member);
        }
        public void NotifyPropertyChanged([CallerMemberName] string memberName = "") => NotifyPropertyChanged(this, memberName);
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
