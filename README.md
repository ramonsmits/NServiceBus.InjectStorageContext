
# Sample: Inject Storage Context with DI

In NServiceBus 5 the `NHibernateStorageContext` storage context was registered in the container. In NServiceBus 6 this does not work anymore because the context is not registered in the container and cannot be injected via the constructor any more. It can only be passed as an argument meaning you have to pass the context as argument into the `IOrderRepository.Add(Order)` which is - obviously - not what you want if you use the repository pattern. This sample extends NServiceBus with behavior very similar to  `NHibernateStorageContext` which was available prior to NServiceBus 6.


# Solution

The pipeline is extended with a `StorageContextBehavior` behavior. It resolves the storage connection and transaction objects as would be needed in a regular V6 handler via the just before the handlers are invoked and assigns the values to the by the container created `StorageContext` object. The `StorageContext` object is registered as `DependencyLifecycle.InstancePerUnitOfWork` and the repository `OrderRepository` repository is dependant on it. When the handler object graph is created by the container, all potential repository objects will share the same `StorageContext` object instance. This way all created repository objects or any other object that would take a dependeny on `StorageContext` can now access the shared `IDbTransaction` and `IDbConnection` to make sure all queries participate on the same connection and unit of work.
