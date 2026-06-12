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

SAVE_FILE = "selected_channels_real.json"
LOG_FILE = "tracking_log_real.txt"

channels = {}
on_times = {}
selected_channels = []
tracked_nodes = set()

# Mapping für Clean Names
name_map = {}

# Toggle für Clean Names
show_clean_names = True

# =========================================================
# LOAD SAVED
# =========================================================

if os.path.exists(SAVE_FILE):
    try:
        with open(SAVE_FILE, "r") as f:
            selected_channels = json.load(f)
    except:
        selected_channels = []

# =========================================================
# ROOT UI
# =========================================================

root = tk.Tk()
root.title("B&R OPC UA Monitor (Real)")
root.geometry("1920x1080")
root.configure(bg="#111827")

style = ttk.Style()
style.theme_use("clam")

style.configure(
    "Treeview",
    background="#1F2937",
    foreground="white",
    fieldbackground="#1F2937",
    rowheight=28,
    font=("Segoe UI", 10)
)

style.configure(
    "Treeview.Heading",
    background="#374151",
    foreground="white",
    font=("Segoe UI", 10, "bold")
)

style.map("Treeview", background=[("selected", "#2563EB")])

# =========================================================
# HEADER
# =========================================================

header = tk.Frame(root, bg="#111827")
header.pack(fill="x", pady=15)

tk.Label(
    header,
    text="B&R OPC UA Live Monitor (Real Connection)",
    font=("Segoe UI", 20, "bold"),
    bg="#111827",
    fg="white"
).pack()

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

columns = ("Track", "NodeID", "Value", "State", "ON Time (ms)")

tree = ttk.Treeview(table_frame, columns=columns, show="headings")

for col in columns:
    tree.heading(col, text=col)

tree.column("Track", width=80, anchor="center")
tree.column("NodeID", width=550)
tree.column("Value", width=100, anchor="center")
tree.column("State", width=100, anchor="center")
tree.column("ON Time (ms)", width=140, anchor="center")

scroll = ttk.Scrollbar(table_frame, orient="vertical", command=tree.yview)
tree.configure(yscrollcommand=scroll.set)

tree.pack(side="left", fill="both", expand=True)
scroll.pack(side="right", fill="y")

# =========================================================
# HELPERS
# =========================================================

def clean_name(nodeid: str) -> str:
    """Zeige nur den letzten Teil des NodeID wenn clean_names aktiv"""
    if not show_clean_names:
        return nodeid
    if "::" in nodeid:
        return nodeid.split("::")[-1]
    return nodeid

# =========================================================
# BUTTONS
# =========================================================

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
# SAVE
# =========================================================

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

    tk.Label(
        win,
        text="Tracked Channels",
        font=("Segoe UI", 18, "bold"),
        bg="#111827",
        fg="white"
    ).pack(pady=15)

    text = tk.Text(
        win,
        bg="#1F2937",
        fg="white",
        insertbackground="white",
        font=("Consolas", 11)
    )
    text.pack(fill="both", expand=True, padx=20, pady=10)

    def update():
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

            text.insert(
                tk.END,
                f"{clean_name(nodeid)}\n"
                f"   Value: {val}\n"
                f"   State: {state}\n"
                f"   ON Time: {t}ms\n\n"
            )

        win.after(1000, update)

    update()

# =========================================================
# TOGGLE CLEAN NAMES
# =========================================================

def toggle_names():
    global show_clean_names
    show_clean_names = not show_clean_names

# =========================================================
# BUTTON FRAME
# =========================================================

button_frame = tk.Frame(root, bg="#111827")
button_frame.pack(fill="x", pady=10)

modern_button(button_frame, "Save Selection", save_selection).pack(side="left", padx=20)
modern_button(button_frame, "Open Tracking", open_tracking).pack(side="left")
modern_button(button_frame, "Toggle Clean Names", toggle_names).pack(side="left", padx=20)

# =========================================================
# FILTER
# =========================================================

def is_valid(nodeid, name):
    text = (nodeid + name).lower()
    return any(k in text for k in [
        "x20", "ai", "input", "analog", "channel",
        "in", "value", "switchvaluehw", "room",
        "motor", "valve", "pressure", "humidity", "temp"
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

            f.write(f"{ts} | {nodeid} | value={val} | on_time={t}ms\n")

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
# GUI UPDATE (Millisekunden!)
# =========================================================

last_update = datetime.now()

def update_gui():
    global last_update, name_map

    now = datetime.now()
    delta = (now - last_update).total_seconds()
    last_update = now

    # Umwandlung in Millisekunden
    delta_ms = delta * 1000

    # Akkumuliere die ON-Zeit in Millisekunden
    for nodeid, val in channels.items():
        try:
            if float(val) == 1:
                on_times[nodeid] += delta_ms
        except:
            pass

    # Aktualisiere die GUI
    tree.delete(*tree.get_children())
    name_map = {}

    for nodeid in sorted(channels.keys()):
        val = channels.get(nodeid, 0)

        state = "OFF"
        try:
            if float(val) == 1:
                state = "ON"
        except:
            pass

        tracked = "✓" if nodeid in tracked_nodes else ""
        clean = clean_name(nodeid)

        name_map[clean] = nodeid

        tree.insert(
            "",
            "end",
            values=(
                tracked,
                clean,
                val,
                state,
                f"{on_times.get(nodeid, 0):.0f}"
            )
        )

    status_label.config(text=f"{len(channels)} channels")
    write_log()
    root.after(250, update_gui)

# =========================================================
# CLICK TRACKING
# =========================================================

def on_click(event):
    item = tree.identify_row(event.y)
    if not item:
        return

    values = tree.item(item, "values")
    clean = values[1]
    nodeid = name_map.get(clean)

    if not nodeid:
        return

    if nodeid in tracked_nodes:
        tracked_nodes.remove(nodeid)
    else:
        tracked_nodes.add(nodeid)

tree.bind("<Button-1>", on_click)

# =========================================================
# RESTORE TRACKED
# =========================================================

for n in selected_channels:
    tracked_nodes.add(n)

# =========================================================
# OPC UA SETUP
# =========================================================

async def browse(node, sub):
    children = await node.get_children()

    for c in children:
        try:
            nodeid = c.nodeid.to_string()
            name = (await c.read_browse_name()).Name

            if is_valid(nodeid, name):
                await sub.subscribe_data_change(c)

            await browse(c, sub)

        except:
            pass

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
# START THREADS
# =========================================================

def start():
    asyncio.run(opcua())

threading.Thread(target=start, daemon=True).start()

# =========================================================
# START GUI
# =========================================================

update_gui()
root.mainloop()
