using Microsoft.EntityFrameworkCore;
using orderApi.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using orderApi.Data;
using orderApi.Models;
using static orderApi.Enums.OrderStatusEnum;
namespace orderApi.Repository
{
    
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDBContext _dbContext;

        public OrderRepository(OrderDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OrderModel> GetOrderByIdAsync(int id)
        {
            return await _dbContext.Orders.FindAsync(id);
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync()
        {
            return await _dbContext.Orders.ToListAsync();
        }

        public async Task AddOrderAsync(OrderModel order)
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();
        }
       

    }

}
