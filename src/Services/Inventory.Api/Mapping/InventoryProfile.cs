using AutoMapper;
using Inventory.Api.Dtos;
using Inventory.Api.Models;

namespace Inventory.Api.Mapping;

public class InventoryProfile : Profile
{
	public InventoryProfile()
	{
		CreateMap<Stock, InventoryDto>();
	}
}
