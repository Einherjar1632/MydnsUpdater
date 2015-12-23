using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;

namespace MydnsUpdater.Model
{
    class DynamicDNSResponse : BindableBase
    {
        private string _status;
        public string Status
        {
            get { return this._status; }
            //set { this.SetProperty(ref this._status, value); this.OnPropertyChanged(nameof(this.Status)); }
            set { this._status = value; }
        }

        private string _time;
        public string Time
        {
            get { return this._time; }
            //set { this.SetProperty(ref this._time, value); this.OnPropertyChanged(nameof(this.Time)); }
            set { this._time = value; }
        }
    }
}
