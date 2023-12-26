using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controller
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepo commandRepo;
        private readonly IMapper mapper;

        public PlatformsController(ICommandRepo commandRepo, IMapper mapper)
        {
            this.commandRepo = commandRepo;
            this.mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Platform>> GetPlatforms()
        {
            Console.WriteLine("\n--> Getting Platforms from CommandsService\n");
            var platformItems = commandRepo.GetAllPlatforms();

            return Ok(mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));

        }
        
        [HttpPost]
        public ActionResult TestingInboundConnection()
        {
            Console.WriteLine("--> Inbound Test");
            return Ok("Inbound request success!");
        }
    }
}