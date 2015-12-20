using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reactive.Bindings;

namespace MydnsUpdater.Model
{
    class MyDnsDnsHttpAccess
    {
        private static readonly string _jsonIpUri = "http://jsonip.com/";
        private static readonly string _myDnsUri = " http://www.mydns.jp/directip.html?MID={0}&PWD={1}&IPV4ADDR={2}";
        private string _masterId = string.Empty;
        private string _password = string.Empty;

        public ReactiveCollection<DynamicDNSResponse> ItemsCollection { get; } = new ReactiveCollection<DynamicDNSResponse>();

        public MyDnsDnsHttpAccess(string masterId, string password)
        {
            _masterId = masterId;
            _password = password;
        }

        public async Task UpdateDnsServerAsync()
        {
            var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(_jsonIpUri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var networkInfomation = JsonConvert.DeserializeObject<NetworkInfomation>(json);
                    var uri = string.Format(_myDnsUri, _masterId, _password, networkInfomation.Ip);
                    using (var responses = await httpClient.GetAsync(uri))
                    {
                        if (responses.IsSuccessStatusCode)
                        {
                            var jsons = await responses.Content.ReadAsStringAsync();
                            var item = new DynamicDNSResponse()
                            {
                                Status = "更新成功",
                                Time = DateTime.Now.ToString()
                            };
                            ItemsCollection.Add(item);
                        }
                        else
                        {
                            var item = new DynamicDNSResponse()
                            {
                                Status = "更新失敗",
                                Time = DateTime.Now.ToString()
                            };
                            ItemsCollection.Add(item);
                        }
                    }
                }
            }
        }

    }
}
