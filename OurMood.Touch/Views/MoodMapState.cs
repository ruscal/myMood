using System;

namespace OurMood.Touch
{
	public class MoodMapState
	{
		public int ZoomIndex {
			get;
			set;
		}
		
		public string ViewName {
			get;
			set;
		}
		
		public int HoursPerWindow {
			get;
			set;
		}
		
		public int IntervalMins {
			get;
			set;
		}
		
		public int OffsetMins {
			get;
			set;
		}

		public float DayMarkerWidth {
			get;
			set;
		}
	}
}

