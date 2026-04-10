# 4bhits-SmartHome-SystemIntegration

## Project Goal
Interactive SmartHome system combining VR, tablet control, physical interfaces and industrial automation concepts.  
Designed to practice cross-system integration and collaborative development via pull requests.

In this project, we develop a Smart Home system consisting of multiple platforms:

- Windows (PC)
- Tablet (mobile control)
- VR (virtual environment)
- Analog (physical model)
- Industrial control (backend / OPC UA)

All systems are connected and share common states (e.g., light on/off).

---

## Project Structure

```text
SmartHomeProject/
├── shared/
├── windows/
├── tablet/
├── vr/
├── analog/
└── industrial/
```

### Platform Structure

Each platform follows this structure:

```text
platform/
├── UnityProject/   → main implementation
└── sandbox/        → experiments and prototypes
```

### Explanation

- **UnityProject/**  
  → Main implementation  
  → Only stable and tested features  

- **sandbox/**  
  → Space for experiments and prototypes  
  → Can include separate small projects (e.g., independent Unity projects)  
  → These do NOT have to be integrated into the main project  
  → Used to explore ideas and test concepts  

---

## Sandbox Rules

- Free experimentation is allowed  
- Independent prototype projects are encouraged  
- Not every experiment needs to be integrated  
- Only validated solutions are moved to the main project  

---

## Workflow

### Core Rule

- Experiments → `sandbox`
- Final features → `UnityProject`

---

## Git Workflow

1. Create a new branch  
   Examples:
   - `feature/vr-switch`
   - `experiment/tablet-ui`

2. Implement changes  

3. Commit with meaningful messages  

4. Create a Pull Request  

---

## Issues

All tasks are managed using **GitHub Issues**.

Each Issue includes:
- description
- platform
- goal
- task list
- acceptance criteria  

👉 One Issue = one clear task  

---

## Shared Data Model

All platforms must use the same variables.

Defined in:
shared/

Example:
- LivingRoom.Light (bool)
- Kitchen.Light (bool)

👉 Important:  
All teams must use identical names!

---

## System Goal

A state change should propagate through the entire system.

Example:
1. A physical switch is pressed  
2. The industrial system updates the state  
3. Windows, Tablet, and VR reflect the change  
4. The LED in the physical model reacts  

---

## Definition of Done

A task is completed when:

- functionality is implemented  
- tested  
- no errors exist  
- commit is created  
- pull request is submitted  

---

## Rules

- No experiments in the main project  
- Follow the folder structure  
- Use clear commit messages  
- Complete Issues fully  
- Always test your changes  

---

## Final Goal

Build a system where:

👉 all platforms communicate with each other  
👉 physical and digital systems are connected  
👉 state changes flow through the entire system  
