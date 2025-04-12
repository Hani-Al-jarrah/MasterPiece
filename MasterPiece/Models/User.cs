using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Role { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<HouseBooking> HouseBookings { get; set; } = new List<HouseBooking>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}
