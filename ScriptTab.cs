namespace VertexLuaExecutor;

public class ScriptTab
{
    public string Title { get; set; } = "New Tab";
    public string Content { get; set; } = "";
    public string? FilePath { get; set; }
    public bool IsModified { get; set; }
    public Panel TabPanel { get; set; } = null!;
    public Label TabLabel { get; set; } = null!;
    public Button CloseButton { get; set; } = null!;
    public RichTextBox Editor { get; set; } = null!;

    public string DisplayTitle => IsModified ? Title + " *" : Title;
}
