using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace RestaurantSystem.Models;

public partial class WasteItem
{
    public int Id { get; set; }

    public int WasteRecordId { get; set; }

    public int RawItemId { get; set; }
    [Precision(18, 2)]
    public decimal Quantity { get; set; }

    public virtual RawItem RawItem { get; set; } = null!;

    public virtual WasteRecord WasteRecord { get; set; } = null!;
}
