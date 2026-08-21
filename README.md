# KuroAcad

A comprehensive AutoCAD plugin designed for urban planning applications. Built with C#, KuroAcad provides specialized tools for architects, urban planners, and developers to streamline planning workflows in AutoCAD.

## Overview

KuroAcad is a plugin extension for AutoCAD that offers a suite of commands and utilities tailored for urban planning tasks. It includes tools for geometric calculations, block creation with planning data, polyline operations, and urban planning data management.

## Features

- **Geometric Calculations**: Calculate area, perimeter, and other spatial measurements
- **Urban Planning Blocks**: Create and manage blocks with planning attributes (name, area, density, floors, FAR)
- **Polyline Operations**: Trim roads, find intersections, and analyze polyline geometry
- **Block Management**: Automated block creation and attribute assignment for planning projects
- **Data Analysis**: Extract and analyze urban planning data from CAD drawings
- **Intersection Detection**: Find intersection points between polylines for precise planning analysis

## Requirements

- AutoCAD (compatible version with .NET API support)
- .NET Framework 4.7+ or .NET Core equivalent
- Administrator privileges to install the plugin

## Installation

1. **Clone or Download** the repository:
   ```bash
   git clone https://github.com/KuroMeido/KuroAcad.git
   ```

2. **Build the Project**:
   - Open `KuroAcad.sln` in Visual Studio
   - Build the solution (Release configuration recommended)
   - Output DLL will be generated in the bin/Release folder

3. **Install the Plugin**:
   - Copy the compiled DLL to your AutoCAD plugins folder
   - Or load directly in AutoCAD using `NETLOAD` command

4. **License Activation**:
   - The plugin includes key verification. You'll need a valid activation key
   - Keys are generated using the computer identification system

## Usage

### Command Reference

All commands are accessed directly in AutoCAD using the command line:

#### 1. **KTemDat** - Create Urban Planning Block
Creates a block with urban planning data (area, density, floors, FAR).

```
Command: KTemDat
```

**Steps**:
- Opens a dialog for configuring block parameters
- Select layout mode (simple or detailed)
- Enter planning data:
  - Name (project/block name)
  - Area (in square units)
  - Density (residential/commercial density)
  - Number of Floors
  - FAR (Floor Area Ratio)
- Select polyline in drawing
- Block is created with attribute values populated

**Example**:
```csharp
// Internal usage
CmdCreateBlock cmdBlock = new CmdCreateBlock();
cmdBlock.KuroTemDat();  // Launches the command
```

#### 2. **KGetTD** - Get Terrain Data & Create Table
Extracts polyline data and generates a summary table with coordinates and segment lengths.

```
Command: KGetTD
```

**Steps**:
- Prompts for decimal precision (default: 2)
- Select polylines to analyze
- Specifies table insertion point
- Creates table with vertex coordinates and segment information

#### 3. **KTrimRoad** - Trim Road/Polyline
Trims or processes polyline geometry for road network analysis.

```
Command: KTrimRoad
```

**Steps**:
- Select polylines to trim
- Specify trim boundaries or criteria
- Results are displayed in command line

#### 4. **KuroIntersect** - Find Intersection Points
Detects and displays intersection points between two polylines.

```
Command: KuroIntersect
```

**Output**:
- Number of intersection points found
- Coordinates (X, Y) of each intersection point

#### 5. **KTKLD** - Urban Planning Analysis
Advanced analysis tool for block references and planning data.

```
Command: KTKLD
```

**Steps**:
- Select block references to analyze
- Process planning data
- Generate analysis results

### Example Workflow

**Creating a Planning Block**:

1. Open your AutoCAD drawing with planning area polylines
2. Run command: `KTemDat`
3. Configure block parameters in dialog:
   - Name: "District A"
   - Area: "50000" sq.m
   - Density: "150" persons/hectare
   - Floors: "8"
   - FAR: "2.5"
4. Click OK
5. Select the polyline boundary
6. Block with all planning data is inserted into drawing

**Analyzing Terrain Data**:

1. Have polylines representing terrain contours or boundaries
2. Run command: `KGetTD`
3. Set decimal precision (e.g., 2 decimals)
4. Select polylines
5. Specify table position
6. Table is created showing all coordinates and distances

**Finding Road Intersections**:

1. Have two polylines representing roads
2. Run command: `KuroIntersect`
3. System displays all intersection points with coordinates

## Code Structure

### Main Components

- **Commands/**: AutoCAD command implementations
  - `CmdCreateBlock.cs` - Block creation with planning attributes
  - `CmdGetTD.cs` - Terrain data extraction
  - `CmdTrimRoad.cs` - Road trimming operations
  - `CmdTKLD.cs` - Advanced planning analysis

- **Extensions/**: Utility methods
  - `KuroExtensions.cs` - Helper functions for:
    - Roman numeral conversion
    - Block attribute sorting
    - Point calculations
    - Table operations
    - Layer management

- **Entry/**: Plugin initialization
  - `ExtensionApplication.cs` - Plugin startup, key verification

### Key Classes

**KuroDemo** - Basic geometric calculations
```csharp
KuroDemo demo = new KuroDemo(100, 50);  // Length, Width
demo.TinhToan();                        // Calculate
// Results: KQ_DienTich (Area), KQ_ChuVi (Perimeter)
```

**KuroExtensions** - Utility methods for CAD operations
- `InsertingABlock()` - Insert blocks at specific locations
- `GetCenterPoint()` - Calculate polyline center
- `CopyEntities()` - Duplicate CAD objects
- `GetBlockAttributes()` - Extract block attributes

## Configuration

- **License Key**: Configured in `ExtensionApplication.cs` (Key: "18DBE8E0")
- **Default Settings**: Block radius, precision, table formatting can be customized in command files

## Troubleshooting

**Plugin fails to load**:
- Check license key validity
- Ensure .NET Framework is installed
- Verify AutoCAD compatibility

**Commands not recognized**:
- Reload plugin using `NETLOAD`
- Check if plugin is loaded: `APPLOAD`

**Selection issues**:
- Ensure objects are visible and unlocked
- Select objects on correct layer

## Contributing

Contributions are welcome! Please feel free to:
1. Report issues
2. Suggest enhancements
3. Submit pull requests

## License

[Add your chosen license here]

## Support

For issues, questions, or feature requests, please open an [issue on GitHub](https://github.com/KuroMeido/KuroAcad/issues).

---

**Project Info**:
- Language: C#
- Target Platform: AutoCAD via .NET API
- Created: June 3, 2024
- Repository: [KuroMeido/KuroAcad](https://github.com/KuroMeido/KuroAcad)
