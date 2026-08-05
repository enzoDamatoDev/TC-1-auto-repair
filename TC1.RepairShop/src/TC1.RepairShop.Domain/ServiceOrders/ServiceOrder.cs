using TC1.RepairShop.Domain.Common;

namespace TC1.RepairShop.Domain.ServiceOrders;

public class ServiceOrder
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid VehicleId { get; private set; }
    public ServiceOrderStatus OrderStatusValue { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? QuoteId { get; private set; }
    public Status Status { get; private set; }

    private ServiceOrder()
    {
    }

    public static ServiceOrder Create(Guid customerId, Guid vehicleId)
    {
        return new ServiceOrder
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            VehicleId = vehicleId,
            OrderStatusValue = ServiceOrderStatus.Received,
            OpenedAt = DateTime.UtcNow,
            Status = Status.Active,
        };
    }

    public void AttachQuote(Guid quoteId)
    {
        QuoteId = quoteId;
    }

    public void AdvanceTo(ServiceOrderStatus newStatus)
    {
        // Valida transições de estado permitidas
        var allowed = new List<(ServiceOrderStatus From, ServiceOrderStatus To)>
        {
            (ServiceOrderStatus.Received, ServiceOrderStatus.UnderDiagnosis),
            (ServiceOrderStatus.Received, ServiceOrderStatus.InProgress),
            (ServiceOrderStatus.UnderDiagnosis, ServiceOrderStatus.AwaitingApproval),
            (ServiceOrderStatus.UnderDiagnosis, ServiceOrderStatus.InProgress),
            (ServiceOrderStatus.AwaitingApproval, ServiceOrderStatus.InProgress),
            (ServiceOrderStatus.InProgress, ServiceOrderStatus.Completed),
            (ServiceOrderStatus.InProgress, ServiceOrderStatus.Delivered),
            (ServiceOrderStatus.Completed, ServiceOrderStatus.Delivered),
        };

        var current = OrderStatusValue;
        if (!allowed.Contains((current, newStatus)))
        {
            throw new InvalidOperationException($"Transição inválida de {current} para {newStatus}");
        }

        OrderStatusValue = newStatus;

        if (newStatus == ServiceOrderStatus.Completed || newStatus == ServiceOrderStatus.Delivered)
        {
            CompletedAt = DateTime.UtcNow;
        }
    }

    public void Delete()
    {
        Status = Status.Deleted;
    }
}
