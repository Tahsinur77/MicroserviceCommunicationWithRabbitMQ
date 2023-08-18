using MassTransit;
using Microsoft.Extensions.Options;
using Share;

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
