import asyncio
import threading
import tkinter as tk
from tkinter import Toplevel
from datetime import datetime
from asyncua import Client
import json
import os

# -------------------------------------------------
# OPC UA
# -------------------------------------------------

URL = "opc.tcp://192.168.1.61:4840"

channels = {}
on_times = {}

# gespeicherte Auswahl
SAVE_FILE = "selected_channels.json"

# checkboxen
checkboxes = {}

# ausgewählte channels
selected_channels = []

# -------------------------------------------------
# LOAD SAVED
# -------------------------------------------------

if os.path.exists(SAVE_FILE):

    with open(SAVE_FILE, "r") as f:

        try:
            selected_channels = json.load(f)
        except:
            selected_channels = []

# -------------------------------------------------
# GUI
# -------------------------------------------------

root = tk.Tk()
root.title("B&R OPC UA Live Monitor")
root.geometry("900x600")

status = tk.Label(root, text="Starting...", font=("Arial", 12))
status.pack()

counter_lbl = tk.Label(root, text="0 active channels", font=("Arial", 12))
counter_lbl.pack()

frame = tk.Frame(root)
frame.pack(fill="both", expand=True)

canvas = tk.Canvas(frame)

scrollbar = tk.Scrollbar(
    frame,
    orient="vertical",
    command=canvas.yview
)

scrollable_frame = tk.Frame(canvas)

scrollable_frame.bind(
    "<Configure>",
    lambda e: canvas.configure(
        scrollregion=canvas.bbox("all")
    )
)

canvas.create_window(
    (0, 0),
    window=scrollable_frame,
    anchor="nw"
)

canvas.configure(
    yscrollcommand=scrollbar.set
)

canvas.pack(
    side="left",
    fill="both",
    expand=True
)

scrollbar.pack(
    side="right",
    fill="y"
)

# -------------------------------------------------
# SAVE SELECTION
# -------------------------------------------------

def save_selection():

    selected = []

    for nodeid, var in checkboxes.items():

        if var.get():
            selected.append(nodeid)

    with open(SAVE_FILE, "w") as f:
        json.dump(selected, f, indent=2)

# -------------------------------------------------
# TRACK WINDOW
# -------------------------------------------------

def open_tracking():

    win = Toplevel(root)

    win.title("Tracking Window")
    win.geometry("700x400")

    labels = {}

    # aktuelle Auswahl laden
    selected = []

    for nodeid, var in checkboxes.items():

        if var.get():
            selected.append(nodeid)

    if len(selected) == 0:

        lbl = tk.Label(
            win,
            text="No channels selected",
            font=("Arial", 12)
        )

        lbl.pack()

        return

    # Labels
    for nodeid in selected:

        lbl = tk.Label(
            win,
            text="",
            font=("Consolas", 11)
        )

        lbl.pack(anchor="w")

        labels[nodeid] = lbl

    # live update
    def update_tracking():

        for nodeid in selected:

            val = channels.get(nodeid, 0)

            t = int(on_times.get(nodeid, 0))

            state = "OFF"

            try:
                if float(val) == 1:
                    state = "ON"
            except:
                pass

            labels[nodeid].config(
                text=f"{nodeid} | {state} | ON Time: {t}s | Value={val}"
            )

        win.after(1000, update_tracking)

    update_tracking()

# -------------------------------------------------
# BUTTONS
# -------------------------------------------------

btn_frame = tk.Frame(root)
btn_frame.pack(pady=10)

save_btn = tk.Button(
    btn_frame,
    text="Save Selection",
    command=save_selection
)

save_btn.pack(side="left", padx=10)

track_btn = tk.Button(
    btn_frame,
    text="Open Tracking Window",
    command=open_tracking
)

track_btn.pack(side="left", padx=10)

# -------------------------------------------------
# LOGGING
# -------------------------------------------------

def write_log():

    with open("tracking_log.txt", "a", encoding="utf-8") as f:

        ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        for nodeid, var in checkboxes.items():

            if var.get():

                val = channels.get(nodeid, 0)

                t = int(on_times.get(nodeid, 0))

                f.write(
                    f"{ts} | {nodeid} | value={val} | on_time={t}s\n"
                )

# -------------------------------------------------
# GUI UPDATE
# -------------------------------------------------

last_update = datetime.now()

def update_gui():

    global last_update

    now = datetime.now()

    delta = (now - last_update).total_seconds()

    last_update = now

    # zeit zählen
    for nodeid, val in channels.items():

        try:
            if float(val) == 1:
                on_times[nodeid] += delta
        except:
            pass

    counter_lbl.config(
        text=f"{len(channels)} active channels"
    )

    write_log()

    root.after(1000, update_gui)

# -------------------------------------------------
# FILTER
# -------------------------------------------------

def is_valid(nodeid, name):

    text = (nodeid + name).lower()

    return any(k in text for k in [
        "x20",
        "ai",
        "input",
        "analog",
        "channel",
        "in",
        "value",
        "switchvaluehw",
        "room"
    ])

# -------------------------------------------------
# HANDLER
# -------------------------------------------------

class Handler:

    def datachange_notification(self, node, val, data):

        try:

            key = node.nodeid.to_string()

            channels[key] = float(val)

            if key not in on_times:
                on_times[key] = 0

        except:
            pass

# -------------------------------------------------
# ADD CHANNEL GUI
# -------------------------------------------------

def add_channel(nodeid):

    if nodeid in checkboxes:
        return

    var = tk.BooleanVar()

    # gespeicherte Auswahl laden
    if nodeid in selected_channels:
        var.set(True)

    cb = tk.Checkbutton(
        scrollable_frame,
        text=nodeid,
        variable=var
    )

    cb.pack(anchor="w")

    checkboxes[nodeid] = var

# -------------------------------------------------
# BROWSER
# -------------------------------------------------

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

                    root.after(
                        0,
                        lambda nid=nodeid: add_channel(nid)
                    )

            await browse(c, sub)

        except:
            pass

# -------------------------------------------------
# OPC UA
# -------------------------------------------------

async def opcua():

    async with Client(url=URL, timeout=30) as client:

        status.config(text="CONNECTED")

        plc = client.get_node("ns=4;i=20000")

        handler = Handler()

        sub = await client.create_subscription(100, handler)

        await browse(plc, sub)

        status.config(text="MONITORING")

        while True:
            await asyncio.sleep(1)

# -------------------------------------------------
# THREAD
# -------------------------------------------------

def start():

    asyncio.run(opcua())

threading.Thread(
    target=start,
    daemon=True
).start()

# -------------------------------------------------
# START
# -------------------------------------------------

update_gui()

root.mainloop()