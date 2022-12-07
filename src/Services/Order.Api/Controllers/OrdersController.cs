using Microsoft.AspNetCore.Mvc;
using Order.Api.Dtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Order.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;

    public OrdersController(
        ILogger<OrdersController> logger,
        IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async void Post([FromBody] CreateOrderDto createOrderDto)
    {

        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Put,
            "https://localhost:7294/api/inventory");

        var httpClient = httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            // TODO: add order to ActiveMQ
        }

        // TODO:
        // - http request to inventory service
        // - IO -> save order history entry; place OrderProcessing Message to ActiveMQ; IO result
        // - NIO -> NIO result
    }
}
