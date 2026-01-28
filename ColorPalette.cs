namespace VertexLuaExecutor;

public class ColorPalette
{
    public string Name { get; set; } = "Default";
    public Color BackgroundDark { get; set; }
    public Color BackgroundMedium { get; set; }
    public Color BackgroundLight { get; set; }
    public Color AccentColor { get; set; }
    public Color TextColor { get; set; }
    public Color TextSecondary { get; set; }
    public Color ButtonBackground { get; set; }
    public Color ButtonHover { get; set; }
    public Color TabActive { get; set; }
    public Color TabInactive { get; set; }
    public Color BorderColor { get; set; }

    // Syntax highlighting colors
    public Color SyntaxKeyword { get; set; }
    public Color SyntaxString { get; set; }
    public Color SyntaxComment { get; set; }
    public Color SyntaxNumber { get; set; }
    public Color SyntaxFunction { get; set; }
    public Color SyntaxOperator { get; set; }

    public static ColorPalette DefaultDark => new ColorPalette
    {
        Name = "Default Dark",
        BackgroundDark = Color.FromArgb(10, 10, 10),
        BackgroundMedium = Color.FromArgb(20, 20, 20),
        BackgroundLight = Color.FromArgb(18, 18, 18),
        AccentColor = Color.FromArgb(0, 122, 204),
        TextColor = Color.FromArgb(220, 220, 220),
        TextSecondary = Color.FromArgb(120, 120, 120),
        ButtonBackground = Color.FromArgb(45, 45, 45),
        ButtonHover = Color.FromArgb(65, 65, 65),
        TabActive = Color.FromArgb(35, 35, 35),
        TabInactive = Color.FromArgb(25, 25, 25),
        BorderColor = Color.FromArgb(50, 50, 50),
        // Syntax colors matching reference: red keywords, pink strings
        SyntaxKeyword = Color.FromArgb(248, 90, 90),      // Red/coral for keywords
        SyntaxString = Color.FromArgb(255, 180, 200),     // Pink for strings
        SyntaxComment = Color.FromArgb(90, 90, 90),       // Gray for comments
        SyntaxNumber = Color.FromArgb(220, 220, 220),     // White for numbers
        SyntaxFunction = Color.FromArgb(220, 220, 220),   // White for function names
        SyntaxOperator = Color.FromArgb(220, 220, 220)    // White for operators
    };

    public static ColorPalette BluePurple => new ColorPalette
    {
        Name = "Blue Purple",
        BackgroundDark = Color.FromArgb(15, 15, 35),
        BackgroundMedium = Color.FromArgb(25, 25, 55),
        BackgroundLight = Color.FromArgb(40, 40, 80),
        AccentColor = Color.FromArgb(138, 43, 226),
        TextColor = Color.FromArgb(230, 230, 255),
        TextSecondary = Color.FromArgb(160, 160, 200),
        ButtonBackground = Color.FromArgb(50, 50, 100),
        ButtonHover = Color.FromArgb(70, 70, 130),
        TabActive = Color.FromArgb(40, 40, 80),
        TabInactive = Color.FromArgb(25, 25, 55),
        BorderColor = Color.FromArgb(80, 80, 140),
        SyntaxKeyword = Color.FromArgb(199, 146, 234),
        SyntaxString = Color.FromArgb(195, 232, 141),
        SyntaxComment = Color.FromArgb(99, 119, 119),
        SyntaxNumber = Color.FromArgb(247, 140, 108),
        SyntaxFunction = Color.FromArgb(130, 170, 255),
        SyntaxOperator = Color.FromArgb(137, 221, 255)
    };

    public static ColorPalette GreenMatrix => new ColorPalette
    {
        Name = "Matrix Green",
        BackgroundDark = Color.FromArgb(10, 20, 10),
        BackgroundMedium = Color.FromArgb(15, 35, 15),
        BackgroundLight = Color.FromArgb(25, 55, 25),
        AccentColor = Color.FromArgb(0, 255, 65),
        TextColor = Color.FromArgb(200, 255, 200),
        TextSecondary = Color.FromArgb(100, 180, 100),
        ButtonBackground = Color.FromArgb(30, 70, 30),
        ButtonHover = Color.FromArgb(40, 100, 40),
        TabActive = Color.FromArgb(25, 55, 25),
        TabInactive = Color.FromArgb(15, 35, 15),
        BorderColor = Color.FromArgb(0, 150, 50),
        SyntaxKeyword = Color.FromArgb(0, 255, 100),
        SyntaxString = Color.FromArgb(180, 255, 180),
        SyntaxComment = Color.FromArgb(80, 140, 80),
        SyntaxNumber = Color.FromArgb(150, 255, 150),
        SyntaxFunction = Color.FromArgb(100, 255, 200),
        SyntaxOperator = Color.FromArgb(200, 255, 200)
    };

    public static ColorPalette RedCrimson => new ColorPalette
    {
        Name = "Crimson Red",
        BackgroundDark = Color.FromArgb(25, 10, 10),
        BackgroundMedium = Color.FromArgb(45, 20, 20),
        BackgroundLight = Color.FromArgb(70, 35, 35),
        AccentColor = Color.FromArgb(220, 20, 60),
        TextColor = Color.FromArgb(255, 220, 220),
        TextSecondary = Color.FromArgb(180, 140, 140),
        ButtonBackground = Color.FromArgb(90, 40, 40),
        ButtonHover = Color.FromArgb(120, 50, 50),
        TabActive = Color.FromArgb(70, 35, 35),
        TabInactive = Color.FromArgb(45, 20, 20),
        BorderColor = Color.FromArgb(150, 60, 60),
        SyntaxKeyword = Color.FromArgb(255, 100, 100),
        SyntaxString = Color.FromArgb(255, 200, 150),
        SyntaxComment = Color.FromArgb(150, 100, 100),
        SyntaxNumber = Color.FromArgb(255, 180, 180),
        SyntaxFunction = Color.FromArgb(255, 150, 200),
        SyntaxOperator = Color.FromArgb(255, 200, 200)
    };

    public static ColorPalette OceanBlue => new ColorPalette
    {
        Name = "Ocean Blue",
        BackgroundDark = Color.FromArgb(10, 20, 30),
        BackgroundMedium = Color.FromArgb(20, 40, 60),
        BackgroundLight = Color.FromArgb(35, 65, 95),
        AccentColor = Color.FromArgb(0, 191, 255),
        TextColor = Color.FromArgb(220, 240, 255),
        TextSecondary = Color.FromArgb(140, 180, 200),
        ButtonBackground = Color.FromArgb(40, 80, 120),
        ButtonHover = Color.FromArgb(60, 110, 160),
        TabActive = Color.FromArgb(35, 65, 95),
        TabInactive = Color.FromArgb(20, 40, 60),
        BorderColor = Color.FromArgb(60, 120, 180),
        SyntaxKeyword = Color.FromArgb(100, 200, 255),
        SyntaxString = Color.FromArgb(255, 220, 150),
        SyntaxComment = Color.FromArgb(100, 150, 180),
        SyntaxNumber = Color.FromArgb(180, 255, 220),
        SyntaxFunction = Color.FromArgb(150, 220, 255),
        SyntaxOperator = Color.FromArgb(200, 230, 255)
    };

    public static List<ColorPalette> GetAllPalettes()
    {
        return new List<ColorPalette>
        {
            DefaultDark,
            BluePurple,
            GreenMatrix,
            RedCrimson,
            OceanBlue
        };
    }
}
