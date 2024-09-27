using Elevator.Api.Helpers;
using Elevator.Api.Models;
using System.Collections.Concurrent;

namespace Elevator.Api.Services
{
    public record Elevator(Direction Direction, int Floor, Status Status);
    public record PassengerLocation(int ElevatorId, int Floor);
    public record PassengerRequest(Direction Direction, Floor Floor);
    public record ElevatorOperation(int ElevatorId, Direction Direction, PassengerRequest PassengerRequest, Elevator Elevator);

    public class ElevatorService : IElevatorService
    {
        private static readonly ConcurrentDictionary<string, ElevatorOperation> _elevatorOperations = [];
        private static readonly Dictionary<int, Elevator> _elevators = [];
        private readonly IConfiguration _configuration;
        private readonly ILogger<ElevatorService> _logger;
        private readonly int _maxFloor;
        private readonly int _numberOfElevator;

        static ElevatorService()
        {
            _elevators.Add(1, new(Direction.Idle, 1, Status.Stop));
            _elevators.Add(2, new(Direction.Idle, 1, Status.Stop));
            _elevators.Add(3, new(Direction.Idle, 1, Status.Stop));
            _elevators.Add(4, new(Direction.Idle, 1, Status.Stop));
        }

        public ElevatorService(IConfiguration configuration, ILogger<ElevatorService> logger)
        {
           _configuration = configuration;
            _logger = logger;
            _maxFloor = _configuration.GetValue<int>("Elevator:MaxFloor");
            _numberOfElevator = _configuration.GetValue<int>("Elevator:NumberOfElevator");
        }

        public async Task<ICollection<ElevatorOperation>> GetGenerateElevatorRequests()
        {
            await GenerateElevatorRequests();

            return await Task.FromResult(_elevatorOperations.Values);
        }

        public async Task<ICollection<ElevatorOperation>> GetElevatorOperations()
        {
            return await Task.FromResult(_elevatorOperations.Values);
        }

        public async Task<Elevator> ElevatorMovement(PassengerLocation passnger, Status status)
        {
            await Task.Run(() =>
            {
                var currentElevator = _elevators[passnger.ElevatorId];

                try
                {
                    if (status == Status.Move)
                    {
                        var elevatorOperations = _elevatorOperations.Select(s => s.Value).Where(a => a.ElevatorId == passnger.ElevatorId);

                        if (elevatorOperations.Any())
                        {
                            var direciton = elevatorOperations.First().PassengerRequest.Direction;

                            var upDirection = elevatorOperations
                                .OrderBy(o => o.PassengerRequest.Floor.Destination)
                                .FirstOrDefault(e => e.Direction == Direction.Up &&
                                    e.Direction == direciton);

                            var downDirection = elevatorOperations
                                .OrderByDescending(o => o.PassengerRequest.Floor.Destination)
                                .FirstOrDefault(e => e.Direction == Direction.Down &&
                                    e.Direction == direciton);

                            if (upDirection != null)
                            {
                                var direction = upDirection.Direction;

                                if (upDirection.PassengerRequest.Floor.Destination > currentElevator.Floor)
                                {
                                    _elevators[passnger.ElevatorId] = new(direction, upDirection.PassengerRequest.Floor.Current, status);
                                }
                                else
                                {
                                    var upDirectionRecheck = elevatorOperations
                                        .OrderBy(o => o.PassengerRequest.Floor.Destination)
                                        .FirstOrDefault(e => e.Direction == Direction.Up &&
                                            e.Direction == direciton && e.PassengerRequest.Floor.Destination > currentElevator.Floor);

                                    if (upDirectionRecheck != null)
                                    {
                                        upDirection = upDirectionRecheck;
                                        _elevators[passnger.ElevatorId] = new(direction, currentElevator.Floor, status);
                                    }
                                }

                                if (upDirection.PassengerRequest.Floor.Destination <= currentElevator.Floor)
                                {
                                    direction = Direction.Down;

                                    var downRequest = _elevatorOperations
                                        .OrderByDescending(o => o.Value.PassengerRequest.Floor.Destination)
                                        .FirstOrDefault(e => e.Value.ElevatorId == passnger.ElevatorId &&
                                            e.Value.Direction == direction &&
                                            e.Value.PassengerRequest.Floor.Destination < currentElevator.Floor &&
                                            e.Value.PassengerRequest.Floor.Destination <= upDirection.PassengerRequest.Floor.Current);

                                    var floor = downRequest.Key == null ? upDirection.PassengerRequest.Floor.Current : downRequest.Value.PassengerRequest.Floor.Current;

                                    _elevators[passnger.ElevatorId] = new(direction, floor, status);

                                    _elevatorOperations.TryAdd(
                                        ElevatorHelper.ElevatorKey(passnger.ElevatorId, floor),
                                            new(passnger.ElevatorId, direction,
                                                new(direction,
                                                    new() { Current = currentElevator.Floor, Destination = floor }), currentElevator));
                                }
                            }

                            if (downDirection != null)
                            {
                                var direction = downDirection.Direction;

                                if (downDirection.PassengerRequest.Floor.Destination < currentElevator.Floor)
                                {
                                    _elevators[passnger.ElevatorId] = new(downDirection.Direction, currentElevator.Floor, status);
                                }
                                else
                                {
                                    var downDirectionRecheck = elevatorOperations
                                        .OrderByDescending(o => o.PassengerRequest.Floor.Destination)
                                        .FirstOrDefault(e => e.Direction == Direction.Down &&
                                            e.Direction == direciton && e.PassengerRequest.Floor.Destination < currentElevator.Floor);

                                    if (downDirectionRecheck != null)
                                    {
                                        downDirection = downDirectionRecheck;
                                        _elevators[passnger.ElevatorId] = new(direction, currentElevator.Floor, status);
                                    }
                                }

                                if (downDirection.PassengerRequest.Floor.Destination >= currentElevator.Floor)
                                {
                                    direction = Direction.Up;
                                    _elevators[passnger.ElevatorId] = new(Direction.Up, currentElevator.Floor, status);

                                    var upRequest = _elevatorOperations
                                        .OrderByDescending(o => o.Value.PassengerRequest.Floor.Destination)
                                        .FirstOrDefault(e => e.Value.ElevatorId == passnger.ElevatorId &&
                                            e.Value.Direction == direction &&
                                            e.Value.PassengerRequest.Floor.Destination > currentElevator.Floor &&
                                            e.Value.PassengerRequest.Floor.Destination >= downDirection.PassengerRequest.Floor.Current);

                                    var floor = upRequest.Key == null ? downDirection.PassengerRequest.Floor.Current : upRequest.Value.PassengerRequest.Floor.Current;

                                    _elevators[passnger.ElevatorId] = new(direction, floor, status);

                                    _elevatorOperations.TryAdd(
                                        ElevatorHelper.ElevatorKey(passnger.ElevatorId, floor),
                                            new(passnger.ElevatorId, direction,
                                                new(direction,
                                                    new() { Current = currentElevator.Floor, Destination = floor }), currentElevator));
                                }
                            }

                            _logger.LogInformation("Elevator: {elevatorId}, Elevator Current Floor: {floor} Elevator Status: {status}",
                               passnger.ElevatorId, _elevators[passnger.ElevatorId].Floor, status);

                        }
                    }

                    if (status == Status.Stop)
                    {
                        var lookUpNextFloor = ElevatorEvalateFloorToStop(passnger, currentElevator.Direction, currentElevator);

                        if (!string.IsNullOrWhiteSpace(lookUpNextFloor.Key))
                        {
                            _elevators[passnger.ElevatorId] = new(lookUpNextFloor.Value.Direction, lookUpNextFloor.Value.PassengerRequest.Floor.Destination, status);

                            _elevatorOperations.TryRemove(lookUpNextFloor);

                            _logger.LogInformation("Elevator: {elevatorId}, Elevator Current Floor: {floor} Elevator Status: {status}",
                                passnger.ElevatorId, lookUpNextFloor.Value.PassengerRequest.Floor.Destination, status);
                        }
                        else
                        {
                            var malfunction = _elevatorOperations.FirstOrDefault(e => e.Value.ElevatorId == passnger.ElevatorId);

                            if (!string.IsNullOrWhiteSpace(malfunction.Key)) _elevatorOperations.TryRemove(malfunction);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Elevator error on moving to the next distination.");
                }
            });

            return _elevators[passnger.ElevatorId];
        }

        public async Task AddElevatorOperation(ElevatorOperation elevatorOperation)
        {
            await Task.Run(() =>
            {
                var elevatorId = elevatorOperation.ElevatorId;
                var destinationFloor = elevatorOperation.PassengerRequest.Floor.Destination;

                _elevatorOperations.TryAdd(ElevatorHelper.ElevatorKey(elevatorId, destinationFloor), elevatorOperation);
            });
        }

        #region Private Methods

        private static KeyValuePair<string, ElevatorOperation> ElevatorEvalateFloorToStop(PassengerLocation passnger, Direction direction, Elevator currentElevator)
        {
            return direction switch
            {
                Direction.Up => _elevatorOperations
                    .OrderBy(o => o.Value.PassengerRequest.Floor.Destination)
                    .FirstOrDefault(e => e.Value.ElevatorId == passnger.ElevatorId &&
                        e.Value.Direction == direction &&
                        e.Value.PassengerRequest.Floor.Destination > currentElevator.Floor),

                Direction.Down => _elevatorOperations
                    .OrderByDescending(o => o.Value.PassengerRequest.Floor.Destination)
                    .FirstOrDefault(e => e.Value.ElevatorId == passnger.ElevatorId && 
                        e.Value.Direction == direction &&
                        e.Value.PassengerRequest.Floor.Destination < currentElevator.Floor),

                _ => new KeyValuePair<string, ElevatorOperation>()
            }; ;
        }

        private async Task GenerateElevatorRequests()
        {
            await Task.Run(async () =>
            {
                var random = new Random();
                var hasOperation = !_elevatorOperations.IsEmpty;

                var randomIteration = hasOperation ? random.Next(3, 6) : 3;

                for (int i = 0; i < randomIteration; i++)
                {
                    var direction = hasOperation ? (Direction)random.Next(0, 2) : Direction.Up;
                    var currentFloor = hasOperation ? random.Next(1, 11) : 1;

                    if (direction == Direction.Up && currentFloor != _maxFloor)
                    {
                        var destinationFloor = random.Next(currentFloor + 1, 11);
                        var elevatorIdle = _elevators.FirstOrDefault(e => e.Value.Direction == Direction.Idle).Key;

                        if (!hasOperation)
                        {
                            await AddElevatorOperation(
                                new(1, Direction.Up,
                                    new(direction,
                                        new Floor { Current = currentFloor, Destination = destinationFloor }),
                                    GetElevator(1)));

                            continue;
                        }

                        if (elevatorIdle > 0)
                        {
                            await AddElevatorOperation(
                                new(elevatorIdle, Direction.Up, 
                                    new(direction, 
                                        new Floor { Current = currentFloor, Destination = destinationFloor }), 
                                    GetElevator(elevatorIdle)));

                            continue;
                        }

                        await ChooseClosestElevator(currentFloor, destinationFloor, direction);
                    }

                    if (direction == Direction.Down && currentFloor != 1)
                    {
                        var destinationFloor = random.Next(1, currentFloor);
                        var elevatorIdle = _elevators.FirstOrDefault(e => e.Value.Direction == Direction.Idle).Key;

                        if (elevatorIdle > 0)
                        {
                            await AddElevatorOperation(
                                new(elevatorIdle, Direction.Up, 
                                    new(direction, 
                                        new Floor { Current = currentFloor, Destination = destinationFloor }), 
                                    GetElevator(elevatorIdle)));

                            continue;
                        }

                        await ChooseClosestElevator(currentFloor, destinationFloor, direction);
                    }
                }
            });
        }

        private async Task ChooseClosestElevator(int currentFloor, int destinationFloor, Direction direction)
        {
            var groupByDirections = _elevators.GroupBy(g => g.Value.Direction);

            Task elevatorTask = AddElevatorOperation(
                new(1, Direction.Up,
                    new(direction, 
                        new Floor { Current = currentFloor, Destination = destinationFloor }),
                    GetElevator(1)));

            var groupDirection = groupByDirections.FirstOrDefault(g => g.Key == direction);

            if (groupDirection?.Key == null)
            {
                await elevatorTask;

                return;
            }

            if (groupDirection.Key == Direction.Up)
            {
                await UpElevator(groupDirection, direction, currentFloor, destinationFloor, elevatorTask);
            }

            if (groupDirection.Key == Direction.Down)
            {
                await DownElevator(groupDirection, direction, currentFloor, destinationFloor, elevatorTask);
            }

            await elevatorTask;
        }

        private async Task UpElevator(
            IGrouping<Direction, KeyValuePair<int, Elevator>> directionGroup,
            Direction direction,
            int currentFloor,
            int destinationFloor,
            Task elevatorTask)
        {
            await Task.Run(() =>
            {
                var selectedElevator = directionGroup
                        .Select(s => new { s.Key, s.Value })
                        .OrderByDescending(g => g.Value.Floor)
                        .FirstOrDefault(f => f.Value.Floor < currentFloor);

                var bottomMostElevator = directionGroup.FirstOrDefault(f => f.Value.Floor == directionGroup.Min(m => m.Value.Floor));

                elevatorTask = AddElevatorOperation(
                    new(bottomMostElevator.Key, Direction.Up,
                        new(direction,
                            new Floor { Current = currentFloor, Destination = destinationFloor }),
                        GetElevator(bottomMostElevator.Key)));

                if (selectedElevator != null)
                {
                    elevatorTask = AddElevatorOperation(
                        new(selectedElevator.Key, Direction.Up,
                            new(direction,
                                new Floor { Current = currentFloor, Destination = destinationFloor }),
                            GetElevator(selectedElevator.Key)));
                }

                if (directionGroup.Count() == _numberOfElevator)
                {
                    var topMostElevator = directionGroup.FirstOrDefault(f => f.Value.Floor == directionGroup.Max(m => m.Value.Floor));
                    elevatorTask = AddElevatorOperation(
                        new(topMostElevator.Key, Direction.Up,
                            new(direction,
                                new Floor { Current = currentFloor, Destination = destinationFloor }),
                            GetElevator(topMostElevator.Key)));
                }
            });
        }

        private async Task DownElevator(
            IGrouping<Direction, KeyValuePair<int, Elevator>> directionGroup, 
            Direction direction, 
            int currentFloor, 
            int destinationFloor, 
            Task elevatorTask)
        {
            await Task.Run(() =>
            {
                var selectedElevator = directionGroup
                        .Select(s => new { s.Key, s.Value })
                        .OrderBy(g => g.Value.Floor)
                        .FirstOrDefault(f => f.Value.Floor > currentFloor);

                var bottomMostElevator = directionGroup.FirstOrDefault(f => f.Value.Floor == directionGroup.Max(m => m.Value.Floor));

                elevatorTask = AddElevatorOperation(
                    new(bottomMostElevator.Key, Direction.Up,
                        new(direction,
                            new Floor { Current = currentFloor, Destination = destinationFloor }),
                        GetElevator(bottomMostElevator.Key)));

                if (selectedElevator != null)
                {
                    elevatorTask = AddElevatorOperation(
                        new(selectedElevator.Key, Direction.Up,
                            new(direction,
                                new Floor { Current = currentFloor, Destination = destinationFloor }),
                            GetElevator(selectedElevator.Key)));
                }

                if (directionGroup.Count() == _numberOfElevator)
                {
                    var topMostElevator = directionGroup.FirstOrDefault(f => f.Value.Floor == directionGroup.Min(m => m.Value.Floor));
                    elevatorTask = AddElevatorOperation(
                        new(topMostElevator.Key, Direction.Up,
                            new(direction,
                                new Floor { Current = currentFloor, Destination = destinationFloor }),
                            GetElevator(topMostElevator.Key)));

                }
            });
        }

        private static Elevator GetElevator(int elevatorId) => _elevators.Single(e => e.Key == elevatorId).Value;

        #endregion Private Methods
    }
}
