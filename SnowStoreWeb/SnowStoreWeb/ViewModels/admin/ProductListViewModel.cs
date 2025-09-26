namespace SnowStoreWeb.ViewModels.admin
{
    using SnowStoreWeb.Models;

    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages
        {
            get
            {
                return (int)Math.Ceiling(TotalCount / (double)PageSize);
            }
        }

        // Filter properties
        public string SearchTerm { get; set; } = "";
        public int? ProductId { get; set; }  // New property for Product ID search
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string PriceRange { get; set; } = "";
        public string Status { get; set; } = "";
        public string SortBy { get; set; } = "name";

        // Helper properties for pagination
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => (CurrentPage - 1) * PageSize + 1;
        public int EndItem => Math.Min(CurrentPage * PageSize, TotalCount);
    }
}
