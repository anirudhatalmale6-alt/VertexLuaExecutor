# Vertex Lua Executor

Interfaz de escritorio para ejecutar scripts Lua con un diseño moderno y oscuro.

## Características

- **Tema oscuro** moderno similar a la referencia
- **Logo "V"** de Vertex en color azul
- **Sistema de pestañas** para múltiples scripts
- **Editor de código** con resaltado de sintaxis Lua
- **Números de línea** sincronizados con el scroll
- **Barra de herramientas** con botones: Execute, Clear, Open, Save, Attach, Kill, Palette
- **5 paletas de colores** personalizables:
  - Default Dark
  - Blue Purple
  - Matrix Green
  - Crimson Red
  - Ocean Blue

## Atajos de teclado

- `Ctrl+N` - Nueva pestaña
- `Ctrl+O` - Abrir archivo
- `Ctrl+S` - Guardar archivo
- `Ctrl+W` - Cerrar pestaña actual
- `Tab` - Insertar 4 espacios

## Requisitos

- Visual Studio 2022
- .NET 6.0 SDK o superior
- Windows 10/11

## Cómo abrir y compilar

1. Abrir `VertexLuaExecutor.sln` en Visual Studio 2022
2. Restaurar paquetes NuGet si es necesario
3. Compilar con `F5` o `Ctrl+Shift+B`

## Personalización

### Integrar con tu runtime Lua

Los métodos placeholder están en `MainForm.cs`:

- `ExecuteBtn_Click` - Ejecutar el script actual
- `AttachBtn_Click` - Conectar a un proceso
- `KillBtn_Click` - Terminar la ejecución

### Agregar nuevas paletas de colores

Añadir nuevas paletas en `ColorPalette.cs` siguiendo el patrón existente y agregarlas al método `GetAllPalettes()`.

## Estructura del proyecto

```
VertexLuaExecutor/
├── VertexLuaExecutor.sln      # Solución de Visual Studio
├── VertexLuaExecutor.csproj   # Proyecto WinForms .NET 6
├── Program.cs                 # Punto de entrada
├── MainForm.cs                # Formulario principal con toda la UI
├── ColorPalette.cs            # Sistema de paletas de colores
├── ScriptTab.cs               # Modelo para las pestañas
├── LuaSyntaxHighlighter.cs    # Resaltado de sintaxis Lua
└── README.md                  # Este archivo
```
