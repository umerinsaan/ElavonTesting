using CWSWrapper;
using ElavonTesting.ConfigModels;
using ElavonTesting.DTOs;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
namespace ElavonTesting.Services
{
    public class ElavonService
    {
        private readonly ElavonConfig elavonConfig;
        private CWS m_cws;

        private int cnt = 0;

        private PaymentTransactionResults payment_results;
        private PaymentTransactionResults void_results;
        public ElavonService(IOptions<ElavonConfig> _elavonConfig)
        {
            elavonConfig = _elavonConfig.Value;
            m_cws = new CWS();
            payment_results = new PaymentTransactionResults();
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
            payment_results = paymentResults;

            cnt = 0;
            Console.WriteLine("VALUE_OF_CNT_RESET: {0}", cnt);
            Console.WriteLine("INSIDE_PAYMENT_COMPLETE_EVENT");
        }

        private void MyNotifyCWSEvent(CWSEvent cwsEvent)
        {
            
            Console.WriteLine("VALUE_OF_CNT: {0}", ++cnt);
            Console.WriteLine("INSIDE_NOTIFY_EVENT_chanId: {0}", cwsEvent.chanId);
        }

        public PaymentTransactionResults GetPaymentResults()
        {
            return payment_results;
        } 


        public PaymentTransactionResults VoidTransaction(VoidRequestDto request)
        {
            PaymentArgs p_args = new PaymentArgs();

            p_args.paymentGatewayId = request.paymentGatewayId;
            p_args.originalTransId = request.originalTransId;
            p_args.transactionType = TransactionType.VOID;
            p_args.tenderType = "CARD";

            
            m_cws.StartPaymentTransaction(p_args, VoidNotifyEvent, VoidPaymentCompleteEvent);

           PaymentTransactionResults res = new PaymentTransactionResults();

            while(res.RawJSON == null)
            {
                res = void_results;
            }


            return res;

        }

        public void VoidNotifyEvent(CWSEvent ne)
        {
            Console.WriteLine("INSIDE_VOID_NOTIFY_EVENT");
        }

        public void VoidPaymentCompleteEvent(PaymentTransactionResults results, String[] warnings)
        {
            Console.WriteLine("INSIDE_VOID_COMPLETE_EVENT");
            void_results = results;
        }


    }
}
