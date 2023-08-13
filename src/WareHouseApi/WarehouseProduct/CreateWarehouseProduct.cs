using MediatR;
using WareHouseApi.Infrastructure;

namespace WareHouseApi.WarehouseProduct;

public record CreateWarehouseProductCommand(int Quantity) : IRequest<Guid>;

public class CreateWarehouseProductHandler : IRequestHandler<CreateWarehouseProductCommand, Guid>
{
    private readonly Repository<Domain.Entities.WarehouseProduct> _repository;

    public CreateWarehouseProductHandler(Repository<Domain.Entities.WarehouseProduct> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateWarehouseProductCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var warehouseProduct = new Domain.Entities.WarehouseProduct(id);
        warehouseProduct.ReceiveProduct(request.Quantity);

        await _repository.SaveAsync(warehouseProduct);

        return id;
    }
}
