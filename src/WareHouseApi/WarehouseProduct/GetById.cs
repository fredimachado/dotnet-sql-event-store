using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record GetByIdQuery(Guid Id) : IRequest<WarehouseProductDto>;

public class GetByIdHandler : IRequestHandler<GetByIdQuery, WarehouseProductDto>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public GetByIdHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<WarehouseProductDto> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouseProduct = await _repository.GetByIdAsync(request.Id, cancellationToken);

        return warehouseProduct is not null
            ? new WarehouseProductDto(warehouseProduct.Id, warehouseProduct.QuantityOnHand)
            : null;
    }
}
