using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ElavonTesting.Services;
using CWSWrapper;
using ElavonTesting.DTOs;
using System.Text.Json.Nodes;

namespace ElavonTesting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElavonController : ControllerBase
    {
        private ElavonService elavonService;
        public ElavonController(ElavonService _elavonService) { 
            elavonService = _elavonService;
        }

        [HttpPost]
        [Route("Sale")]
        public IActionResult SaleTransaction([FromBody] long amount)
        {

            OpenPaymentGatewayResults results = elavonService.OpenPaymentGateway();

            if(results.OpenPaymentData.paymentGatewayId == null)
            {
                return BadRequest(new {
                    ErrorMessage = "PayemntGatewayId is null."
                });
            }

            bool transRes = elavonService.StartSaleTransaction(results.OpenPaymentData.paymentGatewayId, amount);

            PaymentTransactionResults p_results = new PaymentTransactionResults();

            

            while(p_results.RawJSON == null)
            {
                p_results = elavonService.GetPaymentResults();
            }

            var PaymentResult = p_results.RawJSON;
            if(results.Completed == true) {
                return Ok(PaymentResult);
            
            }
            else
            {
                return BadRequest(PaymentResult);
            }
            
        }

        [HttpPost]
        [Route("Void")]
        public IActionResult VoidTransaction([FromBody] VoidRequestDto request)
        {
            var res = elavonService.VoidTransaction(request);

            
            return Ok(res.RawJSON);
        }
    }
}
