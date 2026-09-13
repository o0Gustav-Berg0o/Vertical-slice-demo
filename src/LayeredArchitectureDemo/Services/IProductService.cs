using LayeredArchitectureDemo.Models.Dtos;

namespace LayeredArchitectureDemo.Services;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<List<ProductSummaryResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
}
