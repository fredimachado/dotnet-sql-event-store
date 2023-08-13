using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record AdjustInventoryCommand(Guid id, int Quantity, string Reason) : IRequest<Result<WarehouseProductDto, ValidationResult>>;

public class AdjustInventoryCommandHandler : IRequestHandler<AdjustInventoryCommand, Result<WarehouseProductDto, ValidationResult>>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public AdjustInventoryCommandHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WarehouseProductDto, ValidationResult>> Handle(AdjustInventoryCommand request, CancellationToken cancellationToken)
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

        warehouseProduct.AdjustInventory(request.Quantity, request.Reason);

        await _repository.SaveAsync(warehouseProduct);

        return new WarehouseProductDto(warehouseProduct.Id, warehouseProduct.QuantityOnHand);
    }
}
