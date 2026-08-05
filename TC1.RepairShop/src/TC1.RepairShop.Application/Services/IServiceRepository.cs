using TC1.RepairShop.Domain.Services;

namespace TC1.RepairShop.Application.Services;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id);
}
