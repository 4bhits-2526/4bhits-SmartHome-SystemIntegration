import { useEffect, useState } from "react";

export default function App() {

  const [sidebarOpen, setSidebarOpen] = useState(false);

  const items = [
    "X20_Input_1",
    "AI_Temperature",
    "Channel_2",
    "Analog_Pressure",
    "Motor_Speed",
    "Valve_Status"
  ];

  const [active, setActive] = useState([
    "X20_Input_1",
    "AI_Temperature"
  ]);

  const [data, setData] = useState({});

  useEffect(() => {
    const ws = new WebSocket("ws://localhost:8000/ws");
    ws.onopen = () => console.log("Connected to backend WebSocket");
    ws.onmessage = (event) => {
      try {
        const { channels, on_times } = JSON.parse(event.data);
        const d = {};
        active.forEach((name) => {
          const match = Object.keys(channels).find(nodeId =>
            nodeId.toLowerCase().includes(name.toLowerCase()) ||
            name.toLowerCase().includes(nodeId.toLowerCase())
          );
          if (match) {
            d[name] = { value: channels[match], time: Math.floor(on_times[match] || 0) };
          }
        });
        setData(d);
      } catch (err) {
        console.error("WebSocket message error:", err);
      }
    };
    ws.onerror = (err) => console.error("WebSocket error:", err);
    ws.onclose = () => console.log("WebSocket closed");
    return () => ws.close();
  }, [active]);

  function toggle(n) {
    setActive((prev) =>
      prev.includes(n) ? prev.filter((x) => x !== n) : [...prev, n]
    );
  }

  return (
    <div style={styles.app}>
      <div style={{ ...styles.sidebar, width: sidebarOpen ? 220 : 48 }}>
        <button style={styles.burger} onClick={() => setSidebarOpen(!sidebarOpen)}>
          ☰
        </button>
        {sidebarOpen && (
          <>
            <div style={styles.sidebarHeader}>Sensors</div>
            <div style={styles.sidebarList}>
              {items.map((n) => {
                const isOn = active.includes(n);
                return (
                  <button
                    key={n}
                    onClick={() => toggle(n)}
                    style={{
                      ...styles.sidebarItem,
                      ...(isOn ? styles.sidebarItemActive : {})
                    }}
                  >
                    <span style={styles.sidebarName}>{n}</span>
                    <span style={{ ...styles.sidebarDot, background: isOn ? "#22c55e" : "#6b7280" }} />
                  </button>
                );
              })}
            </div>
          </>
        )}
      </div>
      <div style={{ ...styles.main, marginLeft: sidebarOpen ? 252 : 72 }}>
        <div style={styles.header}>
          <span style={styles.headerAccent}>OPC UA</span> Dashboard
        </div>
        <div style={styles.grid}>
          {active.map((n) => {
            const d = data[n] || {};
            return (
              <div
                key={n}
                style={styles.card}
                onMouseEnter={(e) => {
                  e.currentTarget.style.transform = "translateY(-2px)";
                  e.currentTarget.style.borderColor = "rgba(255,255,255,0.12)";
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.transform = "translateY(0)";
                  e.currentTarget.style.borderColor = "rgba(255,255,255,0.06)";
                }}
              >
                <div style={styles.name}>{n}</div>
                <div style={styles.value}>
                  {typeof d.value === 'number' ? d.value.toFixed(1) : (d.value ?? 0)}
                </div>
                <div style={{ ...styles.status, color: Number(d.value) > 0 ? "#22c55e" : "#6b7280" }}>
                  {Number(d.value) > 0 ? "Active" : "Idle"}
                </div>
                <div style={styles.time}>
                  {Math.floor(d.time || 0).toLocaleString()} ms
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

/* =========================
   CLEAN UI
========================= */

const styles = {
  app: {
    minHeight: "100vh",
    display: "flex",
    background: "#0f1117",
    color: "rgba(255,255,255,0.9)",
    fontFamily: "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
    WebkitFontSmoothing: "antialiased",
    MozOsxFontSmoothing: "grayscale"
  },

  sidebar: {
    position: "fixed",
    top: 16,
    left: 16,
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    background: "rgba(255,255,255,0.02)",
    backdropFilter: "blur(24px)",
    borderRadius: 12,
    padding: 10,
    transition: "width 0.25s ease",
    boxShadow: "0 4px 24px rgba(0,0,0,0.25)",
    overflow: "hidden"
  },

  burger: {
    width: 28,
    height: 28,
    borderRadius: 8,
    border: "none",
    background: "rgba(255,255,255,0.06)",
    color: "rgba(255,255,255,0.7)",
    cursor: "pointer",
    fontSize: 14,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    flexShrink: 0,
    transition: "background 0.15s, color 0.15s"
  },

  sidebarHeader: {
    fontSize: 10,
    fontWeight: 600,
    textTransform: "uppercase",
    letterSpacing: "1px",
    color: "rgba(255,255,255,0.35)",
    marginBottom: 10,
    paddingLeft: 4
  },

  sidebarList: {
    display: "flex",
    flexDirection: "column",
    gap: 2
  },

  sidebarItem: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    padding: "8px 10px",
    borderRadius: 8,
    border: "none",
    background: "transparent",
    color: "rgba(255,255,255,0.55)",
    cursor: "pointer",
    textAlign: "left",
    transition: "background 0.15s, color 0.15s",
    position: "relative"
  },

  sidebarItemActive: {
    background: "rgba(255,255,255,0.05)",
    color: "rgba(255,255,255,0.9)"
  },

  sidebarName: {
    fontSize: 12,
    fontWeight: 500,
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap"
  },

  sidebarDot: {
    width: 6,
    height: 6,
    borderRadius: "50%",
    flexShrink: 0,
    marginLeft: 8,
    transition: "background 0.2s"
  },

  main: {
    padding: "32px 36px",
    width: "100%",
    transition: "margin-left 0.25s ease"
  },

  header: {
    fontSize: 24,
    fontWeight: 600,
    marginBottom: 28,
    letterSpacing: "-0.3px",
    color: "rgba(255,255,255,0.85)"
  },

  headerAccent: {
    color: "rgba(255,255,255,0.45)",
    fontWeight: 500
  },

  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))",
    gap: 12
  },

  card: {
    background: "rgba(255,255,255,0.02)",
    backdropFilter: "blur(12px)",
    borderRadius: 12,
    padding: "14px 16px",
    border: "1px solid rgba(255,255,255,0.05)",
    transition: "transform 0.2s ease, border-color 0.2s ease",
    cursor: "default"
  },

  name: {
    fontSize: 10,
    fontWeight: 600,
    textTransform: "uppercase",
    letterSpacing: "0.8px",
    color: "rgba(255,255,255,0.35)",
    marginBottom: 10,
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap"
  },

  value: {
    fontSize: 28,
    fontWeight: 700,
    fontVariantNumeric: "tabular-nums",
    letterSpacing: "-0.5px",
    color: "rgba(255,255,255,0.95)",
    marginBottom: 6,
    lineHeight: 1.1
  },

  status: {
    fontSize: 10,
    fontWeight: 600,
    textTransform: "uppercase",
    letterSpacing: "0.6px"
  },

  time: {
    fontSize: 10,
    color: "rgba(255,255,255,0.25)",
    marginTop: 8,
    fontVariantNumeric: "tabular-nums"
  }
};