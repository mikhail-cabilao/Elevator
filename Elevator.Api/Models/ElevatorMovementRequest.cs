using Elevator.Api.Helpers;
using Elevator.Api.Services;

namespace Elevator.Api.Models
{
    public class ElevatorMovementRequest
    {
        public PassengerLocation PassengerLocation { get; set; }
        public Status Status { get; set; }
    }
}
