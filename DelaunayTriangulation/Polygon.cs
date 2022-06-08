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
                MessageBox.Show("Ошибка! Попытка создать многоугольник из менее 3х точек!");
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

            // Рисуем вершины многоугольника
            for (int i = 0; i < points.Count; i++)
            {
                geometryGroup.Children.Add(new EllipseGeometry(points[i], 0.01, 0.01));
            }

            /* Алгоритм на случай, если нужно нарисовать многоугольник
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

            string text = "Координаты точек:";
            for (int i = 0; i < points.Count; i++)
            {
                text += "\n" + Math.Round(points[i].X * 10, 2) + " " + Math.Round(points[i].Y * 10, 2);
            }

            string fileText = text;

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, fileText);
            else
                MessageBox.Show("Ошибка при сохранении файла!");
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

            // Удаляем совпадающие точки
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

        private void ArrayNumbersNormalization(List<double> arrayNumbers) //Нормализация для построения графика
        {
            if (arrayNumbers.Count % 2 != 0) //Для составления точек нужно чётное количество чисел (для х и у)
                arrayNumbers.RemoveAt(arrayNumbers.Count);

            //На графике цифры от 0 до 10, но на самом деле строятся точки от 0 до 1
            //(цифры от 0 до 10 умножены на 0.1)
            for (int i = 0; i < arrayNumbers.Count; i++)
            {
                if (arrayNumbers[i] < 0)
                    arrayNumbers[i] *= -1;
                while (arrayNumbers[i] >= 1)
                    arrayNumbers[i] *= 0.1;
            }

            //Оставляем число до 2 знака после запятой: *,**
            for (int i = 0; i < arrayNumbers.Count; i++)
            {
                arrayNumbers[i] = Math.Round(arrayNumbers[i], 2);
            }
        }

        private bool IsSelfIntersection()
        {
            if (points.Count == 3)
            {
                return false;
            }

            for (int i = 1; i < points.Count - 1; i++)
            {
                for (int j = i + 2; j < points.Count; j++)
                {
                    if (IsIntersectionOfTwoLineSegments(points[i - 1], points[i], points[j - 1], points[j]))
                    {
                        return true;
                    }
                }
            }

            // Проверка для стороны, образующейся первой и последней точками
            for (int i = 2; i < points.Count - 1; i++)
            {
                if (IsIntersectionOfTwoLineSegments(points[points.Count - 1], points[0], points[i - 1], points[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsIntersectionOfTwoLineSegments(Point a, Point b, Point c, Point d)
        {
            Point p1 = a;
            Point p2 = b;
            Point p3 = c;
            Point p4 = d;
            // Сначала расставим точки по порядку, то есть чтобы было p1.X <= p2.X
            if (p2.X < p1.X)
            {
                Point tmp = p1;
                p1 = p2;
                p2 = tmp;
            }

            // и p3.x <= p4.x
            if (p4.X < p3.X)
            {
                Point tmp = p3;
                p3 = p4;
                p4 = tmp;
            }

            // Проверим существование потенциального интервала для точки пересечения отрезков
            if (p2.X < p3.X)
                return false; // Так как у отрезков нету взаимной абсциссы

            // Если оба отрезка вертикальные
            if ((p1.X - p2.X == 0) && (p3.X - p4.X == 0))
            {
                if (p1.X == p3.X) // Если они лежат на одном X
                {
                    // Проверим пересекаются ли они, т.е. есть ли у них общий Y
                    // Для этого возьмём отрицание от случая, когда они НЕ пересекаются
                    if (!((Math.Max(p1.Y, p2.Y) < Math.Min(p3.Y, p4.Y)) ||
                        (Math.Min(p1.Y, p2.Y) > Math.Max(p3.Y, p4.Y))))
                        return true;

                }
                return false;
            }

            // Найдём коэффициенты уравнений, содержащих отрезки
            // f1(x) = A1*x + b1 = y
            // f2(x) = A2*x + b2 = y

            double Xa, A2, b2, Ya, A1, b1;

            // Если первый отрезок вертикальный
            if (p1.X - p2.X == 0)
            {
                // Найдём Xa, Ya - точки пересечения двух прямых
                Xa = p1.X;
                A2 = (p3.Y - p4.Y) / (p3.X - p4.X);
                b2 = p3.Y - A2 * p3.X;
                Ya = A2 * Xa + b2;

                if (p3.X <= Xa && p4.X >= Xa && Math.Min(p1.Y, p2.Y) <= Ya && Math.Max(p1.Y, p2.Y) >= Ya)
                    return true;
                return false;
            }

            if (p3.X - p4.X == 0) // Если второй отрезок вертикальный
            {
                // Найдём Xa, Ya - точки пересечения двух прямых
                Xa = p3.X;
                A1 = (p1.Y - p2.Y) / (p1.X - p2.X);
                b1 = p1.Y - A1 * p1.X;
                Ya = A1 * Xa + b1;

                if (p1.X <= Xa && p2.X >= Xa && Math.Min(p3.Y, p4.Y) <= Ya && Math.Max(p3.Y, p4.Y) >= Ya)
                    return true;
                return false;
            }

            // Оба отрезка невертикальные
            A1 = (p1.Y - p2.Y) / (p1.X - p2.X);
            A2 = (p3.Y - p4.Y) / (p3.X - p4.X);
            b1 = p1.Y - A1 * p1.X;
            b2 = p3.Y - A2 * p3.X;

            if (A1 == A2)
                return false; // Отрезки параллельны

            Xa = (b2 - b1) / (A1 - A2); //Xa - абсцисса точки пересечения двух прямых

            if ((Xa < Math.Max(p1.X, p3.X)) || (Xa > Math.Min(p2.X, p4.X)))
                return false; // Точка Xa находится вне пересечения проекций отрезков на ось X

            else
                return true;
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