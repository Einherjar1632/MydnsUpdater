namespace ReactiveDynamicDnsUpdater.Model
{
    /// <summary>
    /// JsonIPから取得した結果を格納します
    /// </summary>
    public class JsonIp
    {
        /// <summary>
        /// 自身のIPアドレス
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// JsonIPから返されるおまけ情報その1
        /// </summary>
        public string About { get; set; }

        /// <summary>
        /// JsonIPから返されるおまけ情報その2
        /// </summary>
        public string Pro { get; set; }

        /// <summary>
        /// JsonIPのUri
        /// </summary>
        public static string Uri => "http://jsonip.com/";
    }
}
