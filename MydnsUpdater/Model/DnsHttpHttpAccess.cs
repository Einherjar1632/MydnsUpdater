using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace MydnsUpdater.Model
{
    class DnsHttpAccess
    {
        private string _masterId = string.Empty;
        private string _password = string.Empty;

        public DnsHttpAccess(string masterId , string password)
        {
            _masterId = masterId;
            _password = password;
        }

        public async Task UpdateDnsServerAsync()
        {
            var hc = new HttpClient();
            using (var res = await hc.GetAsync("http://jsonip.com/"))
            {
                var contents = await res.Content.ReadAsStringAsync();
                await Task.Delay(1000);
            }


        }

    }
}
