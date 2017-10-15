using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.CoreGraphics;
using System.Collections.Generic;
using System.Linq;

namespace Discover.Drawing
{
	public static class GraphicExtensions
	{


		public static void AddSpline (this CGContext g, PointF[] points, float tension)
		{
			List<float> pts = new List<float>();
			foreach (var p in points) {
				pts.Add(p.X);
				pts.Add(p.Y);
			}
			DrawSplineFromPoints(g, pts, tension);
		}

		private static void DrawSplineFromPoints(CGContext ctx,List<float> pts,float t){

			var cp= new List<float>();   // array of control points, as x0,y0,x1,y1,...
			var n=pts.Count();
			// Draw an open curve, not connected at the ends
			for(var i=0;i<n-4;i+=2){
				cp=cp.Concat(GetControlPointsForSpline(pts[i],pts[i+1],pts[i+2],pts[i+3],pts[i+4],pts[i+5],t)).ToList();
			}    
			ctx.MoveTo(pts[0],pts[1]);
			ctx.AddQuadCurveToPoint(cp[0],cp[1],pts[2],pts[3]);

			for(var i=2;i<n-5;i+=2){
				ctx.AddCurveToPoint(cp[2*i-2],cp[2*i-1],cp[2*i],cp[2*i+1],pts[i+2],pts[i+3]);
			}

			ctx.AddQuadCurveToPoint(cp[2*n-10],cp[2*n-9],pts[n-2],pts[n-1]);
		}

		private static List<float> GetControlPointsForSpline (float x0, float y0, float x1, float y1, float x2, float y2, float t)
		{
			//  x0,y0,x1,y1 are the coordinates of the end (knot) pts of this segment
			//  x2,y2 is the next knot -- not connected here but needed to calculate p2
			//  p1 is the control point calculated here, from x1 back toward x0.
			//  p2 is the next control point, calculated here and returned to become the 
			//  next segment's p1.
			//  t is the 'tension' which controls how far the control points spread.
			
			//  Scaling factors: distances from this knot to the previous and following knots.
			var d01 = Math.Sqrt (Math.Pow (x1 - x0, 2) + Math.Pow (y1 - y0, 2));
			var d12 = Math.Sqrt (Math.Pow (x2 - x1, 2) + Math.Pow (y2 - y1, 2));
			
			var fa = t * d01 / (d01 + d12);
			var fb = t - fa;
			
			var p1x = (float)(x1 + fa * (x0 - x2));
			var p1y = (float)(y1 + fa * (y0 - y2));
			
			var p2x = (float)(x1 - fb * (x0 - x2));
			var p2y = (float)(y1 - fb * (y0 - y2));  
			
			return new List<float> (){ p1x,p1y,p2x,p2y};
		}

		public static void AddSpline2 (this CGContext g, PointF[] points, double tension)
		{
			List<PointF> pointsIterator = points.ToList ();
			
			PointF v0;
			PointF vector0;
			if (pointsIterator.Count () < 3)
				return;
			
			PointF v1 = pointsIterator [0];
			PointF pom = pointsIterator [1];
			
			g.MoveTo (v1.X, v1.Y);
			
			PointF vector1 = new PointF ((int)Math.Round (pom.X)
				- (int)Math.Round (v1.X), (int)Math.Round (pom.Y)
				- (int)Math.Round (v1.Y));
			
			PointF P;
			PointF Q;
			PointF R;
			PointF S;
			
			int index = 2;
			while (index < pointsIterator.Count()) {
				v0 = v1;
				vector0 = vector1;
				v1 = pom;
				pom = pointsIterator [index];
				vector1 = new PointF (pom.X - v0.X, pom.Y - v0.Y);
				
				P = v0;
				Q = new PointF ((float)(v0.X + vector0.X * (1 - tension)), (float)(v0.Y
					+ vector0.Y * (1 - tension)));
				R = new PointF ((float)(v1.X - vector1.X * (1 - tension)), (float)(v1.Y
					- vector1.Y * (1 - tension)));
				S = v1;
				
				drawBezierArch (P, Q, R, S, g);
				index++;
			}
			
			P = v1;
			Q = new PointF ((float)(v1.X + vector1.X * (1 - tension)), (float)(v1.Y
				+ vector1.Y * (1 - tension)));
			R = new PointF ((float)((v1.X - pom.X) * (1 - tension) + pom.X), (float)((v1
			                                                                         .Y - pom.Y)
				* (1 - tension) + pom.Y));
			S = pom;
			
			drawBezierArch (P, Q, R, S, g);

		}

		public static void DrawSpline (this CGContext g, PointF[] points, float tension)
		{
			g.AddSpline (points, tension);
			
			g.StrokePath ();
		}

		private static double archSize (PointF P, PointF Q, PointF R, PointF S)
		{
			return Math.Sqrt ((P.X - S.X) * (P.X - S.X)
				+ (P.Y - S.Y) * (P.Y - S.Y));
		}
		
		private static void drawBezierArch (PointF P, PointF Q, PointF R, PointF S, CGContext g)
		{
			if (archSize (P, Q, R, S) <= 1) {
				//g.FillRect (new Rectangle ((int)Math.Round (P.X), (int)Math.Round (P.Y),1, 1));
				g.AddLineToPoint (P.X, P.Y);
			} else {
				PointF PQ = getCenter (P, Q);
				PointF QR = getCenter (Q, R);
				PointF RS = getCenter (R, S);
				
				PointF PQR = getCenter (PQ, QR);
				PointF QRS = getCenter (QR, RS);

				PointF PQRS = getCenter (PQR, QRS);
				
				drawBezierArch (P, PQ, PQR, PQRS, g);
				drawBezierArch (PQRS, QRS, RS, S, g);
			}
		}

		private static PointF getCenter (PointF p, PointF q)
		{
			return new PointF (p.X + ((q.X - p.X) / 2), p.Y + ((q.Y - p.Y) / 2));
		}
	}
	


}

