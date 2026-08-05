using System;
using System.Threading.Tasks;
using TC1.RepairShop.Domain.Quotes;

namespace TC1.RepairShop.Application.Quotes.UseCases;

public class RejectQuoteUseCase
{
    private readonly IQuoteRepository _quoteRepository;

    public RejectQuoteUseCase(IQuoteRepository quoteRepository)
    {
        _quoteRepository = quoteRepository;
    }

    public async Task ExecuteAsync(Guid quoteId)
    {
        var quote = await _quoteRepository.GetByIdAsync(quoteId);
        if (quote == null) throw new InvalidOperationException("Orçamento não encontrado.");

        quote.Reject();
        await _quoteRepository.UpdateAsync(quote);
    }
}
