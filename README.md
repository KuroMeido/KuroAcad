# KuroAcad

KuroAcad is an AutoCAD plugin for urban planning workflows. Built in C#, it provides commands and utilities for geometric analysis, planning block creation, polyline processing, and data extraction.

---

## Table of Contents

- [Quick Start](#quick-start)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Usage](#usage)
- [Example Workflows](#example-workflows)
- [Code Structure](#code-structure)
- [Configuration](#configuration)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License & Support](#license--support)

---

## Quick Start

```bash
# Clone the repository
git clone https://github.com/KuroMeido/KuroAcad.git

# Build the project
# 1. Open KuroAcad.sln in Visual Studio
# 2. Build in Release mode
# 3. Copy bin/Release/KuroAcad.dll to your AutoCAD plugins folder
# 4. Load in AutoCAD with NETLOAD command
```

---

## Features

- **Geometric Calculations** — area, perimeter, and spatial measurements
- **Urban Planning Blocks** — create and manage blocks with name, area, density, floors, and FAR attributes
- **Polyline Operations** — trim roads, find intersections, analyze geometry
- **Block Management** — automate block creation and attribute assignment
- **Data Analysis** — extract and analyze urban planning data from CAD drawings
- **Intersection Detection** — identify intersection points between polylines for precise analysis

---

## Requirements

| Requirement | Version |
|---|---|
| AutoCAD | With .NET API support |
| .NET Framework | 4.7+ or compatible runtime |
| Privileges | Administrator (for installation) |

---

## Installation

### Step 1: Clone the Repository
```bash
git clone https://github.com/KuroMeido/KuroAcad.git
```

### Step 2: Build the Project
1. Open `KuroAcad.sln` in Visual Studio
2. Build the solution in `Release` mode
3. Output DLL will be in `bin/Release/`

### Step 3: Install the Plugin
- **Option A:** Copy DLL to AutoCAD plugins folder
- **Option B:** Use `NETLOAD` command in AutoCAD to load directly

---

## Usage

All commands are entered directly in AutoCAD through the command line.

### KTemDat — Create Urban Planning Block

Creates a block with urban planning attributes.

| Aspect | Details |
|---|---|
| **Command** | `KTemDat` |
| **Workflow** | 1. Configure block parameters via dialog<br>2. Choose simple or detailed layout<br>3. Enter: Name, Area, Density, Floors, FAR<br>4. Select polyline boundary<br>5. Block is created with attributes |

**Code Example:**
```csharp
CmdTemDat cmdBlock = new CmdTemDat();
cmdBlock.KuroTemDat();
```

---

### KGetTD — Get Terrain Data and Create Table

Extracts polyline data and generates a summary table with coordinates and segment lengths.

| Aspect | Details |
|---|---|
| **Command** | `KGetTD` |
| **Workflow** | 1. Enter decimal precision (default: 2)<br>2. Select polylines to analyze<br>3. Choose table insertion point<br>4. Table is created with vertex coordinates and segment info |

---

### KTrimRoad — Trim Road / Polyline

Trims or processes polyline geometry for road network analysis.

| Aspect | Details |
|---|---|
| **Command** | `KTrimRoad` |
| **Workflow** | 1. Select polylines to trim<br>2. Specify trim boundaries<br>3. Review results in command line |

---

### KuroIntersect — Find Intersection Points

Detects and displays intersection points between two polylines.

| Aspect | Details |
|---|---|
| **Command** | `KIntersection` |
| **Output** | • Number of intersection points<br>• X and Y coordinates for each point |

---

### KTKLD — Urban Planning Analysis

Performs advanced analysis for block references and planning data.

| Aspect | Details |
|---|---|
| **Command** | `KTKLD` |
| **Workflow** | 1. Select block references to analyze<br>2. Process planning data<br>3. Review generated analysis results |

---

## Example Workflows

### Create a Planning Block

1. Open an AutoCAD drawing with a planning boundary polyline
2. Run `KTemDat`
3. Enter parameters:
   - **Name:** `District A`
   - **Area:** `50000`
   - **Density:** `150`
   - **Floors:** `8`
   - **FAR:** `2.5`
4. Confirm dialog
5. Select boundary polyline
6. Planning block is inserted

### Analyze Terrain Data

1. Open drawing with terrain contours or boundary polylines
2. Run `KGetTD`
3. Set decimal precision
4. Select polylines
5. Choose table insertion point
6. Review generated coordinate and distance table

### Find Road Intersections

1. Select two polylines (roads)
2. Run `KIntersection`
3. View intersection points and coordinates

---

## Code Structure

### Main Components

```
KuroAcad/
├── KuroAcad_Commands/
│   ├── Commands/
│   │   ├── CmdTemDat/            — Tem dat command entry point
│   │   ├── CmdTD/                — Terrain data commands
│   │   ├── CmdTrimRoad/          — Road trimming command
│   │   ├── CmdTKLD/              — TKLD analysis command
│   │   ├── CmdTKSDD/             — TKSDD analysis command
│   │   ├── CmdIntersection/      — Intersection command
│   │   ├── CmdRoad/              — Road creation command
│   │   ├── CmdMakeRoad/          — Intersection marking / fillet commands
│   │   └── CmdPalette/           — Palette and WPF UI commands
│   ├── Lib/
│   │   ├── Helper/               — Shared helper utilities
│   │   ├── System/               — Ribbon and command infrastructure
│   │   ├── Utils/                — Main command implementations
│   │   └── WPFStyles/            — Shared WPF styles
│   └── ...
└── README.md
```

### Key Areas

#### **Lib/Helper** — Shared CAD Utilities
Helpers used across commands, such as:
- `LayerHelper`
- `BlockInsertHelper`
- `RomanNumeralHelper`

#### **Lib/System** — Ribbon and UI Infrastructure
Ribbon setup and command definitions, including:
- `KuroRibbon`
- `KuroRibbonButtons`

#### **Lib/Utils** — Main Command Logic
Command implementations and workflows, such as:
- `TKSDDUtil`
- `GetTDUtil`
- `SetTDUtil`
- `TrimRoadUtil`
- `TemDatUtil`
- `IntersectionUtils`

#### **Lib/WPFStyles** — Shared WPF Resources
Style resources used by the palette and UI:
- `ButtonStyle.xaml`
- `CheckBoxStyle.xaml`
- `ListBoxItemStyle.xaml`

---

## Configuration

| Setting | Location | Details |
|---|---|---|
| **License Key** | `ExtensionApplication.cs` | Key: `18DBE8E0` |
| **Default Settings** | Command files | Adjust block radius, precision, table formatting |

---

## Troubleshooting

### Plugin Fails to Load
- ✓ Verify license key is valid
- ✓ Ensure .NET Framework 4.7+ is installed
- ✓ Confirm AutoCAD compatibility

### Commands Are Not Recognized
- ✓ Reload plugin with `NETLOAD`
- ✓ Check if plugin is loaded: `APPLOAD`

### Selection Issues
- ✓ Ensure objects are visible and unlocked
- ✓ Confirm selection is on correct layer

---

## Contributing

We welcome contributions!

1. **Report issues** on [GitHub Issues](https://github.com/KuroMeido/KuroAcad/issues)
2. **Suggest enhancements** with detailed descriptions
3. **Submit pull requests** with clear commit messages

---

## License & Support

### License
This project is provided free of charge for personal use. If you plan to contribute or redistribute, please keep the original attribution intact.

### Support
For issues, questions, or feature requests:
- 📧 Open an issue: [GitHub Issues](https://github.com/KuroMeido/KuroAcad/issues)
- 💬 Discussion in the repository

---

## Project Info

| Field | Value |
|---|---|
| **Language** | C# |
| **Platform** | AutoCAD via .NET API |
| **Created** | June 3, 2024 |
| **Repository** | https://github.com/KuroMeido/KuroAcad |
