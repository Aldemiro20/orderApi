using orderApi.Services;
using RabbitMQ.Client;
using Microsoft.EntityFrameworkCore;
using orderApi.Data;
using orderApi.Repository.Interfaces;
using orderApi.Controllers;
using orderApi.Repository;



var builder = WebApplication.CreateBuilder(args);

// Configurar RabbitMQ ConnectionFactory
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var factory = new ConnectionFactory
    {
        HostName = "rabbitmq", 
        Port = 5672,           
        UserName = "guest",    
        Password = "guest"     
    };
    return factory;
});

builder.Services.AddControllers()
       .AddNewtonsoftJson(options =>
       {
           options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
       });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddEntityFrameworkSqlServer()
.AddDbContext<OrderDBContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("DataBase"))
                );

builder.Services.AddControllers();
builder.Services.AddSingleton<RabbitMQProducer>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations(); 
});
var redisConnectionString = builder.Configuration.GetSection("RedisSettings:ConnectionString").Value;
builder.Services.AddSingleton(new RedisService(redisConnectionString));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddHostedService<RabbitMQConsumerService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDBContext>();
    dbContext.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
