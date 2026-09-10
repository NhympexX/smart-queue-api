using SmartQueueApi.Models.Enums;

namespace SmartQueueApi.Dtos
{
    public class GetCustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public CustomerStatus Status { get; set; }
    }
}
