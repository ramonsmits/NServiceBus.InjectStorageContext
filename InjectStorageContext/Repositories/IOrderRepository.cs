using System.Threading.Tasks;

public interface IOrderRepository
{
    Task Add(Order entity);
}