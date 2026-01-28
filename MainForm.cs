using System.Drawing.Drawing2D;

namespace VertexLuaExecutor;

public partial class MainForm : Form
{
    private readonly List<ScriptTab> _tabs = new();
    private ScriptTab? _activeTab;
    private int _tabCounter = 1;

    // Colors matching reference exactly
    private readonly Color _bgColor = Color.FromArgb(20, 20, 20);
    private readonly Color _editorBg = Color.FromArgb(30, 30, 30);
    private readonly Color _tabBg = Color.FromArgb(40, 40, 40);
    private readonly Color _textColor = Color.FromArgb(220, 220, 220);
    private readonly Color _textDim = Color.FromArgb(100, 100, 100);
    private readonly Color _accentOrange = Color.FromArgb(255, 100, 50);
    private readonly Color _logoBlue = Color.FromArgb(0, 120, 215);

    // Syntax colors from reference
    private readonly Color _syntaxKeyword = Color.FromArgb(240, 80, 80);  // Red
    private readonly Color _syntaxString = Color.FromArgb(206, 145, 120); // Orange/brown
    private readonly Color _syntaxComment = Color.FromArgb(106, 153, 85); // Green
    private readonly Color _syntaxNumber = Color.FromArgb(220, 220, 220); // White

    // UI Components
    private Panel _titleBar = null!;
    private Panel _logoPanel = null!;
    private Button _settingsBtn = null!;
    private Button _minimizeBtn = null!;
    private Button _maximizeBtn = null!;
    private Button _closeBtn = null!;

    private Panel _tabBar = null!;
    private FlowLayoutPanel _tabContainer = null!;
    private Label _addTabBtn = null!;

    private Panel _editorPanel = null!;
    private RichTextBox _lineNumbers = null!;
    private RichTextBox? _currentEditor;

    private Panel _toolbarPanel = null!;

    private LuaSyntaxHighlighter? _highlighter;
    private bool _isDragging;
    private Point _dragStart;

    public MainForm()
    {
        InitializeComponent();
        CreateNewTab();
    }

    private void InitializeComponent()
    {
        SuspendLayout();

        Text = "Vertex";
        Size = new Size(950, 650);
        MinimumSize = new Size(700, 500);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = _bgColor;
        DoubleBuffered = true;

        // ===== TITLE BAR =====
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = _bgColor
        };
        _titleBar.MouseDown += TitleBar_MouseDown;
        _titleBar.MouseMove += TitleBar_MouseMove;
        _titleBar.MouseUp += TitleBar_MouseUp;
        _titleBar.DoubleClick += (s, e) => ToggleMaximize();

        // Logo (V shape) - BLUE instead of red like reference
        _logoPanel = new Panel
        {
            Size = new Size(35, 35),
            Location = new Point(12, 5),
            BackColor = Color.Transparent
        };
        _logoPanel.Paint += LogoPanel_Paint;
        _titleBar.Controls.Add(_logoPanel);

        // Window buttons (right side)
        _closeBtn = CreateTitleButton("✕", 0);
        _closeBtn.Click += (s, e) => Close();
        _closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 50);

        _maximizeBtn = CreateTitleButton("□", 45);
        _maximizeBtn.Click += (s, e) => ToggleMaximize();

        _minimizeBtn = CreateTitleButton("─", 90);
        _minimizeBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;

        _settingsBtn = CreateTitleButton("⚙", 135);

        var btnPanel = new Panel
        {
            Size = new Size(180, 45),
            Dock = DockStyle.Right,
            BackColor = Color.Transparent
        };
        btnPanel.Controls.AddRange(new Control[] { _closeBtn, _maximizeBtn, _minimizeBtn, _settingsBtn });
        _titleBar.Controls.Add(btnPanel);

        // ===== TAB BAR =====
        _tabBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 38,
            BackColor = _bgColor,
            Padding = new Padding(10, 8, 10, 0)
        };

        _tabContainer = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Location = new Point(10, 6)
        };
        _tabBar.Controls.Add(_tabContainer);

        // Add tab button (+)
        _addTabBtn = new Label
        {
            Text = "+",
            Font = new Font("Segoe UI", 14),
            ForeColor = _textDim,
            Size = new Size(24, 24),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        _addTabBtn.Click += (s, e) => CreateNewTab();
        _addTabBtn.MouseEnter += (s, e) => _addTabBtn.ForeColor = _textColor;
        _addTabBtn.MouseLeave += (s, e) => _addTabBtn.ForeColor = _textDim;

        // ===== EDITOR PANEL =====
        _editorPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _editorBg,
            Padding = new Padding(0)
        };

        // Line numbers
        _lineNumbers = new RichTextBox
        {
            Width = 55,
            Dock = DockStyle.Left,
            BackColor = _editorBg,
            ForeColor = _textDim,
            Font = new Font("Consolas", 11),
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.None,
            Text = "1",
            SelectionAlignment = HorizontalAlignment.Right,
            Cursor = Cursors.Arrow
        };
        _lineNumbers.Enter += (s, e) => _currentEditor?.Focus();
        _editorPanel.Controls.Add(_lineNumbers);

        // ===== TOOLBAR (floating at bottom) =====
        _toolbarPanel = new Panel
        {
            Height = 45,
            Dock = DockStyle.Bottom,
            BackColor = _editorBg,
            Padding = new Padding(0, 8, 0, 8)
        };

        var toolbarInner = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Anchor = AnchorStyles.None
        };

        toolbarInner.Controls.Add(CreateToolbarButton("▷", "Execute"));
        toolbarInner.Controls.Add(CreateToolbarButton("🗑", "Clear"));
        toolbarInner.Controls.Add(CreateToolbarButton("📂", "Open"));
        toolbarInner.Controls.Add(CreateToolbarButton("💾", "Save"));
        toolbarInner.Controls.Add(CreateToolbarButton("📡", "Attach"));
        toolbarInner.Controls.Add(CreateToolbarButton("⊗", "Kill"));

        _toolbarPanel.Controls.Add(toolbarInner);
        _toolbarPanel.Resize += (s, e) =>
        {
            toolbarInner.Location = new Point(
                (_toolbarPanel.Width - toolbarInner.Width) / 2,
                (_toolbarPanel.Height - toolbarInner.Height) / 2
            );
        };

        // Add all to form
        Controls.Add(_editorPanel);
        Controls.Add(_toolbarPanel);
        Controls.Add(_tabBar);
        Controls.Add(_titleBar);

        ResumeLayout(false);
    }

    private void LogoPanel_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        // Draw V shape - BLUE
        using var pen = new Pen(_logoBlue, 3);
        e.Graphics.DrawLine(pen, 5, 5, 17, 30);
        e.Graphics.DrawLine(pen, 30, 5, 17, 30);
    }

    private Button CreateTitleButton(string text, int rightOffset)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(45, 45),
            Location = new Point(rightOffset, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11),
            ForeColor = _textDim,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 50);
        return btn;
    }

    private Panel CreateToolbarButton(string icon, string text)
    {
        var btn = new Panel
        {
            Size = new Size(85, 30),
            BackColor = Color.FromArgb(50, 50, 50),
            Cursor = Cursors.Hand,
            Margin = new Padding(4, 0, 4, 0)
        };

        // Round corners
        var path = new GraphicsPath();
        path.AddArc(0, 0, 10, 10, 180, 90);
        path.AddArc(btn.Width - 10, 0, 10, 10, 270, 90);
        path.AddArc(btn.Width - 10, btn.Height - 10, 10, 10, 0, 90);
        path.AddArc(0, btn.Height - 10, 10, 10, 90, 90);
        path.CloseFigure();
        btn.Region = new Region(path);

        var lbl = new Label
        {
            Text = $"{icon} {text}",
            Font = new Font("Segoe UI", 9),
            ForeColor = _textColor,
            AutoSize = false,
            Size = btn.Size,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btn.Controls.Add(lbl);

        btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);
        btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(50, 50, 50);
        lbl.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);
        lbl.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(50, 50, 50);

        // Click handlers
        Action? clickAction = text switch
        {
            "Execute" => () => MessageBox.Show("Execute script", "Execute"),
            "Clear" => () => _activeTab?.Editor.Clear(),
            "Open" => OpenFile,
            "Save" => SaveFile,
            "Attach" => () => MessageBox.Show("Attach to process", "Attach"),
            "Kill" => () => MessageBox.Show("Kill execution", "Kill"),
            _ => null
        };

        btn.Click += (s, e) => clickAction?.Invoke();
        lbl.Click += (s, e) => clickAction?.Invoke();

        return btn;
    }

    private void CreateNewTab()
    {
        var tab = new ScriptTab
        {
            Title = $"New Tab {_tabCounter++}"
        };

        // Tab panel
        tab.TabPanel = new Panel
        {
            Size = new Size(130, 26),
            BackColor = _tabBg,
            Margin = new Padding(0, 0, 4, 0),
            Cursor = Cursors.Hand
        };
        RoundCorners(tab.TabPanel, 5);

        // Orange circle indicator
        var indicator = new Panel
        {
            Size = new Size(12, 12),
            Location = new Point(8, 7),
            BackColor = _accentOrange
        };
        RoundCorners(indicator, 6);
        tab.TabPanel.Controls.Add(indicator);

        // Tab label
        tab.TabLabel = new Label
        {
            Text = tab.Title,
            Font = new Font("Segoe UI", 9),
            ForeColor = _textColor,
            Location = new Point(24, 5),
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        tab.TabPanel.Controls.Add(tab.TabLabel);

        // Close button
        tab.CloseButton = new Label
        {
            Text = "×",
            Font = new Font("Segoe UI", 11),
            ForeColor = _textDim,
            Size = new Size(20, 20),
            Location = new Point(106, 3),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        tab.CloseButton.Click += (s, e) => CloseTab(tab);
        tab.CloseButton.MouseEnter += (s, e) => tab.CloseButton.ForeColor = _textColor;
        tab.CloseButton.MouseLeave += (s, e) => tab.CloseButton.ForeColor = _textDim;
        tab.TabPanel.Controls.Add(tab.CloseButton);

        // Click to select
        tab.TabPanel.Click += (s, e) => SelectTab(tab);
        tab.TabLabel.Click += (s, e) => SelectTab(tab);
        indicator.Click += (s, e) => SelectTab(tab);

        // Editor
        tab.Editor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = _editorBg,
            ForeColor = _textColor,
            Font = new Font("Consolas", 11),
            BorderStyle = BorderStyle.None,
            AcceptsTab = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both
        };
        tab.Editor.TextChanged += Editor_TextChanged;
        tab.Editor.VScroll += Editor_VScroll;
        tab.Editor.KeyDown += Editor_KeyDown;

        _tabs.Add(tab);
        RefreshTabs();
        SelectTab(tab);
    }

    private void RoundCorners(Control ctrl, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
        path.AddArc(ctrl.Width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
        path.AddArc(ctrl.Width - radius * 2, ctrl.Height - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(0, ctrl.Height - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        ctrl.Region = new Region(path);
    }

    private void SelectTab(ScriptTab tab)
    {
        if (_activeTab == tab) return;

        if (_activeTab != null)
        {
            _editorPanel.Controls.Remove(_activeTab.Editor);
            _activeTab.TabPanel.BackColor = Color.FromArgb(30, 30, 30);
        }

        _activeTab = tab;
        _currentEditor = tab.Editor;

        _editorPanel.Controls.Add(tab.Editor);
        tab.Editor.BringToFront();
        tab.TabPanel.BackColor = _tabBg;

        _highlighter = new LuaSyntaxHighlighter(tab.Editor, CreatePalette());
        _highlighter.HighlightAll();

        UpdateLineNumbers();
        tab.Editor.Focus();
    }

    private ColorPalette CreatePalette()
    {
        return new ColorPalette
        {
            TextColor = _textColor,
            SyntaxKeyword = _syntaxKeyword,
            SyntaxString = _syntaxString,
            SyntaxComment = _syntaxComment,
            SyntaxNumber = _syntaxNumber,
            SyntaxFunction = _textColor,
            SyntaxOperator = _textColor
        };
    }

    private void CloseTab(ScriptTab tab)
    {
        if (_tabs.Count == 1)
        {
            tab.Editor.Clear();
            tab.Title = "New Tab 1";
            tab.TabLabel.Text = tab.Title;
            return;
        }

        int idx = _tabs.IndexOf(tab);
        _tabs.Remove(tab);
        _tabContainer.Controls.Remove(tab.TabPanel);

        if (_activeTab == tab)
        {
            SelectTab(_tabs[Math.Min(idx, _tabs.Count - 1)]);
        }

        RefreshTabs();
    }

    private void RefreshTabs()
    {
        _tabContainer.Controls.Clear();

        foreach (var tab in _tabs)
        {
            tab.TabLabel.Text = tab.DisplayTitle;
            tab.TabPanel.BackColor = tab == _activeTab ? _tabBg : Color.FromArgb(30, 30, 30);
            _tabContainer.Controls.Add(tab.TabPanel);
        }

        _tabContainer.Controls.Add(_addTabBtn);
    }

    private void UpdateLineNumbers()
    {
        if (_activeTab == null) return;

        int count = Math.Max(1, _activeTab.Editor.Lines.Length);
        string nums = string.Join("\n", Enumerable.Range(1, count));
        if (_lineNumbers.Text != nums)
        {
            _lineNumbers.Text = nums;
        }
    }

    private void ToggleMaximize()
    {
        WindowState = WindowState == FormWindowState.Maximized
            ? FormWindowState.Normal
            : FormWindowState.Maximized;
    }

    #region Events

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
            var pt = PointToScreen(e.Location);
            Location = new Point(pt.X - _dragStart.X, pt.Y - _dragStart.Y);
        }
    }

    private void TitleBar_MouseUp(object? sender, MouseEventArgs e) => _isDragging = false;

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;
        _activeTab.IsModified = true;
        UpdateLineNumbers();

        int line = _activeTab.Editor.GetLineFromCharIndex(_activeTab.Editor.SelectionStart);
        _highlighter?.HighlightLine(line);
    }

    private void Editor_VScroll(object? sender, EventArgs e)
    {
        if (_activeTab == null) return;

        int firstChar = _activeTab.Editor.GetCharIndexFromPosition(new Point(0, 0));
        int firstLine = _activeTab.Editor.GetLineFromCharIndex(firstChar);

        if (firstLine > 0 && firstLine < _lineNumbers.Lines.Length)
        {
            int charIdx = _lineNumbers.GetFirstCharIndexFromLine(firstLine);
            if (charIdx >= 0)
            {
                _lineNumbers.SelectionStart = charIdx;
                _lineNumbers.ScrollToCaret();
            }
        }
    }

    private void Editor_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_activeTab == null) return;

        if (e.KeyCode == Keys.Tab)
        {
            e.SuppressKeyPress = true;
            _activeTab.Editor.SelectedText = "    ";
        }
        else if (e.Control && e.KeyCode == Keys.S)
        {
            e.SuppressKeyPress = true;
            SaveFile();
        }
        else if (e.Control && e.KeyCode == Keys.O)
        {
            e.SuppressKeyPress = true;
            OpenFile();
        }
        else if (e.Control && e.KeyCode == Keys.N)
        {
            e.SuppressKeyPress = true;
            CreateNewTab();
        }
        else if (e.Control && e.KeyCode == Keys.W)
        {
            e.SuppressKeyPress = true;
            if (_activeTab != null) CloseTab(_activeTab);
        }
    }

    private void OpenFile()
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*"
        };

        if (dlg.ShowDialog() == DialogResult.OK)
        {
            string content = File.ReadAllText(dlg.FileName);

            if (_activeTab != null && string.IsNullOrEmpty(_activeTab.Editor.Text))
            {
                _activeTab.Editor.Text = content;
                _activeTab.FilePath = dlg.FileName;
                _activeTab.Title = Path.GetFileName(dlg.FileName);
            }
            else
            {
                CreateNewTab();
                _activeTab!.Editor.Text = content;
                _activeTab.FilePath = dlg.FileName;
                _activeTab.Title = Path.GetFileName(dlg.FileName);
            }

            _highlighter?.HighlightAll();
            RefreshTabs();
        }
    }

    private void SaveFile()
    {
        if (_activeTab == null) return;

        string? path = _activeTab.FilePath;

        if (string.IsNullOrEmpty(path))
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Lua Scripts (*.lua)|*.lua|All Files (*.*)|*.*",
                DefaultExt = "lua"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
                path = dlg.FileName;
            else
                return;
        }

        File.WriteAllText(path, _activeTab.Editor.Text);
        _activeTab.FilePath = path;
        _activeTab.Title = Path.GetFileName(path);
        _activeTab.IsModified = false;
        RefreshTabs();
    }

    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Color.FromArgb(50, 50, 50), 1);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
