using CWSWrapper;
using ElavonTesting.ConfigModels;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Security.Cryptography.Xml;
namespace ElavonTesting.Services
{
    public class ElavonService
    {
        private readonly ElavonConfig elavonConfig;
        private CWS m_cws;

        private int cnt = 0;
        public ElavonService(IOptions<ElavonConfig> _elavonConfig)
        {
            elavonConfig = _elavonConfig.Value;
            m_cws = new CWS();
        }

        public OpenPaymentGatewayResults OpenPaymentGateway()
        {
            OpenPaymentArgs openPaymentArgs = new OpenPaymentArgs();

            openPaymentArgs.overrideDefaultTerminalLanguage = new OverrideDefaultTerminalLanguage();
            openPaymentArgs.overrideDefaultTerminalLanguage.languageInformation = new LanguageInformation();

            openPaymentArgs.overrideDefaultTerminalLanguage.languageInformation.languageCode = "EN";
            openPaymentArgs.overrideDefaultTerminalLanguage.languageInformation.countryCode = "US";

            openPaymentArgs.merchantId = elavonConfig.merchantId;
            openPaymentArgs.userId = elavonConfig.userId;
            openPaymentArgs.vendorId = elavonConfig.vendorId;
            openPaymentArgs.bridgeMaintenanceSystemUsername = elavonConfig.bmsUsername;
            openPaymentArgs.bridgeMaintenanceSystemPassword = elavonConfig.bmsPassword;
            openPaymentArgs.pin = elavonConfig.pin;
            openPaymentArgs.partnerAppId = "01";
            openPaymentArgs.paymentGatewayEnvironment = elavonConfig.serverType;

            openPaymentArgs.vendorAppName = elavonConfig.vendorAppName;
            openPaymentArgs.vendorAppVersion = elavonConfig.vendorAppVersion;

            openPaymentArgs.healthCheckStaleTransactionTimeout = 0;
            openPaymentArgs.healthCheckTaskTimeout = 0;

            openPaymentArgs.proxyInfo = null;

            return m_cws.OpenPaymentGateway(openPaymentArgs, null);

        }

        public bool StartSaleTransaction(string paymentGatewayId, long amount)
        {
            amount = amount * 100; // because 2000 is equals to 20$

            PaymentArgs paymentArgs = new PaymentArgs();

            Money money = new Money(Money.CurrencyCodefromString("USD"), amount);

            paymentArgs.paymentGatewayId = paymentGatewayId;

            paymentArgs.transactionType = "SALE";

            paymentArgs.baseTransactionAmount = money;
            paymentArgs.tenderedAmount = money;

            paymentArgs.tenderType = "CARD";
            paymentArgs.cardType = null;
            paymentArgs.isTaxInclusive = false;
            paymentArgs.partialApprovalAllowed = true;

            Money taxAmounts = new Money(Money.CurrencyCodefromString("USD"), 0);

            paymentArgs.taxAmounts = new Money[1];
            paymentArgs.taxAmounts[0] = taxAmounts;

            paymentArgs.discountAmounts = null;

            return m_cws.StartPaymentTransaction(paymentArgs, MyNotifyCWSEvent, MyPaymentCompleteEvent);
        }

        private void MyPaymentCompleteEvent(PaymentTransactionResults paymentResults, String[] warnings)
        {
            Console.WriteLine("INSIDE_PAYMENT_COMPLETE");
        }

        private void MyNotifyCWSEvent(CWSEvent cwsEvent)
        {
            Console.WriteLine("VALUE_OF_CNT: {0}", ++cnt);
            Console.WriteLine("ChanId: {0}", cwsEvent.chanId);
        }

    }
}
