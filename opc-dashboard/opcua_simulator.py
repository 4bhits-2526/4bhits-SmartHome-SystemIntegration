"""OPC UA server simulator for the dashboard.

Run this script to start a local OPC UA server that exposes simulated
channel variables and updates them continuously.

Use the dashboard backend with a URL like:
    opc.tcp://localhost:4843
"""

import argparse
import asyncio
import json
import math
import random
import socket
from pathlib import Path
from asyncua import Server

ROOT = Path(__file__).resolve().parent
CONFIG_FILE = ROOT / "opcua_simulator_config.json"


CHANNELS = [
    ("X20_Input_1", 0),
    ("AI_Temperature", 22.5),
    ("Channel_2", 0),
    ("Analog_Pressure", 1.2),
    ("Motor_Speed", 0),
    ("Valve_Status", 0),
]


def is_port_free(host: str, port: int) -> bool:
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        try:
            sock.bind((host, port))
            return True
        except OSError:
            return False


def choose_free_port(host: str, start: int = 4840, end: int = 4860) -> int:
    for port in range(start, end):
        if is_port_free(host, port):
            return port
    raise RuntimeError(f"No free port found between {start} and {end}")


async def create_simulated_server(host: str, port: int, interval: float):
    server = Server()
    await server.init()

    endpoint = f"opc.tcp://{host}:{port}"
    server.set_endpoint(endpoint)
    server.set_server_name("OPC UA Simulator")

    uri = "http://examples.opcfoundation.org/UA/SimulatedServer"
    idx = await server.register_namespace(uri)

    objects = server.get_objects_node()
    device = await objects.add_object(idx, "SimulatedDevice")

    nodes = []
    for name, value in CHANNELS:
        node = await device.add_variable(idx, name, value)
        await node.set_writable()
        nodes.append((name, node))

    print(f"OPC UA simulator running at {endpoint}")
    print("Available nodes:")
    for name, _ in nodes:
        print(f"  - {name}")
    print("Press Ctrl+C to stop.")

    await server.start()

    try:
        tick = 0
        while True:
            tick += 1

            # toggle some boolean-style channels
            await nodes[0][1].write_value(int((tick // 2) % 2))
            await nodes[2][1].write_value(int((tick // 4) % 2))
            await nodes[5][1].write_value(int((tick // 3) % 2))

            # analog / numeric values with smooth updates
            await nodes[1][1].write_value(22.0 + math.sin(tick / 5.0) * 2.5)
            await nodes[3][1].write_value(1.0 + math.sin(tick / 4.0) * 0.3)
            await nodes[4][1].write_value((tick * 5) % 120)

            await asyncio.sleep(interval)
    finally:
        print("Stopping OPC UA simulator...")
        await server.stop()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Start an OPC UA server simulator.")
    parser.add_argument("--host", default="127.0.0.1", help="Host address for the OPC UA endpoint")
    parser.add_argument("--port", type=int, default=4843, help="OPC UA server port")
    parser.add_argument("--interval", type=float, default=1.0, help="Update interval in seconds")
    return parser.parse_args()


def save_config(host: str, port: int) -> None:
    CONFIG_FILE.write_text(json.dumps({"host": host, "port": port}), encoding="utf-8")


def main() -> None:
    args = parse_args()
    host = args.host
    port = args.port

    if not is_port_free(host, port):
        port = choose_free_port(host, port, port + 20)
        print(f"Port {args.port} is busy, using {port} instead.")

    save_config(host, port)

    try:
        asyncio.run(create_simulated_server(host, port, args.interval))
    except KeyboardInterrupt:
        print("Received interrupt, shutting down.")


if __name__ == "__main__":
    main()
