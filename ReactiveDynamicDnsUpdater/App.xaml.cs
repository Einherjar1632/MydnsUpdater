using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ReactiveDynamicDnsUpdater.View;

namespace ReactiveDynamicDnsUpdater
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void EntryPoint(object sender, StartupEventArgs e)
        {
            var mw = new ReactiveDynamicDnsUpdaterView();
            mw.Show();
        }
    }
}
