using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using MydnsUpdater.Model;

namespace MydnsUpdater.ViewModel
{
    class DynamicDNSResponseViewModel
    {
        public ReactiveProperty<string> Status { get; set; }
        public ReactiveProperty<string> Time { get; set; }

        public DynamicDNSResponse Model { get; }

        public DynamicDNSResponseViewModel(DynamicDNSResponse Model)
        {
            this.Model = Model;

            this.Status = this.Model
                .ObserveProperty(x => x.Status)
                .ToReactiveProperty();

            this.Time = this.Model
                .ObserveProperty(x => x.Time)
                .ToReactiveProperty();

            //this.Status = new ReactiveProperty<string>(Model.Status);
            //this.Time = new ReactiveProperty<string>(Model.Time);
            
        }

    }
}
