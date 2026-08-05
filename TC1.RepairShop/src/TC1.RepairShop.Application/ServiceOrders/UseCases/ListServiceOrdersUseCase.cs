using System.Collections.Generic;
using System.Threading.Tasks;
using TC1.RepairShop.Domain.ServiceOrders;

namespace TC1.RepairShop.Application.ServiceOrders.UseCases;

public class ListServiceOrdersUseCase
{
    private readonly IServiceOrderRepository _repository;

    public ListServiceOrdersUseCase(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ServiceOrder>> ExecuteAsync()
    {
        return await _repository.ListAsync();
    }
}
