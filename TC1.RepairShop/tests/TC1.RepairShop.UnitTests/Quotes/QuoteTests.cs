using System;
using TC1.RepairShop.Application.Quotes;
using TC1.RepairShop.Application.Quotes.UseCases;
using TC1.RepairShop.Domain.Common;
using TC1.RepairShop.Domain.Quotes;
using Xunit;

namespace TC1.RepairShop.UnitTests.Quotes;

public class QuoteTests
{


    private class InMemoryQuoteRepository : IQuoteRepository
    {
        private readonly Dictionary<Guid, Quote> _store = new();

        public Task AddAsync(Quote quote)
        {
            _store[quote.Id] = quote;
            return Task.CompletedTask;
        }

        public Task<Quote?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var q);
            return Task.FromResult(q);
        }

        public Task UpdateAsync(Quote quote)
        {
            _store[quote.Id] = quote;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Quote>> ListByServiceOrderIdAsync(Guid serviceOrderId)
        {
            var items = _store.Values.Where(x => x.ServiceOrderId == serviceOrderId);
            return Task.FromResult<IEnumerable<Quote>>(items);
        }
    }
    [Fact]
    public void Create_ShouldInitializeQuoteAndSetAmounts()
    {
        var serviceOrderId = Guid.NewGuid();
        var quote = Quote.Create(serviceOrderId, 1000m, 10m);

        Assert.NotEqual(Guid.Empty, quote.Id);
        Assert.Equal(serviceOrderId, quote.ServiceOrderId);
        Assert.Equal(1000m, quote.TotalAmount);
        Assert.Equal(10m, quote.Discount);
        Assert.Equal(900m, quote.FinalPrice);
        Assert.Equal(QuoteStatus.Draft, quote.QuoteStatusValue);
        Assert.Equal(0, quote.RejectionCount);
        Assert.Equal(Status.Active, quote.Status);
    }

    [Fact]
    public void SetAmount_ShouldUpdateAmounts()
    {
        var quote = Quote.Create(Guid.NewGuid(), 500m, 0);

        quote.SetAmount(800m, 5m);

        Assert.Equal(800m, quote.TotalAmount);
        Assert.Equal(5m, quote.Discount);
        Assert.Equal(760m, quote.FinalPrice);
    }

    [Fact]
    public void Reject_ShouldSetRejectedAndIncrementCount()
    {
        var quote = Quote.Create(Guid.NewGuid(), 200m, 0);

        quote.Reject();

        Assert.Equal(QuoteStatus.Rejected, quote.QuoteStatusValue);
        Assert.Equal(1, quote.RejectionCount);
    }

    [Fact]
    public void Approve_ShouldSetApproved()
    {
        var quote = Quote.Create(Guid.NewGuid(), 200m, 0);

        quote.Approve();

        Assert.Equal(QuoteStatus.Approved, quote.QuoteStatusValue);
    }

    [Fact]
    public void Delete_ShouldSetStatusDeleted()
    {
        var quote = Quote.Create(Guid.NewGuid(), 200m, 0);

        quote.Delete();

        Assert.Equal(Status.Deleted, quote.Status);
    }

    [Fact]
    public async Task ApproveQuoteUseCase_ShouldSetQuoteApproved()
    {
        var repo = new InMemoryQuoteRepository();
        var useCase = new ApproveQuoteUseCase(repo);

        var quote = Quote.Create(Guid.NewGuid(), 1000m, 0);
        await repo.AddAsync(quote);

        await useCase.ExecuteAsync(quote.Id);

        var persisted = await repo.GetByIdAsync(quote.Id);
        Assert.NotNull(persisted);
        Assert.Equal(QuoteStatus.Approved, persisted!.QuoteStatusValue);
    }

    [Fact]
    public async Task RejectQuoteUseCase_ShouldIncrementRejectionCountAndSetRejected()
    {
        var repo = new InMemoryQuoteRepository();
        var useCase = new RejectQuoteUseCase(repo);

        var quote = Quote.Create(Guid.NewGuid(), 500m, 0);
        await repo.AddAsync(quote);

        await useCase.ExecuteAsync(quote.Id);

        var persisted = await repo.GetByIdAsync(quote.Id);
        Assert.NotNull(persisted);
        Assert.Equal(1, persisted!.RejectionCount);
        Assert.Equal(QuoteStatus.Rejected, persisted.QuoteStatusValue);
    }

    [Fact]
    public async Task RejectQuoteUseCase_ShouldThrow_WhenExceedsLimit()
    {
        var repo = new InMemoryQuoteRepository();
        var useCase = new RejectQuoteUseCase(repo);

        var quote = Quote.Create(Guid.NewGuid(), 200m, 0);
        await repo.AddAsync(quote);

        // reject 3 times - allowed
        await useCase.ExecuteAsync(quote.Id);
        await useCase.ExecuteAsync(quote.Id);
        await useCase.ExecuteAsync(quote.Id);

        var persisted = await repo.GetByIdAsync(quote.Id);
        Assert.NotNull(persisted);
        Assert.Equal(3, persisted!.RejectionCount);

        // fourth rejection should throw
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await useCase.ExecuteAsync(quote.Id));
    }
}
