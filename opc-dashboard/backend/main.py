from fastapi import FastAPI, WebSocket
from fastapi.middleware.cors import CORSMiddleware
from asyncua import Client
import asyncio
import json
import time
from pathlib import Path
from contextlib import asynccontextmanager
from pydantic import BaseModel

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


SIMULATOR_URL = load_simulator_url()

class ModeRequest(BaseModel):
    url: str  # empty string = simulator, otherwise custom OPC UA URL

@asynccontextmanager
async def lifespan(app: FastAPI):
    asyncio.create_task(opcua_loop())
    yield

app = FastAPI(lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

clients = set()
channels = {}
on_times = {}
last = time.time()
current_url = SIMULATOR_URL
reconnect_needed = False

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
    global last, current_url, reconnect_needed
    while True:
        try:
            async with Client(current_url, timeout=30) as client:
                objects = client.get_objects_node()
                subscription = await client.create_subscription(100, Handler())
                await browse(objects, subscription)

                while True:
                    if reconnect_needed:
                        print(f"Reconnecting to {current_url}...")
                        reconnect_needed = False
                        break
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

@app.post("/mode")
async def set_mode(request: ModeRequest):
    global current_url, reconnect_needed, channels, on_times, last
    if not request.url or request.url.strip() == "":
        current_url = SIMULATOR_URL
    else:
        current_url = request.url.strip()
    reconnect_needed = True
    channels.clear()
    on_times.clear()
    last = time.time()
    return {"url": current_url}

@app.get("/mode")
async def get_mode():
    return {"url": current_url}

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    clients.add(websocket)
    try:
        while True:
            await websocket.receive_text()
    except Exception:
        clients.discard(websocket)

