using System;
using System.Collections.Generic;

namespace MasterPiece.Models;

public partial class Wishlist
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? HouseId { get; set; }

    public virtual House? House { get; set; }

    public virtual User? User { get; set; }
}
