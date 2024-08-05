namespace ElavonTesting.DTOs
{
    public class TransactionResponseDto
    {
        public string? transactionType { get; set; }
        public string? originalTransId { get; set; }
        public string? paymentGatewayId { get; set; }
        public string? chanId { get; set; }
        public string? result { get; set; }
        public string? approved { get; set; }
        public string? cardType { get; set; }
        public string? rawJSONresponse { get; set; }
    }
}
