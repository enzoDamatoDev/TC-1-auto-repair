using TC1.RepairShop.Domain.Parts;

namespace TC1.RepairShop.Application.Parts;

public interface IPartRepository
{
    Task<Part?> GetByIdAsync(Guid id);
}
