using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int? BookingId { get; set; }

    public int? TourBookingId { get; set; }

    public int? UserId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Status { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual HouseBooking? Booking { get; set; }

    public virtual TourBooking? TourBooking { get; set; }

    public virtual User? User { get; set; }
}
