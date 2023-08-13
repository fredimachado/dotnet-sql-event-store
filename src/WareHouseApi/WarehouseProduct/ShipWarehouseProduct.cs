using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record ShipWarehouseProductCommand(Guid id, int Quantity) : IRequest<Result<WarehouseProductDto, ValidationResult>>;

public class ShipWarehouseProductCommandHandler : IRequestHandler<ShipWarehouseProductCommand, Result<WarehouseProductDto, ValidationResult>>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public ShipWarehouseProductCommandHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WarehouseProductDto, ValidationResult>> Handle(ShipWarehouseProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ValidationResult();
        }

        var warehouseProduct = await _repository.GetByIdAsync(request.id, cancellationToken);

        if (warehouseProduct is null)
        {
            return (WarehouseProductDto)null;
        }

        warehouseProduct.ShipProduct(request.Quantity);

        await _repository.SaveAsync(warehouseProduct);

        return new WarehouseProductDto(warehouseProduct.Id, warehouseProduct.QuantityOnHand);
    }
}
