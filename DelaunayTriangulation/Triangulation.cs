using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;


namespace DelaunayTriangulation
{
    internal class Triangulation
    {
        private List<Point> nodes;
        private List<(Point, Point)> edges;
        private List<(Point, Point, Point)> triangles;

        public List<(Point, Point)> GetEdges() { return edges; }
        public List<Point> GetNodes() { return nodes; }
        public Triangulation()
        {
            nodes = new List<Point>();
            edges = new List<(Point, Point)>();
            triangles = new List<(Point, Point, Point)>();
        }

        public Triangulation (Polygon polygon)
        {
            nodes = polygon.GetPoints();
            edges = new List<(Point, Point)>();
            triangles = new List<(Point, Point, Point)>();
            CreateTriangulation();
        }

        public void DrawTriangulation(DrawingGroup drawingGroup)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Pen = new Pen(Brushes.MediumPurple, 0.01);

            for (int i = 0; i < edges.Count; i++)
            {
                LineGeometry lineFromPolygon = new LineGeometry(edges[i].Item1, edges[i].Item2);
                geometryGroup.Children.Add(lineFromPolygon);
            }

            geometryDrawing.Geometry = geometryGroup;
            drawingGroup.Children.Add(geometryDrawing);
        }

        private void CreateTriangulation()
        {
            if (nodes.Count == 3)
            {
                edges.Add((nodes[0], nodes[1]));
                edges.Add((nodes[1], nodes[2]));
                edges.Add((nodes[0], nodes[2]));
                return;
            }
            CreateBasicTriangulation();
            CreateLocalRearrangementOfTriangles();
        }

        private void CreateLocalRearrangementOfTriangles()
        { // Для каждого из двух треугольников, имеющих общее ребро проверяем,
          // уменьшится ли его длина при перестроении. Если да - перестраиваем
            for (int i = 0; i < triangles.Count - 1; i++)
            {
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    List<Point> tI = new List<Point>();
                    tI.Add(new Point(triangles[i].Item1.X, triangles[i].Item1.Y));
                    tI.Add(new Point(triangles[i].Item2.X, triangles[i].Item2.Y));
                    tI.Add(new Point(triangles[i].Item3.X, triangles[i].Item3.Y));

                    List<Point> tJ = new List<Point>();
                    tJ.Add(new Point(triangles[j].Item1.X, triangles[j].Item1.Y));
                    tJ.Add(new Point(triangles[j].Item2.X, triangles[j].Item2.Y));
                    tJ.Add(new Point(triangles[j].Item3.X, triangles[j].Item3.Y));

                    int indexI = -1;
                    int indexJ = -1;
                    bool isNotTwo = true;
                    for (int tIi = 0; tIi < 3; tIi++)
                    {
                        for (int tJj = 0; tJj < 3; tJj++)
                        {
                            if (tI[tIi].X == tJ[tJj].X && tI[tIi].Y == tJ[tJj].Y && indexI == -1 && isNotTwo)
                            { // Запоминаем первое совпадение точек
                                indexI = tIi;
                                indexJ = tJj;
                                isNotTwo = false;
                            }
                            else if (tI[tIi].X == tJ[tJj].X && tI[tIi].Y == tJ[tJj].Y && indexI > -1 &&
                                tIi != indexI && tJj != indexJ)
                            {
                                int changeI = 3 - indexI - tIi;
                                int changeJ = 3 - indexJ - tJj;
                                //MessageBox.Show(tI[tIi] + "; " + tI[indexI] + "; " + tI[changeI] + "; " + tJ[changeJ]);
                                if (IsTwoSidesIntersect(tI[changeI], tJ[changeJ], tI[indexI], tI[tIi]) &&

                                    Math.Sqrt((tI[changeI].X - tJ[changeJ].X) * (tI[changeI].X - tJ[changeJ].X) +
                                    (tI[changeI].Y - tJ[changeJ].Y) * (tI[changeI].Y - tJ[changeJ].Y)) <
                                    Math.Sqrt((tI[indexI].X - tI[tIi].X) * (tI[indexI].X - tI[tIi].X) +
                                    (tI[indexI].Y - tI[tIi].Y) * (tI[indexI].Y - tI[tIi].Y)))
                                {
                                    for (int k = 0; k < edges.Count; k++) // Удаляем старое ребро и добавляем новое
                                    {
                                        if (edges[k].Item1.X == tI[indexI].X && edges[k].Item1.Y == tI[indexI].Y &&
                                            edges[k].Item2.X == tI[tIi].X && edges[k].Item2.Y == tI[tIi].Y ||

                                            edges[k].Item2.X == tI[indexI].X && edges[k].Item2.Y == tI[indexI].Y &&
                                            edges[k].Item1.X == tI[tIi].X && edges[k].Item1.Y == tI[tIi].Y)
                                        {
                                            edges.RemoveAt(k);
                                            edges.Add((new Point(tI[changeI].X, tI[changeI].Y), new Point(tJ[changeJ].X, tJ[changeJ].Y)));
                                            break;
                                        }
                                    }

                                    triangles.RemoveAt(j);
                                    triangles.RemoveAt(i);
                                    triangles.Add((new Point(tI[indexI].X, tI[indexI].Y), new Point(tI[changeI].X, tI[changeI].Y), new Point(tJ[changeJ].X, tJ[changeJ].Y)));
                                    triangles.Add((new Point(tI[tIi].X, tI[tIi].Y), new Point(tI[changeI].X, tI[changeI].Y), new Point(tJ[changeJ].X, tJ[changeJ].Y)));
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CreateBasicTriangulation()
        {
            // Алгоритм
            // Берём ребро, находим ближайшую к нему не рассмотренную точку.
            // Если она входит в какую-нибудь окружность, описывающую треугольник из построенных -
            // удаляем ближайшее ребро этого треульника.
            // Из получившегося многоугольника из всех его узлов достраиваем рёбра до точки.
            // Берём ребро этой точки, находим ближайшую к нему не рассмотренную точку. И так далее.

            List<Point> nodesAdded = new List<Point>();
            List<Point> nodesNotAdded = new List<Point>();
            nodesNotAdded.AddRange(nodes);

            // Создаём первый треугольник из первой стороны и ближайшей к ней точке
            edges.Add((nodes[0], nodes[1]));
            int minI = 2;
            double minDistance = GetDistanceToSegment(nodes[2], nodes[0], nodes[1]);
            double newDistance;
            for (int i = 3; i < nodesNotAdded.Count(); i++)
            {
                newDistance = GetDistanceToSegment(nodes[i], nodes[0], nodes[1]);
                if (minDistance > newDistance)
                {
                    minI = i;
                    minDistance = newDistance;
                }
            }
            edges.Add((nodes[1], nodes[minI]));
            edges.Add((nodes[0], nodes[minI]));
            nodesNotAdded.RemoveAt(minI);
            nodesNotAdded.RemoveAt(1);
            nodesNotAdded.RemoveAt(0);
            triangles.Add((nodes[0], nodes[1], nodes[minI]));
            nodesAdded.Add(nodes[0]);
            nodesAdded.Add(nodes[1]);
            nodesAdded.Add(nodes[minI]);

            Point nowA, nowB;
            if (minI == 2) // Берём ребро, не являющееся ребром многоугольника
            { // (оптимизация, чтобы не перестраивать только что построенный треугольник)
                nowA = nodes[0];
                nowB = nodes[2];
            }
            else
            {
                nowA = nodes[1];
                nowB = nodes[minI];
            }

            while (nodesNotAdded.Count > 0)
            {
                // Находим точку, ближайшую к текущему ребру
                minI = 0;
                minDistance = GetDistanceToSegment(nodesNotAdded[minI], nowA, nowB);
                for (int i = 1; i < nodesNotAdded.Count(); i++)
                {
                    newDistance = GetDistanceToSegment(nodesNotAdded[i], nowA, nowB);
                    if (minDistance > newDistance)
                    {
                        minI = i;
                        minDistance = newDistance;
                    }
                }

                // Проходимся по треугольникам
                for (int i = 0; i < triangles.Count; i++)
                {
                    // Если описанная окружность треугольника содержит найденную точку
                    if (IsPointInsideCircumscribedCircle(nodesNotAdded[minI],
                        triangles[i].Item1, triangles[i].Item2, triangles[i].Item3))
                    {
                        // Находим дальнюю вершину и удаляем ребро напротив неё
                        string farthestNode = SearctNodeFarthestFromPoint(nodesNotAdded[minI],
                            triangles[i].Item1, triangles[i].Item2, triangles[i].Item3);
                        Point a, b;
                        if (farthestNode == "a")
                        {
                            a = triangles[i].Item2;
                            b = triangles[i].Item3;
                        }
                        if (farthestNode == "b")
                        {
                            a = triangles[i].Item1;
                            b = triangles[i].Item3;
                        }
                        if (farthestNode == "c")
                        {
                            a = triangles[i].Item1;
                            b = triangles[i].Item2;
                        }
                        for (int j = 0; j < edges.Count; j++) // Удаляем ближайшее к точке ребро
                        {
                            if (edges[j].Item1.X == a.X && edges[j].Item1.Y == a.Y &&
                                edges[j].Item2.X == b.X && edges[j].Item2.Y == b.Y ||
                                edges[j].Item2.X == a.X && edges[j].Item2.Y == a.Y &&
                                edges[j].Item1.X == b.X && edges[j].Item1.Y == b.Y)
                            {
                                edges.RemoveAt(j);
                                break;
                            }
                        }

                        // Добавляем 2 треугольника, образующихся из 3х рёбер
                        // от трёх вершин найденного треугольника
                        edges.Add((new Point(triangles[i].Item1.X, triangles[i].Item1.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                        edges.Add((new Point(triangles[i].Item2.X, triangles[i].Item2.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                        edges.Add((new Point(triangles[i].Item3.X, triangles[i].Item3.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));

                        triangles.Add((new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y), new Point(triangles[i].Item1.X, triangles[i].Item1.Y),
                            new Point(triangles[i].Item2.X, triangles[i].Item2.Y)));
                        triangles.Add((new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y), new Point(triangles[i].Item2.X, triangles[i].Item2.Y),
                            new Point(triangles[i].Item3.X, triangles[i].Item3.Y)));

                        triangles.RemoveAt(i);
                        MessageBox.Show("Содержит внутри!");
                    }
                }

                // Соединяем точку nodesNotAdded[minI] со всеми рёбрами триангуляции так,
                // чтобы при соединении не было пересечений с уже построенными рёбрами.
                // То есть соединяем точку со всеми "видимыми ей вершинами, не загороженными другими рёбрами"
                List<Point> nodesVisible = new List<Point>();
                for (int i = 0; i < nodesAdded.Count; i++) // Перебираем рёбра (nodesAdded[i], nodesNotAdded[minI])
                {
                    bool notIntersection = true;
                    for (int j = 0; j < edges.Count; j++) // Перебираем рёбра из текущей триангуляции
                    {
                        if (// Если nodesAdded[i] не является точкой ребра
                            !(nodesAdded[i].X == edges[j].Item1.X && nodesAdded[i].Y == edges[j].Item1.Y ||
                            nodesAdded[i].X == edges[j].Item2.X && nodesAdded[i].Y == edges[j].Item2.Y))
                        { // И если рёбра пересекаются
                            if (IsTwoSidesIntersect(nodesAdded[i], nodesNotAdded[minI],
                            edges[j].Item1, edges[j].Item2))
                            {
                                notIntersection = false;
                                break;
                            }
                        }
                    }
                    
                    if(notIntersection)
                    {
                        nodesVisible.Add(nodesAdded[i]);
                    }
                }

                // Соединяем текущую точку с видимыми ей точками
                // Точки в nodesVisible не отсортированы, поэтому нужно составлять из них треугольники на основе
                // двух, если они образуют ребро триангуляции
                for (int i = 0; i < nodesVisible.Count - 1; i++)
                {
                    for (int j = i; j < nodesVisible.Count; j++)
                    {
                        for (int k = 0; k < edges.Count; k++)
                        {
                            if (nodesVisible[i].X == edges[k].Item1.X && nodesVisible[i].Y == edges[k].Item1.Y &&
                                nodesVisible[j].X == edges[k].Item2.X && nodesVisible[j].Y == edges[k].Item2.Y ||

                                nodesVisible[i].X == edges[k].Item2.X && nodesVisible[i].Y == edges[k].Item2.Y &&
                                nodesVisible[j].X == edges[k].Item1.X && nodesVisible[j].Y == edges[k].Item1.Y)
                            {
                                edges.Add((new Point(nodesVisible[i].X, nodesVisible[i].Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                                edges.Add((new Point(nodesVisible[j].X, nodesVisible[j].Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                                triangles.Add((new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y),
                                    new Point(nodesVisible[i].X, nodesVisible[i].Y), new Point(nodesVisible[j].X, nodesVisible[j].Y)));
                            }
                        }
                    }
                }

                // Берём следующее ребро, берём последнее добавленное
                nowA = new Point(edges[edges.Count - 1].Item1.X, edges[edges.Count - 1].Item1.Y);
                nowB = new Point(edges[edges.Count - 1].Item2.X, edges[edges.Count - 1].Item2.Y);

                nodesAdded.Add(nodesNotAdded[minI]);
                nodesNotAdded.RemoveAt(minI);
            }

            // Удаляем повторяющееся
            for (int i = 0; i < edges.Count - 1; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                {
                    if(edges[i].Item1.X == edges[j].Item1.X && edges[i].Item1.Y == edges[j].Item1.Y &&
                       edges[i].Item2.X == edges[j].Item2.X && edges[i].Item2.Y == edges[j].Item2.Y ||

                       edges[i].Item1.X == edges[j].Item2.X && edges[i].Item1.Y == edges[j].Item2.Y &&
                       edges[i].Item2.X == edges[j].Item1.X && edges[i].Item2.Y == edges[j].Item1.Y)
                    {
                        edges.RemoveAt(j);
                    }
                }
            }

            for (int i = 0; i < triangles.Count - 1; i++)
            {
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    if(triangles[i].Item1.X == triangles[j].Item1.X && triangles[i].Item1.Y == triangles[j].Item1.Y &&
                       triangles[i].Item2.X == triangles[j].Item2.X && triangles[i].Item2.Y == triangles[j].Item2.Y &&
                       triangles[i].Item3.X == triangles[j].Item3.X && triangles[i].Item3.Y == triangles[j].Item3.Y)
                    {
                        triangles.RemoveAt(j);
                    }
                }
            }
        }

        private bool IsPointInsideCircumscribedCircle(Point x, Point a, Point b, Point c)
        {
            double R = RadiusOfCircumscribedCircle(a, b, c);
            Point O = CenterOfCircumscribedCircle(a, b, c);
            double Ox = Math.Sqrt((O.X - x.X) * (O.X - x.X) + (O.Y - x.Y) * (O.Y - x.Y));
            return Ox <= R;
        }

        private double RadiusOfCircumscribedCircle(Point a, Point b, Point c)
        {
            double ab = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
            double bc = Math.Sqrt((b.X - c.X) * (b.X - c.X) + (b.Y - c.Y) * (b.Y - c.Y));
            double ac = Math.Sqrt((a.X - c.X) * (a.X - c.X) + (a.Y - c.Y) * (a.Y - c.Y));
            double p = ab + bc + ac;
            return (ab * bc * ac) / (4 * Math.Sqrt(p * (p - ab) * (p - bc) * (p - ac)));
        }

        private Point CenterOfCircumscribedCircle(Point a, Point b, Point c)
        {
            Point res;
            double xab = a.X - b.X;
            double xbc = b.X - c.X;
            double xca = c.X - a.X;
            double yab = a.Y - b.Y;
            double ybc = b.Y - c.Y;
            double yca = c.Y - a.Y;
            double za = a.X * a.X + a.Y * a.Y;
            double zb = b.X * b.X + b.Y * b.Y;
            double zc = c.X * c.X + c.X * c.X;
            double zx = yab * zc + ybc * za + yca * zb;
            double zy = xab * zc + xbc * za + xca * zb;
            double z = xab * yca - yab * xca;
            res.X = -(zx / (2 * z));
            res.Y = zy / (2 * z);
            return res;
        }

        private static double GetDistanceToSegment(Point pt, Point a, Point b)
        { // Используемый метод описан здесь: https://ru.stackoverflow.com/questions/721414/Евклидова-геометрия-Расстояние-от-точки-до-отрезка
            double t = ((pt.X - a.X) * (b.X - a.X) + (pt.Y - a.Y) * (b.Y - a.Y)) /
                ((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

            if (t < 0)
                t = 0;
            if (t > 1)
                t = 1;

            // Используем формулу Хари:
            return Math.Sqrt((a.X - pt.X + (b.X - a.X) * t) * (a.X - pt.X + (b.X - a.X) * t) +
                             (a.Y - pt.Y + (b.Y - a.Y) * t) * (a.Y - pt.Y + (b.Y - a.Y) * t));
        }

        private string SearctNodeFarthestFromPoint(Point pt, Point a, Point b, Point c)
        {
            double apt = Math.Sqrt((a.X - pt.X) * (a.X - pt.X) + (a.Y - pt.Y) * (a.Y - pt.Y));
            double bpt = Math.Sqrt((b.X - pt.X) * (b.X - pt.X) + (b.Y - pt.Y) * (b.Y - pt.Y));
            double cpt = Math.Sqrt((c.X - pt.X) * (c.X - pt.X) + (c.Y - pt.Y) * (c.Y - pt.Y));
            if (apt >= bpt)
                return apt >= cpt ? "a" : "c";
            else
                return bpt >= cpt ? "b" : "c";
        }

        private bool IsTwoSidesIntersect(Point a0, Point a1, Point b0, Point b1) // true if a0a1 and b0b1 is intersect
        {
            //Подробнее: http://espressocode.top/check-if-two-given-line-segments-intersect/

            // Находим четыре ориентации, необходимые для общего и особого случаев
            int o1 = Orientation(a0, a1, b0);
            int o2 = Orientation(a0, a1, b1);
            int o3 = Orientation(b0, b1, a0);
            int o4 = Orientation(b0, b1, a1);

            // Общий случай
            if (o1 != o2 && o3 != o4)
                return true;


            // Особые случаи
            // a0, a1 и b0 коллинеарны, а b0 лежит на отрезке a0a1
            if (o1 == 0 && IsOnSegment(a0, b0, a1))
                return true;

            // a0, a1 и b1 коллинеарны, а b1 лежит на отрезке a0a1
            if (o2 == 0 && IsOnSegment(a0, b1, a1))
                return true;

            // b0, b1 и a0 коллинеарны, а a0 лежит на отрезке b0b1
            if (o3 == 0 && IsOnSegment(b0, a0, b1))
                return true;

            // b0, b1 и a1 коллинеарны, а a1 лежит на отрезке b0b1
            if (o4 == 0 && IsOnSegment(b0, a1, b1))
                return true;

            return false; // Не попадает ни в один из вышеперечисленных случаев
        }

        private int Orientation(Point a, Point b, Point c)
        {
            // Найти ориентацию упорядоченного триплета (a, b, c).
            // Функция возвращает следующие значения
            // 0 -> a, b и c являются коллинеарными
            // 1 -> по часовой стрелке
            // -1 -> против часовой стрелки

            // Подробнее о формуле ниже: Https://www.geeksforgeeks.org/orientation-3-ordered-points/amp/

            double val = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);

            if (val == 0) // коллинеар
                return 0;

            return (val > 0) ? 1 : -1; // по часовой или против часовой стрелки
        }

        private bool IsOnSegment(Point a, Point b, Point c)
        {
            // Учитывая три коллинеарных точки a, b, c, функция проверяет, точка b лежит на отрезке прямой ac
            if (b.X <= Max(a.X, c.X) && b.X >= Min(a.X, c.X) && b.Y <= Max(a.Y, c.Y) && b.Y >= Min(a.Y, c.Y))
                return true;

            return false;
        }

        private double Max(double x, double y)
        {
            return (x >= y) ? x : y;
        }

        private double Min(double x, double y)
        {
            return (x >= y) ? y : x;
        }


        private bool IsIntersectionOfTwoLineSegments(Point ptA0, Point ptA1, Point ptB0, Point ptB1)
        {
            Point p1 = ptA0;
            Point p2 = ptA1;
            Point p3 = ptB0;
            Point p4 = ptB1;
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
    }
}