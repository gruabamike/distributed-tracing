using AutoMapper;
using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> logger;
    private readonly IMapper mapper;
    private readonly IInventoryService inventoryService;

    public InventoryController(
        ILogger<InventoryController> logger,
        IMapper mapper,
        IInventoryService inventoryService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<InventoryDto>> Get(Guid productId)
    {
        var stock = await inventoryService.GetByProductId(productId);
        if (stock is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<InventoryDto>(stock));
    }
}
