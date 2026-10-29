namespace OrderService.DTOs;

public class ReceiptResponseDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
}
