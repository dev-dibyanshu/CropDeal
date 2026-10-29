namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int DealerId { get; set; }
    public int FarmerId { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Invoice? Invoice { get; set; }
    public Receipt? Receipt { get; set; }
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
