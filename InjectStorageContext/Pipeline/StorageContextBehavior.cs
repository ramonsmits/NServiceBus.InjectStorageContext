using System;
using System.Data;
using System.Threading.Tasks;
using NHibernate;
using NServiceBus;
using NServiceBus.Pipeline;

public class StorageContextBehavior : Behavior<IInvokeHandlerContext>
{
    public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var builder = context.Builder;
        var storageContext = builder.Build<StorageContext>();

        try
        {
            // Get the NHibernate session from the context to access the connection used
            // by NHibernate. In order to get the transaction, we create a dummy command
            // and Enlist it in the NHibernate session, after that, the Transaction
            // property on the SqlCommand is set and copy these into the StorageContext.

            var session = context.SynchronizedStorageSession.Session();
            var connection = session.Connection;
            IDbTransaction transaction;

            using (var helper = connection.CreateCommand())
            {
                session.GetCurrentTransaction().Enlist(helper); // Cannot return null, as transaction is always created.
                transaction = helper.Transaction;
            }

            storageContext.Connection = connection;
            storageContext.Transaction = transaction;
            await next().ConfigureAwait(false);
        }
        finally
        {
            builder.Release(storageContext);
        }
    }

    public class Registration : RegisterStep
    {
        public Registration()
            : base(typeof(StorageContextBehavior).Name, typeof(StorageContextBehavior), "Database context")
        {
        }
    }
}
