using System;
using System.Collections.Generic;

namespace SnowStoreWeb.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int? StockQuantity { get; set; }

    public bool? IsHot { get; set; }

    public bool? IsBestSeller { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? BrandId { get; set; }

    public virtual Brand? Brand { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
