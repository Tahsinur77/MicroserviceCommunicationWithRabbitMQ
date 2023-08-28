using MassTransit;
using Microsoft.Extensions.Options;
using Share;
using static MassTransit.Monitoring.Performance.BuiltInCounters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//add service to the container using Options Pattern 
builder.Services.Configure<RabbitMqConfiguration>( options => 
    builder.Configuration.GetSection("RabbitMq").Bind(options));

var rabbitMqConfig = builder.Services.BuildServiceProvider()
    .GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;

builder.Services.AddMassTransit(config =>
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMqConfig.Url), hst =>
        {
            hst.Username(rabbitMqConfig.User);
            hst.Password(rabbitMqConfig.Password);
        });

        //Temporary endpoint
        cfg.Publish<CustomerModel>(x =>
        {
            //durable bool Save messages to disk
            x.Durable = false;
            //autodelete  bool Delete when bus is stopped
            x.AutoDelete = true;
            //Direct exchanges route messages to queues based on an exact match of the routing key
            //Fanout exchanges route messages to all bound queues
            x.ExchangeType = "fanout";
            // x.Exclude = true; // do not create an exchange for this type
        });

    })
);


var app = builder.Build();

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
