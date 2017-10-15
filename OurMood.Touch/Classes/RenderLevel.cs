using System;
using System.Collections.Generic;

namespace OurMood.Touch
{
	public class RenderLevel
	{
		public float DayImageWidth {
			get;
			set;
		}

		public float DayImageHeight {
			get;
			set;
		}

		public float DayMarkerWidth {
			get;
			set;
		}

		public float TotalDayWidth {
			get {
				return DayImageWidth + DayMarkerWidth;
			}
		}

		public int Index {
			get;
			set;
		}

		public static RenderLevel RenderLevel1 = new RenderLevel(){
			Index = 1,
			DayImageWidth = 973f, 
			DayImageHeight = 444f,
			DayMarkerWidth = 2
		};

		public static IEnumerable<RenderLevel> RenderLevels {
			get {
				return new List<RenderLevel> (){
				RenderLevel.RenderLevel1
			};
			}
		}
	}
}

