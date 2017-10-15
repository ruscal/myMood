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

		public static void DrawSpline (this CGContext g, PointF[] points, double tension)
		{
			List<PointF> pointsIterator = points.ToList();
			
			PointF v0;
			PointF vector0;
			if(pointsIterator.Count() < 3) return;
			
			PointF v1 = pointsIterator[0];
			PointF pom = pointsIterator[1];
			
			g.MoveTo(v1.X, v1.Y);
			
			PointF vector1 = new PointF((int) Math.Round(pom.X)
			                            - (int) Math.Round(v1.X), (int) Math.Round(pom.Y)
			                            - (int) Math.Round(v1.Y));
			
			PointF P;
			PointF Q;
			PointF R;
			PointF S;

			int index = 2;
			while (index < pointsIterator.Count()) {
				v0 = v1;
				vector0 = vector1;
				v1 = pom;
				pom = pointsIterator[index];
				vector1 = new PointF(pom.X - v0.X, pom.Y - v0.Y);
				
				P = v0;
				Q = new PointF((float)(v0.X + vector0.X * (1 - tension)), (float)(v0.Y
				                                                                  + vector0.Y * (1 - tension)));
				R = new PointF((float)(v1.X - vector1.X * (1 - tension)), (float)(v1.Y
				                                                                  - vector1.Y * (1 - tension)));
				S = v1;

				drawBezierArch(P, Q, R, S, g);
				index++;
			}
			
			P = v1;
			Q = new PointF((float)(v1.X + vector1.X * (1 - tension)), (float)(v1.Y
			                                                                  + vector1.Y * (1 - tension)));
			R = new PointF((float)((v1.X - pom.X) * (1 - tension) + pom.X), (float)((v1
			                                                                   .Y - pom.Y)
			                                                                        * (1 - tension) + pom.Y));
			S = pom;

			drawBezierArch(P, Q, R, S, g);
			
			g.StrokePath();
		}

		private static double archSize(PointF P, PointF Q, PointF R, PointF S) {
			return Math.Sqrt((P.X - S.X) * (P.X - S.X)
			                 + (P.Y - S.Y) * (P.Y - S.Y));
		}
		
		private static void drawBezierArch (PointF P, PointF Q, PointF R, PointF S, CGContext g)
		{
			if (archSize (P, Q, R, S) <= 1) {
				//g.FillRect (new Rectangle ((int)Math.Round (P.X), (int)Math.Round (P.Y),1, 1));
				g.AddLineToPoint (P.X, P.Y);
			}
			else {
				PointF PQ = getCenter(P, Q);
				PointF QR = getCenter(Q, R);
				PointF RS = getCenter(R, S);
				
				PointF PQR = getCenter(PQ, QR);
				PointF QRS = getCenter(QR, RS);

				PointF PQRS = getCenter(PQR, QRS);
				
				drawBezierArch(P, PQ, PQR, PQRS, g);
				drawBezierArch(PQRS, QRS, RS, S, g);
			}
		}

		private static PointF getCenter (PointF p, PointF q)
		{
			return new PointF(p.X + ((q.X - p.X)/2), p.Y + ((q.Y - p.Y)/2));
		}
	}

	
	public class Spline {
		
		private PointF[] points;
		
		public Spline(PointF[] points) {
			this.points = points;
		}

		
		/**
	 * 
	 * @param P
	 * @param Q
	 * @param R
	 * @param S
	 * @return distance between P and S
	 */
		double archSize(PointF P, PointF Q, PointF R, PointF S) {
			return Math.Sqrt((P.X - S.X) * (P.X - S.X)
			                 + (P.Y - S.Y) * (P.Y - S.Y));
		}
		
		void drawBezierArch (PointF P, PointF Q, PointF R, PointF S, CGContext g)
		{
			if (archSize (P, Q, R, S) <= 1) {
				//g.FillRect (new Rectangle ((int)Math.Round (P.X), (int)Math.Round (P.Y),1, 1));
				g.AddLineToPoint (P.X, P.Y);
			}
			else {
				PointF PQ = getCenter(P, Q);
				PointF QR = getCenter(Q, R);
				PointF RS = getCenter(R, S);
				
				PointF PQR = getCenter(PQ, QR);
				PointF QRS = getCenter(QR, RS);
				
				PointF PQRS = getCenter(PQR, QRS);
				
				drawBezierArch(P, PQ, PQR, PQRS, g);
				drawBezierArch(PQRS, QRS, RS, S, g);
			}
		}

		PointF getCenter (PointF p, PointF q)
		{
			return new PointF(p.X + ((q.X - p.X)/2), p.Y + ((q.Y - p.Y)/2));
		}
//		
//		public void drawPoints(Graphics g, int radius, Color color) {
//			Iterator<Point> pointsIterator = this.points.getList().iterator();
//			while (pointsIterator.hasNext()) {
//				pointsIterator.next().draw(g, radius, color);
//			}
//		}
		
		/**
	 * draws lines between points
	 * 
	 * @param g
	 * @param color
	 */
//		void drawSkelet(Graphics g, Color color) {
//			Iterator<Point> pointsIterator = this.points.getList().iterator();
//			Point pointFrom;
//			if (!pointsIterator.hasNext())
//				return;
//			Point pointTo = pointsIterator.next();
//			g.setColor(color);
//			while (pointsIterator.hasNext()) {
//				pointFrom = pointTo;
//				pointTo = pointsIterator.next();
//				g.drawLine((int) Math.round(pointFrom.X), (int) Math
//				           .round(pointFrom.Y), (int) Math.round(pointTo.X),
//				           (int) Math.round(pointTo.Y));
//				
//			}
//			g.setColor(Color.black);
//			
//		}

		public void drawCardinalSpline(CGContext g, double a, bool drawVectors,
		                               bool drawSpline) {

			List<PointF> pointsIterator = this.points.ToList();

			PointF v0;
			PointF vector0;
			if(pointsIterator.Count() < 3) return;

			PointF v1 = pointsIterator[0];
			PointF pom = pointsIterator[1];

			g.MoveTo(v1.X, v1.Y);

			PointF vector1 = new PointF((int) Math.Round(pom.X)
			                          - (int) Math.Round(v1.X), (int) Math.Round(pom.Y)
			                          - (int) Math.Round(v1.Y));
			
			PointF P;
			PointF Q;
			PointF R;
			PointF S;
			
			// Color red = new Color(154, 40, 30);
			int index = 2;
			while (index < pointsIterator.Count()) {
				v0 = v1;
				vector0 = vector1;
				v1 = pom;
				pom = pointsIterator[index];
				vector1 = new PointF(pom.X - v0.X, pom.Y - v0.Y);
				
				P = v0;
				Q = new PointF((float)(v0.X + vector0.X * (1 - a)), (float)(v0.Y
				              + vector0.Y * (1 - a)));
				R = new PointF((float)(v1.X - vector1.X * (1 - a)), (float)(v1.Y
				              - vector1.Y * (1 - a)));
				S = v1;

				if (drawVectors) {
					g.MoveTo(P.X, P.Y);
					g.AddLineToPoint(Q.X, Q.Y);
					g.MoveTo(R.X, R.Y);
					g.AddLineToPoint(S.X, S.Y);

//					g.drawLine((int) Math.round(P.X), (int) Math.round(P
//					                                                        .Y), (int) Math.round(Q.X), (int) Math
//					           .round(Q.Y));
//					g.drawLine((int) Math.round(R.X), (int) Math.round(R
//					                                                        .Y), (int) Math.round(S.X), (int) Math
//					           .round(S.Y));
//					R.select();
//					R.draw(g, 3, vectorColor);
//					Q.select();
//					Q.draw(g, 3, vectorColor);
//					g.setColor(Color.black);
				}
				if (drawSpline)
					drawBezierArch(P, Q, R, S, g);
				index++;
			}
			
			P = v1;
			Q = new PointF((float)(v1.X + vector1.X * (1 - a)), (float)(v1.Y
			              + vector1.Y * (1 - a)));
			R = new PointF((float)((v1.X - pom.X) * (1 - a) + pom.X), (float)((v1
			                                                                .Y - pom.Y)
			              * (1 - a) + pom.Y));
			S = pom;
			
//			if (drawVectors) {
//				g.setColor(vectorColor);
//				g.drawLine((int) Math.round(P.X), (int) Math.round(P.Y),
//				           (int) Math.round(Q.X), (int) Math.round(Q.Y));
//				g.drawLine((int) Math.round(R.X), (int) Math.round(R.Y),
//				           (int) Math.round(S.X), (int) Math.round(S.Y));
//				R.select();
//				R.draw(g, 3, vectorColor);
//				Q.select();
//				Q.draw(g, 3, vectorColor);
//				g.setColor(Color.black);
//			}
			if (drawSpline)
				drawBezierArch(P, Q, R, S, g);

			g.StrokePath();
		}
		
//		public void setDefaultSpline(Dimension d) {
//			this.addPoint(new Point(d.getWidth() * 2 / 15,
//			                        d.getHeight() * 13 / 15 - 30));
//			this.addPoint(new Point(d.getWidth() * 6 / 15,
//			                        d.getHeight() * 12 / 15 - 30));
//			this.addPoint(new Point(d.getWidth() * 1 / 2,
//			                        d.getHeight() * 4 / 15 - 30));
//			this.addPoint(new Point(d.getWidth() * 9 / 15,
//			                        d.getHeight() * 12 / 15 - 30));
//			this.addPoint(new Point(d.getWidth() * 13 / 15,
//			                        d.getHeight() * 13 / 15 - 30));
//		}
		
		/**
	 * 
	 * @param click
	 * @return two closest points(with index i and i+1) to specified point(click) in an array
	 */
//		public Point[] getClosestPoints(Point click) {
//			Point[] closestPoints = new Point[2];
//			closestPoints[0] = null;
//			closestPoints[1] = null;
//			Iterator<Point> pointsIterator = this.points.getList().iterator();
//			if (pointsIterator.hasNext())
//				closestPoints[0] = pointsIterator.next();
//			if (pointsIterator.hasNext())
//				closestPoints[1] = pointsIterator.next();
//			double distance = Point.getDistance(click, closestPoints[0]) + Point.getDistance(click, closestPoints[1]);
//			Point p1 = closestPoints[1];
//			Point p2;
//			while (pointsIterator.hasNext()) {
//				p2 = pointsIterator.next();
//				if (Point.getDistance(click, p1) + Point.getDistance(
//					click, p2) < distance) {
//					closestPoints[0] = p1;
//					closestPoints[1] = p2;
//					distance = Point.getDistance(click, closestPoints[0]) + Point.getDistance(click, closestPoints[1]);
//				}
//				p1 = p2;
//			}
//			return closestPoints;
//		}
		
	}

}

