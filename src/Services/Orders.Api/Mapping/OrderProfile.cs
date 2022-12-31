using AutoMapper;
using Orders.Api.Commands;
using Orders.Api.Dtos;
using OrderService.Api.Models;

namespace Orders.Api.Mapping;

public class OrderProfile : Profile
{
	public OrderProfile()
	{
		CreateMap<Order, OrderDto>();
		CreateMap<OrderForCreationDto, CreateOrderCommand>();
	}
}
