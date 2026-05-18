from fastapi.testclient import TestClient
from src.main import app

client = TestClient(app)
with client.websocket_connect('/ws') as ws:
    ws.send_text('ping')
    print('websocket connected successfully')
