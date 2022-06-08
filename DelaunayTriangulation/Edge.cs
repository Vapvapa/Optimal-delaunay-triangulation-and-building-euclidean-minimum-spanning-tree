using System.Windows;

namespace DelaunayTriangulation
{
    internal class Edge
    {
        public int v1Index, v2Index;
        public Point v1Point, v2Point;
        public double distance;

        public Edge(int v1Index, int v2Index, Point v1Point, Point v2Point, double distance)
        {
            this.v1Index = v1Index;
            this.v2Index = v2Index;
            this.v1Point = v1Point;
            this.v2Point = v2Point;
            this.distance = distance;
        }
    }
}
