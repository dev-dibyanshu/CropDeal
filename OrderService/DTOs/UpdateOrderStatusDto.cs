using System.ComponentModel.DataAnnotations;
using OrderService.Models;

namespace OrderService.DTOs;

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }
}
