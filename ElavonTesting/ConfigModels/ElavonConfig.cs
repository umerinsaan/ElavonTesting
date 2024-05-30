namespace ElavonTesting.ConfigModels
{
    public class ElavonConfig
    {
        public ElavonConfig() {
            elavonConfig = this;
        }

        public static ElavonConfig elavonConfig;

        public string merchantId {  get; set; }
        public string userId { get; set; }
        public string pin { get; set; }
        public string vendorId { get; set; }
        public string vendorAppName { get; set; }
        public string vendorAppVersion { get; set; }
        public string bmsUsername { get; set; }
        public string bmsPassword { get; set; }

        public string serverType { get; set; }
    }
}
