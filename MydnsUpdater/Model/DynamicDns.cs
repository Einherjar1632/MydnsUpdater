using Microsoft.Practices.Prism.Mvvm;

namespace MydnsUpdater.Model
{
    class DynamicDns : BindableBase
    {
        private string _status;
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); OnPropertyChanged(nameof(Status)); }
        }

        private string _time;
        public string Time
        {
            get { return _time; }
            set { SetProperty(ref _time, value); OnPropertyChanged(nameof(Time)); }
        }
    }
}
