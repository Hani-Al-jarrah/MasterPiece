using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class TourBooking
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? TourId { get; set; }

    public DateTime? BookingDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Tour? Tour { get; set; }

    public virtual User? User { get; set; }
}
