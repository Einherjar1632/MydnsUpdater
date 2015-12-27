using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MydnsUpdater.Model
{
    class MyDns
    {
        private static readonly string _jsonIpUri = "http://jsonip.com/";
        private static readonly string _myDnsUri = " http://www.mydns.jp/directip.html?MID={0}&PWD={1}&IPV4ADDR={2}";

        public ObservableCollection<DynamicDnsInfomation> ItemsCollection { get; } = new ObservableCollection<DynamicDnsInfomation>();

        public async Task UpdateDnsServerAsync(string masterId , string password)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(_jsonIpUri))
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var networkInfomation = JsonConvert.DeserializeObject<MyNetworkInfomation>(json);
                        var uri = string.Format(_myDnsUri, masterId, password, networkInfomation.Ip);
                        using (var responses = await httpClient.GetAsync(uri))
                        {
                            if (responses.IsSuccessStatusCode)
                            {
                                await responses.Content.ReadAsStringAsync();
                                ItemsCollection.Add(new DynamicDnsInfomation { Status = "更新成功", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                            else
                            {
                                ItemsCollection.Add(new DynamicDnsInfomation { Status = "更新失敗", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                        }
                    }
                }
            }
        }

    }
}
