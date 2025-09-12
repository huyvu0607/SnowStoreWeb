using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SnowStoreWeb.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int? StockQuantity { get; set; }

    public bool? IsHot { get; set; }

    public bool? IsBestSeller { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public Product()
    {
        IsHot = false;
        IsBestSeller = false;
        CreatedDate = DateTime.Now; // Nếu cần giá trị mặc định cho CreatedDate
    }
}

