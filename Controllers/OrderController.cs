using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using orderApi.Models;
using orderApi.Repository.Interfaces;
using orderApi.Services;
using orderApi.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace orderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private string cacheKey = "orders";
        private readonly RedisService _redisService;
        private readonly IOrderRepository _orderRepository;
        private readonly RabbitMQProducer _producer;

        public OrderController(IOrderRepository orderRepository, RabbitMQProducer producer, RedisService redisService)
        {
            _orderRepository = orderRepository;
            _producer = producer;
            _redisService = redisService;
        }

      
        [HttpPost("orders")]
        [SwaggerOperation(Summary = "Cria um novo pedido", Description = "Cria um pedido e envia mensagem para a fila no RabbitMQ.")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel order)
        {
            if (order == null)
            {
               
                return BadRequest(ApiResponse.ErrorResponse("Dados de pedido inválidos", 400));
            }

            await _orderRepository.AddOrderAsync(order);

            _producer.SendMessageAsync(order, "order_queue");
            
            await _redisService.RemoveValueAsync(cacheKey);
            return Ok(ApiResponse.SuccessResponse(order, "Pedido criado com sucesso"));
        }

   
        [HttpGet("orders/{id}")]
        [SwaggerOperation(Summary = "Lista pedido por ID", Description = "Lista os detalhes de um pedido pelo ID.")]
        public async Task<IActionResult> GetOrderStatus(int id)
        {
            var order = await _orderRepository.GetOrderByIdAsync(id);
            if (order == null)
            {
             
                return NotFound(ApiResponse.ErrorResponse("Pedido não encontrado", 404));
            }

           
            return Ok(ApiResponse.SuccessResponse(order));
        }

     
        [HttpGet("orders")]
        [SwaggerOperation(Summary = "Lista todos os pedidos", Description = "Lista todos os pedidos registrados.")]
        public async Task<IActionResult> GetOrders()
        {
           
            var cachedData = await _redisService.GetValueAsync(cacheKey);
            if (cachedData != null)
            {
                var orders = System.Text.Json.JsonSerializer.Deserialize<List<OrderModel>>(cachedData);
                return Ok(ApiResponse.SuccessResponse(orders));
            }

            var ordersFromDb = await _orderRepository.GetAllOrdersAsync();
            if (ordersFromDb == null || !ordersFromDb.Any())
            {
                return NotFound(ApiResponse.ErrorResponse("Nenhum pedido encontrada", 404));
            }
            await _redisService.SetValueAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(ordersFromDb));

            return Ok(ApiResponse.SuccessResponse(ordersFromDb));
        }
    }
}
