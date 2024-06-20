namespace ElavonTesting.DTOs
{
    public class LinkedRefundRequestDto
    {
        public string? paymetGatewayId { get; set; }
        public string? originalTransId { get; set; }
        public long amount { get; set; }
    }
}
