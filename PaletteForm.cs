using System.Drawing.Drawing2D;

namespace VertexLuaExecutor;

public class PaletteForm : Form
{
    private ColorPalette? _selectedPalette;
    public ColorPalette? SelectedPalette => _selectedPalette;

    private readonly List<ColorPalette> _palettes;
    private Panel _contentPanel = null!;

    public PaletteForm(ColorPalette currentPalette)
    {
        _palettes = ColorPalette.GetAllPalettes();
        _selectedPalette = currentPalette;
        InitializeComponent(currentPalette);
    }

    private void InitializeComponent(ColorPalette currentPalette)
    {
        // Form settings
        Text = "Color Palette";
        Size = new Size(400, 350);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(25, 25, 25);
        DoubleBuffered = true;

        // Title bar
        var titleBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.FromArgb(20, 20, 20)
        };

        var titleLabel = new Label
        {
            Text = "🎨 Select Color Palette",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(15, 10),
            AutoSize = true
        };
        titleBar.Controls.Add(titleLabel);

        var closeBtn = new Button
        {
            Text = "✕",
            Size = new Size(40, 40),
            Location = new Point(Width - 40, 0),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.FromArgb(150, 150, 150),
            BackColor = Color.FromArgb(20, 20, 20),
            Cursor = Cursors.Hand,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        closeBtn.FlatAppearance.BorderSize = 0;
        closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 50, 50);
        closeBtn.Click += (s, e) =>
        {
            _selectedPalette = null;
            Close();
        };
        titleBar.Controls.Add(closeBtn);

        // Content panel
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            BackColor = Color.FromArgb(25, 25, 25)
        };

        int y = 10;
        foreach (var palette in _palettes)
        {
            var card = CreatePaletteCard(palette, palette.Name == currentPalette.Name, y);
            _contentPanel.Controls.Add(card);
            y += 55;
        }

        Controls.Add(_contentPanel);
        Controls.Add(titleBar);

        // Border
        Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(60, 60, 60), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        };

        // Make form draggable
        titleBar.MouseDown += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                titleBar.Capture = false;
                var msg = Message.Create(Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                WndProc(ref msg);
            }
        };
    }

    private Panel CreatePaletteCard(ColorPalette palette, bool isSelected, int y)
    {
        var card = new Panel
        {
            Size = new Size(350, 48),
            Location = new Point(10, y),
            BackColor = isSelected ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35),
            Cursor = Cursors.Hand
        };

        // Color preview squares
        var colors = new[]
        {
            palette.BackgroundDark,
            palette.BackgroundMedium,
            palette.SyntaxKeyword,
            palette.SyntaxString,
            palette.AccentColor
        };

        int previewX = 10;
        foreach (var color in colors)
        {
            var preview = new Panel
            {
                Size = new Size(20, 20),
                Location = new Point(previewX, 14),
                BackColor = color
            };
            MakeRounded(preview, 4);
            card.Controls.Add(preview);
            previewX += 26;
        }

        // Palette name
        var nameLabel = new Label
        {
            Text = palette.Name,
            Font = new Font("Segoe UI", 10, isSelected ? FontStyle.Bold : FontStyle.Regular),
            ForeColor = Color.White,
            Location = new Point(previewX + 10, 14),
            AutoSize = true
        };
        card.Controls.Add(nameLabel);

        // Checkmark if selected
        if (isSelected)
        {
            var check = new Label
            {
                Text = "✓",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 100),
                Location = new Point(320, 12),
                AutoSize = true
            };
            card.Controls.Add(check);
        }

        MakeRounded(card, 8);

        // Hover effect
        card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(55, 55, 55);
        card.MouseLeave += (s, e) => card.BackColor = isSelected ? Color.FromArgb(50, 50, 50) : Color.FromArgb(35, 35, 35);

        // Click to select
        card.Click += (s, e) =>
        {
            _selectedPalette = palette;
            Close();
        };

        // Make all children clickable
        foreach (Control ctrl in card.Controls)
        {
            ctrl.Click += (s, e) =>
            {
                _selectedPalette = palette;
                Close();
            };
        }

        return card;
    }

    private void MakeRounded(Control ctrl, int radius)
    {
        var path = new GraphicsPath();
        var rect = new Rectangle(0, 0, ctrl.Width, ctrl.Height);
        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
        path.CloseFigure();
        ctrl.Region = new Region(path);
    }
}
