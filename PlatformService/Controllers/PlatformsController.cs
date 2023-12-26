using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepo repo;
        private readonly IMapper mapper;
        private readonly ICommandDataClient commandDataClient;

        public PlatformsController(IPlatformRepo repo,
        IMapper mapper,
        ICommandDataClient commandDataClient)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.commandDataClient = commandDataClient;
        }

        [HttpGet]
        public ActionResult GetPlatforms()
        {
            var platformItem = repo.GetAllPlatforms();
            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));
        }

        [HttpGet("GetAll")]
        public ActionResult GetAll()
        {
            try
            {
                var platformItem = repo.GetAllPlatforms();
                if (platformItem != null)
                {
                    return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItem));

                }
                return NotFound();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformCreateDto> GetPlatformById(int id)
        {
            var platform = repo.GetPlatfomrById(id);
            if (platform != null)
            {
                return Ok(mapper.Map<PlatformReadDto>(platform));
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platformCreateDto)
        {
            var platformModel = mapper.Map<Platform>(platformCreateDto);
            repo.CreatePlatform(platformModel);
            repo.SaveChanges();

            var platformReadDto = mapper.Map<PlatformReadDto>(platformModel);

            try {
                await commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"--> Could not send data synchronously: {ex.Message}");
            }
            return CreatedAtRoute(nameof(GetPlatformById), new {Id = platformReadDto.Id}, platformReadDto);
        }
    }
}
