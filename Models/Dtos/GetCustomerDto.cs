using SmartQueueApi.Models.Enums;

namespace SmartQueueApi.Models.Dtos
{
    public class GetCustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
