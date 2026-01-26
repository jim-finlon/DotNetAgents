# Visual Workflow Designer UI Guide

**Version:** 1.0  
**Date:** January 2026  
**Status:** Complete & Polished

## Overview

The Visual Workflow Designer UI is a beautiful, modern Blazor WebAssembly application that provides an intuitive drag-and-drop interface for creating and managing DotNetAgents workflows.

## Features

- ✅ **Modern Design**: Beautiful gradient backgrounds, smooth animations, and professional styling
- ✅ **Drag-and-Drop Interface**: Intuitive visual node placement
- ✅ **Node Types**: Function, Condition, Parallel, Human-in-Loop
- ✅ **Node Properties**: Rich property editor with helpful hints
- ✅ **Workflow Management**: Save, load, validate workflows
- ✅ **Execution Visualization**: Real-time execution status with color-coded nodes
- ✅ **Export/Import**: JSON workflow definitions
- ✅ **Responsive Design**: Works on desktop and tablet devices

## Design Highlights

### Modern UI Elements

- **Gradient Backgrounds**: Beautiful purple gradient theme
- **Smooth Animations**: Hover effects, transitions, and pulse animations
- **Professional Typography**: Inter font family for clean, modern text
- **Card-Based Layout**: Clean card designs with shadows and hover effects
- **Color-Coded States**: Visual feedback for node states (executing, completed, failed)
- **Custom Scrollbars**: Styled scrollbars for better aesthetics

### User Experience

- **Intuitive Navigation**: Clear sidebar navigation
- **Helpful Tooltips**: Contextual help throughout
- **Visual Feedback**: Immediate response to user actions
- **Empty States**: Friendly messages when no workflows exist
- **Loading States**: Smooth loading animations

## Architecture

### Components

1. **WorkflowDesigner.razor** - Main designer page
   - Canvas for workflow design
   - Node palette with drag-and-drop
   - Properties panel
   - Toolbar with workflow actions

2. **WorkflowNodeComponent.razor** - Individual node component
   - Visual representation with icons
   - Drag and drop functionality
   - Selection handling
   - Execution state visualization

3. **NodePropertiesPanel.razor** - Node property editor
   - Type-specific configuration
   - Helpful hints and placeholders
   - Real-time updates

4. **WorkflowDesignerServiceClient** - API client
   - REST API integration
   - Workflow CRUD operations
   - Error handling

## Usage

### Starting the Application

```bash
cd src/DotNetAgents.Workflow.Designer.Web
dotnet run
```

Navigate to `https://localhost:5001` (or configured port).

### Creating a Workflow

1. Click "New Workflow" on the home page
2. Drag nodes from the palette onto the canvas
3. Connect nodes by dragging from output to input ports
4. Click nodes to select and edit properties
5. Configure node properties in the properties panel
6. Save the workflow

### Node Types

- **⚙️ Function**: Executes a C# function or method
- **❓ Condition**: Branches based on state condition
- **⚡ Parallel**: Executes multiple branches in parallel
- **👤 Human-in-Loop**: Pauses for human approval

### Workflow Operations

- **💾 Save**: Persist workflow to backend
- **✓ Validate**: Check workflow for errors
- **▶ Execute**: Run workflow with initial state
- **📤 Export**: Download workflow as JSON
- **🗑 Clear**: Remove all nodes from canvas

## Visual Design

### Color Scheme

- **Primary**: Indigo (#6366f1) - Main actions and highlights
- **Success**: Green (#10b981) - Completed states
- **Warning**: Amber (#f59e0b) - Executing states
- **Danger**: Red (#ef4444) - Errors and failures
- **Info**: Cyan (#06b6d4) - Information and exports

### Typography

- **Font**: Inter (Google Fonts)
- **Headings**: 600-700 weight
- **Body**: 400-500 weight
- **Small Text**: 11-13px for labels and hints

### Spacing & Layout

- **Consistent Padding**: 12px, 16px, 20px, 24px, 32px
- **Border Radius**: 8px standard, 12px for large elements
- **Shadows**: Multiple levels for depth
- **Grid System**: Responsive flexbox layout

## Customization

### Styling

Edit `wwwroot/app.css` to customize:
- Color scheme (CSS variables in `:root`)
- Node appearance
- Canvas styling
- Layout and spacing
- Animations

### Adding Node Types

1. Add node type to palette in `WorkflowDesigner.razor`
2. Update `GetNodeIcon()` in `WorkflowNodeComponent.razor`
3. Add type-specific properties in `NodePropertiesPanel.razor`
4. Update CSS for node styling

## Deployment

### Development

```bash
dotnet run
```

### Production

```bash
dotnet publish -c Release
```

Deploy the `bin/Release/net10.0/publish/wwwroot` directory to a web server.

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/DotNetAgents.Workflow.Designer.Web/DotNetAgents.Workflow.Designer.Web.csproj", "src/DotNetAgents.Workflow.Designer.Web/"]
RUN dotnet restore "src/DotNetAgents.Workflow.Designer.Web/DotNetAgents.Workflow.Designer.Web.csproj"
COPY . .
RUN dotnet build "src/DotNetAgents.Workflow.Designer.Web/DotNetAgents.Workflow.Designer.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/DotNetAgents.Workflow.Designer.Web/DotNetAgents.Workflow.Designer.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DotNetAgents.Workflow.Designer.Web.dll"]
```

## Best Practices

1. **Save Frequently**: Save your work regularly
2. **Validate Before Execute**: Always validate workflows before running
3. **Use Descriptive Names**: Name nodes clearly for better organization
4. **Organize Layout**: Arrange nodes logically for readability
5. **Test Incrementally**: Test workflows as you build them

## Related Documentation

- [Visual Workflow Designer Guide](./VISUAL_WORKFLOW_DESIGNER.md)
- [Workflow API Reference](./API_REFERENCE.md)
- [Workflow Examples](../examples/)

---

**Last Updated:** January 2026
