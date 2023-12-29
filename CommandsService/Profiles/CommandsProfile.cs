using AutoMapper;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.Profiles
{
    public class CommandsProfile : Profile
    {
        public CommandsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<CommandCreateDto, Command>();
            CreateMap<Command, CommandReadDto>();
            // Mapping EnternalId from PlatformModel to Id from PlatformPublishedDto
            CreateMap<PlatformPublishedDto, Platform>()
                .ForMember(dest=> dest.ExternalPlatId, opt => opt.MapFrom(src => src.Id));
        }
    }
}