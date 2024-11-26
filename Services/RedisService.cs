using StackExchange.Redis;
using System.Threading.Tasks;

namespace orderApi.Services
{
    public class RedisService
    {
        private readonly ConnectionMultiplexer _redis;

        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
        }
     
        public async Task SetValueAsync(string key, string value)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, value);
        }

        public async Task RemoveValueAsync(string key)
        {
            // Deleta a chave especificada no Redis
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key); // Remove a chave do Redis
        }
        public async Task<string> GetValueAsync(string key)
        {
            var db = _redis.GetDatabase();
            return await db.StringGetAsync(key);
        }
    }
}
