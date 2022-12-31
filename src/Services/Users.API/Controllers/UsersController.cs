using AutoMapper;
using DistributedTracingDotNet.Services.Users.Api.Dtos;
using DistributedTracingDotNet.Services.Users.Api.Models;
using DistributedTracingDotNet.Services.Users.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DistributedTracingDotNet.Services.Users.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> logger;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public UsersController(
        ILogger<UsersController> logger,
        IMapper mapper,
        IUserService userService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return Ok(mapper.Map<IEnumerable<UserDto>>(await userService.GetAllAsync()));
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid userId) {
        var user = await userService.GetAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<UserDto>(await userService.GetAsync(userId)));
    }

    [HttpPost]
    public async Task<ActionResult> AddUser(UserForCreationDto newUser)
    {
        var user = mapper.Map<User>(newUser);

        user = await userService.AddUserAsync(user);
        if (user is null)
        {
            return BadRequest();
        }

        return CreatedAtAction(
            nameof(GetUser),
            new { userId = user.Id },
            mapper.Map<UserDto>(user));
    }

    /*
    [HttpPut]
    public async Task<ActionResult> UpdateUser(User updatedUser)
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
    */
}