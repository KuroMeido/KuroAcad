# KuroAcad

KuroAcad is an AutoCAD plugin for urban planning workflows. Built in C#, it provides commands and utilities for geometric analysis, planning block creation, polyline processing, and data extraction to help architects, urban planners, and developers work more efficiently.

## Overview

KuroAcad extends AutoCAD with tools tailored for urban planning and CAD-based analysis. The plugin focuses on common planning tasks such as measuring geometry, managing planning blocks, extracting terrain data, and finding intersections in drawings.

## Features

- **Geometric calculations** — area, perimeter, and other spatial measurements
- **Urban planning blocks** — create and manage blocks with attributes such as name, area, density, floors, and FAR
- **Polyline operations** — trim roads, find intersections, and analyze polyline geometry
- **Block management** — automate block creation and attribute assignment for planning projects
- **Data analysis** — extract and analyze urban planning data from CAD drawings
- **Intersection detection** — identify intersection points between polylines for precise analysis

## Requirements

- AutoCAD with .NET API support
- .NET Framework 4.7+ or compatible .NET runtime
- Administrator privileges may be required for installation

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/KuroMeido/KuroAcad.git
   ```

2. **Build the project**
   - Open `KuroAcad.sln` in Visual Studio
   - Build the solution, preferably in `Release`
   - The output DLL will be generated in the `bin/Release` folder

3. **Install the plugin**
   - Copy the compiled DLL to your AutoCAD plugins folder
   - Or load it directly in AutoCAD using the `NETLOAD` command

4. **Activate the license**
   - The plugin includes license verification
   - A valid activation key is required
   - Keys are generated using the computer identification system

## Usage

Commands are entered directly in AutoCAD through the command line.

### KTemDat — Create Urban Planning Block

Creates a block with urban planning data such as area, density, floors, and FAR.

**Command:**
```text
KTemDat
```

**Workflow:**
- Open a dialog to configure block parameters
- Choose a layout mode: simple or detailed
- Enter planning data:
  - Name
  - Area
  - Density
  - Number of floors
  - FAR
- Select a polyline in the drawing
- The block is created and populated with attribute values

**Example**
```csharp
CmdCreateBlock cmdBlock = new CmdCreateBlock();
cmdBlock.KuroTemDat();
```

### KGetTD — Get Terrain Data and Create Table

Extracts polyline data and generates a summary table with coordinates and segment lengths.

**Command:**
```text
KGetTD
```

**Workflow:**
- Enter decimal precision, defaulting to 2
- Select polylines to analyze
- Choose the table insertion point
- A table is created with vertex coordinates and segment information

### KTrimRoad — Trim Road / Polyline

Trims or processes polyline geometry for road network analysis.

**Command:**
```text
KTrimRoad
```

**Workflow:**
- Select polylines to trim
- Specify trim boundaries or criteria
- Review the results in the command line

### KuroIntersect — Find Intersection Points

Detects and displays intersection points between two polylines.

**Command:**
```text
KuroIntersect
```

**Output:**
- Number of intersection points found
- X and Y coordinates for each intersection point

### KTKLD — Urban Planning Analysis

Performs advanced analysis for block references and planning data.

**Command:**
```text
KTKLD
```

**Workflow:**
- Select block references to analyze
- Process planning data
- Review generated analysis results

## Example Workflows

### Create a Planning Block

1. Open an AutoCAD drawing containing a planning boundary polyline
2. Run `KTemDat`
3. Enter block parameters such as:
   - Name: `District A`
   - Area: `50000`
   - Density: `150`
   - Floors: `8`
   - FAR: `2.5`
4. Confirm the dialog
5. Select the boundary polyline
6. The planning block is inserted into the drawing

### Analyze Terrain Data

1. Open a drawing with terrain contours or boundary polylines
2. Run `KGetTD`
3. Set the desired decimal precision
4. Select the polylines
5. Choose the table insertion point
6. Review the generated table of coordinates and distances

### Find Road Intersections

1. Select two polylines representing roads
2. Run `KuroIntersect`
3. View the reported intersection points and coordinates

## Code Structure

### Main Components

- **Commands/**
  - `CmdCreateBlock.cs` — planning block creation
  - `CmdGetTD.cs` — terrain data extraction
  - `CmdTrimRoad.cs` — road trimming operations
  - `CmdTKLD.cs` — planning analysis

- **Extensions/**
  - `KuroExtensions.cs` — helper utilities for:
    - Roman numeral conversion
    - Block attribute sorting
    - Point calculations
    - Table operations
    - Layer management

- **Entry/**
  - `ExtensionApplication.cs` — plugin startup and license verification

### Key Classes

**KuroDemo** — Basic geometric calculations
```csharp
KuroDemo demo = new KuroDemo(100, 50);
demo.TinhToan();
// Results: KQ_DienTich (Area), KQ_ChuVi (Perimeter)
```

**KuroExtensions** — Utility methods for CAD operations
- `InsertingABlock()` — insert blocks at specific locations
- `GetCenterPoint()` — calculate polyline center point
- `CopyEntities()` — duplicate CAD objects
- `GetBlockAttributes()` — extract block attributes

## Configuration

- **License key**: configured in `ExtensionApplication.cs` with key `18DBE8E0`
- **Default settings**: block radius, precision, and table formatting can be adjusted in command files

## Troubleshooting

### Plugin fails to load
- Check that the license key is valid
- Ensure the required .NET Framework is installed
- Verify AutoCAD compatibility

### Commands are not recognized
- Reload the plugin using `NETLOAD`
- Check whether the plugin is loaded with `APPLOAD`

### Selection issues
- Ensure objects are visible and unlocked
- Confirm you are selecting entities on the correct layer

## Contributing

Contributions are welcome.

1. Report issues
2. Suggest enhancements
3. Submit pull requests

## License

This project is provided free of charge for personal use.

If you plan to contribute or redistribute it, please keep the original attribution intact.

## Support

For issues, questions, or feature requests, please open an issue on GitHub:
https://github.com/KuroMeido/KuroAcad/issues

---

**Project Info**
- Language: C#
- Target platform: AutoCAD via .NET API
- Created: June 3, 2024
- Repository: https://github.com/KuroMeido/KuroAcad
