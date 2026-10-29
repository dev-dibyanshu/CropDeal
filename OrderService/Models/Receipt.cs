namespace OrderService.Models;

public class Receipt
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;

    public Order Order { get; set; } = null!;
}
