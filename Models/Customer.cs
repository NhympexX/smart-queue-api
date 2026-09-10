using SmartQueueApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartQueueApi.Models
{
    
    public class Customer
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public CustomerStatus Status { get; set; } 
    }
}
