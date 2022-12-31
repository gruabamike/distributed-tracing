﻿using DistributedTracingDotNet.Services.Users.Api.Models;

namespace DistributedTracingDotNet.Services.Users.Api.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllAsync();

    Task<User?> GetAsync(Guid id);

    Task<User?> AddUserAsync(User user);
}
