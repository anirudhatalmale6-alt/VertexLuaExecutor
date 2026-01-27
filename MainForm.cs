using System.Drawing.Drawing2D;

namespace VertexLuaExecutor;

public partial class MainForm : Form
{
    private ColorPalette _currentPalette;
    private readonly List<ScriptTab> _tabs = new();
    private ScriptTab? _activeTab;
    private int _tabCounter = 1;

    // UI Components
    private Panel _titleBar = null!;
    private Label _logoLabel = null!;
    private Panel _windowControls = null!;
    private Button _minimizeBtn = null!;
    private Button _maximizeBtn = null!;
    private Button _closeBtn = null!;

    private Panel _tabBar = null!;
    private Panel _tabContainer = null!;
    private Button _addTabBtn = null!;

    private Panel _editorContainer = null!;
    private Panel _lineNumberPanel = null!;
    private RichTextBox _lineNumbers = null!;

    private Panel _toolbar = null!;
    private Button _executeBtn = null!;
    private Button _clearBtn = null!;
    private Button _openBtn = null!;
    private Button _saveBtn = null!;
    private Button _attachBtn = null!;
    private Button _killBtn = null!;
    private Button _paletteBtn = null!;

    private ContextMenuStrip _paletteMenu = null!;

    private LuaSyntaxHighlighter? _highlighter;
    private bool _isDragging;
    private Point _dragStart;

    public MainForm()
    {
        _currentPalette = ColorPalette.DefaultDark;
        InitializeComponent();
        CreateNewTab();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        // Form settings
        Text = "Vertex Lua Executor";
        Size = new Size(900, 600);
        MinimumSize = new Size(600, 400);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;

        // Title Bar
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 35
        };
        _titleBar.MouseDown += TitleBar_MouseDown;
        _titleBar.MouseMove += TitleBar_MouseMove;
        _titleBar.MouseUp += TitleBar_MouseUp;
        _titleBar.DoubleClick += TitleBar_DoubleClick;

        // Logo
        _logoLabel = new Label
        {
            Text = "V",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 122, 204), // Blue V
            Location = new Point(10, 3),
            AutoSize = true
        };
        _logoLabel.MouseDown += TitleBar_MouseDown;
        _logoLabel.MouseMove += TitleBar_MouseMove;
        _logoLabel.MouseUp += TitleBar_MouseUp;
        _titleBar.Controls.Add(_logoLabel);

        // Window Controls
        _windowControls = new Panel
        {
            Dock = DockStyle.Right,
            Width = 140
        };

        _minimizeBtn = CreateWindowButton("─", 0);
        _minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;

        _maximizeBtn = CreateWindowButton("□", 46);
        _maximizeBtn.Click += (s, e) =>
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        };

        _closeBtn = CreateWindowButton("✕", 92);
        _closeBtn.Click += (s, e) => Close();

        _windowControls.Controls.AddRange(new Control[] { _minimizeBtn, _maximizeBtn, _closeBtn });
        _titleBar.Controls.Add(_windowControls);

        // Tab Bar
        _tabBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32
        };

        _tabContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5, 5, 0, 0)
        };

        _addTabBtn = new Button
        {
            Text = "+",
            Size = new Size(28, 24),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12),
            Cursor = Cursors.Hand
        };
        _addTabBtn.FlatAppearance.BorderSize = 0;
        _addTabBtn.Click += (s, e) => CreateNewTab();

        _tabBar.Controls.Add(_tabContainer);

        // Editor Container
        _editorContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 5, 10, 5)
        };

        // Line Numbers Panel
        _lineNumberPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 45
        };

        _lineNumbers = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            Font = new Font("Consolas", 11),
            Cursor = Cursors.Arrow,
            Text = "1"
        };
        _lineNumbers.Enter += (s, e) => _activeTab?.Editor.Focus();
        _lineNumberPanel.Controls.Add(_lineNumbers);

        _editorContainer.Controls.Add(_lineNumberPanel);

        // Toolbar
        _toolbar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            Padding = new Padding(10, 8, 10, 8)
        };

        var toolbarFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        _executeBtn = CreateToolbarButton("Execute");
        _clearBtn = CreateToolbarButton("Clear");
        _openBtn = CreateToolbarButton("Open");
        _saveBtn = CreateToolbarButton("Save");
        _attachBtn = CreateToolbarButton("Attach");
        _killBtn = CreateToolbarButton("Kill");
        _paletteBtn = CreateToolbarButton("Palette");

        _executeBtn.Click += ExecuteBtn_Click;
        _clearBtn.Click += ClearBtn_Click;
        _openBtn.Click += OpenBtn_Click;
        _saveBtn.Click += SaveBtn_Click;
        _attachBtn.Click += AttachBtn_Click;
        _killBtn.Click += KillBtn_Click;
        _paletteBtn.Click += PaletteBtn_Click;

        toolbarFlow.Controls.AddRange(new Control[]
        {
            _executeBtn, _clearBtn, _openBtn, _saveBtn, _attachBtn, _killBtn, _paletteBtn
        });
        _toolbar.Controls.Add(toolbarFlow);

        // Palette Context Menu
        _paletteMenu = new ContextMenuStrip();
        foreach (var palette in ColorPalette.GetAllPalettes())
        {
            var item = new ToolStripMenuItem(palette.Name);
            item.Click += (s, e) => ApplyPalette(palette);
            _paletteMenu.Items.Add(item);
        }

        // Add controls to form
        Controls.Add(_editorContainer);
        Controls.Add(_toolbar);
        Controls.Add(_tabBar);
        Controls.Add(_titleBar);

        ApplyPalette(_currentPalette);

        ResumeLayout(false);
    }

    private Button CreateWindowButton(string text, int x)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(46, 35),
            Location = new Point(x, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private Button CreateToolbarButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(75, 28),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 0)
        };
        btn.FlatAppearance.BorderSize = 1;
        return btn;
    }

    private void CreateNewTab()
    {
        var tab = new ScriptTab
        {
            Title = $"New Tab {_tabCounter++}"
        };

        // Create tab header panel
        tab.TabPanel = new Panel
        {
            Size = new Size(140, 24),
            Margin = new Padding(0, 0, 2, 0),
            Cursor = Cursors.Hand
        };

        tab.TabLabel = new Label
        {
            Text = tab.Title,
            Location = new Point(8, 4),
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        tab.TabLabel.Click += (s, e) => SelectTab(tab);
        tab.TabPanel.Click += (s, e) => SelectTab(tab);

        tab.CloseButton = new Button
        {
            Text = "×",
            Size = new Size(20, 20),
            Location = new Point(116, 2),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        tab.CloseButton.FlatAppearance.BorderSize = 0;
        tab.CloseButton.Click += (s, e) => CloseTab(tab);

        tab.TabPanel.Controls.Add(tab.TabLabel);
        tab.TabPanel.Controls.Add(tab.CloseButton);

        // Create editor
        tab.Editor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 11),
            AcceptsTab = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both
        };
        tab.Editor.TextChanged += Editor_TextChanged;
        tab.Editor.VScroll += Editor_VScroll;
        tab.Editor.KeyDown += Editor_KeyDown;

        _tabs.Add(tab);
        RefreshTabBar();
        SelectTab(tab);
    }

    private void SelectTab(ScriptTab tab)
    {
        if (_activeTab == tab) return;

        // Hide current editor
        if (_activeTab != null)
        {
            _editorContainer.Controls.Remove(_activeTab.Editor);
        }

        _activeTab = tab;

        // Show new editor
        _editorContainer.Controls.Add(tab.Editor);
        tab.Editor.BringToFront();

        // Update highlighter
        _highlighter = new LuaSyntaxHighlighter(tab.Editor, _currentPalette);
        _highlighter.HighlightAll();

        UpdateLineNumbers();
        RefreshTabBar();
        tab.Editor.Focus();
    }

    private void CloseTab(ScriptTab tab)
    {
        if (_tabs.Count == 1)
        {
            // Don't close the last tab, just clear it
            tab.Editor.Clear();
            tab.Title = "New Tab 1";
            tab.FilePath = null;
            tab.IsModified = false;
            RefreshTabBar();
            return;
        }

        int index = _tabs.IndexOf(tab);
        _tabs.Remove(tab);
        _tabContainer.Controls.Remove(tab.TabPanel);

        if (_activeTab == tab)
        {
            SelectTab(_tabs[Math.Min(index, _tabs.Count - 1)]);
        }

        RefreshTabBar();
    }

    private void RefreshTabBar()
    {
        _tabContainer.Controls.Clear();

        int x = 5;
        foreach (var tab in _tabs)
        {
            tab.TabLabel.Text = tab.DisplayTitle;
            tab.TabPanel.Location = new Point(x, 4);
            tab.TabPanel.BackColor = tab == _activeTab ? _currentPalette.TabActive : _currentPalette.TabInactive;
            tab.TabLabel.ForeColor = _currentPalette.TextColor;
            tab.CloseButton.ForeColor = _currentPalette.TextSecondary;
            tab.CloseButton.BackColor = tab.TabPanel.BackColor;
            _tabContainer.Controls.Add(tab.TabPanel);
            x += tab.TabPanel.Width + 2;
        }

        _addTabBtn.Location = new Point(x, 4);
        _addTabBtn.BackColor = _currentPalette.BackgroundMedium;
        _addTabBtn.ForeColor = _currentPalette.TextColor;
        _tabContainer.Controls.Add(_addTabBtn);
    }

    private void UpdateLineNumbers()
    {
        if (_activeTab == null) return;

        int lineCount = _activeTab.Editor.Lines.Length;
        if (lineCount == 0) lineCount = 1;

        string numbers = string.Join("\n", Enumerable.Range(1, lineCount));
        if (_lineNumbers.Text != numbers)
        {
            _lineNumbers.Text = numbers;
        }
    }

    private void ApplyPalette(ColorPalette palette)
    {
        _currentPalette = palette;

        // Form
        BackColor = _currentPalette.BackgroundDark;

        // Title bar
        _titleBar.BackColor = _currentPalette.BackgroundDark;
        _logoLabel.ForeColor = Color.FromArgb(0, 122, 204); // Keep logo blue always

        // Window controls
        foreach (Control ctrl in _windowControls.Controls)
        {
            if (ctrl is Button btn)
            {
                btn.BackColor = _currentPalette.BackgroundDark;
                btn.ForeColor = _currentPalette.TextColor;
                btn.FlatAppearance.MouseOverBackColor = _currentPalette.ButtonHover;
            }
        }

        // Tab bar
        _tabBar.BackColor = _currentPalette.BackgroundMedium;
        _tabContainer.BackColor = _currentPalette.BackgroundMedium;

        // Editor
        _editorContainer.BackColor = _currentPalette.BackgroundLight;
        _lineNumberPanel.BackColor = _currentPalette.BackgroundMedium;
        _lineNumbers.BackColor = _currentPalette.BackgroundMedium;
        _lineNumbers.ForeColor = _currentPalette.TextSecondary;

        foreach (var tab in _tabs)
        {
            tab.Editor.BackColor = _currentPalette.BackgroundLight;
            tab.Editor.ForeColor = _currentPalette.TextColor;
        }

        // Toolbar
        _toolbar.BackColor = _currentPalette.BackgroundDark;
        ApplyToolbarButtonStyle(_executeBtn);
        ApplyToolbarButtonStyle(_clearBtn);
        ApplyToolbarButtonStyle(_openBtn);
        ApplyToolbarButtonStyle(_saveBtn);
        ApplyToolbarButtonStyle(_attachBtn);
        ApplyToolbarButtonStyle(_killBtn);
        ApplyToolbarButtonStyle(_paletteBtn);

        // Refresh tabs and syntax highlighting
        RefreshTabBar();

        if (_activeTab != null)
        {
            _highlighter?.UpdatePalette(palette);
            _highlighter?.HighlightAll();
        }
    }

    private void ApplyToolbarButtonStyle(Button btn)
    {
        btn.BackColor = _currentPalette.ButtonBackground;
        btn.ForeColor = _currentPalette.TextColor;
        btn.FlatAppearance.BorderColor = _currentPalette.BorderColor;
        btn.FlatAppearance.MouseOverBackColor = _currentPalette.ButtonHover;
    }

    #region Title Bar Dragging

    private void TitleBar_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStart = e.Location;
        }
    }

    private void TitleBar_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            Point current = PointToScreen(e.Location);
            Location = new Point(current.X - _dragStart.X - _titleBar.Left,
                                current.Y - _dragStart.Y - _titleBar.Top);
        }
    }

    private void TitleBar_MouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    private void TitleBar_DoubleClick(object? sender, EventArgs e)
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
    }

    #endregion

    #region Editor Events

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;

        _activeTab.IsModified = true;
        UpdateLineNumbers();
        RefreshTabBar();

        // Highlight current line for performance
        int lineIndex = _activeTab.Editor.GetLineFromCharIndex(_activeTab.Editor.SelectionStart);
        _highlighter?.HighlightLine(lineIndex);
    }

    private void Editor_VScroll(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;

        // Sync line numbers scroll with editor
        int firstVisibleChar = _activeTab.Editor.GetCharIndexFromPosition(new Point(0, 0));
        int firstVisibleLine = _activeTab.Editor.GetLineFromCharIndex(firstVisibleChar);
        int lineHeight = _activeTab.Editor.Font.Height;

        _lineNumbers.SelectionStart = 0;
        _lineNumbers.ScrollToCaret();

        // Approximate scroll sync
        if (firstVisibleLine > 0)
        {
            int charIndex = _lineNumbers.GetFirstCharIndexFromLine(Math.Min(firstVisibleLine, _lineNumbers.Lines.Length - 1));
            if (charIndex >= 0)
            {
                _lineNumbers.SelectionStart = charIndex;
                _lineNumbers.ScrollToCaret();
            }
        }
    }

    private void Editor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_activeTab == null) return;

        // Handle Tab key
        if (e.KeyCode == Keys.Tab)
        {
            e.SuppressKeyPress = true;
            int selStart = _activeTab.Editor.SelectionStart;
            _activeTab.Editor.SelectedText = "    "; // 4 spaces
            _activeTab.Editor.SelectionStart = selStart + 4;
        }

        // Ctrl+S to save
        if (e.Control && e.KeyCode == Keys.S)
        {
            e.SuppressKeyPress = true;
            SaveBtn_Click(sender, e);
        }

        // Ctrl+O to open
        if (e.Control && e.KeyCode == Keys.O)
        {
            e.SuppressKeyPress = true;
            OpenBtn_Click(sender, e);
        }

        // Ctrl+N for new tab
        if (e.Control && e.KeyCode == Keys.N)
        {
            e.SuppressKeyPress = true;
            CreateNewTab();
        }

        // Ctrl+W to close tab
        if (e.Control && e.KeyCode == Keys.W)
        {
            e.SuppressKeyPress = true;
            if (_activeTab != null)
            {
                CloseTab(_activeTab);
            }
        }
    }

    #endregion

    #region Toolbar Events

    private void ExecuteBtn_Click(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;

        // Placeholder - Execute would connect to Lua runtime
        MessageBox.Show("Execute: Script execution would happen here.\n\nTo integrate with your Lua runtime, modify the ExecuteBtn_Click method.",
            "Execute", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ClearBtn_Click(object? sender, EventArgs e)
    {
        if (_activeTab != null)
        {
            _activeTab.Editor.Clear();
            _activeTab.IsModified = true;
            RefreshTabBar();
        }
    }

    private void OpenBtn_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*",
            Title = "Open Lua Script"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                string content = File.ReadAllText(dialog.FileName);

                if (_activeTab != null && string.IsNullOrEmpty(_activeTab.Editor.Text) && !_activeTab.IsModified)
                {
                    // Use current empty tab
                    _activeTab.Editor.Text = content;
                    _activeTab.FilePath = dialog.FileName;
                    _activeTab.Title = Path.GetFileName(dialog.FileName);
                    _activeTab.IsModified = false;
                }
                else
                {
                    // Create new tab
                    CreateNewTab();
                    _activeTab!.Editor.Text = content;
                    _activeTab.FilePath = dialog.FileName;
                    _activeTab.Title = Path.GetFileName(dialog.FileName);
                    _activeTab.IsModified = false;
                }

                _highlighter?.HighlightAll();
                RefreshTabBar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SaveBtn_Click(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;

        string? filePath = _activeTab.FilePath;

        if (string.IsNullOrEmpty(filePath))
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*",
                Title = "Save Lua Script",
                DefaultExt = "lua"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
            }
            else
            {
                return;
            }
        }

        try
        {
            File.WriteAllText(filePath, _activeTab.Editor.Text);
            _activeTab.FilePath = filePath;
            _activeTab.Title = Path.GetFileName(filePath);
            _activeTab.IsModified = false;
            RefreshTabBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AttachBtn_Click(object? sender, EventArgs e)
    {
        // Placeholder - Attach would connect to a process
        MessageBox.Show("Attach: Process attachment would happen here.\n\nTo integrate with your injection system, modify the AttachBtn_Click method.",
            "Attach", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void KillBtn_Click(object? sender, EventArgs e)
    {
        // Placeholder - Kill would terminate execution
        MessageBox.Show("Kill: Script termination would happen here.\n\nTo integrate with your Lua runtime, modify the KillBtn_Click method.",
            "Kill", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void PaletteBtn_Click(object? sender, EventArgs e)
    {
        _paletteMenu.Show(_paletteBtn, new Point(0, -_paletteMenu.Height));
    }

    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Draw border
        using var pen = new Pen(_currentPalette.BorderColor, 1);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}
