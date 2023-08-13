using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record CreateWarehouseProductCommand(int Quantity) : IRequest<Result<WarehouseProductDto, ValidationResult>>;

public class CreateWarehouseProductHandler : IRequestHandler<CreateWarehouseProductCommand, Result<WarehouseProductDto, ValidationResult>>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public CreateWarehouseProductHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<Result<WarehouseProductDto, ValidationResult>> Handle(CreateWarehouseProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity <= 0)
        {
            return new ValidationResult();
        }

        var id = Guid.NewGuid();
        var warehouseProduct = new Domain.Entities.WarehouseProduct(id);
        warehouseProduct.ReceiveProduct(request.Quantity);

        await _repository.SaveAsync(warehouseProduct);

        return new WarehouseProductDto(warehouseProduct.Id, warehouseProduct.QuantityOnHand);
    }
}
