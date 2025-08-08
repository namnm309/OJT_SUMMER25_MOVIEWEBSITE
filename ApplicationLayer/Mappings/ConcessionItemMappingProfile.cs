using ApplicationLayer.DTO.ConcessionManagement;
using AutoMapper;
using DomainLayer.Entities;

namespace ApplicationLayer.Mappings
{
    public class ConcessionItemMappingProfile : Profile
    {
        public ConcessionItemMappingProfile()
        {
            // Entity -> DTO
            CreateMap<ConcessionItem, ConcessionItemDto>();

            // DTO -> Entity
            CreateMap<CreateConcessionItemDto, ConcessionItem>();
            CreateMap<UpdateConcessionItemDto, ConcessionItem>();
        }
    }
}
