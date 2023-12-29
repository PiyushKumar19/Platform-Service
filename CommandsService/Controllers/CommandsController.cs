using System.ComponentModel.Design;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controller
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo commandRepo;
        private readonly IMapper mapper;

        public CommandsController(ICommandRepo commandRepo, IMapper mapper)
        {
            this.commandRepo = commandRepo;
            this.mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            Console.WriteLine($"\n--> Hit GetCommandsForPlatform: {platformId}\n");

            if (!commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commands = commandRepo.GetCommandsForPlatform(platformId);
            return Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            Console.WriteLine($"\n--> Hit GetCommandForPlatform: {platformId} / {commandId}\n");

            if (!commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = commandRepo.GetCommand(platformId, commandId);

            if (command == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<CommandReadDto>(command));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto createCommandDto)
        {
            Console.WriteLine($"\n--> Hit GetCommandsForPlatform: {platformId}\n");

            if (!commandRepo.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = mapper.Map<Command>(createCommandDto);

            commandRepo.CreateCommand(platformId, command);
            commandRepo.SaveChanges();

            var commandReadDto = mapper.Map<CommandReadDto>(command);
            return CreatedAtRoute(nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id }, commandReadDto);
        }
    }
}