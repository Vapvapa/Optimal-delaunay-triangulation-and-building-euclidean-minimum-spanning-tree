using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace DelaunayTriangulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Polygon polygon = new Polygon(new List<Point>() { new Point(8, 1), new Point(7, 3), new Point(3, 1) });
        private Triangulation triangulation = new Triangulation();
        public MainWindow()
        {
            InitializeComponent();
            DrawingImageInForm();
        }

        private void DrawingImageInForm()
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }

        private void DrawСoordinateSystem(DrawingGroup drawingGroup)
        {
            DrawBackground(drawingGroup);
            DrawGrid(drawingGroup);
            DrawNumbers(drawingGroup);
        }

        private void DrawBackground(DrawingGroup drawingGroup)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Brush = Brushes.White; // Square-graph background
            //geometryDrawing.Pen = new Pen(Brushes.Black, 0.01); // The border of the square-graph

            RectangleGeometry imageBackground = new RectangleGeometry { Rect = new Rect(0, 0, 1.1, 1.1) };
            geometryGroup.Children.Add(imageBackground);

            geometryDrawing.Geometry = geometryGroup;
            drawingGroup.Children.Add(geometryDrawing);
        }

        private void DrawGrid(DrawingGroup drawingGroup)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Brush = Brushes.Black;
            geometryDrawing.Pen = new Pen(Brushes.Gray, 0.001);
            //geometryDrawing.Pen.DashStyle = DashStyles.Dot; // Можно попробовать заменить пунктир на точки

            DoubleCollection dashes = new DoubleCollection();
            for (int i = 1; i < 10; i++)
                dashes.Add(0.1);
            geometryDrawing.Pen.DashStyle = new DashStyle(dashes, 0);

            geometryDrawing.Pen.EndLineCap = PenLineCap.Round;
            geometryDrawing.Pen.StartLineCap = PenLineCap.Round;
            geometryDrawing.Pen.DashCap = PenLineCap.Round;

            for (int i = 1; i <= 10; i++)
            {
                LineGeometry linesX = new LineGeometry(new Point(0, i * 0.1), new Point(1.1, i * 0.1));
                LineGeometry linesY = new LineGeometry(new Point(i * 0.1, 0), new Point(i * 0.1, 1.1));
                geometryGroup.Children.Add(linesX);
                geometryGroup.Children.Add(linesY);
            }

            geometryDrawing.Geometry = geometryGroup;
            drawingGroup.Children.Add(geometryDrawing);
        }

        private void DrawNumbers(DrawingGroup drawingGroup)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Brush = Brushes.White;
            geometryDrawing.Pen = new Pen(Brushes.White, 0.001);

            for (int i = 1; i <= 10; i++)
            {

                // Create a formatted text string.
                FormattedText textWithNumber = new FormattedText(i.ToString(), CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight, new Typeface("Consolas"), 0.05, Brushes.Black, 1);

                // Build a geometry out of the formatted text.
                Geometry geometryNumber = textWithNumber.BuildGeometry(new Point(-0.1, i * 0.1 - 0.02));
                geometryGroup.Children.Add(geometryNumber);

                geometryNumber = textWithNumber.BuildGeometry(new Point(i * 0.1 - 0.02, -0.1));
                geometryGroup.Children.Add(geometryNumber);

                geometryDrawing.Geometry = geometryGroup;
                drawingGroup.Children.Add(geometryDrawing);
            }
        }

        private void buttonFromFin_Click(object sender, RoutedEventArgs e)
        {
            FinPolygon();
        }

        private void FinPolygon()
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);


            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "fin",
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            Nullable<bool> result = openFileDialog.ShowDialog();

            if (result == true)
                polygon = new Polygon(openFileDialog.FileName);
            else
                MessageBox.Show("Ошибка при открытии файла!");

            polygon.DrawPolygon(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }

        private void buttonFromRandom_Click(object sender, RoutedEventArgs e)
        {
            polygon = new Polygon();
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);
            polygon.DrawPolygon(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }
        private void buttonTriangulation_Click(object sender, RoutedEventArgs e)
        {
            triangulation = new Triangulation(polygon);
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);
            polygon.DrawPolygon(drawingGroup);
            triangulation.DrawTriangulation(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }

        private void buttonMinimumSpanningTree_Click(object sender, RoutedEventArgs e)
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);
            polygon.DrawPolygon(drawingGroup);
            triangulation.DrawTriangulation(drawingGroup);
            MinimumSpanningTree minimumSpanningTree = new MinimumSpanningTree(triangulation.GetEdges(), triangulation.GetNodes());
            minimumSpanningTree.DrawMinimumSpanningTree(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            DrawСoordinateSystem(drawingGroup);
            image.Source = new DrawingImage(drawingGroup);
        }

        private void buttonFout_Click(object sender, RoutedEventArgs e)
        {
            polygon.FoutPoints();
        }
    }
}