using Microsoft.AspNetCore.Mvc;
using SmartQueueApi.Responses;

namespace SmartQueueApi.Services
{
    public interface ICustomerService
    {
        public Task<ServiceResult> CreateCustomersAsync(string name);
        public Task<ServiceResult> GetAllWaitingCustomersAsync();
        public Task<ServiceResult> DeleteCustomerAsync(int id);
        public Task<ServiceResult> GetCustomerQueuePositionAsync(int id);
        public Task<ServiceResult> CallNextCustomerAsync();
    }
}
