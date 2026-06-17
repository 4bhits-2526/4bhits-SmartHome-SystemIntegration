# Systemübersicht der dargestellten Eingangslogik

## Beschreibung der Abbildung

Die Abbildung zeigt eine vereinfachte Eingangslogik zur Steuerung einer Lampe innerhalb des Smart-Home-Systems.

Als Eingangsquellen sind drei digitale Anwendungen und eine Hardware-Komponente dargestellt:

* Tablet-Anwendung unter Android
* Laptop-Anwendung unter Windows
* VR-Anwendung unter Android
* analoge beziehungsweise physische Hardware

Die drei digitalen Anwendungen sind jeweils über eine OPC-UA-Schnittstelle mit der zentralen Logik verbunden. Die analoge Hardware wird direkt als weiterer Eingang in die Logik geführt.

Alle vier Eingangssignale werden in einem zentralen ODER-Baustein zusammengeführt. Der Ausgang dieses Bausteins ist anschließend mit einer Lampe verbunden.

## Digitale Eingänge

Das Tablet, der Laptop und die VR-Anwendung stellen jeweils eine eigene Bedienmöglichkeit dar.

Jede Anwendung sendet ihr Eingangssignal über OPC UA an die zentrale Steuerung. In der Abbildung wird für jede Anwendung ein eigener OPC-UA-Kommunikationsweg dargestellt.

Die Anwendungen werden dabei als gleichwertige Eingangsquellen behandelt. Es spielt für die ODER-Verknüpfung keine Rolle, über welche Plattform das Signal ausgelöst wurde.

## Hardware-Eingang

Zusätzlich zu den digitalen Anwendungen ist ein analoger beziehungsweise physischer Hardware-Eingang dargestellt.

Dieser Eingang kann beispielsweise von einem realen Schalter oder Taster des Analogmodells stammen. Das Hardware-Signal wird direkt an die zentrale ODER-Logik übergeben.

## ODER-Verknüpfung

Die Eingangssignale aller Plattformen sowie der Hardware werden in einer ODER-Verknüpfung zusammengeführt.

Für diese Logik gilt:

* Ist mindestens ein Eingang aktiv, ist auch der Ausgang der ODER-Verknüpfung aktiv.
* Der Ausgang ist nur dann inaktiv, wenn alle Eingänge inaktiv sind.

Damit kann jede der dargestellten Eingangsquellen grundsätzlich die Lampe beeinflussen.

Die Logik kann vereinfacht folgendermaßen beschrieben werden:

```text
Ausgang = Tablet OR Laptop OR VR OR Hardware
```

## Invertierter Ausgang

Zwischen der ODER-Verknüpfung und der Lampe ist in der Abbildung die Bezeichnung `!Input` eingetragen.

Das Ausrufezeichen kennzeichnet üblicherweise eine logische Negation beziehungsweise Invertierung. Dadurch würde das Ergebnis der ODER-Verknüpfung vor der Weitergabe an die Lampe umgekehrt.

Für den dargestellten Ausgang gilt daher:

```text
Lampe = NOT (Tablet OR Laptop OR VR OR Hardware)
```

Das bedeutet:

* Wenn kein Eingang aktiv ist, wird die Lampe eingeschaltet.
* Wenn mindestens ein Eingang aktiv ist, wird die Lampe ausgeschaltet.

## Beispiel

Sind alle Eingänge inaktiv, ergibt die ODER-Verknüpfung zunächst den Wert `0`.

Durch die Invertierung wird daraus:

```text
NOT 0 = 1
```

Die Lampe wäre damit eingeschaltet.

Wird beispielsweise der Eingang der Tablet-Anwendung aktiviert, ergibt die ODER-Verknüpfung den Wert `1`.

Durch die Invertierung wird daraus:

```text
NOT 1 = 0
```

Die Lampe wäre damit ausgeschaltet.

## Aussage der Abbildung

Die Abbildung zeigt somit ein System, bei dem mehrere digitale und physische Eingangsquellen über eine gemeinsame ODER-Verknüpfung auf eine Lampe wirken.

Die digitalen Anwendungen kommunizieren dabei über OPC UA mit der zentralen Logik. Die physische Hardware wird als zusätzlicher Eingang verarbeitet.

Der Ausgang der ODER-Verknüpfung wird laut Beschriftung invertiert, bevor er an die Lampe weitergegeben wird.

## Einschränkung der dargestellten Lösung

Die Abbildung stellt eine stark vereinfachte Eingangslogik dar. Sie zeigt keine Rückmeldung des tatsächlichen Lampenzustands an die Anwendungen. Dadurch ist aus der Grafik nicht ersichtlich, wie Tablet, Laptop und VR-Anwendung über eine Änderung informiert werden.

Außerdem eignet sich eine reine ODER-Verknüpfung nur bedingt für eine Toggle-Steuerung. Wenn mehrere Eingänge einen dauerhaften Zustand liefern, bleibt der Ausgang so lange aktiv, wie mindestens einer dieser Eingänge aktiv ist.

Für das überarbeitete Smart-Home-System wird deshalb eine andere Logik benötigt, bei der ein kurzer Toggle-Befehl verarbeitet, der Lampenzustand zentral gespeichert und anschließend an alle Anwendungen zurückgemeldet wird.
