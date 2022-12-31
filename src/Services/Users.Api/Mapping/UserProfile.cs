using AutoMapper;
using DistributedTracingDotNet.Services.Users.Api.Dtos;
using DistributedTracingDotNet.Services.Users.Api.Models;

namespace DistributedTracingDotNet.Services.Users.Api.Mapping;

public class UserProfile : Profile
{
	public UserProfile()
	{
		CreateMap<User, UserDto>();
		CreateMap<UserForCreationDto, User>();
	}
}
