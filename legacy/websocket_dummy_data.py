import asyncio
from websockets.server import serve
import math
import time

# CSV: pitch,roll,yaw,accel_x,accel_y,accel_z,flex_1,flex_2,button_1,button_2

PORT = 8765  # Change this to your desired port

ROTATION_MAX = math.pi  # 180 degrees in radians
ROTATION_MIN = -math.pi  # -180 degrees in radians

ACCEL_MAX = 2**31 - 1  # int32 max
ACCEL_MIN = 2**31  # int32 min

FLEX_MAX = 1.0
FLEX_MIN = 0.0

BUTTON_MAX = 1
BUTTON_MIN = 0


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
        return f"{self.pitch},{self.roll},{self.yaw},{self.accel_x},{self.accel_y},{self.accel_z},{self.flex_1},{self.flex_2},{self.button_1},{self.button_2}"

    def next_attribute(self, attribute="button_2"):
        # If no attribute is provided, start with pitch (button_2 is the last attribute, so it will loop back to pitch)
        idx = self.list_of_attributes.index(attribute)
        idx += 1
        if idx >= len(self.list_of_attributes):
            idx = 0
        return self.list_of_attributes[idx]


# Function to send dummy data over the WebSocket
async def send_data(websocket):
    sensor_data = SensorData()
    current_attribute = sensor_data.list_of_attributes[
        0
    ]  # Start with the first attribute
    while True:
        print(
            f"Current attribute: {current_attribute}, Value: {getattr(sensor_data, current_attribute)}"
        )
        await websocket.send(str(sensor_data))  # Send as CSV
        # Increment logic for attributes
        current_value = getattr(sensor_data, current_attribute)
        if current_attribute in ["pitch", "roll", "yaw"]:
            new_value = current_value + math.pi / 500  # Increment by 0.1 radians
            if new_value > ROTATION_MAX:
                new_value = ROTATION_MIN
        elif current_attribute in ["accel_x", "accel_y", "accel_z"]:
            new_value = current_value + 100000000  # Increment by a large value
            if new_value > ACCEL_MAX:
                new_value = ACCEL_MIN
        elif current_attribute in ["flex_1", "flex_2"]:
            new_value = current_value + 0.01  # Increment by 0.01
            if new_value > FLEX_MAX:
                new_value = FLEX_MIN
        elif current_attribute in ["button_1", "button_2"]:
            new_value = (current_value + 1) % 2  # Toggle between 0 and 1
        else:
            new_value = current_value  # No change for unknown attributes
        setattr(
            sensor_data, current_attribute, new_value
        )  # Update the current attribute with the
        await asyncio.sleep(0)  # Yield control to allow other tasks to run


async def main():
    # Start the WebSocket server on the local machine
    async with serve(send_data, "localhost", PORT):
        print(f"WebSocket server started on ws://localhost:{PORT}")
        await asyncio.Future()  # Run forever


asyncio.run(main())
