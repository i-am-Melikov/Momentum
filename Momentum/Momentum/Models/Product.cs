﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Momentum.Models
{
    public class Product : BaseEntity
    {
        [StringLength(255)]
        public string Title { get; set; }
        [Column(TypeName = "money")]
        public double Price { get; set; }
        [Column(TypeName = "money")]
        public int DiscountedPrice { get; set; }
        [Column(TypeName = "money")]
        public double EcoTax { get; set; }
        [Range(0, int.MaxValue)]
        public int Count { get; set; }
        [StringLength(5000)]
        public string Description { get; set; }
        [StringLength(255)]
        public string? MainImage { get; set; }
        public List<ProductCategory>? ProductCategories { get; set; }
        public List<ProductColor>? ProductColor { get; set; }
        public bool IsTopSeller { get; set; }
        public bool IsOurProduct { get; set; }
        public List<ProductImage>? ProductImages { get; set; }
        [NotMapped]
        public IFormFile? MainFile { get; set; }
        [NotMapped]
        public IEnumerable<IFormFile>? Files { get; set; }
        [NotMapped]
        public List<int> CategoryIds { get; set; }
        public Product()
        {
            CategoryIds = new List<int>();
        }
    }
}
