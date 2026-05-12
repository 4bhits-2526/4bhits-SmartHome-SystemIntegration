from fastapi import FastAPI, WebSocket
from asyncua import Client
import asyncio
import json
import time
from pathlib import Path

# The local simulator writes its active port to opcua_simulator_config.json
ROOT = Path(__file__).resolve().parent.parent
CONFIG_FILE = ROOT / "opcua_simulator_config.json"


def load_simulator_url() -> str:
    if CONFIG_FILE.exists():
        try:
            config = json.loads(CONFIG_FILE.read_text(encoding="utf-8"))
            host = config.get("host", "localhost")
            port = config.get("port", 4843)
            return f"opc.tcp://{host}:{port}"
        except Exception:
            pass
    return "opc.tcp://localhost:4843"


URL = load_simulator_url()

app = FastAPI()

clients = set()
channels = {}
on_times = {}
last = time.time()

async def broadcast(data):
    dead = []
    for client in clients:
        try:
            await client.send_json(data)
        except Exception:
            dead.append(client)
    for dead_client in dead:
        clients.discard(dead_client)


def is_valid(nodeid: str, name: str) -> bool:
    text = (nodeid + str(name)).lower()
    return any(k in text for k in [
        "x20", "ai", "input", "analog", "channel",
        "in", "value", "switchvaluehw", "room",
        "motor", "valve", "pressure", "humidity", "temp"
    ])

class Handler:
    def datachange_notification(self, node, val, data):
        try:
            nodeid = node.nodeid.to_string()
            channels[nodeid] = float(val)
            if nodeid not in on_times:
                on_times[nodeid] = 0
        except Exception:
            pass

async def browse(node, subscription):
    children = await node.get_children()
    for child in children:
        try:
            nodeid = child.nodeid.to_string()
            name = (await child.read_browse_name()).Name
            if is_valid(nodeid, name):
                await subscription.subscribe_data_change(child)
            await browse(child, subscription)
        except Exception:
            pass

async def opcua_loop():
    global last
    while True:
        try:
            async with Client(URL, timeout=30) as client:
                objects = client.get_objects_node()
                subscription = await client.create_subscription(100, Handler())
                await browse(objects, subscription)

                while True:
                    now = time.time()
                    delta = (now - last) * 1000
                    last = now
                    for nodeid, value in list(channels.items()):
                        try:
                            if float(value) == 1:
                                on_times[nodeid] += delta
                        except Exception:
                            pass
                    await broadcast({
                        "channels": channels,
                        "on_times": on_times
                    })
                    await asyncio.sleep(0.2)
        except Exception as error:
            print("OPC UA connection error:", error)
            await asyncio.sleep(5)

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    clients.add(websocket)
    try:
        while True:
            await websocket.receive_text()
    except Exception:
        clients.discard(websocket)

@app.on_event("startup")
async def startup_event():
    asyncio.create_task(opcua_loop())
