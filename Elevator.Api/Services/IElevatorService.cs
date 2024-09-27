

using Elevator.Api.Helpers;

namespace Elevator.Api.Services
{
    public interface IElevatorService
    {
        Task AddElevatorOperation(ElevatorOperation elevatorOperation);
        Task<ICollection<ElevatorOperation>> GetElevatorOperations();
        Task<Elevator> ElevatorMovement(PassengerLocation passnger, Status status);
        Task<ICollection<ElevatorOperation>> GetGenerateElevatorRequests();
    }
}
