using System;
using TC1.RepairShop.Application.ServiceOrders;
using TC1.RepairShop.Application.ServiceOrders.UseCases;
using TC1.RepairShop.Domain.Common;
using TC1.RepairShop.Domain.ServiceOrders;
using Xunit;

namespace TC1.RepairShop.UnitTests.ServiceOrders;

public class ServiceOrderTests
{
    private class InMemoryServiceOrderRepository : IServiceOrderRepository
    {
        private readonly Dictionary<Guid, ServiceOrder> _store = new();

        public Task AddAsync(ServiceOrder order)
        {
            _store[order.Id] = order;
            return Task.CompletedTask;
        }

        public Task<ServiceOrder?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task<IEnumerable<ServiceOrder>> ListAsync()
        {
            return Task.FromResult<IEnumerable<ServiceOrder>>(_store.Values.ToList());
        }

        public Task UpdateAsync(ServiceOrder order)
        {
            _store[order.Id] = order;
            return Task.CompletedTask;
        }
    }
    [Fact]
    public void Create_ShouldInitializeServiceOrder()
    {
        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();
        var order = ServiceOrder.Create(customerId, vehicleId);

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(customerId, order.CustomerId);
        Assert.Equal(vehicleId, order.VehicleId);
        Assert.Equal(ServiceOrderStatus.Received, order.OrderStatusValue);
        Assert.Equal(Status.Active, order.Status);
        Assert.True(order.OpenedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void AttachQuote_ShouldSetQuoteId()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var quoteId = Guid.NewGuid();

        order.AttachQuote(quoteId);

        Assert.Equal(quoteId, order.QuoteId);
    }

    [Fact]
    public void AdvanceTo_ShouldUpdateStatusAndSetCompletedAt_WhenDelivered()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        order.AdvanceTo(ServiceOrderStatus.InProgress);
        Assert.Equal(ServiceOrderStatus.InProgress, order.OrderStatusValue);

        order.AdvanceTo(ServiceOrderStatus.Delivered);
        Assert.Equal(ServiceOrderStatus.Delivered, order.OrderStatusValue);
        Assert.NotNull(order.CompletedAt);
        Assert.True(order.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Delete_ShouldSetStatusDeleted()
    {
        var order = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());

        order.Delete();

        Assert.Equal(Status.Deleted, order.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCreateAndPersistServiceOrder()
    {
        var repo = new InMemoryServiceOrderRepository();
        var useCase = new OpenServiceOrderUseCase(repo);

        var customerId = Guid.NewGuid();
        var vehicleId = Guid.NewGuid();

        var id = await useCase.ExecuteAsync(customerId, vehicleId);

        Assert.NotEqual(Guid.Empty, id);

        var persisted = await repo.GetByIdAsync(id);
        Assert.NotNull(persisted);
        Assert.Equal(customerId, persisted!.CustomerId);
        Assert.Equal(vehicleId, persisted.VehicleId);
        Assert.Equal(ServiceOrderStatus.Received, persisted.OrderStatusValue);
    }

    [Fact]
    public async Task ListServiceOrdersUseCase_ShouldReturnItems()
    {
        var repo = new InMemoryServiceOrderRepository();
        // add some orders
        var o1 = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        var o2 = ServiceOrder.Create(Guid.NewGuid(), Guid.NewGuid());
        await repo.AddAsync(o1);
        await repo.AddAsync(o2);

        var listUseCase = new TC1.RepairShop.Application.ServiceOrders.UseCases.ListServiceOrdersUseCase(repo);
        var items = await listUseCase.ExecuteAsync();

        Assert.NotNull(items);
        Assert.Equal(2, items.Count());
    }
}
