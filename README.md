# Asheron Builder

Asheron Builder is a next-generation world builder tool inspired by Asheron's Call, incorporating modern game development practices. It provides a powerful and intuitive interface for creating, editing, and managing complex game environments.

## Table of Contents
- [Features](#features)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
- [Usage](#usage)
- [Core Components](#core-components)
- [Contributing](#contributing)
- [License](#license)

## Features

### Modular Design System
- **Dungeon Creation Kit**: Rapidly create dungeons using preset pieces with auto-snapping and alignment features.
- **Outdoor Environment Kit**: Create diverse outdoor areas with terrain pieces, vegetation clusters, and landmarks.

### Advanced Visualization
- Z-Buffer implementation for proper depth rendering
- Snap-to-Ground functionality for automatic object placement
- Layer system with visibility toggles
- Transparency controls for better visualization

### Asset Management
- Intuitive naming convention and robust tagging system
- Visual asset browser with thumbnail previews
- Advanced search functionality

### Object Manipulation
- Grouping and hierarchical structures for complex objects
- Advanced transform tools for scaling, rotating, and positioning

### Terrain Editing
- Intuitive sculpting tools
- Texture painting with easy blending
- Procedural generation for natural-looking landscapes
- Vegetation painter for rapid placement

### Seamless Transitions
- Tools for creating smooth transitions between indoor and outdoor areas
- Level streaming system for seamless loading

### Custom Asset Creation
- In-tool asset editor for basic modeling and texturing
- Asset variation system
- Import/Export functionality for custom assets

### Technical Features
- Performance optimization with LOD system and occlusion culling
- Built-in version control with branching and merging capabilities
- Multi-user editing and collaboration tools
- Visual scripting system for game logic and events

### User Interface
- Customizable layout
- Context-sensitive UI
- Multiple view modes (top-down, perspective, first-person)
- Real-time rendering in the viewport

## Getting Started

### Prerequisites
- .NET 6.0 SDK or later
- Visual Studio 2022 or later (recommended)
- OpenTK 4.8.2 or later

### Installation
1. Clone the repository:
   ```
   git clone https://github.com/Vermino/asheron-builder.git
   ```
2. Open the `AsheronBuilder.sln` solution file in Visual Studio.
3. Restore NuGet packages.
4. Build the solution.

## Usage
1. Run the `AsheronBuilder.UI` project.
2. Use the menu to create a new dungeon or open an existing one.
3. Utilize the various tools in the interface to build and edit your environment:
    - Use the Dungeon Creation Kit for indoor areas
    - Switch to the Outdoor Environment Kit for exterior landscapes
    - Manage assets through the Asset Browser
    - Edit terrain with the sculpting and painting tools
    - Add and manipulate objects in the 3D viewport
4. Save your work using the File menu.

## Core Components

- **AsheronBuilder.Core**: Contains the core logic, data structures, and asset management.
- **AsheronBuilder.Rendering**: Handles 3D rendering using OpenTK.
- **AsheronBuilder.UI**: Implements the user interface using WPF.
- **AsheronBuilder.Tests**: Contains unit tests for the project.

## Contributing
We welcome contributions to Asheron Builder! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) file for details on our code of conduct and the process for submitting pull requests.

## Disclaimer
**This project is for educational and non-commercial purposes only, use of the game client is for interoperability with the emulated server.**

- Asheron's Call was a registered trademark of Turbine, Inc. and WB Games Inc which has since expired.
- ACEmulator is not associated or affiliated in any way with Turbine, Inc. or WB Games Inc.