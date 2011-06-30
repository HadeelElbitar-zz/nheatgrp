using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;


namespace VeditorGP
{
    class GrahamHull
    {
        List<Vector2F> Upper;
        List<Vector2F> Lower;
        List<Vector2F> convexHull;
        public void Compute(List<Vector2F> points, ref List<Vector2F> UpperPoints, ref List<Vector2F> LowerPoints)
        {
            Upper = new List<Vector2F>();
            Lower = new List<Vector2F>();
            convexHull = new List<Vector2F>();
            Vector2F maxX, minX;
            maxX = new Vector2F();
            minX = new Vector2F();
            Sort(points, ref maxX, ref minX);
            for (int i = 0; i < points.Count; i++)
                if (determinate(minX, points[i], maxX) >= 0)
                    Upper.Add(points[i]);
            for (int i = points.Count - 1; i >= 0; i--)
                if (determinate(minX, points[i], maxX) < 0)
                    Lower.Add(points[i]);
            convexHull.AddRange(Upper);
            convexHull.AddRange(Lower);
            UpperPoints = Upper;
            LowerPoints = Lower;
           // return convexHull;
        }
        public List<Vector2F> Compute(List<Vector2F> points)
        {
            Upper = new List<Vector2F>();
            Lower = new List<Vector2F>();
            convexHull = new List<Vector2F>();
            Vector2F maxX, minX;
            maxX = new Vector2F();
            minX = new Vector2F();
            Sort(points, ref maxX, ref minX);
            for (int i = 0; i < points.Count; i++)
                if (determinate(minX, points[i], maxX) >= 0)
                    Upper.Add(points[i]);
            for (int i = points.Count-1; i>=0; i--)
                if (determinate(minX, points[i], maxX) <= 0)
                    Lower.Add(points[i]);
            convexHull.AddRange(Upper);
            convexHull.AddRange(Lower);
            return convexHull;
        }
        private double determinate(Vector2F p1, Vector2F p2, Vector2F p3)
        {
            double det = ((p1.X * p2.Y) + (p2.X * p3.Y) + (p3.X * p1.Y)) - ((p2.X * p1.Y) + (p1.X * p3.Y) + (p3.X * p2.Y));
            return det;
        }
        private void Sort(List<Vector2F> points, ref Vector2F maxX, ref Vector2F minX)
        {
            maxX.X = points[0].X;
            maxX.Y = points[0].Y;
            minX.X = points[0].X;
            minX.Y = points[0].Y;
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count - 1; j++)
                {
                    if (points[j].X > points[j + 1].X)
                    {
                        Vector2F temp = points[j];
                        points[j] = points[j + 1];
                        points[j + 1] = temp;
                    }
                    else if (points[j].X == points[j + 1].X)
                    {
                        if (points[j].Y > points[j + 1].Y)
                        {
                            Vector2F temp = points[j];
                            points[j] = points[j + 1];
                            points[j + 1] = temp;
                        }
                    }
                }
            }
            for (int i = 0; i < points.Count; i++)
            {
                if (maxX.X < points[i].X)
                    maxX = points[i];
                if (minX.X > points[i].X)
                    minX = points[i];
            }
        }
    }
}
