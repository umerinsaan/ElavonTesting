using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ElavonTesting.Services;
using CWSWrapper;
using ElavonTesting.DTOs;

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

            return Ok(new
            {
                TransactionResponse = transRes
            });
            
        }
    }
}
