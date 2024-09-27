namespace Elevator.Api.Helpers
{
    public static class ElevatorHelper
    {
        public static string ElevatorKey(int elevatorId, int floor) => $"E{elevatorId}:F{floor}";
    }
}
