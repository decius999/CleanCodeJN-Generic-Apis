# 💬 CleanCodeJN.GenericApis.Chat

> _A ready-made **AI Chat UI** for Blazor WebAssembly — drop-in `/ai` page with MudBlazor, SSE streaming, MCP tool discovery, and markdown rendering._

## Installation

```bash
dotnet add package CleanCodeJN.GenericApis.Chat
```

Requires the backend package:

```bash
dotnet add package CleanCodeJN.GenericApis
```

---

## Setup

### 1. Blazor WASM host project — `Program.cs`

```csharp
builder.Services.AddCleanCodeJNWithAiChat(options =>
{
    options.BackendUrl   = "https://localhost:7132"; // your API backend
    options.Title        = "My AI Assistant";        // header title
    options.ShowToolCalls = false;                   // show/hide tool call cards
    options.BearerToken  = "...";                    // optional auth token
});
```

### 2. `_Imports.razor`

```razor
@using CleanCodeJN.GenericApis.Chat.Pages
@using CleanCodeJN.GenericApis.Chat.Extensions
```

### 3. Register the page route — `App.razor` / router config

The component `AiChatPage` is shipped inside the RCL. Add a route or redirect to `/ai`:

```razor
@inject NavigationManager Nav
@code {
    protected override void OnInitialized()
    {
        // auto-redirect on start
        if (Nav.Uri == Nav.BaseUri || Nav.Uri == Nav.BaseUri.TrimEnd('/'))
            Nav.NavigateTo("/ai", replace: true);
    }
}
```

### 4. Dark theme (recommended) — `MainLayout.razor`

```razor
@using MudBlazor
<MudThemeProvider IsDarkMode="true" Theme="_theme" />
<MudDialogProvider />
<MudSnackbarProvider />

@code {
    private readonly MudTheme _theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary           = "#4CAF50",
            AppbarBackground  = "#0d1b2a",
            Background        = "#0a1520",
            Surface           = "#112233",
        }
    };
}
```

### 5. `index.html` — remove default loading spinner

Replace the default Blazor loading `<div>` with a plain dark placeholder so there is no flash on startup:

```html
<div id="app" style="background:#0a1520; height:100vh;"></div>
```

---

## Backend requirements

The backend must expose two endpoints (added automatically by `CleanCodeJN.GenericApis`):

| Endpoint | Method | Description |
|---|---|---|
| `/ai/chat` | POST | SSE streaming chat with Anthropic + MCP tool execution |
| `/mcp` | GET | Returns available MCP tools as JSON |

Enable them in your API `Program.cs`:

```csharp
app.UseCleanCodeJNWithMinimalApis(); // REST endpoints → also registers /ai/chat
app.UseCleanCodeJNWithMcp();         // MCP tool discovery → /mcp
```

Backend `appsettings.json`:

```json
{
  "AiProxy": {
    "LlmApiKey": "sk-ant-...",
    "Model":           "claude-opus-4-5",
    "MaxTokens":       8096,
    "SelfBaseUrl":     "https://localhost:7132"
  }
}
```

---

## ChatOptions reference

| Property | Type | Default | Description |
|---|---|---|---|
| `BackendUrl` | `string` | `""` | Base URL of the backend API |
| `Title` | `string` | `"AI Assistant"` | Title shown in the chat header |
| `ShowToolCalls` | `bool` | `false` | Show/hide tool call detail cards in chat |
| `BearerToken` | `string` | `null` | JWT / Bearer token forwarded on every chat request |

---

## Features

- **SSE streaming** — responses appear token by token as they arrive from Claude
- **MCP tool sidebar** — auto-discovers all tools from `/mcp`; click a chip to insert the tool name into the input
- **Collapsible sidebar** — toggle the tools panel via the toolbar button
- **Markdown rendering** — tables, code blocks, headings, and lists are rendered as styled HTML (powered by [Markdig](https://github.com/xoofx/markdig))
- **Splash screen** — shown on first load when no messages exist; explains the assistant and provides example prompts
- **Dark CleanCodeJN theme** — green-on-dark colour scheme matching the CleanCodeJN brand

---

## License

MIT — [github.com/decius999/CleanCodeJN-Generic-Apis](https://github.com/decius999/CleanCodeJN-Generic-Apis)
