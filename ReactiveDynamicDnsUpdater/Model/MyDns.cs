using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReactiveDynamicDnsUpdater.Model
{
    /// <summary>
    /// MyDnsにアクセスするためのクラス
    /// </summary>
    class MyDns
    {
        /// <summary>
        /// MyDnsへの更新結果を保持するコレクション
        /// </summary>
        public ObservableCollection<DynamicDnsInfomation> ItemsList { get; set; } = new ObservableCollection<DynamicDnsInfomation>();

        /// <summary>
        /// MyDnsへの更新結果を保持するコレクション(ソート前)
        /// </summary>
        private readonly IList<DynamicDnsInfomation> _itemsList = new List<DynamicDnsInfomation>();

        /// <summary>
        /// MyDNSのUri
        /// </summary>
        private static string MyDnsUri => " http://www.mydns.jp/directip.html?MID={0}&PWD={1}&IPV4ADDR={2}";

        /// <summary>
        /// MyDNSへのIPアドレス更新を行います
        /// </summary>
        /// <param name="masterId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task UpdateDnsServerAsync(string masterId , string password)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(JsonIp.Uri))
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var networkInfomation = JsonConvert.DeserializeObject<JsonIp>(json);
                        var uri = string.Format(MyDnsUri, masterId, password, networkInfomation.Ip);
                        using (var responses = await httpClient.GetAsync(uri))
                        {
                            if (responses.IsSuccessStatusCode)
                            {
                                await responses.Content.ReadAsStringAsync();
                                _itemsList.Add(new DynamicDnsInfomation { Status = "更新成功", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                            else
                            {
                                _itemsList.Add(new DynamicDnsInfomation { Status = "更新失敗", Time = DateTime.Now.ToString(CultureInfo.CurrentCulture) });
                            }
                            var dynamicDnsInfomations = _itemsList.OrderByDescending(x => x.Time);

                            ItemsList?.Clear();
                            foreach (var value in dynamicDnsInfomations)
                            {
                                ItemsList?.Add(value);
                            }
                        }
                    }
                }
            }
        }

    }
}
