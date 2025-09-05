# Napominator

**Napominator** is a Windows application designed to periodically remind users about tasks and control access to specific applications based on customizable allow/block lists and time schedules. The project targets .NET 9 and is implemented as a WinForms application.

## Key Features

- **Task Reminders:** Periodically displays notifications to remind users about important tasks or schedules.
- **Application Blocking:** Monitors the active window and can block or warn about the use of forbidden applications, based on user-specific and computer-specific block/allow lists.
- **Time-Based Control:** Enforces allowed and forbidden usage periods, with flexible scheduling for different users.
- **Sound and Noise Monitoring:** Uses a microphone to detect noise levels. If excessive noise is detected, the app can play warning sounds, show messages, take webcam photos, or even lock the workstation.
- **Screenshot Capture:** Captures screenshots on mouse clicks or at scheduled times for monitoring purposes.
- **User Profiles:** Supports different profiles (e.g., "Polina", "Mama", "Papa") with individual settings and restrictions.
- **Logging:** Maintains logs of actions and events for review.

## Usage

1. Configure user and computer-specific settings in the `SETTINGS` directory.
2. Run the application. It will start monitoring according to the configured rules.
3. Use the provided PowerShell script (`ConvertToUtf8WithBom.ps1`) to ensure all `.cs` files are encoded in UTF-8 with BOM for compatibility.

## Typical Scenarios

- Remind a child to take breaks or avoid certain applications during study hours.
- Block access to browsers or games outside of allowed time windows.
- Alert or lock the workstation if the environment becomes too noisy at night.

## Technologies

- .NET 9, C# 13.0, WinForms
- NAudio for audio/microphone access
- EmguCV for webcam integration
- PowerShell for file encoding management

---
