using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class WasteRecord
{
    public int Id { get; set; }

    public DateTime WasteDate { get; set; }

    public string Reason { get; set; } = null!;

    public virtual ICollection<WasteItem> WasteItems { get; set; } = new List<WasteItem>();
}
