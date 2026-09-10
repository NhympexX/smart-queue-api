
using Microsoft.AspNetCore.Builder;
using SmartQueueApi.Handlers;
using SmartQueueApi.Infrastructure;
using SmartQueueApi.Services;
using SmartQueueApi.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<ICustomerService,CustomerService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.RegisterDatabaseServices(builder.Configuration);
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseAuthorization();

await app.Services.InitialiseDatabaseAsync();

app.MapControllers();

app.Run();
        
    

