using System;
using System.Drawing;
using System.Windows.Forms;

namespace GraphicEditor
{
    public class Form1 : Form
    {
        // --- Encapsulation: Private state variables ---
        private Bitmap _canvas = null!;
        private Point _startPoint;
        private bool _isDrawing = false;
        private Color _currentColor = Color.Black;
        private int _penWidth = 3;
        private string _activeTool = "Pen"; 

        public Form1()
        {
            InitializeComponent();
            InitializeCanvas();
            CreateUI();
        }

        #region Initialization
        private void InitializeComponent()
        {
            this.Text = "Graphic Editor - Multi-Color OOP";
            this.Size = new Size(1100, 550); // Increased width for more buttons
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true; 
        }

        private void InitializeCanvas()
        {
            _canvas = new Bitmap(1100, 550);
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                g.Clear(Color.White);
            }
        }
        #endregion

        #region UI Construction
        private void CreateUI()
        {
            int x = 10, y = 10, spacing = 75;

            // Maintenance
            AddButton("Clear", x, y, (s, e) => ResetCanvas()); x += spacing;
            
            // --- Color Palette (5+ Colors) ---
            var btnBlack = AddButton("Black", x, y, (s, e) => _currentColor = Color.Black);
            btnBlack.BackColor = Color.Black; btnBlack.ForeColor = Color.White; x += spacing;

            var btnRed = AddButton("Red", x, y, (s, e) => _currentColor = Color.Red);
            btnRed.BackColor = Color.Red; x += spacing;

            var btnBlue = AddButton("Blue", x, y, (s, e) => _currentColor = Color.Blue);
            btnBlue.BackColor = Color.Blue; btnBlue.ForeColor = Color.White; x += spacing;

            var btnGreen = AddButton("Green", x, y, (s, e) => _currentColor = Color.Green);
            btnGreen.BackColor = Color.Green; x += spacing;

            var btnOrange = AddButton("Orange", x, y, (s, e) => _currentColor = Color.Orange);
            btnOrange.BackColor = Color.Orange; x += spacing;

            // Extra: Custom Color Dialog (OOP Component usage)
            var btnCustom = AddButton("Custom", x, y, (s, e) => SelectCustomColor());
            btnCustom.BackColor = Color.LightGray; x += (spacing + 15);

            // Shape Tools
            AddButton("Pen", x, y, (s, e) => _activeTool = "Pen"); x += spacing;
            AddButton("Line", x, y, (s, e) => _activeTool = "Line"); x += spacing;
            AddButton("Rect", x, y, (s, e) => _activeTool = "Rect"); x += spacing;
            AddButton("Circle", x, y, (s, e) => _activeTool = "Circle"); x += spacing;
            AddButton("Triangle", x, y, (s, e) => _activeTool = "Triangle"); x += (spacing + 15);

            // Settings
            AddButton("Thin", x, y, (s, e) => _penWidth = 2); x += spacing;
            AddButton("Thick", x, y, (s, e) => _penWidth = 8); x += (spacing + 15);

            // Export
            var btnSave = AddButton("Save", x, y, (s, e) => SaveToFile());
            btnSave.BackColor = Color.LightBlue;
        }

        private Button AddButton(string text, int x, int y, EventHandler action)
        {
            Button btn = new Button { Text = text, Location = new Point(x, y), Size = new Size(70, 32) };
            btn.Click += action;
            this.Controls.Add(btn);
            return btn;
        }
        #endregion

        #region OOP Event Handling (Method Overriding)
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _startPoint = e.Location;
                _isDrawing = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_isDrawing && _activeTool == "Pen")
            {
                DrawToBitmap(g => {
                    using Pen p = new Pen(_currentColor, _penWidth);
                    g.DrawLine(p, _startPoint, e.Location);
                });
                _startPoint = e.Location;
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (_isDrawing && _activeTool != "Pen")
            {
                DrawToBitmap(g => {
                    using Pen p = new Pen(_currentColor, _penWidth);
                    Rectangle r = GetBounds(_startPoint, e.Location);
                    
                    switch (_activeTool)
                    {
                        case "Line": g.DrawLine(p, _startPoint, e.Location); break;
                        case "Rect": g.DrawRectangle(p, r); break;
                        case "Circle": g.DrawEllipse(p, r); break;
                        case "Triangle": g.DrawPolygon(p, GetTrianglePoints(r)); break;
                    }
                });
            }
            _isDrawing = false;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(_canvas, 0, 0);
        }
        #endregion

        #region Helper Methods (Geometry & Logic)
        private void DrawToBitmap(Action<Graphics> action)
        {
            using (Graphics g = Graphics.FromImage(_canvas))
            {
                action(g);
            }
        }

        private Rectangle GetBounds(Point p1, Point p2) => new Rectangle(
            Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y),
            Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));

        private Point[] GetTrianglePoints(Rectangle r) => new Point[] {
            new Point(r.Left + r.Width / 2, r.Top),
            new Point(r.Right, r.Bottom),
            new Point(r.Left, r.Bottom)
        };

        private void ResetCanvas()
        {
            DrawToBitmap(g => g.Clear(Color.White));
            this.Invalidate();
        }

        private void SelectCustomColor()
        {
            using ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                _currentColor = cd.Color;
            }
        }

        private void SaveToFile()
        {
            using SaveFileDialog sfd = new SaveFileDialog { Filter = "PNG|*.png|JPG|*.jpg" };
            if (sfd.ShowDialog() == DialogResult.OK) _canvas.Save(sfd.FileName);
        }
        #endregion
    }
}