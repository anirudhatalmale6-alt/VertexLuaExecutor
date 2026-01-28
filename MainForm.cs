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
    private Panel _logoPanel = null!;
    private Button _settingsBtn = null!;
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
    private Panel _toolbarInner = null!;
    private Button _executeBtn = null!;
    private Button _clearBtn = null!;
    private Button _openBtn = null!;
    private Button _saveBtn = null!;
    private Button _attachBtn = null!;
    private Button _killBtn = null!;

    private ContextMenuStrip _paletteMenu = null!;
    private ContextMenuStrip _settingsMenu = null!;

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
            Height = 40
        };
        _titleBar.MouseDown += TitleBar_MouseDown;
        _titleBar.MouseMove += TitleBar_MouseMove;
        _titleBar.MouseUp += TitleBar_MouseUp;
        _titleBar.DoubleClick += TitleBar_DoubleClick;

        // Logo Panel (draws the stylized V)
        _logoPanel = new Panel
        {
            Size = new Size(40, 40),
            Location = new Point(5, 0)
        };
        _logoPanel.Paint += LogoPanel_Paint;
        _logoPanel.MouseDown += TitleBar_MouseDown;
        _logoPanel.MouseMove += TitleBar_MouseMove;
        _logoPanel.MouseUp += TitleBar_MouseUp;
        _titleBar.Controls.Add(_logoPanel);

        // Window Controls Panel
        _windowControls = new Panel
        {
            Dock = DockStyle.Right,
            Width = 180
        };

        // Settings button (gear icon)
        _settingsBtn = new Button
        {
            Text = "⚙",
            Size = new Size(40, 40),
            Location = new Point(0, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 14),
            Cursor = Cursors.Hand
        };
        _settingsBtn.FlatAppearance.BorderSize = 0;
        _settingsBtn.Click += SettingsBtn_Click;

        _minimizeBtn = CreateWindowButton("─", 46);
        _minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;

        _maximizeBtn = CreateWindowButton("□", 92);
        _maximizeBtn.Click += (s, e) =>
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        };

        _closeBtn = CreateWindowButton("✕", 138);
        _closeBtn.Click += (s, e) => Close();

        _windowControls.Controls.AddRange(new Control[] { _settingsBtn, _minimizeBtn, _maximizeBtn, _closeBtn });
        _titleBar.Controls.Add(_windowControls);

        // Tab Bar
        _tabBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36
        };

        _tabContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 6, 0, 0)
        };

        _addTabBtn = new Button
        {
            Text = "+",
            Size = new Size(24, 24),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11),
            Cursor = Cursors.Hand
        };
        _addTabBtn.FlatAppearance.BorderSize = 0;
        _addTabBtn.Click += (s, e) => CreateNewTab();

        _tabBar.Controls.Add(_tabContainer);

        // Editor Container
        _editorContainer = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0)
        };

        // Line Numbers Panel
        _lineNumberPanel = new Panel
        {
            Dock = DockStyle.Left,
            Width = 50
        };

        _lineNumbers = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            Font = new Font("Consolas", 11),
            Cursor = Cursors.Arrow,
            Text = "1",
            RightToLeft = RightToLeft.Yes
        };
        _lineNumbers.Enter += (s, e) => _activeTab?.Editor.Focus();
        _lineNumberPanel.Controls.Add(_lineNumbers);

        _editorContainer.Controls.Add(_lineNumberPanel);

        // Toolbar
        _toolbar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(0)
        };

        _toolbarInner = new Panel
        {
            Height = 36,
            Anchor = AnchorStyles.None
        };

        _executeBtn = CreateToolbarButton("▷ Execute", 0);
        _clearBtn = CreateToolbarButton("🗑 Clear", 1);
        _openBtn = CreateToolbarButton("📂 Open", 2);
        _saveBtn = CreateToolbarButton("💾 Save", 3);
        _attachBtn = CreateToolbarButton("📡 Attach", 4);
        _killBtn = CreateToolbarButton("⊗ Kill", 5);

        _executeBtn.Click += ExecuteBtn_Click;
        _clearBtn.Click += ClearBtn_Click;
        _openBtn.Click += OpenBtn_Click;
        _saveBtn.Click += SaveBtn_Click;
        _attachBtn.Click += AttachBtn_Click;
        _killBtn.Click += KillBtn_Click;

        _toolbarInner.Controls.AddRange(new Control[]
        {
            _executeBtn, _clearBtn, _openBtn, _saveBtn, _attachBtn, _killBtn
        });

        _toolbar.Controls.Add(_toolbarInner);
        _toolbar.Resize += (s, e) => CenterToolbar();

        // Settings Context Menu (includes palette options)
        _settingsMenu = new ContextMenuStrip();
        var paletteSubmenu = new ToolStripMenuItem("🎨 Color Palette");
        foreach (var palette in ColorPalette.GetAllPalettes())
        {
            var item = new ToolStripMenuItem(palette.Name);
            item.Click += (s, e) => ApplyPalette(palette);
            paletteSubmenu.DropDownItems.Add(item);
        }
        _settingsMenu.Items.Add(paletteSubmenu);
        _settingsMenu.Items.Add(new ToolStripSeparator());
        _settingsMenu.Items.Add(new ToolStripMenuItem("ℹ About", null, (s, e) =>
            MessageBox.Show("Vertex Lua Executor\nVersion 1.0", "About", MessageBoxButtons.OK, MessageBoxIcon.Information)));

        // Add controls to form
        Controls.Add(_editorContainer);
        Controls.Add(_toolbar);
        Controls.Add(_tabBar);
        Controls.Add(_titleBar);

        ApplyPalette(_currentPalette);

        ResumeLayout(false);
    }

    private void CenterToolbar()
    {
        if (_toolbarInner == null || _toolbar == null) return;

        int totalWidth = 6 * 90 + 5 * 8; // 6 buttons * width + 5 gaps
        _toolbarInner.Width = totalWidth;
        _toolbarInner.Location = new Point((_toolbar.Width - totalWidth) / 2, (_toolbar.Height - 36) / 2);
    }

    private void LogoPanel_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Draw stylized "V" with two lines - blue color
        using var pen = new Pen(Color.FromArgb(0, 122, 204), 3);

        // Left line of V
        e.Graphics.DrawLine(pen, 8, 8, 20, 32);
        // Right line of V
        e.Graphics.DrawLine(pen, 32, 8, 20, 32);
    }

    private Button CreateWindowButton(string text, int x)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(40, 40),
            Location = new Point(x, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private Button CreateToolbarButton(string text, int index)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(90, 32),
            Location = new Point(index * 98, 2),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter
        };
        btn.FlatAppearance.BorderSize = 0;
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
            Size = new Size(130, 26),
            Margin = new Padding(0, 0, 2, 0),
            Cursor = Cursors.Hand
        };
        tab.TabPanel.Paint += (s, e) => PaintTabPanel(e, tab);

        // Colored circle indicator
        var indicator = new Panel
        {
            Size = new Size(10, 10),
            Location = new Point(10, 8),
            BackColor = Color.FromArgb(255, 140, 50) // Orange indicator like reference
        };
        MakeCircular(indicator);
        tab.TabPanel.Controls.Add(indicator);

        tab.TabLabel = new Label
        {
            Text = tab.Title,
            Location = new Point(26, 5),
            Size = new Size(75, 18),
            Font = new Font("Segoe UI", 9),
            Cursor = Cursors.Hand
        };
        tab.TabLabel.Click += (s, e) => SelectTab(tab);
        tab.TabPanel.Click += (s, e) => SelectTab(tab);

        tab.CloseButton = new Button
        {
            Text = "×",
            Size = new Size(20, 20),
            Location = new Point(106, 3),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
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

    private void MakeCircular(Panel panel)
    {
        var path = new GraphicsPath();
        path.AddEllipse(0, 0, panel.Width, panel.Height);
        panel.Region = new Region(path);
    }

    private void PaintTabPanel(PaintEventArgs e, ScriptTab tab)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, tab.TabPanel.Width - 1, tab.TabPanel.Height - 1);
        var bgColor = tab == _activeTab ? _currentPalette.TabActive : _currentPalette.TabInactive;

        using var brush = new SolidBrush(bgColor);
        using var path = CreateRoundedRectangle(rect, 6);
        g.FillPath(brush, path);
    }

    private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        return path;
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

        int x = 10;
        foreach (var tab in _tabs)
        {
            tab.TabLabel.Text = tab.DisplayTitle;
            tab.TabPanel.Location = new Point(x, 5);
            tab.TabPanel.BackColor = Color.Transparent;
            tab.TabLabel.ForeColor = _currentPalette.TextColor;
            tab.CloseButton.ForeColor = _currentPalette.TextSecondary;
            tab.CloseButton.BackColor = Color.Transparent;
            tab.TabPanel.Invalidate();
            _tabContainer.Controls.Add(tab.TabPanel);
            x += tab.TabPanel.Width + 4;
        }

        _addTabBtn.Location = new Point(x + 5, 6);
        _addTabBtn.BackColor = Color.Transparent;
        _addTabBtn.ForeColor = _currentPalette.TextSecondary;
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

        // Form - use pure black for background
        BackColor = Color.FromArgb(15, 15, 15);

        // Title bar
        _titleBar.BackColor = Color.FromArgb(15, 15, 15);
        _logoPanel.Invalidate();

        // Settings button
        _settingsBtn.BackColor = Color.FromArgb(15, 15, 15);
        _settingsBtn.ForeColor = _currentPalette.TextSecondary;
        _settingsBtn.FlatAppearance.MouseOverBackColor = _currentPalette.ButtonHover;

        // Window controls
        foreach (Control ctrl in _windowControls.Controls)
        {
            if (ctrl is Button btn && ctrl != _settingsBtn)
            {
                btn.BackColor = Color.FromArgb(15, 15, 15);
                btn.ForeColor = _currentPalette.TextSecondary;
                btn.FlatAppearance.MouseOverBackColor = _currentPalette.ButtonHover;
            }
        }

        // Tab bar - slightly lighter
        _tabBar.BackColor = Color.FromArgb(25, 25, 25);
        _tabContainer.BackColor = Color.FromArgb(25, 25, 25);

        // Editor - darker
        _editorContainer.BackColor = Color.FromArgb(20, 20, 20);
        _lineNumberPanel.BackColor = Color.FromArgb(20, 20, 20);
        _lineNumbers.BackColor = Color.FromArgb(20, 20, 20);
        _lineNumbers.ForeColor = Color.FromArgb(100, 100, 100); // Dimmer line numbers

        foreach (var tab in _tabs)
        {
            tab.Editor.BackColor = Color.FromArgb(20, 20, 20);
            tab.Editor.ForeColor = _currentPalette.TextColor;
        }

        // Toolbar - same as form background
        _toolbar.BackColor = Color.FromArgb(25, 25, 25);
        _toolbarInner.BackColor = Color.FromArgb(25, 25, 25);

        ApplyToolbarButtonStyle(_executeBtn);
        ApplyToolbarButtonStyle(_clearBtn);
        ApplyToolbarButtonStyle(_openBtn);
        ApplyToolbarButtonStyle(_saveBtn);
        ApplyToolbarButtonStyle(_attachBtn);
        ApplyToolbarButtonStyle(_killBtn);

        // Refresh tabs and syntax highlighting
        RefreshTabBar();

        if (_activeTab != null)
        {
            _highlighter?.UpdatePalette(palette);
            _highlighter?.HighlightAll();
        }

        Invalidate();
    }

    private void ApplyToolbarButtonStyle(Button btn)
    {
        btn.BackColor = Color.FromArgb(45, 45, 45);
        btn.ForeColor = _currentPalette.TextColor;
        btn.FlatAppearance.BorderColor = Color.FromArgb(45, 45, 45);
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 65, 65);
        btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(75, 75, 75);

        // Round the buttons
        var path = new GraphicsPath();
        var rect = new Rectangle(0, 0, btn.Width, btn.Height);
        int radius = 8;
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        btn.Region = new Region(path);
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

    private void SettingsBtn_Click(object? sender, EventArgs e)
    {
        _settingsMenu.Show(_settingsBtn, new Point(0, _settingsBtn.Height));
    }

    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Draw subtle border
        using var pen = new Pen(Color.FromArgb(40, 40, 40), 1);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        CenterToolbar();
        Invalidate();
    }
}
