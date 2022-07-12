using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DelaunayTriangulation
{
    internal class Polygon
    {
        private List<Point> points;
        public List<Point> GetPoints() { return points; }

        public Polygon()
        {
            points = CreatingRandomPolygon();
        }

        public Polygon(List<Point> points)
        {
            this.points = points;
        }

        public Polygon(string filename)
        {
            points = new List<Point>();
            if (ConvertTextToArrayPoints(GetTextFromFile(filename)))
            {
                return;
            }
            else
            {
                MessageBox.Show("Error! Attempt to create a polygon of less than 3 points!");
                return;
            }
        }

        public void DrawPolygon(DrawingGroup drawingGroup)
        {
            if (points.Count < 3)
                return;

            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Pen = new Pen(Brushes.Black, 0.02);

            // Draw the vertices of the polygon
            for (int i = 0; i < points.Count; i++)
            {
                geometryGroup.Children.Add(new EllipseGeometry(points[i], 0.01, 0.01));
            }

            /* Algorithm in case you need to draw a polygon
            for (int i = 1; i < points.Count; i++)
            {
                LineGeometry lineFromPolygon = new LineGeometry(points[i - 1], points[i]);
                geometryGroup.Children.Add(lineFromPolygon);
            }
            LineGeometry lineFromPolygonLast = new LineGeometry(points[0], points[points.Count - 1]);
            geometryGroup.Children.Add(lineFromPolygonLast);
            */

            geometryDrawing.Geometry = geometryGroup;
            drawingGroup.Children.Add(geometryDrawing);
        }

        public void FoutPoints()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "fout",
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            string text = "Coordinates of points:";
            for (int i = 0; i < points.Count; i++)
            {
                text += "\n" + Math.Round(points[i].X * 10, 2) + " " + Math.Round(points[i].Y * 10, 2);
            }

            string fileText = text;

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, fileText);
            else
                MessageBox.Show("Error saving the file!");
        }

        private List<Point> CreatingRandomPolygon()
        {
            List<Point> points = new List<Point>();
            Random random = new Random();
            int pointsCount = random.Next(6, 10);
            double x, y;

            for (int i = 0; i < pointsCount; i++)
            {
                x = Math.Round(random.Next(1, 9) * 0.1, 2);
                y = Math.Round(random.Next(1, 9) * 0.1, 2);

                points.Add(new Point(x, y));
            }

            // Removing matching points
            for (int i = 0; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    if (points[i].X == points[j].X && points[i].Y == points[j].Y)
                    {
                        points.RemoveAt(j);
                    }
                }
            }

            return points;
        }

        private bool ConvertTextToArrayPoints(string text)
        {
            double x = 0;
            bool IsX = false;
            List<double> arrayNumbers = new List<double>();
            for (int k = 0; k < text.Length; k++)
            {
                if (char.IsDigit(text[k]))
                {
                    while (k < text.Length && char.IsDigit(text[k]))
                    {
                        x = x * 10 + text[k] - 48;
                        k++;
                    }
                    if (k < text.Length && text[k] == '.')
                    {
                        k++;
                        double zeros = 0.1;
                        while (k < text.Length && char.IsDigit(text[k]))
                        {
                            x += (text[k] - 48) * zeros;
                            zeros *= 0.1;
                            k++;
                        }
                    }
                    IsX = true;
                }
                if (IsX)
                {
                    arrayNumbers.Add(x);
                    x = 0;
                    IsX = false;
                }
            }

            if (arrayNumbers.Count < 6)
                return false;

            ArrayNumbersNormalization(arrayNumbers);


            for (int k = 0; k < arrayNumbers.Count; k += 2)
                points.Add(new Point(arrayNumbers[k], arrayNumbers[k + 1]));
            
            return true;
        }

        private void ArrayNumbersNormalization(List<double> arrayNumbers) // Normalization for plotting
        {
            if (arrayNumbers.Count % 2 != 0) // To add points, you need an even number of numbers (for x and y)
                arrayNumbers.RemoveAt(arrayNumbers.Count);

            // The numbers on the graph are in the range from 0 to 10, but in fact the graph shows points from 0 to 1
            // (digits from 0 to 10 multiplied by 0.1)
            for (int i = 0; i < arrayNumbers.Count; i++)
            {
                if (arrayNumbers[i] < 0)
                    arrayNumbers[i] *= -1;
                while (arrayNumbers[i] >= 1)
                    arrayNumbers[i] *= 0.1;
            }

            // Up to 2 decimal places: *,**
            for (int i = 0; i < arrayNumbers.Count; i++)
            {
                arrayNumbers[i] = Math.Round(arrayNumbers[i], 2);
            }
        }

        private string GetTextFromFile(string filename)
        {
            string textFromFin;
            using (FileStream fin = File.OpenRead(filename))
            {
                byte[] textByte = new byte[fin.Length];
                fin.Read(textByte, 0, textByte.Length);

                textFromFin = Encoding.Default.GetString(textByte);
                MessageBox.Show("The read text:\n" + textFromFin);
                fin.Close();
            }
            return textFromFin;
        }
    }
}