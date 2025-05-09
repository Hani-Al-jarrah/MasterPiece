using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? HouseId { get; set; }

    public int? TourId { get; set; }

    public int? Rating { get; set; }

    public string? Email { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual House? House { get; set; }

    public virtual Tour? Tour { get; set; }

    public virtual User? User { get; set; }
}
