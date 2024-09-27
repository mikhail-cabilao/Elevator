/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from 'react';
import './App.css';
import { ElementElevator, Elevator, ElevatorMovementRequest } from './types/interfaces';
import { Direction, Status } from './types/enums';
function App() {
  const floorIncrement: number = 55;
  const delayDuration: number = 10000;

  useEffect(() => {
    elevatorGeneratedData();

    const minDuration = 10;
    const randomInterval = (Math.random() * (15 - minDuration) + minDuration) * 1000;

    const intervalId = setInterval(elevatorGeneratedData, randomInterval);
    return () => clearInterval(intervalId);
  }, []);

  const Building = () => {
    return <div className="container">
      <div className="grid-container">
        {Array.from({ length: 40 }, (_, i) => i).map(i => <div key={i} className="grid-item"></div>)}
      </div>
      <Elevator id={1} left={'20px'}></Elevator>
      <Elevator id={2} left={'75px'}></Elevator>
      <Elevator id={3} left={'130px'}></Elevator>
      <Elevator id={4} left={'185px'}></Elevator>
    </div>
  }

  const Elevator = ({ id, left }: ElementElevator) => {
    const [elevator, setElevator] = useState<Elevator>({ direction: Direction.Up, floor: 1, status: Status.Move });
    const [bottom, setBottom] = useState<string>('20px');

    const style = {
      bottom: bottom,
      left: left,
      color: elevator.direction !== Direction.Idle ? '#383636' : 'white',
      background: elevator.direction !== Direction.Idle ? 'lightgray' : '#383636'
    }

    useEffect(() => {
      const intervalIdStop = setInterval(() => moveElevator(Status.Stop), delayDuration);
      const intervalIdMove = setInterval(() => moveElevator(Status.Move), delayDuration);

      return () => {
        clearInterval(intervalIdMove);
        clearInterval(intervalIdStop)
      };
    }, [])

    async function moveElevator(status: Status) {
      const request: ElevatorMovementRequest = {
        passengerLocation: { elevatorId: id, floor: elevator?.floor || 1 },
        status: status
      }

      const response = await fetch('elevator', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(request),
      });

      const data: Elevator = await response.json();

      setElevator(data);

      if (status === Status.Stop) {
        const floor = ((data.floor * floorIncrement) + 20) - floorIncrement;
        setBottom(`${floor}px`);
      }
    }

    return <div className="elevator" style={style}>
      <div>E{id}</div>
      <div>F{elevator?.floor || 1}</div>
    </div>
  }

  return (
    <div>
      <h1 id="tabelLabel">Elevators</h1>
      <Building></Building>
    </div>
  );

  async function elevatorGeneratedData() {
    const response = await fetch('elevator/generate');
    await response.json();
  }
}

export default App;