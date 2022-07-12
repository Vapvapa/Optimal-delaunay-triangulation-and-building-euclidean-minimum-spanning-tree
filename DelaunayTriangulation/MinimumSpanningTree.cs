using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DelaunayTriangulation
{
    internal class MinimumSpanningTree
    {
        private List<Point> triangleNodes;
        private List<Edge> treeEdges;
        private List<Edge> treeMinimumEdges;

        public MinimumSpanningTree()
        {
            triangleNodes = new List<Point>();
            treeEdges = new List<Edge>();
            treeMinimumEdges = new List<Edge>();
        }
        public MinimumSpanningTree(List<(Point, Point)> triangleEdges, List<Point> triangleNodes)
        {
            this.triangleNodes = triangleNodes;

            treeMinimumEdges = new List<Edge>();
            treeEdges = new List<Edge>();

            for (int i = 0; i < triangleEdges.Count; i++)
            {
                treeEdges.Add(new Edge(SearchIndexPoint(triangleEdges[i].Item1), SearchIndexPoint(triangleEdges[i].Item2),
                    triangleEdges[i].Item1, triangleEdges[i].Item2,
                    Math.Sqrt((triangleEdges[i].Item1.X - triangleEdges[i].Item2.X) * (triangleEdges[i].Item1.X - triangleEdges[i].Item2.X) +
                              (triangleEdges[i].Item1.Y - triangleEdges[i].Item2.Y) * (triangleEdges[i].Item1.Y - triangleEdges[i].Item2.Y))));
            }

            AlgorithmByPrim(triangleNodes.Count, treeEdges, treeMinimumEdges);
        }

        private int SearchIndexPoint(Point point)
        {
            for (int i = 0; i < triangleNodes.Count; i++)
            {
                if (point.X == triangleNodes[i].X && point.Y == triangleNodes[i].Y)
                {
                    return i;
                }
            }
            return -1;
        }

        public void AlgorithmByPrim(int numberNodes, List<Edge> treeEdges, List<Edge> treeMinimumEdges)
        {
            List<Edge> notUsedEdges = new List<Edge>(treeEdges);
            List<int> usedNodes = new List<int>();
            List<int> notUsedNodes = new List<int>();

            for (int i = 0; i < numberNodes; i++)
                notUsedNodes.Add(i);

            // Selecting a random starting vertex
            Random random = new Random();
            usedNodes.Add(random.Next(0, numberNodes));
            notUsedNodes.RemoveAt(usedNodes[0]);

            while (notUsedNodes.Count > 0)
            {
                int minE = -1; // The number of the smallest edge

                for (int i = 0; i < notUsedEdges.Count; i++) // Finding the smallest edge
                {
                    if ((usedNodes.IndexOf(notUsedEdges[i].v1Index) != -1) && (notUsedNodes.IndexOf(notUsedEdges[i].v2Index) != -1) ||
                        (usedNodes.IndexOf(notUsedEdges[i].v2Index) != -1) && (notUsedNodes.IndexOf(notUsedEdges[i].v1Index) != -1))
                    {
                        if (minE != -1)
                        {
                            if (notUsedEdges[i].distance < notUsedEdges[minE].distance)
                                minE = i;
                        }
                        else
                            minE = i;
                    }
                }

                // Adding a new vertex to the used list and removing it from the unused list
                if (usedNodes.IndexOf(notUsedEdges[minE].v1Index) != -1)
                {
                    usedNodes.Add(notUsedEdges[minE].v2Index);
                    notUsedNodes.Remove(notUsedEdges[minE].v2Index);
                }
                else
                {
                    usedNodes.Add(notUsedEdges[minE].v1Index);
                    notUsedNodes.Remove(notUsedEdges[minE].v1Index);
                }

                // Adding a new edge to the tree and removing it from the unused list
                treeMinimumEdges.Add(notUsedEdges[minE]);
                notUsedEdges.RemoveAt(minE);
            }
        }

        public void DrawMinimumSpanningTree(DrawingGroup drawingGroup)
        {
            GeometryDrawing geometryDrawing = new GeometryDrawing();
            GeometryGroup geometryGroup = new GeometryGroup();

            geometryDrawing.Pen = new Pen(Brushes.Purple, 0.01);

            for (int i = 0; i < treeMinimumEdges.Count; i++)
            {
                LineGeometry lineFromPolygon = new LineGeometry(treeMinimumEdges[i].v1Point, treeMinimumEdges[i].v2Point);
                geometryGroup.Children.Add(lineFromPolygon);
            }

            geometryDrawing.Geometry = geometryGroup;
            drawingGroup.Children.Add(geometryDrawing);
        }
    }
}
