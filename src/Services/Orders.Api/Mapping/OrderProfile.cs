using AutoMapper;
using Orders.Api.Commands;
using Orders.Api.Dtos;
using Orders.Api.Models;

namespace Orders.Api.Mapping;

public class OrderProfile : Profile
{
	public OrderProfile()
	{
		CreateMap<Order, OrderDto>();
		CreateMap<OrderForCreationDto, CreateOrderCommand>();
	}
}
