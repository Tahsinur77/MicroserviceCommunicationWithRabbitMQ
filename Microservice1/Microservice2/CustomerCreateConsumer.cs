using MassTransit;
using Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice2
{
    public class CustomerCreateConsumer: IConsumer<CustomerModel>
    {
        public async Task Consume(ConsumeContext<CustomerModel> context)
        {
            var customer = context.Message;
            Console.WriteLine(context.Message);
        }
    }
}
