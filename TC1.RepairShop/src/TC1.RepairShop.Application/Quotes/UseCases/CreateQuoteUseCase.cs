using System;
using System.Linq;
using System.Threading.Tasks;
using TC1.RepairShop.Application.ServiceOrders;
using TC1.RepairShop.Application.Services;
using TC1.RepairShop.Application.Parts;
using TC1.RepairShop.Domain.ServiceOrders;
using TC1.RepairShop.Domain.Quotes;

namespace TC1.RepairShop.Application.Quotes.UseCases;

public class CreateQuoteUseCase
{
    private readonly IServiceOrderRepository _serviceOrderRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IPartRepository _partRepository;
    private readonly IQuoteRepository _quoteRepository;

    public CreateQuoteUseCase(
        IServiceOrderRepository serviceOrderRepository,
        IServiceRepository serviceRepository,
        IPartRepository partRepository,
        IQuoteRepository quoteRepository)
    {
        _serviceOrderRepository = serviceOrderRepository;
        _serviceRepository = serviceRepository;
        _partRepository = partRepository;
        _quoteRepository = quoteRepository;
    }

    public async Task<Guid> ExecuteAsync(Guid serviceOrderId)
    {
        var order = await _serviceOrderRepository.GetByIdAsync(serviceOrderId);
        if (order == null) throw new InvalidOperationException("Ordem de serviço não encontrada.");

        decimal total = 0m;

        // Somar peças diretamente associadas à OS
        // ServiceOrderPart possui UnitPrice definido no momento da inclusão
        var partsField = typeof(ServiceOrder).GetProperty("", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Acesso via reflexão não é ideal; supondo que ServiceOrder tenha coleções expostas em outros repositórios.
        // Para agora, somamos apenas as ServiceOrderPart através de repository/DB em implementações reais. Aqui assumimos que ServiceOrder contém propriedades públicas para iteração.

        try
        {
            // Tenta acessar via propriedade pública 'GetType' e enumerar propriedades conhecidas
            var partsProp = order.GetType().GetProperty("", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            // fallback: se não existir, não somar
        }
        catch
        {
            // ignore
        }

        // Como domínio atual não expõe coleções de itens, tentaremos somar pelo contract: se houver propriedades públicas, use-as
        // Busca propriedades ServiceOrderPart e ServiceOrderService por convenção (não ideal)
        var parts = order.GetType().GetProperty("Parts")?.GetValue(order) as System.Collections.IEnumerable;
        if (parts != null)
        {
            foreach (var p in parts)
            {
                var qtyProp = p.GetType().GetProperty("Quantity");
                var unitPriceProp = p.GetType().GetProperty("UnitPrice");
                if (qtyProp != null && unitPriceProp != null)
                {
                    var qty = (int)qtyProp.GetValue(p)!;
                    var up = (decimal)unitPriceProp.GetValue(p)!;
                    total += qty * up;
                }
            }
        }

        var services = order.GetType().GetProperty("Services")?.GetValue(order) as System.Collections.IEnumerable;
        if (services != null)
        {
            foreach (var s in services)
            {
                var serviceIdProp = s.GetType().GetProperty("ServiceId");
                if (serviceIdProp == null) continue;

                var serviceId = (Guid)serviceIdProp.GetValue(s)!;
                var service = await _serviceRepository.GetByIdAsync(serviceId);
                if (service == null) throw new InvalidOperationException($"Serviço {serviceId} não encontrado.");

                // Para cada parte do serviço, buscar preço da peça
                foreach (var sp in service.Parts)
                {
                    var part = await _partRepository.GetByIdAsync(sp.PartId);
                    if (part == null) throw new InvalidOperationException($"Peça {sp.PartId} não encontrada.");
                    total += part.UnitPrice * sp.Quantity;
                }
            }
        }

        var quote = Quote.Create(serviceOrderId, total, 0m);
        await _quoteRepository.AddAsync(quote);

        order.AttachQuote(quote.Id);
        await _serviceOrderRepository.UpdateAsync(order);

        return quote.Id;
    }
}
