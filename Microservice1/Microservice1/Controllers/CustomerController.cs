using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Share;

namespace Microservice1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly ISendEndpointProvider endpoint;

        public CustomerController(ISendEndpointProvider _endpoint)
        {
            endpoint = _endpoint ?? throw new NullReferenceException(nameof(_endpoint));
        }
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerModel cmd)
        {
            ISendEndpoint sendEndpoint = await endpoint.GetSendEndpoint(new($"queue:{QueueService.CustomerCreateQueue}"));
            await sendEndpoint.Send(cmd);
            return Ok("Success");
        }
    }
}
