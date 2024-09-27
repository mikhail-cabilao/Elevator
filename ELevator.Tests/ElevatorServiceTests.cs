using Elevator.Api.Helpers;
using Elevator.Api.Models;
using Elevator.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Dynamic;

namespace ELevator.Tests
{
    public class ElevatorServiceTests
    {
        private ServiceProvider _servcies;
        private IElevatorService? _elevatorService;

        [SetUp]
        public void Setup()
        {
            var configurarion = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfiguration>(configurarion);
            serviceCollection.AddSingleton<IElevatorService, ElevatorService>();
            serviceCollection.AddLogging();

            _servcies = serviceCollection.BuildServiceProvider();

            _elevatorService = _servcies.GetService<IElevatorService>();
        }

        [TearDown]
        public void TearDown()
        {
            _servcies.Dispose();
        }

        [Test]
        public async Task GetElevatorRequests_MoveUp_WithExraDown_Success()
        {
            // assert
            await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 8 }));
            await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 5 }));
            await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 10 }));

            dynamic request = new ExpandoObject();
            request.First = true;
            request.Second = false;

            ICollection<ElevatorOperation> operations = []; 

            // act
            do
            {
                operations = await _elevatorService!.GetElevatorOperations();

                var operation = operations.FirstOrDefault();

                if (operation != null)
                {
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Move);
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Stop);

                    if (request.Second)
                    {
                        request.Second = false;
                        await SetElevator(1, new(Direction.Down, 1, Status.Move), new(Direction.Down, new() { Current = 5, Destination = 4 }));
                    }

                    if (request.First)
                    {
                        request.First = false;
                        request.Second = true;

                        await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 3 }));
                    }
                }

            } while (operations.Any());

            // assert
            Assert.That(operations.Count == 0);
        }

        [Test]
        public async Task GetElevatorRequests_MoveDown_WithExraUp_Success()
        {
            // assert
            await SetElevator(1, new(Direction.Down, 10, Status.Move), new(Direction.Down, new() { Current = 10, Destination = 8 }));
            await SetElevator(1, new(Direction.Down, 10, Status.Move), new(Direction.Down, new() { Current = 10, Destination = 5 }));
            await SetElevator(1, new(Direction.Down, 10, Status.Move), new(Direction.Down, new() { Current = 10, Destination = 1 }));

            dynamic request = new ExpandoObject();
            request.First = true;
            request.Second = false;

            ICollection<ElevatorOperation> operations = [];

            // act
            do
            {
                operations = await _elevatorService!.GetElevatorOperations();

                var operation = operations.FirstOrDefault();

                if (operation != null)
                {
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Move);
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Stop);

                    if (request.Second)
                    {
                        request.Second = false;
                        await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 7 }));
                    }

                    if (request.First)
                    {
                        request.First = false;
                        request.Second = true;

                        await SetElevator(1, new(Direction.Down, 1, Status.Move), new(Direction.Down, new() { Current = 3, Destination = 2 }));
                    }
                }

            } while (operations.Any());

            // assert
            Assert.That(operations.Count == 0);
        }

        [Test]
        public async Task GetElevatorRequests_MultipleElevators_Success()
        {
            // assert
            await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 8 }));
            await SetElevator(1, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 1, Destination = 5 }));
            await SetElevator(2, new(Direction.Down, 1, Status.Move), new(Direction.Down, new() { Current = 9, Destination = 6 }));
            await SetElevator(3, new(Direction.Down, 1, Status.Move), new(Direction.Down, new() { Current = 5, Destination = 1 }));
            await SetElevator(4, new(Direction.Up, 1, Status.Move), new(Direction.Up, new() { Current = 7, Destination = 10 }));

            ICollection<ElevatorOperation> operations = [];

            // act
            do
            {
                operations = await _elevatorService!.GetElevatorOperations();

                var operation = operations.FirstOrDefault();

                if (operation != null)
                {
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Move);
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Stop);
                }

            } while (operations.Any());

            // assert
            Assert.That(operations.Count == 0);
        }


        [Test]
        public async Task GetElevatorRequests_GeneratedRequests_Success()
        {
            // assert
            await _elevatorService!.GetGenerateElevatorRequests();

            ICollection<ElevatorOperation> operations = [];

            // act
            do
            {
                operations = await _elevatorService!.GetElevatorOperations();

                var operation = operations.FirstOrDefault();

                if (operation != null)
                {
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Move);
                    await _elevatorService.ElevatorMovement(new(operation.ElevatorId, operation.PassengerRequest.Floor.Destination), Status.Stop);
                }

            } while (operations.Any());

            // assert
            Assert.That(operations.Count == 0);
        }

        private async Task SetElevator(int elevatorId, Elevator.Api.Services.Elevator eleavtor, PassengerRequest passenger)
        {
            await _elevatorService!.AddElevatorOperation(
                    new(elevatorId, eleavtor.Direction,
                        new(passenger.Direction,
                            new Floor { Current = passenger.Floor.Current, Destination = passenger.Floor.Destination }),
                        eleavtor));
        }
    }
}