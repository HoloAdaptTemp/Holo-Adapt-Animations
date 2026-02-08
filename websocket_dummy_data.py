import asyncio
from websockets.server import serve

# CSV: pitch,roll,yaw,accel_x,accel_y,accel_z,flex_1,flex_2,button_1,button_2

PORT = 8765  # Change this to your desired port

UINT_MAX = 2**32 - 1
SPEED = (UINT_MAX + 1) / (2**13)


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
        await websocket.send(str(sensor_data))  # Send as CSV
        # Increment logic for attributes
        current_value = getattr(sensor_data, current_attribute)
        is_button = current_attribute.startswith("button_")
        button_timer = 0
        if current_value < UINT_MAX:
            setattr(sensor_data, current_attribute, current_value + SPEED)
        else:
            setattr(sensor_data, current_attribute, 0)
            # Move to next attribute
            current_attribute = sensor_data.next_attribute(current_attribute)
        await asyncio.sleep(0.001)  # Send every millisecond


async def main():
    # Start the WebSocket server on the local machine
    async with serve(send_data, "localhost", PORT):
        print(f"WebSocket server started on ws://localhost:{PORT}")
        await asyncio.Future()  # Run forever


asyncio.run(main())
