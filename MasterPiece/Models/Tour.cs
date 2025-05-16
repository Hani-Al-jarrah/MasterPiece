using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MasterPiece.Models;

public partial class Tour
{
    public int Id { get; set; }

    public string? LocationName { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
	[RegularExpression(@"^\d+\s*(hour|hours|day|days)$", ErrorMessage = "Use format like '4 hours' or '2 days'.")]
	public string? Duration { get; set; }

    public decimal? Price { get; set; }

    public int? MaxGuests { get; set; }

    public DateOnly? StartDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public DateOnly? EndDate { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Image> Images { get; set; } = new List<Image>();

    public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
}
