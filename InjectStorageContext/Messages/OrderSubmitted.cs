using System;
using NServiceBus;

public class OrderSubmitted : IMessage
{
    public Guid OrderId { get; set; }
    public string Value { get; set; }
}
