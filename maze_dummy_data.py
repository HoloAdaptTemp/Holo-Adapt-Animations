import asyncio
from websockets.server import serve
import random

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
    rotations = ["pitch", "roll", "yaw"]
    counter = 0
    returning_home = False

    reverse = random.choice([-1, 1]) 
    rotate_idx = random.randint(0, 2)
    current_attribute = rotations[rotate_idx]
    
    while True:
        await websocket.send(str(sensor_data))  # Send as CSV
        
        sensor_data.flex_1 = 0
        sensor_data.flex_2 = 0
        
        if random.randint(0, 3000) == 0:  # 1 in 3000 chance to flex finger 1
            sensor_data.flex_1 = UINT_MAX
        if random.randint(0, 3000) == 0:  # 1 in 3000 chance to flex finger 2
            sensor_data.flex_2 = UINT_MAX
        
        if counter >= UINT_MAX:
            counter = 0
            returning_home = not returning_home  # Change direction after each full cycle
            if not returning_home:
                rotate_idx = random.randint(0, 2)
                reverse = random.choice([-1, 1])
                current_attribute = rotations[rotate_idx]
        else:
            counter += SPEED
            if current_attribute == "pitch":
                sensor_data.pitch += int(SPEED * reverse) * (-1 if returning_home else 1)
            elif current_attribute == "roll":
                sensor_data.roll += int(SPEED * reverse) * (-1 if returning_home else 1)
            elif current_attribute == "yaw":
                sensor_data.yaw += int(SPEED * reverse) * (-1 if returning_home else 1)
        await asyncio.sleep(0.001)  # Send every millisecond


async def main():
    random.seed(0)
    # Start the WebSocket server on the local machine
    async with serve(send_data, "localhost", PORT):
        print(f"WebSocket server started on ws://localhost:{PORT}")
        await asyncio.Future()  # Run forever


asyncio.run(main())
