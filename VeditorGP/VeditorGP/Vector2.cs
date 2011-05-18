using System;

namespace VeditorGP
{
	/// <summary>
	/// Summary description for Vector2F.
	/// </summary>
	public class Vector2F
	{
		public float X;
		public float Y;

		public Vector2F()
		{
			X = Y = 0.0f;
		}
		public Vector2F(float xx, float yy)
		{
			X = xx;
			Y = yy;
		}
		public static Vector2F operator +(Vector2F v1, Vector2F v2)
		{
			return new Vector2F(v1.X + v2.X, v1.Y + v2.Y);
		}
		public static Vector2F operator -(Vector2F v1, Vector2F v2)
		{
			return new Vector2F(v1.X - v2.X, v1.Y - v2.Y);
		}
		public static Vector2F operator *(Vector2F v, float s)
		{
			return new Vector2F(s * v.X, s * v.Y);
		}
		public static Vector2F operator *(float s, Vector2F v)
		{
			return new Vector2F(s * v.X, s * v.Y);
		}

		public void Scale(float s)
		{
			X = s * X;
			Y = s * Y;
		}
		public static float DotProduct(ref Vector2F v1, ref Vector2F v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y;
		}
		public float MagnitudeSquare
		{
			get { return X * X + Y * Y; }
		}
		public float Magnitude
		{
			get { return (float)Math.Sqrt(MagnitudeSquare); }
		}
		public Vector2F NormalizedVetor
		{
			get
			{
				float m = Magnitude;
				return new Vector2F(X / m, Y / m);
			}
		}
		public void Normalize()
		{
			float m = Magnitude;
			X = X / m;
			Y = Y / m;
		}
		public void Set(float xx, float yy)
		{
			X = xx;
			Y = yy;
		}
		public Vector2F Clone()
		{
			return new Vector2F(X, Y);
		}

	}
}
