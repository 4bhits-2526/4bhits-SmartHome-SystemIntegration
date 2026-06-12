import asyncio
import websockets

async def main():
    uri = "ws://127.0.0.1:8000/ws"
    try:
        async with websockets.connect(uri) as websocket:
            await websocket.send("ping")
            msg = await websocket.recv()
            print("RECEIVED:", msg)
    except Exception as error:
        print("ERROR:", error)

if __name__ == "__main__":
    asyncio.run(main())
