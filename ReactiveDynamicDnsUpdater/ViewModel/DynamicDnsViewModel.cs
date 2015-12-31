using ReactiveDynamicDnsUpdater.Model;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace ReactiveDynamicDnsUpdater.ViewModel
{
    internal class DynamicDnsViewModel
    {
        public ReactiveProperty<string> Status { get; set; }
        public ReactiveProperty<string> Time { get; set; }

        private DynamicDnsInfomation Model { get; }

        public DynamicDnsViewModel(DynamicDnsInfomation model)
        {
            Model = model;
            Status = Model
                //Model=>ViewModelのみバインド(双方向は.ToReactivePropertyAsSynchronized(x => x.Status))
                .ObserveProperty(x => x.Status)
                .ToReactiveProperty();
            Time = Model
                //Model=>ViewModelのみバインド(双方向は.ToReactivePropertyAsSynchronized(x => x.Time))
                .ObserveProperty(x => x.Time)
                .ToReactiveProperty();
        }

        public DynamicDnsViewModel()
        {
        }
    }
}
