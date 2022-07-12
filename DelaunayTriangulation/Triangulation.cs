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
        { // For each of the two triangles having a common edge, we check,
          // whether its length will decrease when rebuilding. If yes, we are rebuilding
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
                            { // Memorizing the first match of points
                                indexI = tIi;
                                indexJ = tJj;
                                isNotTwo = false;
                            }
                            else if (tI[tIi].X == tJ[tJj].X && tI[tIi].Y == tJ[tJj].Y && indexI > -1 &&
                                tIi != indexI && tJj != indexJ)
                            {
                                int changeI = 3 - indexI - tIi;
                                int changeJ = 3 - indexJ - tJj;
                                if (IsTwoSidesIntersect(tI[changeI], tJ[changeJ], tI[indexI], tI[tIi]) &&

                                    Math.Sqrt((tI[changeI].X - tJ[changeJ].X) * (tI[changeI].X - tJ[changeJ].X) +
                                    (tI[changeI].Y - tJ[changeJ].Y) * (tI[changeI].Y - tJ[changeJ].Y)) <
                                    Math.Sqrt((tI[indexI].X - tI[tIi].X) * (tI[indexI].X - tI[tIi].X) +
                                    (tI[indexI].Y - tI[tIi].Y) * (tI[indexI].Y - tI[tIi].Y)))
                                {
                                    for (int k = 0; k < edges.Count; k++) // Removing the old edge and adding a new one
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
            // Algorithm
            // We take an edge, find the nearest point not considered to it.
            // If it is included in some circle describing a triangle of those that are constructed
            // we remove the nearest edge of this triangle.
            // From the resulting polygon, from all its nodes, we complete the edges to a point.
            // We take the edge of this point, find the nearest point not considered to it. Etc.

            List<Point> nodesAdded = new List<Point>();
            List<Point> nodesNotAdded = new List<Point>();
            nodesNotAdded.AddRange(nodes);

            // Creating the first triangle from the first side and the point closest to it
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
            if (minI == 2) // Taking an edge that is not an edge of the polygon
            { // (optimization so as not to rebuild the newly constructed triangle)
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
                // Find the point closest to the current edge
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

                for (int i = 0; i < triangles.Count; i++)
                {
                    // If the circumscribed circle of the triangle contains the found point
                    if (IsPointInsideCircumscribedCircle(nodesNotAdded[minI],
                        triangles[i].Item1, triangles[i].Item2, triangles[i].Item3))
                    {
                        // Find the far vertex and remove the edge opposite it
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
                        for (int j = 0; j < edges.Count; j++) // Removing the edge closest to the point
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

                        // Add 2 triangles formed from 3 edges
                        // from the three vertices of the found triangle
                        edges.Add((new Point(triangles[i].Item1.X, triangles[i].Item1.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                        edges.Add((new Point(triangles[i].Item2.X, triangles[i].Item2.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));
                        edges.Add((new Point(triangles[i].Item3.X, triangles[i].Item3.Y), new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y)));

                        triangles.Add((new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y), new Point(triangles[i].Item1.X, triangles[i].Item1.Y),
                            new Point(triangles[i].Item2.X, triangles[i].Item2.Y)));
                        triangles.Add((new Point(nodesNotAdded[minI].X, nodesNotAdded[minI].Y), new Point(triangles[i].Item2.X, triangles[i].Item2.Y),
                            new Point(triangles[i].Item3.X, triangles[i].Item3.Y)));

                        triangles.RemoveAt(i);
                    }
                }

                // We connect the nodesNotAdded[minI] point with all the edges of the triangulation like this,
                // so that when connecting there are no intersections with already constructed edges.
                // That is, we connect the point with all "vertices visible to it, not blocked by other edges"
                List<Point> nodesVisible = new List<Point>();
                for (int i = 0; i < nodesAdded.Count; i++) // Edges (nodesAdded[i], nodesNotAdded[minI])
                {
                    bool notIntersection = true;
                    for (int j = 0; j < edges.Count; j++) // Iterating over the edges from the current triangulation
                    {
                        if (// If nodesAdded[i] is not an edge point
                            !(nodesAdded[i].X == edges[j].Item1.X && nodesAdded[i].Y == edges[j].Item1.Y ||
                            nodesAdded[i].X == edges[j].Item2.X && nodesAdded[i].Y == edges[j].Item2.Y))
                        { // And if the edges intersect
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

                // Connecting the current point with the points visible to it
                // Points in nodesVisible they are not sorted, so you need to make triangles out of them based on
                // two if they form an edge of the triangulation
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

                // Take the next edge, take the last one added
                nowA = new Point(edges[edges.Count - 1].Item1.X, edges[edges.Count - 1].Item1.Y);
                nowB = new Point(edges[edges.Count - 1].Item2.X, edges[edges.Count - 1].Item2.Y);

                nodesAdded.Add(nodesNotAdded[minI]);
                nodesNotAdded.RemoveAt(minI);
            }

            // Removing duplicate
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
        { // The method used is described here: https://ru.stackoverflow.com/questions/721414/Евклидова-геометрия-Расстояние-от-точки-до-отрезка
            double t = ((pt.X - a.X) * (b.X - a.X) + (pt.Y - a.Y) * (b.Y - a.Y)) /
                ((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));

            if (t < 0)
                t = 0;
            if (t > 1)
                t = 1;

            // Use the Hari formula:
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
            // More detailed: http://espressocode.top/check-if-two-given-line-segments-intersect/

            // Find four orientations necessary for general and special cases
            int o1 = Orientation(a0, a1, b0);
            int o2 = Orientation(a0, a1, b1);
            int o3 = Orientation(b0, b1, a0);
            int o4 = Orientation(b0, b1, a1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;


            // Special cases
            // a0, a1 and b0 are collinear, and b0 lies on the segment a0a1
            if (o1 == 0 && IsOnSegment(a0, b0, a1))
                return true;

            // a0, a1 are b1 are collinear, and b1 lies on the segment a0a1
            if (o2 == 0 && IsOnSegment(a0, b1, a1))
                return true;

            // b0, b1 are a0 are collinear, and a0 lies on the segment b0b1
            if (o3 == 0 && IsOnSegment(b0, a0, b1))
                return true;

            // b0, b1 are a1 are collinear, and a1 lies on the segment b0b1
            if (o4 == 0 && IsOnSegment(b0, a1, b1))
                return true;

            return false; // Does not fall into any of the above cases
        }

        private int Orientation(Point a, Point b, Point c)
        {
            // Find the orientation of an ordered triplet (a, b, c).
            // The function returns the following values
            // 0 -> a, b and c are collinear
            // 1 -> clockwise
            // -1 -> counterclockwise

            // Read more about the formula below: Https://www.geeksforgeeks.org/orientation-3-ordered-points/amp/
            double val = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);

            if (val == 0) // collinear
                return 0;

            return (val > 0) ? 1 : -1; // clockwise or counterclockwise
        }

        private bool IsOnSegment(Point a, Point b, Point c)
        {
            // Given three collinear points a, b, c, the function checks if point b lies on a line segment ac
            if (b.X <= Math.Max(a.X, c.X) && b.X >= Math.Min(a.X, c.X) && b.Y <= Math.Max(a.Y, c.Y) && b.Y >= Math.Min(a.Y, c.Y))
                return true;

            return false;
        }
    }
}