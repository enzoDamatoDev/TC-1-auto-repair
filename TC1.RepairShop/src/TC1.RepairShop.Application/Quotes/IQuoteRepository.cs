using TC1.RepairShop.Domain.Quotes;

namespace TC1.RepairShop.Application.Quotes;

public interface IQuoteRepository
{

    Task AddAsync(Quote quote);
    Task<Quote?> GetByIdAsync(Guid id);
    Task UpdateAsync(Quote quote);
    Task<IEnumerable<Quote>> ListByServiceOrderIdAsync(Guid serviceOrderId);
}
