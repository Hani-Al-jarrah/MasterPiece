using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class HouseBooking
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? HouseId { get; set; }

    public DateOnly? CheckInDate { get; set; }

    public DateOnly? CheckOutDate { get; set; }

    public int? Guests { get; set; }

    public string? Status { get; set; }

    public DateTime? BookingDate { get; set; }

    public virtual House? House { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
