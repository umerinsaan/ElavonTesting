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
        private readonly ElavonService elavonService;
        public ElavonController(ElavonService _elavonService) {
            elavonService = _elavonService;
        }

        [HttpPost]
        [Route("Transaction")]
        public async Task<IActionResult> Transaction([FromBody] TransactionRequestDto request)
        {
            PaymentArgsDto p_args_dto = elavonService.GetPaymentArgs(request);

            
            
            if(p_args_dto.errorMessage?.Length > 0) {
                
                return BadRequest(new
                {
                    errorMessage = p_args_dto?.errorMessage
                });
            }
            else
            {
                var res = await elavonService.StartTransaction(p_args_dto.paymentArgs);

                return Ok(new
                {
                    response = res
                });
            }
        }

        [HttpPost]
        [Route("LinkedRefund")]
        public async Task<IActionResult> LinkedRefund([FromBody] LinkedRefundRequestDto request)
        {
            var res = await elavonService.LinkedRefund(request);
            return Ok(new
            {
                response = res
            });
        } 
    }
}
