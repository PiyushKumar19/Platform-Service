using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
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
        private readonly IMessageBusClient messageBusClient;

        public PlatformsController(IPlatformRepo repo,
        IMapper mapper,
        ICommandDataClient commandDataClient,
        IMessageBusClient messageBusClient)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.commandDataClient = commandDataClient;
            this.messageBusClient = messageBusClient;
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

            // Send Sync Message
            try {
                await commandDataClient.SendPlatformToCommand(platformReadDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n--> Could not send data synchronously: {ex.Message}\n");
            }

            // Send Async Message
            try
            {
                var platformPublishedDto = mapper.Map<PlatformPublishedDto>(platformReadDto);
                platformPublishedDto.Event = "Platform_Published";
                messageBusClient.PublishNewPlatform(platformPublishedDto);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n--> Could not send asynchronously: {ex.Message}");
            }
            return CreatedAtRoute(nameof(GetPlatformById), new {Id = platformReadDto.Id}, platformReadDto);
        }
    }
}
