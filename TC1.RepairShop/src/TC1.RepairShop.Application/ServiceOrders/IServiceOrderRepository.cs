using TC1.RepairShop.Domain.ServiceOrders;

namespace TC1.RepairShop.Application.ServiceOrders;

public interface IServiceOrderRepository
{
    Task AddAsync(ServiceOrder order);
    Task<ServiceOrder?> GetByIdAsync(Guid id);
    Task<IEnumerable<ServiceOrder>> ListAsync();
    Task UpdateAsync(ServiceOrder order);
}
