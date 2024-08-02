﻿using CWSWrapper;
using ElavonTesting.ConfigModels;
using ElavonTesting.DTOs;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Transactions;
namespace ElavonTesting.Services
{
    public class ElavonService
    {
        private readonly ElavonConfig elavonConfig;
        private CWS m_cws;

        private int cnt = 0;

        private PaymentTransactionResults payment_results;
        private PaymentTransactionResults void_results;

        private PaymentTransactionResults? transactionResults;

        private TaskCompletionSource<PaymentTransactionResults> transaction_tcs;

        private string? paymentGatewayId;
        private string? chanId;
        private string? originalTransId;
        public ElavonService(IOptions<ElavonConfig> _elavonConfig)
        {
            elavonConfig = _elavonConfig.Value;
            m_cws = new CWS();
            payment_results = new PaymentTransactionResults();
            void_results = new PaymentTransactionResults();

            transactionResults = null;
        }

        public string GetPaymentGatewayId()
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

            return paymentGatewayId =  m_cws.OpenPaymentGateway(openPaymentArgs, null).OpenPaymentData.paymentGatewayId;

        }

        public async Task<PaymentTransactionResults> StartTransaction(PaymentArgs p_args)
        {

            transaction_tcs = new TaskCompletionSource<PaymentTransactionResults>();

            bool check = m_cws.StartPaymentTransaction(p_args, TransactionNotifyEvent, TransactionCompleteEvent);

            if (check == false)
            {
                return new PaymentTransactionResults();
            }

            //while(transactionResults == null){} 

            return await transaction_tcs.Task;
        }

        private void TransactionNotifyEvent(CWSEvent ne)
        {
            chanId = ne.chanId;
            Console.WriteLine("INSIDE_TRANSACTION_NOTIFY_EVENT: {0}", ++cnt);
        }

        private async void TransactionCompleteEvent(PaymentTransactionResults _transactionResults, String[] warnings)
        {
            originalTransId = _transactionResults.PaymentTransactionData.id;

            if(_transactionResults.PaymentTransactionData != null)
            {
                if(_transactionResults.PaymentTransactionData.approved == "PARTIALLY_APPROVED")
                {
                    var p_args = GetPaymentArgs(new TransactionRequestDto
                                 {
                                    originalTransId = this.originalTransId,
                                    chanId = this.chanId,
                                    paymentGatewayId = this.paymentGatewayId,
                                    transactionType = "VOID"
                                 }).paymentArgs;

                    await this.StartTransaction(p_args);
                }
                else
                {
                    transaction_tcs.SetResult(_transactionResults);
                }
            }
            //this.transactionResults = _transactionResults;    
            cnt = 0;
            Console.WriteLine("INSIDE_TRANSACTION_COMPLETE_EVENT: {0}", cnt);
        }

        public PaymentArgsDto GetPaymentArgs(TransactionRequestDto req)
        {
            PaymentArgsDto p_args_dto = new PaymentArgsDto();
            
            var trans_type = req.transactionType?.ToUpper();

            p_args_dto.paymentArgs.transactionType = trans_type; 

            
            if(trans_type == TransactionType.SALE)
            {
                string paymentGatewayId = GetPaymentGatewayId();

                if(paymentGatewayId != null || paymentGatewayId != string.Empty)
                {
                    p_args_dto.paymentArgs.paymentGatewayId = paymentGatewayId;
                }
                else
                {
                    p_args_dto.errorMessage = $"ERROR: Unable to generate PaymentGatewayId for '{trans_type}' transaction.";
                    return p_args_dto;
                }

                var amount = req.amount * 100; //because 2000 means 20$

                Money money = new Money(Money.CurrencyCodefromString("USD"), amount);
                p_args_dto.paymentArgs.baseTransactionAmount = money;
                p_args_dto.paymentArgs.tenderedAmount = money;

                p_args_dto.paymentArgs.tenderType = "CARD";
                p_args_dto.paymentArgs.cardType = null;
                p_args_dto.paymentArgs.isTaxInclusive = false;

                p_args_dto.paymentArgs.partialApprovalAllowed = true; //It is necessary as per the docs for sale transaction.

                Money taxAmounts = new Money(Money.CurrencyCodefromString("USD"), 0);

                p_args_dto.paymentArgs.taxAmounts = new Money[1];
                p_args_dto.paymentArgs.taxAmounts[0] = taxAmounts;

                p_args_dto.paymentArgs.discountAmounts = null;

            }

            else if(trans_type == TransactionType.VOID)
            {
                p_args_dto.paymentArgs.tenderType = TenderType.CARD;
                if(req.paymentGatewayId != null || req.paymentGatewayId != string.Empty)
                {
                    p_args_dto.paymentArgs.paymentGatewayId = req.paymentGatewayId;
                }
                else
                {
                    p_args_dto.errorMessage = $"ERROR: PaymentGatewayId cannot be null or empty for '{trans_type}' transaction.";
                    return p_args_dto;
                }

                if (req.originalTransId != null || req.originalTransId != string.Empty)
                {
                    p_args_dto.paymentArgs.originalTransId = req.originalTransId;
                }
                else
                {
                    p_args_dto.errorMessage = $"ERROR: OriginalTransId cannot be null or empty for '{trans_type}' transaction.";
                    return p_args_dto;
                }
            }
            else
            {
                p_args_dto.errorMessage = $"ERROR: '{trans_type}' is not a supported transaction type.";
            }


            return p_args_dto;

        }

        //public async Task<PaymentTransactionResults> LinkedRefund(LinkedRefundRequestDto request)
        //{
        //    Money amount = new Money()
        //    bool check = m_cws.StartLinkedRefund(request.paymetGatewayId, request.originalTransId, TenderType.CARD, CardType.CREDIT,)
        //}
    }
}