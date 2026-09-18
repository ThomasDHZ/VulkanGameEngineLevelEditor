# Vulkan Game Engine Level Editor

C# WinForms editor that hosts a native C++ engine over a .NET 8 interop layer.

The editor is the tool layer: inspect objects, edit components, view the scene, and run config/asset paths. Rendering and native memory stay in the engine DLL. C# owns orchestration, UI, and data that should not leak across the managed/native boundary.

Related repos:

- [VulkanEngineCore](https://github.com/ThomasDHZ/VulkanEngineCore) — core hybrid .NET 8 + native architecture
- [VulkanGameEngine](https://github.com/ThomasDHZ/VulkanGameEngine) — runtime / engine
- [ListPtr](https://github.com/ThomasDHZ/ListPtr) — dense C# ↔ native transfer
- [MemoryLeakReporterDemo](https://github.com/ThomasDHZ/MemoryLeakReporterDemo) — native leak reporting from managed code

## Screenshots

**Editor shell** — WinForms host: object list, embedded native viewport, component inspector (sprite, transform, collision).

![Editor shell](https://github.com/user-attachments/assets/f01fb2c7-f7ed-456c-acf6-230501243db8)

**Lighting / HDRI** — point lights and packed textures driven from the C# property panel.

![Lighting and HDRI](https://github.com/user-attachments/assets/1b533940-0f0c-461e-a481-b1fc21c60a0d)

## What it does

- Embeds a native render view inside a WinForms shell (`RenderViewForm`)
- Inspects and edits components through typed views (transform, sprite, lights, collision, camera follow, input, debug)
- Registers components, component views, UI controls, lights, and linked objects
- Bridges native object properties into the WinForms property system (`NativePropertyDescriptor`, `DynamicComponentWrapper`)
- Talks to a native material-baker path (packed textures + JSON) via `MaterialBakerDLL`
- Loads engine config from `EngineConfig.json`

## Call path

```text
WinForms editor (this repo)
  → C# wrappers (VulkanEngineCoreCS / VulkanEngineCS)
    → P/Invoke, explicit DLL exports, ListPtr<>
      → native engine + MaterialBakerDLL
```

## Layout

```text
VulkanGameEngineLevelEditor/
  VulkanGameEngineLevelEditor/   # C# WinForms editor
    Component/                   # Component view UIs
    Registries/                  # Component, view, control, light, link registries
    LevelEditor/                 # Native property descriptors + dynamic wrappers
    ControlSubForms/
    Systems/
    RenderViewForm.cs            # Main editor surface
    Program.cs
  MaterialBakerDLL/              # Native baker / texture / material pool
  Assets/
  EngineConfig.json
  VulkanGameEngineLevelEditor.slnx
```

The solution also references the C# engine wrappers and native projects in `VulkanEngineCore` and `VulkanGameEngine` (sibling repos). Build those first, or the editor has nothing to host.

## Stack

| Layer | Tech |
|---|---|
| Editor UI | C#, .NET, WinForms |
| Interop | P/Invoke, explicit DLL boundaries, ownership rules |
| Native host | C++ engine DLL + MaterialBakerDLL |
| Data | JSON config, packed textures |

## Status

Active desktop tool used to drive the hybrid engine. Expect editor and baker paths to move as the interop layer changes. This is not a packaged end-user product.

## Requirements

- Windows, x64
- Visual Studio with C# and C++ workloads
- Sibling repos checked out next to this one (see `.slnx` project paths)
- Native DLLs produced by the engine / baker projects on the loader path
