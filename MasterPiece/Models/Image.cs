using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class Image
{
    public int Id { get; set; }

    public int? HouseId { get; set; }

    public int? TourId { get; set; }

    public string? ImageUrl1 { get; set; }

    public string? ImageUrl2 { get; set; }

    public string? ImageUrl3 { get; set; }

    public virtual House? House { get; set; }

    public virtual Tour? Tour { get; set; }
}
