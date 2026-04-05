import asyncio
import time
import math
import keyboard
from websockets.server import serve


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


# Key mappings
# Pitch: W/S, Roll: Q/E, Yaw: A/D
# accel_x: I/K, accel_y: U/O, accel_z: J/L
# flex_1: N, flex_2: M, button_1: V, button_2: B

# attr: (inc_key, dec_key)
PAIRS = {
    "pitch": ("w", "s"),
    "roll": ("e", "q"),
    "yaw": ("d", "a"),
    "accel_x": ("i", "k"),
    "accel_y": ("o", "u"),
    "accel_z": ("l", "j"),
}

SINGLES_BOOL = {
    "flex_1": "n",
    "flex_2": "m",
    "button_1": "v",
    "button_2": "b",
}

SPEEDS = {
    "pitch": 360 / 500,
    "roll": 360 / 500,
    "yaw": 360 / 500,
    "accel_x": 100 / 1000,
    "accel_y": 100 / 1000,
    "accel_z": 100 / 1000,
}


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


async def keyboard_update_loop(sensor):
    # Poll keyboard state and update sensor values
    # keys held will increment/decrement numeric attributes by SPEED per tick
    tick = 0.001
    while True:
        for attr, (inc_key, dec_key) in PAIRS.items():
            inc = keyboard.is_pressed(inc_key)
            dec = keyboard.is_pressed(dec_key)
            val = getattr(sensor, attr)
            speed = SPEEDS[attr]
            lower, upper = BOUNDS[attr]
            if inc and not dec:
                val += speed
                if val > upper:
                    val = val - (upper - lower)  # Wrap around
            elif dec and not inc:
                val -= speed
                if val < lower:
                    val = val + (upper - lower)  # Wrap around
            setattr(sensor, attr, val)

        for attr, key in SINGLES_BOOL.items():
            setattr(sensor, attr, 1 if keyboard.is_pressed(key) else 0)

        await asyncio.sleep(tick)


async def main():
    global ws_set
    ws_set = set()
    sensor = SensorData()

    async with serve(ws_handler, "localhost", PORT):
        print(f"Keyboard WebSocket server started on ws://localhost:{PORT}")
        await asyncio.gather(
            broadcast_loop(ws_set, sensor), keyboard_update_loop(sensor)
        )


if __name__ == "__main__":
    try:
        asyncio.run(main())
    except KeyboardInterrupt:
        print("Shutting down")
