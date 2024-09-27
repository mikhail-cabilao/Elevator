using Elevator.Api;
using Elevator.Api.Helpers;
using Elevator.Api.Models;
using Elevator.Api.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ELevator.Tests.Integration
{
    public class ElevatorControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _client;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void Teardown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Elevator_GetElevatorOperations_ShouldEmptyResult()
        {
            var response = await _client.GetAsync("/elevator/generate");

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<IEnumerable<ElevatorOperation>>();

            Assert.That(responseBody?.Count(), Is.GreaterThan(0));
        }

        [Test]
        public async Task Elevator_ElevatorMovement_ShouldHaveResult()
        {
            var response = await _client.GetAsync("/elevator/generate");

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<IEnumerable<ElevatorOperation>>();
            var moveFirst = responseBody!.First();

            var payload = new ElevatorMovementRequest 
            { 
                Status = Status.Move,
                PassengerLocation = new(moveFirst.ElevatorId, moveFirst.PassengerRequest.Floor.Current) 
            };
            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var postResponse = await _client.PostAsync("/elevator", content);

            response.EnsureSuccessStatusCode();
            Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}
