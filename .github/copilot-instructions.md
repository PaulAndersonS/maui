# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the .NET MAUI repository. It serves as context for understanding the project structure, development workflow, and best practices.

## Repository Overview

**.NET MAUI** is a cross-platform framework for creating mobile and desktop applications with C# and XAML. This repository contains the core framework code that enables development for Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

### Key Technologies
- **.NET SDK** - Version depends on the branch:
  - **main branch**: Use the latest stable version of .NET to build
  - **net10.0 branch**: Use the latest .NET 10 SDK
  - **etc.**: Each feature branch correlates to its respective .NET version
- **C#** and **XAML** for application development
- **Cake build system** for compilation and packaging
- **MSBuild** with custom build tasks
- **xUnit** for testing

## Development Environment Setup

### Prerequisites

#### Linux Development (Current Environment)
For .NET installation on Linux, follow the official Microsoft documentation:
* https://learn.microsoft.com/en-us/dotnet/core/install/linux

#### Additional Requirements
- **OpenJDK 17** for Android development
- **VS Code** with .NET MAUI Dev Kit extension
- **Android SDK** for Android development

### Initial Repository Setup

1. **Clone and navigate to repository:**
   ```bash
   git clone https://github.com/dotnet/maui.git
   cd maui
   ```

2. **Restore tools and build tasks (REQUIRED before opening IDE):**
   ```bash
   dotnet tool restore
   dotnet build ./Microsoft.Maui.BuildTasks.slnf
   ```

## Project Structure

### Important Directories
- `src/Core/` - Core MAUI framework code
- `src/Controls/` - UI controls and components
- `src/Essentials/` - Platform APIs and essentials
- `src/TestUtils/` - Testing utilities and infrastructure
- `docs/` - Development documentation
- `eng/` - Build engineering and tooling
- `.github/` - GitHub workflows and configuration

### Platform-Specific Code Organization
- **Android** specific code is inside folders labeled `Android`
- **iOS** specific code is inside folders labeled `iOS`
- **MacCatalyst** specific code is inside folders named `MacCatalyst`
- **Windows** specific code is inside folders named `Windows`

### Platform-Specific File Extensions
- Files with `.windows.cs` will only compile for the Windows TFM
- Files with `.android.cs` will only compile for the Android TFM
- Files with `.ios.cs` will only compile for the iOS and MacCatalyst TFM
- Files with `MacCatalyst.cs` will only compile for the MacCatalyst TFM

### Sample Projects
```
├── Controls 
│   ├── samples
│   │   ├── Maui.Controls.Sample
│   │   ├── Maui.Controls.Sample.Sandbox
├── Essentials 
│   ├── samples
│   │   ├── Essentials.Sample
├── BlazorWebView 
│   ├── samples
│   │   ├── BlazorWinFormsApp
│   │   ├── BlazorWpfApp
```

- `src/Controls/samples/Maui.Controls.Sample` - Full gallery sample with all controls and features
- `src/Controls/samples/Maui.Controls.Sample.Sandbox` - Empty project for testing/reproduction
- `src/Essentials/samples/Essentials.Sample` - Essentials API demonstrations (non-UI MAUI APIs)
- `src/BlazorWebView/samples/` - BlazorWebView sample applications

## Development Workflow

### Building

#### Using Cake (Recommended)
```bash
# Build everything
dotnet cake

# Pack NuGet packages
dotnet cake --target=dotnet-pack
```

### Testing and Debugging

#### Testing Guidelines
- Add tests for new functionality
- Ensure existing tests pass:
  - `src/Core/tests/UnitTests/Core.UnitTests.csproj`
  - `src/Essentials/test/UnitTests/Essentials.UnitTests.csproj`
  - `src/Compatibility/Core/tests/Compatibility.UnitTests/Compatibility.Core.UnitTests.csproj`
  - `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj`
  - `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj`

### Local Development with Branch-Specific .NET

For compatibility with specific branches:
```bash
# Use branch-specific .NET version
./dotnet-local.sh build    # Linux/macOS
./dotnet-local.cmd build   # Windows
```

## Platform-Specific Development

### Android
- Requires Android SDK and OpenJDK 17
- Install missing Android SDKs via [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk)
- Android SDK Manager available via: `android` command (after dotnet tool restore)

### iOS (requires macOS)
- Requires current stable Xcode installation from [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
- Pair to Mac required when developing on Windows

### Windows
- Requires Windows SDK

### macOS/Mac Catalyst
- Requires Xcode installation

## Contribution Guidelines

### Files to Never Commit
- **Never** check in changes to `cgmanifest.json` files
- **Never** check in changes to `templatestrings.json` files
- These files are automatically generated and should not be modified manually

### File Reset Guidelines for AI Agents
Since coding agents function as both CI and pair programmers, they need to handle CI-generated files appropriately:

- **Always reset changes to `cgmanifest.json` files** - These are generated during CI builds and should not be committed by coding agents
- **Always reset changes to `templatestrings.json` files** - These localization files are auto-generated and should not be committed by coding agents

### Branching
- `main` - For bug fixes without API changes
- `net10.0` - For new features and API changes

**Note:** The main branch is always pinned to the latest stable release of the .NET SDK, regardless of whether it's a long-term support (LTS) release. Ensure you have that version installed to build the codebase.

### Documentation
- Update XML documentation for public APIs
- Follow existing code documentation patterns
- Update relevant docs in `docs/` folder when needed

### Opening PRs

All PRs are required to have this at the top of the description:

```
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

Always put that at the top, without the block quotes. Without it, the users will NOT be able to try the PR and your work will have been in vain!

## Additional Resources

- [Development Guide](.github/DEVELOPMENT.md)
- [Development Tips](docs/DevelopmentTips.md)
- [Contributing Guidelines](.github/CONTRIBUTING.md)
- [Testing Wiki](https://github.com/dotnet/maui/wiki/Testing)
- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui)

---

**Note for Future Updates:** This document should be expanded as new development patterns, tools, or workflows are discovered. Add sections for specific scenarios, debugging techniques, or tooling as they become relevant to the development process.
