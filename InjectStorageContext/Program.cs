using System;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NServiceBus;
using NServiceBus.Persistence;
using Configuration = NHibernate.Cfg.Configuration;

class Program
{
    private static string Title = "InjectStorageContext";
    public const string TransportConnectionString   = @"Server=localhost;Database=nservicebus;Trusted_Connection=True;App=Transport"; // Using different App values prevents lightweight transactions
    public const string PersistenceConnectionString = @"Server=localhost;Database=nservicebus;Trusted_Connection=True;App=Persistence";

    static async Task Main()
    {
        Console.Title = Title;

        var endpointConfiguration = new EndpointConfiguration(Title);
        endpointConfiguration.UseSerialization<JsonSerializer>();

        // Transport

        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(TransportConnectionString);

        // Persistence

        var hibernateConfig = new Configuration();
        hibernateConfig.DataBaseIntegration(x =>
        {
            x.ConnectionString = PersistenceConnectionString;
            x.Dialect<MsSql2012Dialect>();
        });
        endpointConfiguration.UsePersistence<NHibernatePersistence>().UseConfiguration(hibernateConfig);

        endpointConfiguration.EnableOutbox();

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        // Dependency injection registrations

        endpointConfiguration.RegisterComponents(registration: x =>
        {
            x.ConfigureComponent<OrderRepository>(DependencyLifecycle.InstancePerUnitOfWork);
            x.ConfigureComponent<StorageContext>(DependencyLifecycle.InstancePerUnitOfWork);
        });

        // Pipeline

        endpointConfiguration.Pipeline.Register<StorageContextBehavior.Registration>();

        endpointConfiguration.EnableInstallers();


        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        try
        {
            Console.WriteLine("Press ESC key to exit, any other key to send message");

            while (Console.ReadKey().Key != ConsoleKey.Escape)
            {
                Console.WriteLine("Sending message...");
                var orderSubmitted = new OrderSubmitted
                {
                    OrderId = Guid.NewGuid(),
                    Value = DateTime.UtcNow.ToLongTimeString()
                };
                await endpointInstance.SendLocal(orderSubmitted)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}