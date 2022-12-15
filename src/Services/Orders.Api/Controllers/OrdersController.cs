using Microsoft.AspNetCore.Mvc;
using OrderService.Api.Dtos;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderService.Api.Controllers;
[Route("[controller]")]
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
    public async Task<ActionResult> Post([FromBody] CreateOrderDto createOrderDto)
    {

        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            "http://localhost:7030/inventory/" + Guid.NewGuid());

        var httpClient = httpClientFactory.CreateClient();
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            // TODO: add order to ActiveMQ
        }


        httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Get,
            "http://localhost:7010/user/");

        httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            // TODO: add order to ActiveMQ
        }


        return Ok();

        // TODO:
        // - http request to inventory service
        // - IO -> save order history entry; place OrderProcessing Message to ActiveMQ; IO result
        // - NIO -> NIO result
    }
}
