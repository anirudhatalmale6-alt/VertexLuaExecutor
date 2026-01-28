using System.Drawing.Drawing2D;

namespace VertexLuaExecutor;

public partial class MainForm : Form
{
    private readonly List<ScriptTab> _tabs = new();
    private ScriptTab? _activeTab;
    private int _tabCounter = 1;

    // Exact colors from reference
    private readonly Color _bgBlack = Color.FromArgb(18, 18, 18);
    private readonly Color _editorBg = Color.FromArgb(24, 24, 24);
    private readonly Color _textWhite = Color.FromArgb(225, 225, 225);
    private readonly Color _textGray = Color.FromArgb(85, 85, 85);
    private readonly Color _tabBg = Color.FromArgb(35, 35, 35);
    private readonly Color _accentOrange = Color.FromArgb(240, 100, 60);
    private readonly Color _logoBlue = Color.FromArgb(0, 120, 215);
    private readonly Color _keywordRed = Color.FromArgb(235, 80, 80);

    // UI
    private Panel _titleBar = null!;
    private Panel _tabBar = null!;
    private Panel _editorArea = null!;
    private Panel _toolbar = null!;
    private RichTextBox _lineNumbers = null!;

    private LuaSyntaxHighlighter? _highlighter;
    private bool _dragging;
    private Point _dragStart;

    public MainForm()
    {
        InitializeComponent();
        CreateTab();
    }

    private void InitializeComponent()
    {
        Text = "Vertex";
        Size = new Size(920, 680);
        MinimumSize = new Size(600, 400);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = _bgBlack;
        DoubleBuffered = true;

        // ========== TITLE BAR ==========
        _titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = _bgBlack
        };
        _titleBar.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { _dragging = true; _dragStart = e.Location; } };
        _titleBar.MouseMove += (s, e) => { if (_dragging) { var p = PointToScreen(e.Location); Location = new Point(p.X - _dragStart.X, p.Y - _dragStart.Y); } };
        _titleBar.MouseUp += (s, e) => _dragging = false;
        _titleBar.DoubleClick += (s, e) => ToggleMax();

        // V Logo (blue)
        var logo = new Panel { Size = new Size(30, 30), Location = new Point(15, 5), BackColor = Color.Transparent };
        logo.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(_logoBlue, 2.5f);
            e.Graphics.DrawLine(pen, 4, 4, 15, 26);
            e.Graphics.DrawLine(pen, 26, 4, 15, 26);
        };
        _titleBar.Controls.Add(logo);

        // Window buttons (right)
        var closeBtn = MakeTitleBtn("✕", 0);
        closeBtn.Click += (s, e) => Close();
        closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 40, 40);

        var maxBtn = MakeTitleBtn("□", 46);
        maxBtn.Click += (s, e) => ToggleMax();

        var minBtn = MakeTitleBtn("─", 92);
        minBtn.Click += (s, e) => WindowState = FormWindowState.Minimized;

        var gearBtn = MakeTitleBtn("⚙", 138);

        var btnPanel = new Panel { Size = new Size(184, 40), Dock = DockStyle.Right, BackColor = Color.Transparent };
        btnPanel.Controls.AddRange(new Control[] { closeBtn, maxBtn, minBtn, gearBtn });
        _titleBar.Controls.Add(btnPanel);

        // ========== TAB BAR ==========
        _tabBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 36,
            BackColor = _bgBlack
        };

        // ========== EDITOR AREA ==========
        _editorArea = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = _editorBg
        };

        _lineNumbers = new RichTextBox
        {
            Width = 50,
            Dock = DockStyle.Left,
            BackColor = _editorBg,
            ForeColor = _textGray,
            Font = new Font("Consolas", 11f),
            BorderStyle = BorderStyle.None,
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.None,
            SelectionAlignment = HorizontalAlignment.Right,
            Text = "1",
            Cursor = Cursors.Arrow
        };
        _editorArea.Controls.Add(_lineNumbers);

        // ========== TOOLBAR (floating at bottom) ==========
        _toolbar = new Panel
        {
            Height = 42,
            Dock = DockStyle.Bottom,
            BackColor = _editorBg
        };

        var flow = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight
        };

        flow.Controls.Add(MakeToolBtn("▷", "Execute", () => Msg("Execute")));
        flow.Controls.Add(MakeToolBtn("🗑", "Clear", () => _activeTab?.Editor.Clear()));
        flow.Controls.Add(MakeToolBtn("📂", "Open", DoOpen));
        flow.Controls.Add(MakeToolBtn("💾", "Save", DoSave));
        flow.Controls.Add(MakeToolBtn("📡", "Attach", () => Msg("Attach")));
        flow.Controls.Add(MakeToolBtn("⊗", "Kill", () => Msg("Kill")));

        _toolbar.Controls.Add(flow);
        _toolbar.Resize += (s, e) => flow.Location = new Point((_toolbar.Width - flow.Width) / 2, (_toolbar.Height - flow.Height) / 2);

        // Add to form
        Controls.Add(_editorArea);
        Controls.Add(_toolbar);
        Controls.Add(_tabBar);
        Controls.Add(_titleBar);
    }

    private Button MakeTitleBtn(string txt, int x)
    {
        var b = new Button
        {
            Text = txt,
            Size = new Size(46, 40),
            Location = new Point(x, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11),
            ForeColor = _textGray,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 45, 45);
        return b;
    }

    private Panel MakeToolBtn(string icon, string text, Action onClick)
    {
        var p = new Panel
        {
            Size = new Size(90, 28),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            Margin = new Padding(6, 0, 6, 0)
        };

        var lbl = new Label
        {
            Text = $"{icon}  {text}",
            Font = new Font("Segoe UI", 9.5f),
            ForeColor = _textWhite,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };

        p.Controls.Add(lbl);
        p.MouseEnter += (s, e) => p.BackColor = Color.FromArgb(50, 50, 50);
        p.MouseLeave += (s, e) => p.BackColor = Color.Transparent;
        lbl.MouseEnter += (s, e) => p.BackColor = Color.FromArgb(50, 50, 50);
        lbl.MouseLeave += (s, e) => p.BackColor = Color.Transparent;
        p.Click += (s, e) => onClick();
        lbl.Click += (s, e) => onClick();

        return p;
    }

    private void CreateTab()
    {
        var tab = new ScriptTab { Title = $"New Tab {_tabCounter++}" };

        // Tab header
        tab.TabPanel = new Panel
        {
            Size = new Size(125, 26),
            Location = new Point(10 + _tabs.Count * 130, 5),
            BackColor = _tabBg,
            Cursor = Cursors.Hand
        };
        Round(tab.TabPanel, 4);

        // Orange circle
        var dot = new Panel { Size = new Size(10, 10), Location = new Point(8, 8), BackColor = _accentOrange };
        Round(dot, 5);
        tab.TabPanel.Controls.Add(dot);

        // Label
        tab.TabLabel = new Label
        {
            Text = tab.Title,
            Font = new Font("Segoe UI", 9),
            ForeColor = _textWhite,
            Location = new Point(22, 5),
            AutoSize = true,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        tab.TabPanel.Controls.Add(tab.TabLabel);

        // X button
        tab.CloseButton = new Label
        {
            Text = "×",
            Font = new Font("Segoe UI", 11),
            ForeColor = _textGray,
            Size = new Size(18, 20),
            Location = new Point(103, 3),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand
        };
        tab.CloseButton.Click += (s, e) => CloseTab(tab);
        tab.CloseButton.MouseEnter += (s, e) => tab.CloseButton.ForeColor = _textWhite;
        tab.CloseButton.MouseLeave += (s, e) => tab.CloseButton.ForeColor = _textGray;
        tab.TabPanel.Controls.Add(tab.CloseButton);

        // Click handlers
        tab.TabPanel.Click += (s, e) => SelectTab(tab);
        tab.TabLabel.Click += (s, e) => SelectTab(tab);
        dot.Click += (s, e) => SelectTab(tab);

        // Editor
        tab.Editor = new RichTextBox
        {
            Dock = DockStyle.Fill,
            BackColor = _editorBg,
            ForeColor = _textWhite,
            Font = new Font("Consolas", 11),
            BorderStyle = BorderStyle.None,
            AcceptsTab = true,
            WordWrap = false,
            ScrollBars = RichTextBoxScrollBars.Both
        };
        tab.Editor.TextChanged += OnTextChanged;
        tab.Editor.VScroll += OnScroll;
        tab.Editor.KeyDown += OnKeyDown;

        _tabs.Add(tab);
        _tabBar.Controls.Add(tab.TabPanel);
        AddPlusButton();
        SelectTab(tab);
    }

    private void AddPlusButton()
    {
        // Remove old + button
        foreach (Control c in _tabBar.Controls)
            if (c.Tag?.ToString() == "plus") { _tabBar.Controls.Remove(c); break; }

        var plus = new Label
        {
            Text = "+",
            Font = new Font("Segoe UI", 14),
            ForeColor = _textGray,
            Size = new Size(24, 24),
            Location = new Point(15 + _tabs.Count * 130, 6),
            TextAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            Tag = "plus"
        };
        plus.Click += (s, e) => CreateTab();
        plus.MouseEnter += (s, e) => plus.ForeColor = _textWhite;
        plus.MouseLeave += (s, e) => plus.ForeColor = _textGray;
        _tabBar.Controls.Add(plus);
    }

    private void SelectTab(ScriptTab tab)
    {
        if (_activeTab == tab) return;

        if (_activeTab != null)
        {
            _editorArea.Controls.Remove(_activeTab.Editor);
            _activeTab.TabPanel.BackColor = Color.FromArgb(28, 28, 28);
        }

        _activeTab = tab;
        _editorArea.Controls.Add(tab.Editor);
        tab.Editor.BringToFront();
        tab.TabPanel.BackColor = _tabBg;

        _highlighter = new LuaSyntaxHighlighter(tab.Editor, MakePalette());
        _highlighter.HighlightAll();
        UpdateLines();
        tab.Editor.Focus();
    }

    private ColorPalette MakePalette() => new ColorPalette
    {
        TextColor = _textWhite,
        SyntaxKeyword = _keywordRed,
        SyntaxString = Color.FromArgb(206, 145, 120),
        SyntaxComment = Color.FromArgb(106, 153, 85),
        SyntaxNumber = _textWhite,
        SyntaxFunction = _textWhite,
        SyntaxOperator = _textWhite
    };

    private void CloseTab(ScriptTab tab)
    {
        if (_tabs.Count == 1) { tab.Editor.Clear(); return; }

        int i = _tabs.IndexOf(tab);
        _tabs.Remove(tab);
        _tabBar.Controls.Remove(tab.TabPanel);

        // Reposition tabs
        for (int j = 0; j < _tabs.Count; j++)
            _tabs[j].TabPanel.Location = new Point(10 + j * 130, 5);

        AddPlusButton();
        if (_activeTab == tab) SelectTab(_tabs[Math.Min(i, _tabs.Count - 1)]);
    }

    private void UpdateLines()
    {
        if (_activeTab == null) return;
        int n = Math.Max(1, _activeTab.Editor.Lines.Length);
        var s = string.Join("\n", Enumerable.Range(1, n));
        if (_lineNumbers.Text != s) _lineNumbers.Text = s;
    }

    private void Round(Control c, int r)
    {
        var p = new GraphicsPath();
        p.AddArc(0, 0, r * 2, r * 2, 180, 90);
        p.AddArc(c.Width - r * 2, 0, r * 2, r * 2, 270, 90);
        p.AddArc(c.Width - r * 2, c.Height - r * 2, r * 2, r * 2, 0, 90);
        p.AddArc(0, c.Height - r * 2, r * 2, r * 2, 90, 90);
        p.CloseFigure();
        c.Region = new Region(p);
    }

    private void ToggleMax() => WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
    private void Msg(string t) => MessageBox.Show(t, t);

    private void OnTextChanged(object? s, EventArgs e)
    {
        if (_activeTab == null) return;
        _activeTab.IsModified = true;
        UpdateLines();
        int ln = _activeTab.Editor.GetLineFromCharIndex(_activeTab.Editor.SelectionStart);
        _highlighter?.HighlightLine(ln);
    }

    private void OnScroll(object? s, EventArgs e)
    {
        if (_activeTab == null) return;
        int fc = _activeTab.Editor.GetCharIndexFromPosition(new Point(0, 0));
        int fl = _activeTab.Editor.GetLineFromCharIndex(fc);
        if (fl > 0 && fl < _lineNumbers.Lines.Length)
        {
            int ci = _lineNumbers.GetFirstCharIndexFromLine(fl);
            if (ci >= 0) { _lineNumbers.SelectionStart = ci; _lineNumbers.ScrollToCaret(); }
        }
    }

    private void OnKeyDown(object? s, KeyEventArgs e)
    {
        if (_activeTab == null) return;
        if (e.KeyCode == Keys.Tab) { e.SuppressKeyPress = true; _activeTab.Editor.SelectedText = "    "; }
        else if (e.Control && e.KeyCode == Keys.S) { e.SuppressKeyPress = true; DoSave(); }
        else if (e.Control && e.KeyCode == Keys.O) { e.SuppressKeyPress = true; DoOpen(); }
        else if (e.Control && e.KeyCode == Keys.N) { e.SuppressKeyPress = true; CreateTab(); }
        else if (e.Control && e.KeyCode == Keys.W) { e.SuppressKeyPress = true; if (_activeTab != null) CloseTab(_activeTab); }
    }

    private void DoOpen()
    {
        using var d = new OpenFileDialog { Filter = "Lua (*.lua)|*.lua|All (*.*)|*.*" };
        if (d.ShowDialog() == DialogResult.OK)
        {
            if (_activeTab != null && string.IsNullOrEmpty(_activeTab.Editor.Text))
            {
                _activeTab.Editor.Text = File.ReadAllText(d.FileName);
                _activeTab.FilePath = d.FileName;
                _activeTab.Title = Path.GetFileName(d.FileName);
                _activeTab.TabLabel.Text = _activeTab.Title;
            }
            else
            {
                CreateTab();
                _activeTab!.Editor.Text = File.ReadAllText(d.FileName);
                _activeTab.FilePath = d.FileName;
                _activeTab.Title = Path.GetFileName(d.FileName);
                _activeTab.TabLabel.Text = _activeTab.Title;
            }
            _highlighter?.HighlightAll();
        }
    }

    private void DoSave()
    {
        if (_activeTab == null) return;
        var path = _activeTab.FilePath;
        if (string.IsNullOrEmpty(path))
        {
            using var d = new SaveFileDialog { Filter = "Lua (*.lua)|*.lua|All (*.*)|*.*", DefaultExt = "lua" };
            if (d.ShowDialog() == DialogResult.OK) path = d.FileName; else return;
        }
        File.WriteAllText(path, _activeTab.Editor.Text);
        _activeTab.FilePath = path;
        _activeTab.Title = Path.GetFileName(path);
        _activeTab.TabLabel.Text = _activeTab.Title;
        _activeTab.IsModified = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var pen = new Pen(Color.FromArgb(40, 40, 40), 1);
        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
    }
}
