using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartQueueApi.Infrastructure.Context;
using SmartQueueApi.Models;
using SmartQueueApi.Models.Dtos;
using SmartQueueApi.Responses;

namespace SmartQueueApi.Services.Implementations
{
    public class CustomerService(AppDbContext context) : ICustomerService
    {
        public async Task<ServiceResult> CallNextCustomerAsync()
        {
            IDbContextTransaction dbTransaction = null;
            try
            {
                dbTransaction = await context.Database.BeginTransactionAsync();

                var customer = await context.Customers.FromSqlRaw("""
                    Select * from customers where status = 0 order by created_at asc , id desc  for update  skip locked limit 1
                    """).FirstOrDefaultAsync();

                if(customer is null)
                {
                    await dbTransaction.RollbackAsync();
                    return ServiceResult.Fail(StatusCodes.Status404NotFound, "NoCustomersAreInQueue");
                }
                customer.Status = Models.Enums.CustomerStatus.Serving;

                await context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return ServiceResult.Success(StatusCodes.Status204NoContent);
            }
            catch (Exception)
            {
                if (dbTransaction is not null)
                {
                    await dbTransaction.RollbackAsync();
                }
                throw;
            }
            finally
            {
                if(dbTransaction is not null)
                {
                    await dbTransaction.DisposeAsync();
                }
            }
        }

        public async Task<ServiceResult> CreateCustomersAsync(string name)
        {
            if (string.IsNullOrEmpty(name)){
                return ServiceResult.Fail(StatusCodes.Status400BadRequest, "NameIsRequired");
            }

            Customer customer = new Customer
            {
                Name = name,
                Status = Models.Enums.CustomerStatus.Waiting,
                CreatedAt = DateTime.UtcNow.AddHours(4),

            };
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();

            return ServiceResult.Success(StatusCodes.Status201Created);

        }

        public async Task<ServiceResult> DeleteCustomerAsync(int id)
        {
            var customer = await context.Customers.Where(e => e.Id == id).FirstOrDefaultAsync();
            if (customer == null) return ServiceResult.Fail(StatusCodes.Status404NotFound, "CustomerNotFound");

            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return ServiceResult.Success(StatusCodes.Status204NoContent);
        }

        public async Task<ServiceResult> GetAllWaitingCustomersAsync()
        {
            var customers = await context.Customers.AsNoTracking().Where(e => e.Status == Models.Enums.CustomerStatus.Waiting).Select(e => new GetCustomerDto
            {
                Id = e.Id,
                Name = e.Name,
                CreatedAt = e.CreatedAt,
                Status = e.Status.ToString(),
            }).ToListAsync();

            return ServiceResult.Success(StatusCodes.Status200OK,customers);
        }

        public async Task<ServiceResult> GetCustomerQueuePositionAsync(int id)
        {
            var doesCustomerExist = await context.Customers.AsNoTracking().Where(e => e.Id ==id && e.Status == Models.Enums.CustomerStatus.Waiting).AnyAsync();
            if (!doesCustomerExist) return ServiceResult.Fail(StatusCodes.Status400BadRequest, "CustomerDoesNotExist");

            int position = await context.Customers.AsNoTracking().Where(e => e.Status == Models.Enums.CustomerStatus.Waiting && e.Id <= id).OrderBy(e => e.CreatedAt).ThenByDescending(e => e.Id).CountAsync();
            return ServiceResult.Success(StatusCodes.Status200OK, position);
        }
    }
}
