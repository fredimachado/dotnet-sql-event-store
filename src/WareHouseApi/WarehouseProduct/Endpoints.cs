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

            return result.Match(
                product => Results.CreatedAtRoute("GetWareHouseProductById", new { id = product.Id }),
                error => Results.BadRequest());
        })
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/receive", async (IMediator mediator, ReceiveWarehouseProductCommand command) =>
        {
            var result = await mediator.Send(command);

            return result.Match(
                product => Results.AcceptedAtRoute("GetWareHouseProductById", new { id = product.Id }),
                error => Results.BadRequest());
        })
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/ship", async (IMediator mediator, ShipWarehouseProductCommand command) =>
        {
            var result = await mediator.Send(command);

            return result.Match(
                product => Results.AcceptedAtRoute("GetWareHouseProductById", new { id = product.Id }),
                error => Results.BadRequest());
        })
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/adjust-inventory", async (IMediator mediator, AdjustInventoryCommand command) =>
        {
            var result = await mediator.Send(command);

            return result.Match(
                product => Results.AcceptedAtRoute("GetWareHouseProductById", new { id = product.Id }),
                error => Results.BadRequest());
        })
        .Produces(StatusCodes.Status202Accepted)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
