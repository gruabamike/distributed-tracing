using Inventory.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Inventory.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    // GET api/<InventoryController>/5
    [HttpGet("{inventoryId}")]
    public async Task<ActionResult<InventoryDto>> Get(Guid inventoryId)
    {
        return Ok(await Task.FromResult(new InventoryDto(Guid.NewGuid(), 10)));
    }

    // PUT api/<InventoryController>/5
    [HttpPut("{id}")]
    public async Task<ActionResult> Put(int id, [FromBody] string value)
    {
        return Ok();
    }
}
