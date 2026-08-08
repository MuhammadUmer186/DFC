using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class KitchenOut
{
    public int Id { get; set; }

    public DateTime IssuedAt { get; set; }

    public virtual ICollection<KitchenOutItem> KitchenOutItems { get; set; } = new List<KitchenOutItem>();
}
