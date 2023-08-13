using MediatR;

namespace WareHouseApi.WarehouseProduct;

public static class Endpoints
{
    public static void MapWarehouseProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/warehouseproduct");

        group.MapGet("{id:guid}", async (IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetByIdQuery(id));

            return result is not null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetWareHouseProductById")
        .Produces<WarehouseProductDto>()
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", async (IMediator mediator, CreateWarehouseProductCommand command) =>
        {
            var result = await mediator.Send(command);

            return Results.CreatedAtRoute("GetWareHouseProductById", new { id = result });
        })
        .Produces(StatusCodes.Status201Created);
    }
}
