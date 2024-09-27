import { Direction, Status } from "./enums"

export interface ElevatorOperation {
  elevatorId: number
  direction: Direction
  passengerRequest: PassengerRequest
  elevator: Elevator
}

export interface PassengerRequest {
  direction: Direction
  floor: Floor
}

export interface Floor {
  current: number
  destination: number
}

export interface Elevator {
  direction: Direction
  floor: number
  status: Status
}

export interface ElementElevator {
  id: number,
  left: string
}

export interface PassengerLocation {
  elevatorId: number,
  floor: number
}

export interface ElevatorMovementRequest {
  passengerLocation: PassengerLocation;
  status: Status;
}

export interface OperationProp {
  operation: ElevatorOperator
}