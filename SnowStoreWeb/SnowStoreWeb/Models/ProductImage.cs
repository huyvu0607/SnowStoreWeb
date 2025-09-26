using System;
using System.Collections.Generic;

namespace SnowStoreWeb.Models;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int? ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public int? SortOrder { get; set; }

    public virtual Product? Product { get; set; }
}
