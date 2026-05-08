import asyncio
import threading
import tkinter as tk
from tkinter import ttk, Toplevel
from datetime import datetime
from asyncua import Client
import json
import os

# =========================================================
# CONFIG
# =========================================================

URL = "opc.tcp://192.168.1.61:4840"

SAVE_FILE = "selected_channels.json"
LOG_FILE = "tracking_log.txt"

channels = {}
on_times = {}
checkboxes = {}
selected_channels = []

# =========================================================
# LOAD SAVED CHANNELS
# =========================================================

if os.path.exists(SAVE_FILE):

    try:
        with open(SAVE_FILE, "r") as f:
            selected_channels = json.load(f)

    except:
        selected_channels = []

# =========================================================
# ROOT WINDOW
# =========================================================

root = tk.Tk()
root.title("B&R OPC UA Monitor")
root.geometry("1100x700")
root.configure(bg="#111827")

style = ttk.Style()
style.theme_use("clam")

style.configure(
    "Treeview",
    background="#1F2937",
    foreground="white",
    fieldbackground="#1F2937",
    rowheight=28,
    borderwidth=0,
    font=("Segoe UI", 10)
)

style.configure(
    "Treeview.Heading",
    background="#374151",
    foreground="white",
    font=("Segoe UI", 10, "bold")
)

style.map(
    "Treeview",
    background=[("selected", "#2563EB")]
)

# =========================================================
# HEADER
# =========================================================

header = tk.Frame(root, bg="#111827")
header.pack(fill="x", pady=15)

title = tk.Label(
    header,
    text="B&R OPC UA Live Monitor",
    font=("Segoe UI", 20, "bold"),
    bg="#111827",
    fg="white"
)

title.pack()

status_label = tk.Label(
    header,
    text="Starting...",
    font=("Segoe UI", 11),
    bg="#111827",
    fg="#9CA3AF"
)

status_label.pack()

# =========================================================
# TABLE
# =========================================================

table_frame = tk.Frame(root, bg="#111827")
table_frame.pack(fill="both", expand=True, padx=20, pady=10)

columns = ("Track", "NodeID", "Value", "State", "ON Time")

tree = ttk.Treeview(
    table_frame,
    columns=columns,
    show="headings"
)

for col in columns:
    tree.heading(col, text=col)

tree.column("Track", width=80, anchor="center")
tree.column("NodeID", width=550)
tree.column("Value", width=100, anchor="center")
tree.column("State", width=100, anchor="center")
tree.column("ON Time", width=120, anchor="center")

scroll = ttk.Scrollbar(
    table_frame,
    orient="vertical",
    command=tree.yview
)

tree.configure(yscrollcommand=scroll.set)

tree.pack(side="left", fill="both", expand=True)
scroll.pack(side="right", fill="y")

# =========================================================
# BUTTONS
# =========================================================

button_frame = tk.Frame(root, bg="#111827")
button_frame.pack(fill="x", pady=10)

def modern_button(parent, text, cmd):

    return tk.Button(
        parent,
        text=text,
        command=cmd,
        bg="#2563EB",
        fg="white",
        activebackground="#1D4ED8",
        activeforeground="white",
        relief="flat",
        padx=20,
        pady=10,
        font=("Segoe UI", 10, "bold"),
        cursor="hand2"
    )

# =========================================================
# SAVE SELECTION
# =========================================================

tracked_nodes = set()

def save_selection():

    with open(SAVE_FILE, "w") as f:
        json.dump(list(tracked_nodes), f, indent=2)

# =========================================================
# TRACK WINDOW
# =========================================================

def open_tracking():

    win = Toplevel(root)

    win.title("Live Tracking")
    win.geometry("800x400")
    win.configure(bg="#111827")

    title = tk.Label(
        win,
        text="Tracked Channels",
        font=("Segoe UI", 18, "bold"),
        bg="#111827",
        fg="white"
    )

    title.pack(pady=15)

    text = tk.Text(
        win,
        bg="#1F2937",
        fg="white",
        insertbackground="white",
        relief="flat",
        font=("Consolas", 11)
    )

    text.pack(fill="both", expand=True, padx=20, pady=10)

    def update_tracking():

        text.delete("1.0", tk.END)

        for nodeid in tracked_nodes:

            val = channels.get(nodeid, 0)
            t = int(on_times.get(nodeid, 0))

            state = "OFF"

            try:
                if float(val) == 1:
                    state = "ON"
            except:
                pass

            line = (
                f"{nodeid}\n"
                f"   Value: {val}\n"
                f"   State: {state}\n"
                f"   ON Time: {t}s\n\n"
            )

            text.insert(tk.END, line)

        win.after(1000, update_tracking)

    update_tracking()

# =========================================================
# BUTTONS
# =========================================================

save_btn = modern_button(
    button_frame,
    "Save Selection",
    save_selection
)

save_btn.pack(side="left", padx=20)

track_btn = modern_button(
    button_frame,
    "Open Tracking Window",
    open_tracking
)

track_btn.pack(side="left")

# =========================================================
# FILTER
# =========================================================

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

# =========================================================
# LOGGING
# =========================================================

def write_log():

    with open(LOG_FILE, "a", encoding="utf-8") as f:

        ts = datetime.now().strftime("%Y-%m-%d %H:%M:%S")

        for nodeid in tracked_nodes:

            val = channels.get(nodeid, 0)
            t = int(on_times.get(nodeid, 0))

            f.write(
                f"{ts} | {nodeid} | value={val} | on_time={t}s\n"
            )

# =========================================================
# HANDLER
# =========================================================

class Handler:

    def datachange_notification(self, node, val, data):

        try:

            nodeid = node.nodeid.to_string()

            channels[nodeid] = float(val)

            if nodeid not in on_times:
                on_times[nodeid] = 0

        except:
            pass

# =========================================================
# TABLE UPDATE
# =========================================================

last_update = datetime.now()

def update_gui():

    global last_update

    now = datetime.now()

    delta = (now - last_update).total_seconds()

    last_update = now

    for nodeid, val in channels.items():

        try:
            if float(val) == 1:
                on_times[nodeid] += delta
        except:
            pass

    existing = tree.get_children()

    for item in existing:
        tree.delete(item)

    for nodeid in sorted(channels.keys()):

        val = channels.get(nodeid, 0)

        state = "OFF"

        try:
            if float(val) == 1:
                state = "ON"
        except:
            pass

        tracked = "✓" if nodeid in tracked_nodes else ""

        tree.insert(
            "",
            "end",
            values=(
                tracked,
                nodeid,
                val,
                state,
                f"{int(on_times.get(nodeid, 0))} s"
            )
        )

    status_label.config(
        text=f"{len(channels)} subscribed channels"
    )

    write_log()

    root.after(1000, update_gui)

# =========================================================
# CLICK SELECT
# =========================================================

def on_click(event):

    item = tree.identify_row(event.y)

    if not item:
        return

    values_row = tree.item(item, "values")

    nodeid = values_row[1]

    if nodeid in tracked_nodes:
        tracked_nodes.remove(nodeid)
    else:
        tracked_nodes.add(nodeid)

tree.bind("<Button-1>", on_click)

# =========================================================
# ADD SAVED
# =========================================================

for n in selected_channels:
    tracked_nodes.add(n)

# =========================================================
# BROWSER
# =========================================================

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

# =========================================================
# OPC UA
# =========================================================

async def opcua():

    async with Client(url=URL, timeout=30) as client:

        status_label.config(text="CONNECTED")

        plc = client.get_node("ns=4;i=20000")

        handler = Handler()

        sub = await client.create_subscription(100, handler)

        await browse(plc, sub)

        status_label.config(text="MONITORING")

        while True:
            await asyncio.sleep(1)

# =========================================================
# THREAD
# =========================================================

def start():
    asyncio.run(opcua())

threading.Thread(
    target=start,
    daemon=True
).start()

# =========================================================
# START
# =========================================================

update_gui()

root.mainloop()