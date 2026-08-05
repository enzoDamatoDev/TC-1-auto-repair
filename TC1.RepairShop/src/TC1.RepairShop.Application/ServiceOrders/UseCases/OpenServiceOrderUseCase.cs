using System;
using System.Threading.Tasks;
using TC1.RepairShop.Domain.ServiceOrders;

namespace TC1.RepairShop.Application.ServiceOrders.UseCases;

public class OpenServiceOrderUseCase
{
    private readonly IServiceOrderRepository _repository;

    public OpenServiceOrderUseCase(IServiceOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> ExecuteAsync(Guid customerId, Guid vehicleId)
    {
        var order = ServiceOrder.Create(customerId, vehicleId);
        await _repository.AddAsync(order);
        return order.Id;
    }
}
