using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reactive.Bindings;

namespace MydnsUpdater.Model
{
    class MyDnsDnsHttpAccess
    {
        private static readonly string _jsonIpUri = "http://jsonip.com/";
        private static readonly string _myDnsUri = " http://www.mydns.jp/directip.html?MID={0}&PWD={1}&IPV4ADDR={2}";
        private readonly ReactiveProperty<string> _masterId;
        private readonly ReactiveProperty<string> _password;

        public ObservableCollection<DynamicDns> ItemsCollection { get; } = new ObservableCollection<DynamicDns>();

        public MyDnsDnsHttpAccess(ReactiveProperty<string> masterId, ReactiveProperty<string> password)
        {
            _masterId = masterId;
            _password = password;
        }

        public async Task UpdateDnsServerAsync()
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(_jsonIpUri))
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var networkInfomation = JsonConvert.DeserializeObject<NetworkInfomation>(json);
                        var uri = string.Format(_myDnsUri, _masterId.Value, _password.Value, networkInfomation.Ip);
                        using (var responses = await httpClient.GetAsync(uri))
                        {
                            if (responses.IsSuccessStatusCode)
                            {
                                await responses.Content.ReadAsStringAsync();
                                ItemsCollection.Add(new DynamicDns { Status = "更新成功", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                            else
                            {
                                ItemsCollection.Add(new DynamicDns { Status = "更新失敗", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                        }
                    }
                }
            }
        }

    }
}
