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

        private DynamicDNSResponse Model { get; }

        public DynamicDNSResponseViewModel(DynamicDNSResponse model)
        {
            this.Model = model;


            this.Status = this.Model
                //双方向
                .ToReactivePropertyAsSynchronized(x => x.Status);
                //.ObserveProperty(x => x.Status)
                //.ToReactiveProperty();

            this.Time = this.Model
                //出力のみ
                .ObserveProperty(x => x.Time)
                .ToReactiveProperty();
        }

    }
}
