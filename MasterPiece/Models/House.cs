using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class House
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? LocationName { get; set; }

    public string? TypeName { get; set; }

    public decimal? Price { get; set; }

    public bool? Available { get; set; }

    public int? MaxGuests { get; set; }

    public string? ImageUrl { get; set; }

    public DateOnly? AvailableFrom { get; set; }

    public DateOnly? AvailableTo { get; set; }

    public int? Stars { get; set; }

    public decimal? Rate { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<HouseBooking> HouseBookings { get; set; } = new List<HouseBooking>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
