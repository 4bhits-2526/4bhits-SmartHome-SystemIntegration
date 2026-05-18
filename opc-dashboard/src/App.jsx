import { useEffect, useState } from "react";
import "./App.css";

const NODE_LABELS = {
  "ns=2;i=2": "X20 Input 1",
  "ns=2;i=3": "AI Temperature",
  "ns=2;i=4": "Channel 2",
  "ns=2;i=5": "Analog Pressure",
  "ns=2;i=6": "Motor Speed",
  "ns=2;i=7": "Valve Status"
};

const cleanLabel = (id) => {
  if (!id) return "";
  if (NODE_LABELS[id]) return NODE_LABELS[id];
  if (id.includes("::")) return id.split("::").pop();
  return id;
};

const STORAGE_KEY = "opc-dashboard-settings";

function loadSettings() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch {}
  return {};
}

function saveSettings(settings) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(settings));
}

export default function App() {
  const saved = loadSettings();
  const [sidebarOpen, setSidebarOpen] = useState(saved.sidebarOpen ?? false);
  const [customUrl, setCustomUrl] = useState(saved.customUrl ?? "");
  const [available, setAvailable] = useState([]);
  const [widgets, setWidgets] = useState(saved.widgets ?? []);
  const [expanded, setExpanded] = useState({});
  const [expandedValues, setExpandedValues] = useState({});
  const [showGraph, setShowGraph] = useState({});
  const [showStats, setShowStats] = useState({});
  const [graphType, setGraphType] = useState({});
  const [data, setData] = useState({});
  const [history, setHistory] = useState({});
  const [status, setStatus] = useState("Connecting...");

  useEffect(() => {
    const scheme = window.location.protocol === "https:" ? "wss:" : "ws:";
    const hostname = window.location.hostname || "localhost";
    const backendPorts = [8000, 9001];
    let socket;
    let isCancelled = false;

    const createSocket = (port) => {
      return new Promise((resolve, reject) => {
        let settled = false;
        const url = `${scheme}//${hostname}:${port}/ws`;
        const testSocket = new WebSocket(url);
        const cleanup = () => {
          testSocket.onopen = null;
          testSocket.onerror = null;
          testSocket.onclose = null;
        };

        const timeout = window.setTimeout(() => {
          if (settled) return;
          settled = true;
          cleanup();
          testSocket.close();
          reject(new Error("timeout"));
        }, 2500);

        testSocket.onopen = () => {
          if (settled) return;
          settled = true;
          window.clearTimeout(timeout);
          cleanup();
          resolve(testSocket);
        };

        testSocket.onerror = () => {
          if (settled) return;
          settled = true;
          window.clearTimeout(timeout);
          cleanup();
          testSocket.close();
          reject(new Error("error"));
        };

        testSocket.onclose = () => {
          if (settled) return;
          settled = true;
          window.clearTimeout(timeout);
          cleanup();
          reject(new Error("close"));
        };
      });
    };

    const initWebSocket = async () => {
      for (const port of backendPorts) {
        if (isCancelled) return;
        try {
          const candidate = await createSocket(port);
          if (isCancelled) {
            candidate.close();
            return;
          }

          socket = candidate;
          setStatus(`Connected (${hostname}:${port})`);

          socket.addEventListener("message", (event) => {
            try {
              const parsed = JSON.parse(event.data);
              const channels = parsed.channels || {};
              const onTimes = parsed.on_times || {};

              setAvailable(Object.keys(channels).sort());

              setData((prev) => {
                const next = {};
                Object.entries(channels).forEach(([nodeid, value]) => {
                  const numeric = Number(value);
                  next[nodeid] = {
                    value: Number.isNaN(numeric) ? value : numeric,
                    onTime: Number(onTimes[nodeid] || 0)
                  };
                });
                return next;
              });

              setHistory((prev) => {
                const next = { ...prev };
                Object.entries(channels).forEach(([nodeid, value]) => {
                  const numeric = Number(value);
                  const point = numeric === 1 ? 1 : 0;
                  next[nodeid] = next[nodeid] ? [...next[nodeid], point] : [point];
                  if (next[nodeid].length > 30) {
                    next[nodeid].shift();
                  }
                });
                return next;
              });

              setWidgets((current) => {
                const keys = Object.keys(channels);
                if (current.length > 0) {
                  const filtered = current.filter((key) => keys.includes(key));
                  return filtered.length > 0 ? filtered : keys.slice(0, 3);
                }
                return keys.slice(0, 3);
              });
            } catch (error) {
              console.error("Failed to parse websocket data", error);
            }
          });

          socket.addEventListener("close", () => {
            setStatus("Disconnected");
          });

          socket.addEventListener("error", () => {
            setStatus("Error connecting");
          });

          return;
        } catch (error) {
          console.warn(`WebSocket port ${port} failed:`, error.message);
        }
      }

      setStatus("Disconnected");
    };

    initWebSocket();

    return () => {
      isCancelled = true;
      if (socket) {
        socket.close();
      }
    };
  }, []);

  useEffect(() => {
    saveSettings({ widgets, sidebarOpen, customUrl });
  }, [widgets, sidebarOpen, customUrl]);

  async function connectToUrl(url) {
    try {
      const response = await fetch("http://localhost:8000/mode", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ url: url || "" })
      });
      if (response.ok) {
        setStatus(`Connecting to ${url || "simulator"}...`);
      }
    } catch (error) {
      console.error("Failed to connect:", error);
    }
  }

  function toggleWidget(name) {
    setWidgets((current) =>
      current.includes(name)
        ? current.filter((item) => item !== name)
        : [...current, name]
    );
  }

  function updateSidebar(open) {
    setSidebarOpen(open);
  }

  function onDragStart(e, index) {
    e.dataTransfer.setData("index", index);
  }

  function onDrop(e, index) {
    const draggedIndex = Number(e.dataTransfer.getData("index"));
    const updated = [...widgets];
    const [draggedItem] = updated.splice(draggedIndex, 1);
    updated.splice(index, 0, draggedItem);
    setWidgets(updated);
  }

  return (
    <div className={sidebarOpen ? "app app--sidebar-open" : "app app--sidebar-closed"}>
      <aside className={sidebarOpen ? "app__sidebar" : "app__sidebar app__sidebar--closed"}>
        <button
          type="button"
          className="app__menu-button"
          onClick={() => updateSidebar(!sidebarOpen)}
        >
          {sidebarOpen ? "Sensors" : "☰"}
        </button>

        {sidebarOpen &&
          available.map((name) => {
            const added = widgets.includes(name);
            return (
              <div key={name} className="app__sidebar-item">
                <span>{cleanLabel(name)}</span>
                <button
                  type="button"
                  className={added ? "app__sensor-button app__sensor-button--active" : "app__sensor-button"}
                  onClick={() => toggleWidget(name)}
                >
                  {added ? "−" : "+"}
                </button>
              </div>
            );
          })}
        {sidebarOpen && (
          <div style={{ marginTop: 20, borderTop: "1px solid rgba(255,255,255,0.1)", paddingTop: 16 }}>
            <div style={{ color: "#94a3b8", fontSize: 12, fontWeight: 600, marginBottom: 8 }}>OPC UA URL</div>
            <input
              type="text"
              placeholder="opc.tcp://abc.def.ghi.jkl:mnop"
              value={customUrl}
              onChange={(e) => setCustomUrl(e.target.value)}
              style={{
                width: "100%",
                padding: "10px 12px",
                borderRadius: 8,
                border: "none",
                background: "rgba(30, 41, 59, 0.76)",
                color: "#cbd5e1",
                fontSize: 12,
                outline: "none",
                boxSizing: "border-box"
              }}
            />
            <button
              type="button"
              onClick={() => connectToUrl(customUrl)}
              style={{
                width: "100%",
                marginTop: 8,
                padding: "10px",
                borderRadius: 8,
                border: "none",
                background: "#38bdf8",
                color: "#0f172a",
                fontWeight: 600,
                fontSize: 13,
                cursor: "pointer",
                transition: "background 0.2s"
              }}
            >
              Connect
            </button>
          </div>
        )}
      </aside>

      <main className="app__main">
        <header className="app__header">
          <div className="app__title">OPC Dashboard</div>
          <div className="app__subtitle">{status} — {widgets.length} active widget(s)</div>
        </header>

        <section className="app__grid">
          {widgets.map((name, index) => {
            const d = data[name] || { value: 0, onTime: 0 };
            const on = d.value === 1 || d.value === "1" || d.value === true;
            return (
              <article
                key={name}
                draggable
                onDragStart={(e) => onDragStart(e, index)}
                onDragOver={(e) => e.preventDefault()}
                onDrop={(e) => onDrop(e, index)}
                className={`app__card ${on ? "app__card--online" : "app__card--offline"}`}
              >
                <div className={`app__dot ${on ? "app__dot--online" : "app__dot--offline"}`} />
                <div className="app__node">{cleanLabel(name)}</div>
                <div style={{ position: "relative" }}>
                  <div
                    className="app__value"
                    onClick={() => setExpandedValues((prev) => ({ ...prev, [name]: !prev[name] }))}
                  >
                    {d.value ?? 0}
                  </div>
                  {expandedValues[name] && (
                    <div className="app__value-tooltip">
                      {String(d.value ?? 0)}
                    </div>
                  )}
                </div>
                <div className={`app__state ${on ? "app__state--online" : "app__state--offline"}`}>
                  {on ? "ON" : "OFF"}
                </div>
                <div className="app__time">ON Time: {d.onTime?.toFixed(0) ?? 0} ms</div>

                <div className="hoverMenu app__hover-menu">
                  <button
                    type="button"
                    className="app__detail-button"
                    onClick={() => setExpanded({ ...expanded, [name]: !expanded[name] })}
                  >
                    {expanded[name] ? "Hide Details" : "More Details"}
                  </button>
                </div>

                {expanded[name] && (
                  <div className="app__details-panel">
                    <div className="app__toggle-row">
                      <button
                        type="button"
                        className="app__toggle-button"
                        onClick={() => setShowStats({ ...showStats, [name]: !showStats[name] })}
                      >
                        {showStats[name] ? "− Stats" : "+ Stats"}
                      </button>
                      <button
                        type="button"
                        className="app__toggle-button"
                        onClick={() => setShowGraph({ ...showGraph, [name]: !showGraph[name] })}
                      >
                        {showGraph[name] ? "− Graph" : "+ Graph"}
                      </button>
                    </div>

                    {showStats[name] && (
                      <div className="app__stats-box">
                        <div className="app__stat-row">
                          <span>Node ID</span>
                          <span>{name}</span>
                        </div>
                        <div className="app__stat-row">
                          <span>ON Time</span>
                          <span>{d.onTime?.toFixed(0) ?? 0} ms</span>
                        </div>
                      </div>
                    )}

                    {showGraph[name] && (
                      <div className="app__graph-wrapper">
                        <div className="app__graph-controls">
                          <button
                            type="button"
                            className={`app__graph-switch ${graphType[name] === "bars" ? "app__graph-switch--active" : ""}`}
                            onClick={() => setGraphType({ ...graphType, [name]: "bars" })}
                          >
                            Bars
                          </button>
                          <button
                            type="button"
                            className={`app__graph-switch ${graphType[name] === "line" ? "app__graph-switch--active" : ""}`}
                            onClick={() => setGraphType({ ...graphType, [name]: "line" })}
                          >
                            Line
                          </button>
                        </div>

                        <div className="app__graph-box">
                          {(history[name] || []).map((value, i) =>
                            graphType[name] === "line" ? (
                              <div
                                key={i}
                                className="app__line-point"
                                style={{ height: value ? "70px" : "20px" }}
                              />
                            ) : (
                              <div
                                key={i}
                                className="app__graph-bar"
                                style={{
                                  height: value ? "70px" : "20px",
                                  background: value ? "#38bdf8" : "#475569"
                                }}
                              />
                            )
                          )}
                        </div>

                        <div className="app__time-scale">
                          <span>-30s</span>
                          <span>-20s</span>
                          <span>-10s</span>
                          <span>Now</span>
                        </div>
                      </div>
                    )}
                  </div>
                )}
              </article>
            );
          })}
        </section>
      </main>
    </div>
  );
}
