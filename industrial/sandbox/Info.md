# Systemübersicht: Eingangslogik mit OPC UA und Hardware

## Beschreibung

Das System besteht aus mehreren Eingangsquellen, die über unterschiedliche Wege in eine zentrale Logik (**ODER-Verknüpfung**) geführt werden.  
Das Ergebnis steuert anschließend Aktoren im Smart Home (z. B. Lampen).

Folgende Eingangsquellen sind integriert:

- Physische Schalter (analog / Hardware)
- Tablet-Anwendung (Android)
- VR-Anwendung (Android)
- Laptop / Windows-Anwendung

Alle Eingaben werden über verschiedene Kommunikationswege (z. B. OPC UA, HTTP/WebRequests oder lokale Schnittstellen) an die zentrale Logik übermittelt.

Ein zentrales Ziel des Systems ist die **plattformübergreifende Synchronisation**:  
Wird ein Zustand über eine Plattform geändert, aktualisieren sich alle anderen Plattformen automatisch.  
Dadurch entsteht ein konsistentes Systemverhalten unabhängig vom Einstiegspunkt.

---

## Komponenten

### 1. Digitale Clients (über OPC UA)

**ZU SEHEN IN ABBILDUNG 1.1**

Folgende Geräte senden ihre Signale an die zentrale Logik:

- **Tablet (Android)** → Kommunikation über HTTP/WebRequests oder OPC UA Gateway  
- **Laptop (Windows)** → OPC UA Client  
- **VR-System (Android)** → Kommunikation über HTTP/WebRequests oder Middleware  

> Hinweis: Ein nativer OPC UA Server auf Android-Geräten ist in der Regel nicht praktikabel.  
> Daher erfolgt die Anbindung über alternative Schnittstellen oder Gateways.

Alle Clients kommunizieren logisch unabhängig, greifen jedoch auf dieselbe zentrale Logik zu.

---



### 2. Netzwerk / Kommunikation

- OPC UA für industrielle / strukturierte Kommunikation  
- HTTP/WebRequests für mobile Anwendungen  
- LAN-Verbindungen zwischen:
  - Zentraler Logik (z. B. SPS/Server)
  - OPC UA Clients
  - Optionalen Gateways

---

## Logik

Alle Eingänge werden in einer zentralen **ODER-Verknüpfung (OR)** zusammengeführt:

- Wenn **mindestens ein Eingang aktiv ist**, wird der Ausgang aktiviert  
- Wenn **kein Eingang aktiv ist**, bleibt der Ausgang deaktiviert  

### Zustandsübersicht

| Eingang A | Eingang B | Eingang C | Eingang D | Ausgang |
|----------|----------|----------|----------|---------|
| 0        | 0        | 0        | 0        | 0       |
| 1        | 0        | 0        | 0        | 1       |
| 0        | 1        | 0        | 0        | 1       |
| 0        | 0        | 1        | 0        | 1       |
| 0        | 0        | 0        | 1        | 1       |
| 1        | 1        | 0        | 0        | 1       |
| 1        | 0        | 1        | 0        | 1       |
| 1        | 0        | 0        | 1        | 1       |
| 0        | 1        | 1        | 0        | 1       |
| 0        | 1        | 0        | 1        | 1       |
| 0        | 0        | 1        | 1        | 1       |
| 1        | 1        | 1        | 0        | 1       |
| 1        | 1        | 0        | 1        | 1       |
| 1        | 0        | 1        | 1        | 1       |
| 0        | 1        | 1        | 1        | 1       |
| 1        | 1        | 1        | 1        | 1       |

---

## Ausgang

Das Ergebnis der ODER-Verknüpfung steuert:

- Smart-Home-Aktoren (z. B. Lampen)

**Wichtig:**  
Das Ausgangssignal ist **nicht invertiert**.  
Eine Invertierung (`!Input`) erfolgt nur, wenn dies explizit gewünscht oder erforderlich ist.

---

## Zusammenfassung

- Mehrere Eingangsquellen (Software + Hardware)
- Zentrale Verarbeitung über OR-Logik
- OPC UA für strukturierte Kommunikation (v. a. Windows / industrielle Systeme)
- Alternative Schnittstellen für mobile Geräte
- Direkte Hardwareanbindung für physische Eingänge
- Synchronisation aller Systeme für konsistentes Verhalten