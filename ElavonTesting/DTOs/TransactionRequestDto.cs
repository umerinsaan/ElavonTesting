namespace ElavonTesting.DTOs
{
    public class TransactionRequestDto
    {
        public string? transactionType {  get; set; }
        public string? originalTransId { get; set; }
        public string? paymentGatewayId { get; set; }
        public string? chanId { get; set; }
        public long amount { get; set; }

    }
}
