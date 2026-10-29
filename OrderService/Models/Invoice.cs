namespace OrderService.Models;

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal TotalAmount { get; set; }

    public Order Order { get; set; } = null!;
}
