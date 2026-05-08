import asyncio
import tkinter as tk
from asyncua import Client

channels = {}

# ---------------- GUI ----------------
root = tk.Tk()
root.title("B&R OPC UA Live Monitor")
root.geometry("700x400")

labels = []

status = tk.Label(root, text="Starting...", font=("Arial", 12))
status.pack()

counter_lbl = tk.Label(root, text="0 channels", font=("Arial", 12))
counter_lbl.pack()


def update_gui():
    counter_lbl.config(text=f"{len(channels)} active channels")

    # rebuild display
    for l in labels:
        l.destroy()

    labels.clear()

    for k in sorted(channels.keys()):
        lbl = tk.Label(root, text=f"{k}: {channels[k]:.2f}", font=("Consolas", 11))
        lbl.pack(anchor="w")
        labels.append(lbl)

    root.after(500, update_gui)


# ---------------- FILTER ----------------
def is_valid(nodeid, name):
    text = (nodeid + name).lower()

    return any(k in text for k in [
        "x20",
        "ai",
        "input",
        "analog",
        "channel",
        "in",
        "value"
    ])


# ---------------- HANDLER ----------------
class Handler:
    def datachange_notification(self, node, val, data):
        try:
            key = node.nodeid.to_string()
            channels[key] = float(val)
        except:
            pass


# ---------------- BROWSER ----------------
async def browse(node, sub):
    children = await node.get_children()

    for c in children:
        try:
            nodeid = c.nodeid.to_string()
            name = (await c.read_browse_name()).Name
            node_class = await c.read_node_class()

            if node_class.name == "Variable":
                if is_valid(nodeid, name):
                    print("SUB:", name, nodeid)
                    await sub.subscribe_data_change(c)

            await browse(c, sub)

        except:
            pass


# ---------------- OPC UA ----------------
async def opcua():
    url = "opc.tcp://192.168.1.61:4840"

    async with Client(url=url) as client:
        status.config(text="CONNECTED")

        plc = client.get_node("ns=4;i=20000")  # <-- WICHTIG

        handler = Handler()
        sub = await client.create_subscription(100, handler)

        await browse(plc, sub)

        status.config(text="MONITORING")

        while True:
            await asyncio.sleep(1)


def start():
    asyncio.run(opcua())


import threading
threading.Thread(target=start, daemon=True).start()

root.after(500, update_gui)
root.mainloop()