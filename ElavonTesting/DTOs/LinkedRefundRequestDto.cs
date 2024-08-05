namespace ElavonTesting.DTOs
{
    public class LinkedRefundRequestDto
    {
        public string? paymetGatewayId { get; set; }
        public string? originalTransId { get; set; }
        public double amount { get; set; }
        public string? tenderType { get; set; }
        public string? cardType { get; set; }
    }
}
