using System;
using System.Data;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using NServiceBus.Logging;

public class OrderRepository : IOrderRepository
{
    readonly StorageContext ctxProvider;
    IDbConnection Connection => ctxProvider.Connection;
    IDbTransaction Transaction => ctxProvider.Transaction;

    public OrderRepository(StorageContext ctxProvider)
    {
        this.ctxProvider = ctxProvider;
    }

    public Task Add(Order entity)
    {
        LogManager.GetLogger<OrderRepository>().InfoFormat("Inserting entity...");
        return Connection.InsertAsync(entity, Transaction);
    }
}