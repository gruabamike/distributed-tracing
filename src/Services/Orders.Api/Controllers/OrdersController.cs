using AutoMapper;
using MessageBroker.Contract;
using Microsoft.AspNetCore.Mvc;
using Orders.Api.Commands;
using Orders.Api.Dtos;
using Orders.Api.Services;

namespace Orders.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> logger;
    private readonly IMapper mapper;
    private readonly IOrderService orderService;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IMessageSender messagePublisher;


    public OrdersController(
        ILogger<OrdersController> logger,
        IMapper mapper,
        IOrderService orderService,
        IHttpClientFactory httpClientFactory,
        IMessageSender messagePublisher)
    {
        this.logger = logger;
        this.mapper = mapper;
        this.orderService = orderService;
        this.httpClientFactory = httpClientFactory;
        this.messagePublisher = messagePublisher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        => Ok(mapper.Map<IEnumerable<OrderDto>>(await orderService.GetOrdersAsync()));

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        var order = await orderService.GetOrderAsync(orderId);
        if (order is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<OrderDto>(order));
    }

    [HttpPost]
    public async Task<ActionResult> AddOrder(OrderForCreationDto createOrderDto)
    {
        var createOrderCommand = mapper.Map<CreateOrderCommand>(createOrderDto);
        var order = await orderService.AddOrderAsync(createOrderCommand);

        if (order is null)
        {
            return BadRequest();
        }

        return CreatedAtAction(
            nameof(GetOrder),
            new { orderId = order.Id },
            mapper.Map<OrderDto>(order));
        /*
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
            "http://localhost:7010/users/");

        httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            await ActiveMQInstrumentationBroker.ProduceAsync();
            //await messagePublisher.Publish(new TextMessage(Guid.NewGuid().ToString()));
        }


        return Ok();

        // TODO:
        // - http request to inventory service
        // - IO -> save order history entry; place OrderProcessing Message to ActiveMQ; IO result
        // - NIO -> NIO result
        */
    }
}
