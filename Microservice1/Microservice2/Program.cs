
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice2
{
    public class Program
    {
        public static void main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(app =>
            {
                //set the appsettings.json path to the app
                app.SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<RabbitMqConfiguration>(options =>
                    hostContext.Configuration.GetSection("RabbitMq").Bind(options));

                var rabbitMqConfig = services.BuildServiceProvider()
                    .GetRequiredService<IOptions<RabbitMqConfiguration>>().Value;
                var handledExceptionsTypes = hostContext.Configuration.GetSection("HandledExceptionsTypes")
                                                    .Get<string[]>();
                services.AddMassTransit(config =>
                {
                    config.AddConsumer<CustomerCreateConsumer>();

                    config.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(new Uri(rabbitMqConfig.Url), hst =>
                        {
                            hst.Username(rabbitMqConfig.User);
                            hst.Password(rabbitMqConfig.Password);

                        });

                        //cfg.ConfigureEndpoints(ctx);

                        cfg.ReceiveEndpoint(QueueService.CustomerCreateQueue, ep =>
                        {
                            ep.ConfigureConsumer<CustomerCreateConsumer>(context, c =>
                            {
                                c.UseMessageRetry(r =>
                                {
                                    ///retry configured with 3 times retry count and 3 seconds interval
                                    ///Retry policy configured with configuration
                                    r.Interval(rabbitMqConfig.RetryCount,
                                        TimeSpan.FromSeconds(rabbitMqConfig.RetryIntervalSecond));


                                    ///retry policy would not be applied during below types of exception
                                    foreach (var handledException in handledExceptionsTypes)
                                    {
                                        r.Handle(Type.GetType(handledException));
                                    }
                                });
                            });
                        });
                    });


                });
            });
    }
}
