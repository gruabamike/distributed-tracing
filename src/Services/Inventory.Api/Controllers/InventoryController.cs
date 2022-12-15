using Inventory.Api.Dtos;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Inventory.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService inventoryService;
    public InventoryController(IInventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    [HttpGet("{productId}")]
    public async Task<ActionResult<InventoryDto>> Get(Guid productId)
    {
        return Ok(await inventoryService.GetByProductId(productId));
    }
}
