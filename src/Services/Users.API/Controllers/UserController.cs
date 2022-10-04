using DistributedTracingDotNet.Services.Users.API.Services;
using DistributedTracingDotNet.Shared.Users.Contract;
using Microsoft.AspNetCore.Mvc;

namespace DistributedTracingDotNet.Services.Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpGet()]
    public async Task<ActionResult<List<User>>> Get()
    {
        return Ok(await userService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(int id) {
        var user = await userService.GetAsync(id);
        if (user is null)
        {
            return BadRequestUserNotFound(id);
        }
        return Ok(await userService.GetAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<List<User>>> AddUser(User newUser)
    {
        return Ok(await userService.AddUserAsync(newUser));
    }

    [HttpPut]
    public async Task<ActionResult<List<User>>> UpdateUser(User updatedUser)
    {
        var user = await userService.GetAsync(updatedUser.Id);
        if (user is null)
        {
            return BadRequestUserNotFound(updatedUser.Id);
        }

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;

        return Ok();
    }

    private BadRequestObjectResult BadRequestUserNotFound(int userId)
    {
        return BadRequest($"User with Id '{userId}' not found!");
    }
}