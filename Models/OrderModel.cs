using orderApi.Enums;

namespace orderApi.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "Pendente";

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
