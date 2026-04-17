# Systemübersicht: Eingangslogik mit OPC UA und Hardware

## Beschreibung

Das System besteht aus mehreren Eingangsquellen, die über unterschiedliche Wege in eine zentrale Logik (ODER-Verknüpfung) geführt werden. Das Ergebnis steuert anschließend unseren Smart Home.

## Komponenten

### 1. Digitale Clients (über OPC UA)

Folgende Geräte senden ihre Signale über jeweils einen eigenen OPC UA Server:

- **Tablet (Android)** → OPC UA
- **Laptop (Windows)** → OPC UA
- **VR-System (Android)** → OPC UA

Jeder dieser Clients kommuniziert unabhängig über OPC UA mit der zentralen Logik. Zusätzlich sind werden auch Lan Kabeln angeschlossen.

### 2. Analoge / Hardware-Eingänge

- **Analog / Hardware**
  - Direkte Einspeisung in die zentrale Logik
  - Kein OPC UA erforderlich

## Logik

Alle Eingänge werden in einer zentralen **ODER-Verknüpfung (OR)** zusammengeführt:

- Wenn **mindestens ein Eingang aktiv ist**, wird der Ausgang aktiviert

## Ausgang

- Das Ergebnis der ODER-Verknüpfung steuert:
  - `!Input` (invertiertes Eingangssignal)

## Zusammenfassung

- Mehrere Quellen (Software + Hardware)
- OPC UA für digitale Geräte
- Direkte Verbindung für Hardware
- Zentrale OR-Logik entscheidet über das finale Eingangssignal