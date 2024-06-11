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


            //Need to do it in a better way this thing is not good
            while(p_results.RawJSON == null)
            {
                p_results = elavonService.GetPaymentResults();
            }

            return Ok(new
            {
                PaymentResult = p_results.RawJSON
            }) ;
            
        }
    }
}
