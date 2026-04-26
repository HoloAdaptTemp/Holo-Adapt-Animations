import asyncio
import time
import math
from websockets.server import serve
import serial
import re

PORT = 8765

BOUNDS = {
    "pitch": (0, 360),
    "roll": (0, 360),
    "yaw": (0, 360),
    "accel_x": (-100, 100),
    "accel_y": (-100, 100),
    "accel_z": (-100, 100),
    "flex_1": (0.0, 1.0),
    "flex_2": (0.0, 1.0),
    "button_1": (0, 1),
    "button_2": (0, 1),
}


class SensorData:
    def __init__(self):
        self.list_of_attributes = [
            "pitch",
            "roll",
            "yaw",
            "accel_x",
            "accel_y",
            "accel_z",
            "flex_1",
            "flex_2",
            "button_1",
            "button_2",
        ]
        self.pitch = 0
        self.roll = 0
        self.yaw = 0
        self.accel_x = 0
        self.accel_y = 0
        self.accel_z = 0
        self.flex_1 = 0
        self.flex_2 = 0
        self.button_1 = 0
        self.button_2 = 0

    def __str__(self):
        # Show rotations and flex as floats so small changes are visible;
        # accel and buttons remain integers.
        return (
            f"{self.pitch:.6f},"
            f"{self.roll:.6f},"
            f"{self.yaw:.6f},"
            f"{int(self.accel_x)},"
            f"{int(self.accel_y)},"
            f"{int(self.accel_z)},"
            f"{self.flex_1:.3f},"
            f"{self.flex_2:.3f},"
            f"{int(self.button_1)},"
            f"{int(self.button_2)}"
        )


# Define global variables for sensor data
global pitch, roll, yaw, accel_x, accel_y, accel_z, flex_1, flex_2, button_1, button_2
pitch = 0
roll = 0
yaw = 0
accel_x = 0
accel_y = 0
accel_z = 0
flex_1 = 0
flex_2 = 0
button_1 = 0
button_2 = 0

# Initialize serial connection
ser = serial.Serial("COM12", 115200, timeout=1)


async def broadcast_loop(clients, sensor):
    # Broadcast current sensor CSV to all connected clients at ~100 Hz
    interval = 0.01
    while True:
        if clients:
            msg = str(sensor)
            await asyncio.gather(
                *(ws.send(msg) for ws in set(clients)), return_exceptions=True
            )
        await asyncio.sleep(interval)


async def ws_handler(websocket):
    # Register client
    ws_set.add(websocket)
    try:
        await websocket.wait_closed()
    finally:
        ws_set.discard(websocket)


async def global_update_loop(sensor):
    # Update sensor values from global variables at ~100 Hz
    interval = 0.01
    while True:
        sensor.pitch = pitch
        sensor.roll = roll
        sensor.yaw = yaw
        sensor.accel_x = accel_x
        sensor.accel_y = accel_y
        sensor.accel_z = accel_z
        sensor.flex_1 = flex_1
        sensor.flex_2 = flex_2
        sensor.button_1 = button_1
        sensor.button_2 = button_2
        await asyncio.sleep(interval)


async def serial_update_loop():
    # Update global variables from serial data
    global pitch, roll, yaw, accel_x, accel_y, accel_z, flex_1, flex_2, button_1, button_2
    while True:
        line = ser.readline().decode("utf-8", errors="ignore").strip()

        match = re.search(r":\s*\((.*?)\)", line)
        if not match:
            continue

        try:
            values = [v.strip() for v in match.group(1).split(",")]

            if len(values) != 10:
                continue

            pitch, roll, yaw, accel_x, accel_y, accel_z = map(float, values[:6])
            button_1, button_2, flex_1, flex_2 = map(int, values[6:])

        except Exception:
            print("Bad data:", line)


async def main():
    global ws_set
    ws_set = set()
    sensor = SensorData()

    async with serve(ws_handler, "localhost", PORT):
        print(f"Sensor WebSocket server started on ws://localhost:{PORT}")
        await asyncio.gather(
            broadcast_loop(ws_set, sensor),
            global_update_loop(sensor),
            serial_update_loop(),
        )


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("Shutting down")
