using System.Text.RegularExpressions;

namespace VertexLuaExecutor;

public class LuaSyntaxHighlighter
{
    private readonly RichTextBox _editor;
    private ColorPalette _palette;
    private bool _isHighlighting;

    private static readonly string[] Keywords = {
        "and", "break", "do", "else", "elseif", "end", "false", "for",
        "function", "goto", "if", "in", "local", "nil", "not", "or",
        "repeat", "return", "then", "true", "until", "while"
    };

    private static readonly string[] BuiltInFunctions = {
        "print", "pairs", "ipairs", "next", "type", "tostring", "tonumber",
        "setmetatable", "getmetatable", "rawget", "rawset", "rawequal",
        "select", "unpack", "pack", "pcall", "xpcall", "error", "assert",
        "collectgarbage", "dofile", "loadfile", "load", "loadstring",
        "require", "module", "string", "table", "math", "io", "os",
        "coroutine", "debug", "package"
    };

    public LuaSyntaxHighlighter(RichTextBox editor, ColorPalette palette)
    {
        _editor = editor;
        _palette = palette;
    }

    public void UpdatePalette(ColorPalette palette)
    {
        _palette = palette;
    }

    public void HighlightAll()
    {
        if (_isHighlighting) return;
        _isHighlighting = true;

        int selectionStart = _editor.SelectionStart;
        int selectionLength = _editor.SelectionLength;

        _editor.SuspendLayout();

        // Set default color
        _editor.SelectAll();
        _editor.SelectionColor = _palette.TextColor;

        string text = _editor.Text;

        // Highlight strings (single and double quotes)
        HighlightPattern(text, "\"[^\"\\\\]*(\\\\.[^\"\\\\]*)*\"", _palette.SyntaxString);
        HighlightPattern(text, "'[^'\\\\]*(\\\\.[^'\\\\]*)*'", _palette.SyntaxString);
        HighlightPattern(text, @"\[\[[\s\S]*?\]\]", _palette.SyntaxString);

        // Highlight comments
        HighlightPattern(text, @"--\[\[[\s\S]*?\]\]", _palette.SyntaxComment);
        HighlightPattern(text, "--[^\r\n]*", _palette.SyntaxComment);

        // Highlight numbers
        HighlightPattern(text, @"\b\d+\.?\d*\b", _palette.SyntaxNumber);
        HighlightPattern(text, @"\b0x[0-9a-fA-F]+\b", _palette.SyntaxNumber);

        // Highlight keywords
        foreach (string keyword in Keywords)
        {
            HighlightPattern(text, $@"\b{keyword}\b", _palette.SyntaxKeyword);
        }

        // Highlight built-in functions
        foreach (string func in BuiltInFunctions)
        {
            HighlightPattern(text, $@"\b{func}\b", _palette.SyntaxFunction);
        }

        // Highlight operators
        HighlightPattern(text, @"[+\-*/%^#=<>~]", _palette.SyntaxOperator);
        HighlightPattern(text, @"\.\.\.?", _palette.SyntaxOperator);

        _editor.SelectionStart = selectionStart;
        _editor.SelectionLength = selectionLength;

        _editor.ResumeLayout();
        _isHighlighting = false;
    }

    private void HighlightPattern(string text, string pattern, Color color)
    {
        try
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(text))
            {
                _editor.Select(match.Index, match.Length);
                _editor.SelectionColor = color;
            }
        }
        catch
        {
            // Ignore regex errors
        }
    }

    public void HighlightLine(int lineIndex)
    {
        if (_isHighlighting) return;
        if (lineIndex < 0 || lineIndex >= _editor.Lines.Length) return;

        _isHighlighting = true;

        int lineStart = _editor.GetFirstCharIndexFromLine(lineIndex);
        string line = _editor.Lines[lineIndex];
        int selectionStart = _editor.SelectionStart;
        int selectionLength = _editor.SelectionLength;

        _editor.SuspendLayout();

        // Reset line to default color
        _editor.Select(lineStart, line.Length);
        _editor.SelectionColor = _palette.TextColor;

        // Apply highlighting to the line
        HighlightPatternInRange(line, lineStart, "\"[^\"\\\\]*(\\\\.[^\"\\\\]*)*\"", _palette.SyntaxString);
        HighlightPatternInRange(line, lineStart, "'[^'\\\\]*(\\\\.[^'\\\\]*)*'", _palette.SyntaxString);
        HighlightPatternInRange(line, lineStart, "--[^\r\n]*", _palette.SyntaxComment);
        HighlightPatternInRange(line, lineStart, @"\b\d+\.?\d*\b", _palette.SyntaxNumber);
        HighlightPatternInRange(line, lineStart, @"\b0x[0-9a-fA-F]+\b", _palette.SyntaxNumber);

        foreach (string keyword in Keywords)
        {
            HighlightPatternInRange(line, lineStart, $@"\b{keyword}\b", _palette.SyntaxKeyword);
        }

        foreach (string func in BuiltInFunctions)
        {
            HighlightPatternInRange(line, lineStart, $@"\b{func}\b", _palette.SyntaxFunction);
        }

        HighlightPatternInRange(line, lineStart, @"[+\-*/%^#=<>~]", _palette.SyntaxOperator);

        _editor.SelectionStart = selectionStart;
        _editor.SelectionLength = selectionLength;

        _editor.ResumeLayout();
        _isHighlighting = false;
    }

    private void HighlightPatternInRange(string text, int offset, string pattern, Color color)
    {
        try
        {
            var regex = new Regex(pattern, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(text))
            {
                _editor.Select(offset + match.Index, match.Length);
                _editor.SelectionColor = color;
            }
        }
        catch
        {
            // Ignore regex errors
        }
    }
}
