using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record ReceiveWarehouseProductCommand(Guid id, int Quantity) : IRequest<Result<WarehouseProductDto, ValidationResult>>;

public class ReceiveWarehouseProductCommandHandler : IRequestHandler<ReceiveWarehouseProductCommand, Result<WarehouseProductDto, ValidationResult>>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public ReceiveWarehouseProductCommandHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WarehouseProductDto, ValidationResult>> Handle(ReceiveWarehouseProductCommand request, CancellationToken cancellationToken)
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

        warehouseProduct.ReceiveProduct(request.Quantity);

        await _repository.SaveAsync(warehouseProduct);

        return new WarehouseProductDto(warehouseProduct.Id, warehouseProduct.QuantityOnHand);
    }
}
