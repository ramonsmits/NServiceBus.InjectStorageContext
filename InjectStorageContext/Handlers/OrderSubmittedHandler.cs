using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

public class StoreOrderHandler : IHandleMessages<OrderSubmitted>
{
    private IOrderRepository orderRepository;

    public StoreOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    public async Task Handle(OrderSubmitted message, IMessageHandlerContext context)
    {
        LogManager.GetLogger<StoreOrderHandler>().InfoFormat("Handling order: {0}", message.OrderId);
        var orderAccepted = new Order
        {
            OrderId = new Guid(context.MessageId),
            Value = message.Value
        };

        await orderRepository.Add(orderAccepted).ConfigureAwait(false);
    }
}
