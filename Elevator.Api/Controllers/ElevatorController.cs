using Elevator.Api.Models;
using Elevator.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Elevator.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase
    {
        private readonly IElevatorService _elevatorService;

        public ElevatorController(IElevatorService elevatorService)
        {
            _elevatorService = elevatorService;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _elevatorService.GetElevatorOperations());
        }

        [HttpGet("generate")]
        public async Task<ActionResult> GetGenreated()
        {
            return Ok(await _elevatorService.GetGenerateElevatorRequests());
        }

        [HttpPost]
        public async Task<ActionResult> ElevatorMovement([FromBody] ElevatorMovementRequest request)
        {
            return Ok(await _elevatorService.ElevatorMovement(request.PassengerLocation, request.Status));
        }
    }
}
