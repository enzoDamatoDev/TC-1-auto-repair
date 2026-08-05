using TC1.RepairShop.Domain.Common;

namespace TC1.RepairShop.Domain.Services;

public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public List<ServicePart> Parts { get; private set; } = new List<ServicePart>();
    public Status Status { get; private set; }

    private Service()
    {
    }

    public static Service Create(string name, string description)
    {
        return new Service
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Status = Status.Active,
        };
    }

    public void AddPart(Guid partId, int quantity)
    {
        Parts.Add(ServicePart.Create(Id, partId, quantity));
    }

    public void Delete()
    {
        Status = Status.Deleted;
    }
}
