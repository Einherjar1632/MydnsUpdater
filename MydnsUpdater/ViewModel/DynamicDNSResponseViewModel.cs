using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using MydnsUpdater.Model;

namespace MydnsUpdater.ViewModel
{
    class DynamicDNSResponseViewModel
    {

        public ReactiveProperty<string> Status { get; private set; }
        public ReactiveProperty<string> Time { get; private set; }

        public DynamicDNSResponseViewModel(DynamicDNSResponse Model)
        {
            this.Status = new ReactiveProperty<string>(Model.Status);
            this.Time = new ReactiveProperty<string>(Model.Time);
        }

    }
}
