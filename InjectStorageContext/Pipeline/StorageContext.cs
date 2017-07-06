using System;
using System.Data;
using NServiceBus.Logging;

public class StorageContext : IDisposable
{
    public IDbConnection Connection;
    public IDbTransaction Transaction;

    static readonly ILog Log = LogManager.GetLogger<StorageContext>();
    static long _count;
    readonly long instance = ++_count;

    public StorageContext()
    {
        Log.Info($"Created: {instance}");
    }

    public void Dispose()
    {
        Log.Info($"Dispose: {instance}");
    }
}