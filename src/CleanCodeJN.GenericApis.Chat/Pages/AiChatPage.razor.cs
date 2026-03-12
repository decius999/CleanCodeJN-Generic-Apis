using CleanCodeJN.GenericApis.Chat.Models;
using Markdig;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace CleanCodeJN.GenericApis.Chat.Pages;

/// <summary>Code-behind for the AI chat page component, handling state management, streaming responses, and user interaction logic.</summary>
public partial class AiChatPage
{
    private List<McpTool> _tools = [];
    private List<ChatEntry> _entries = [];
    private List<ChatMessage> _history = [];
    private string _input = string.Empty;
    private bool _isStreaming;
    private bool _focusInput;
    private bool _drawerOpen = true;
    private MudPaper _chatContainer;
    private MudTextField<string> _inputField;

    protected override async Task OnInitializedAsync()
    {
        _tools = await ChatService.GetToolsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender || _focusInput)
        {
            _focusInput = false;
            if (_inputField is not null)
                await _inputField.FocusAsync();
        }
    }

    private static readonly MarkdownPipeline _markdownPipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    private static string RenderMarkdown(string text) =>
        string.IsNullOrEmpty(text) ? string.Empty : Markdown.ToHtml(text, _markdownPipeline);

    private void InsertToolName(string toolName)
    {
        _input = string.IsNullOrWhiteSpace(_input)
            ? $"Use {toolName} "
            : _input.TrimEnd() + $" Use {toolName} ";
        _focusInput = true;
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey && !_isStreaming)
            await SendMessage();
    }

    private async Task SendMessage()
    {
        var text = _input.Trim();
        if (string.IsNullOrWhiteSpace(text) || _isStreaming) return;

        _input = string.Empty;
        _isStreaming = true;

        _entries.Add(new ChatEntry { Role = "user", Text = text });
        _history.Add(new ChatMessage("user", text));

        // Initial assistant entry (captures text before any tool calls, or shows loading spinner)
        var assistantEntry = new ChatEntry { Role = "assistant", Text = string.Empty, IsStreaming = true };
        _entries.Add(assistantEntry);
        StateHasChanged();

        await foreach (var ev in ChatService.SendAsync(_history))
        {
            switch (ev.Type)
            {
                case "text":
                    assistantEntry.Text += ev.Content ?? string.Empty;
                    break;
                case "tool_call":
                    // Stop spinner on current entry, add tool chip
                    assistantEntry.IsStreaming = false;
                    _entries.Add(new ChatEntry { Role = "tool_call", ToolName = ev.ToolName });
                    break;
                case "tool_result":
                    // Add result chip, then create a NEW assistant entry for Claude's upcoming text response
                    _entries.Add(new ChatEntry { Role = "tool_result", ToolName = ev.ToolName });
                    assistantEntry = new ChatEntry { Role = "assistant", Text = string.Empty, IsStreaming = true };
                    _entries.Add(assistantEntry);
                    break;
                case "error":
                    assistantEntry.Text = $"Error: {ev.Content}";
                    assistantEntry.IsStreaming = false;
                    break;
            }
            StateHasChanged();
        }

        assistantEntry.IsStreaming = false;
        if (!string.IsNullOrWhiteSpace(assistantEntry.Text))
            _history.Add(new ChatMessage("assistant", assistantEntry.Text));

        _isStreaming = false;
        _focusInput = true;
        StateHasChanged();
    }
}
