using CWSWrapper;

namespace ElavonTesting.DTOs
{
    public class PaymentArgsDto
    {
        public PaymentArgs paymentArgs {  get; set; }

        public string errorMessage { get; set; }

        public PaymentArgsDto() { 
            paymentArgs = new PaymentArgs();
            errorMessage = string.Empty;
        }
    }
}
