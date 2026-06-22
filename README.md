# PowerToys Command Palette: Change Audio Device Extension

A native Windows PowerToys Command Palette extension that allows users to quickly view, select, and switch default audio input (microphones) and output (speakers, headphones) devices directly from the Command Palette interface.

---

## 🚀 Features

- **Quick Audio Switching**: Change the default Windows input or output audio device with a single keystroke/click.
- **Console, Multimedia & Communications Routing**: Programmatically switches devices for all roles (Console, Multimedia, and Communications) to ensure apps like Discord, Teams, and games update correctly.
- **Dynamic Device List**: Queries active audio endpoints dynamically using low-level Windows Core Audio APIs (WASAPI).
- **Active Device Highlighting**: Displays the currently active/default device with an `(Active)` indicator.
- **Multi-Architecture Support**: Supports building and publishing for both `x64` and `ARM64` architectures.
- **Zero-Dependency Asset Generator**: Includes a helper script to scale and generate all required package icons and splash screens.
- **Automated Bundle Packaging**: Compilation and packaging script to produce single-file multi-architecture `.msixbundle` installers.

---

## 🛠️ Technology Stack

- **Target Framework**: `.NET 10.0` (with Target OS: `net10.0-windows10.0.26100.0`)
- **UI Platform**: PowerToys Command Palette Extension Toolkit (`Microsoft.CommandPalette.Extensions`)
- **Interoperability**: Windows Runtime (`CsWinRT`) and COM server activation (`Shmuelie.WinRTServer`)
- **Low-Level APIs**: Windows Multimedia Device APIs (WASAPI: `IMMDeviceEnumerator`, `IMMDevice`, `IPolicyConfig`)
- **Package Format**: Single-Project MSIX Package / MSIX Bundle

---

## 📁 Repository Structure

```
ChangeAudioDeviceCommandPalleteExtension/
├── ChangeAudioDeviceCommandPalleteExtension.sln  # Visual Studio Solution file
├── Directory.Build.props                         # Shared build configurations (platforms, analyzers)
├── Directory.Packages.props                      # Centralized NuGet package version management
├── GenerateAssets.ps1                             # Script to generate asset resources (logos, splash screens)
├── buildForPublish.ps1                           # Build and bundle script for Release
└── ChangeAudioDeviceCommandPalleteExtension/
    ├── Assets/                                   # Application assets and tile icons
    ├── Pages/
    │   ├── AudioDeviceListPage.cs                # Command Palette list page showing input/output devices
    │   └── SetAudioDeviceCommand.cs              # Action executed when selecting a device
    ├── AudioManager.cs                           # Interop layer querying WASAPI COM interfaces
    ├── CoreAudioInterop.cs                       # COM definition interface declarations (IMMDevice, ERole, etc.)
    ├── ChangeAudioDeviceCommandPalleteExtension.cs # COM extension entry point implementing IExtension
    ├── ChangeAudioDeviceCommandPalleteExtensionCommandsProvider.cs # Registers commands to Command Palette
    ├── Package.appxmanifest                      # AppX package registration manifest (COM server & AppExtension)
    ├── Program.cs                                # Main entry point starting WinRTServer
    └── app.manifest                              # Application identity manifest
```

---

## 💻 How It Works

1. **AppExtension Integration**: The extension registers as a `com.microsoft.commandpalette` app extension in `Package.appxmanifest`. PowerToys Command Palette automatically discovers it.
2. **COM Server Activation**: When the user opens the Command Palette, the host launches `ChangeAudioDeviceCommandPalleteExtension.exe` with arguments `-RegisterProcessAsComServer` and instantiates the COM class via its Guid (`e82d5dfa-d102-421a-a77b-12cdfb71b64c`).
3. **Core Audio Query**: Using `IMMDeviceEnumerator`, the helper class `AudioManager` enumerates active render (speakers) and capture (microphones) endpoints.
4. **Device Selection**: When selected, the private `IPolicyConfig.SetDefaultEndpoint` COM interface is invoked to register the selected device ID as the system default.
5. **UI Refresh**: The list page is refreshed to update the `(Active)` indicator and a confirmation toast is displayed.

---

## ⚙️ Development & Build Instructions

### Prerequisites
- [Windows 10 or 11](https://www.microsoft.com/windows)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Windows SDK](https://developer.microsoft.com/windows/downloads/windows-sdk/) (needed for `makeappx.exe` to build the MSIX bundle)

### Build for Debugging
Open the solution file `ChangeAudioDeviceCommandPalleteExtension.sln` in Visual Studio 2022 or build using CLI:
```powershell
dotnet build
```

### Packaging for Release
Run the automated build and publish script. This script builds both `x64` and `ARM64` packages in Release mode and compiles them into a single, publishable `.msixbundle` package:
```powershell
.\buildForPublish.ps1
```
The output bundle will be located at:
`ChangeAudioDeviceCommandPalleteExtension/ChangeAudioDeviceCommandPalleteExtension_1.0.0.0_Bundle.msixbundle`

### Asset Generation
If you want to customize the extension's icon, place your master image at a target path and update/run `GenerateAssets.ps1`:
```powershell
.\GenerateAssets.ps1
```

---

## 📜 License

This project is licensed under the MIT License - see the `LICENSE` file for details.
