using Entities;
using Entities.DTO;
using Entities.Models;

namespace ECommerce_API.Services.Abstract
{
    public interface IProductService
    {
        Task<ServiceResponse<ProductDTO>> AddProduct(ProductDTO product); //Ekleme

        Task<ServiceResponse<int>> DeleteProduct(int id); // silme

        Task<ServiceResponse<ProductDTO>> UpdateProduct(ProductDTO productDTO); //güncelleme

        Task<ServiceResponse<IEnumerable<ProductDTO>>> GetAll(); //tüm öğeleri döndürmek için Ienumerable kullanıoruz.

        Task<ServiceResponse<ProductDTO>> GetProduct(string productName); // ilgili product dönsün

        Task<ServiceResponse<List<Product>>> GetProductsByCategory(int categoryId); //kategoriye ait ürünleri döndürüyoruz.

        Task<ServiceResponse<List<Product>>> SearchProducts(string searchText); //arama çubugu

        Task<ServiceResponse<ProductSearchResult>> GetProducts(int page); // Pagination
    }
}
