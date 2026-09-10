using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SmartQueueApi.Services;

namespace SmartQueueApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController(ICustomerService customerService) : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreateCustomerAsync([FromBody]string name)
        {
            return await customerService.CreateCustomersAsync(name);
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomersAsync()
        {
            return await customerService.GetAllWaitingCustomersAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerPosition(int id)
        {
            return await customerService.GetCustomerQueuePositionAsync(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomerAsync(int id)
        {
            return await customerService.DeleteCustomerAsync(id);
        }

        [HttpPost("next")]
        public async Task<IActionResult> CallNextCustomerAsync()
        {
            return await customerService.CallNextCustomerAsync();
        }

    }
}
