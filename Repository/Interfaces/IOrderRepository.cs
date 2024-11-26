using orderApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace orderApi.Repository.Interfaces
{


    public interface IOrderRepository
    {
        Task<OrderModel> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync();
        Task AddOrderAsync(OrderModel order);
       // Task UpdateOrderStatusAsync(Guid id, string status);
         
    }
       
}
