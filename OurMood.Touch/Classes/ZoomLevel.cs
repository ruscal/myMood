using System;
using System.Collections.Generic;
using System.Linq;

namespace OurMood.Touch
{
	public class ZoomLevel
	{
		public int ZoomIndex {
			get;
			set;
		}
		
		public string Name {
			get;
			set;
		}

		public float? MaxHoursPerWindow {
			get;
			set;
		}

		public float? MinHoursPerWindow {
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


		public RenderLevel RenderLevel {
			get;
			set;
		}

		public bool ShrinkLabels {
			get;
			set;
		}

		public static IEnumerable<ZoomLevel> ZoomLevels = new List<ZoomLevel>(){
			new ZoomLevel(){
				Name = "Event Overview",
				ZoomIndex = 1,
				MaxHoursPerWindow = null,
				MinHoursPerWindow = 12.5f,
				IntervalMins = 240,
				OffsetMins = 120,
				RenderLevel = RenderLevel.RenderLevel1,
				ShrinkLabels = true
			},
			new ZoomLevel(){
				Name = "Day View",
				ZoomIndex = 2,
				MaxHoursPerWindow = 12.5f,
				MinHoursPerWindow = 12,
				IntervalMins = 60,
				OffsetMins = 30,
				RenderLevel = RenderLevel.RenderLevel1,
				ShrinkLabels = true
			}
//			new ZoomLevel(){
//				Name = "Hour View",
//				ZoomIndex = 3,
//				MaxHoursPerWindow = 2,
//				MinHoursPerWindow = 1,
//				IntervalMins = 10,
//				OffsetMins = 10,
//				RenderLevel = RenderLevel.RenderLevel1,
//				ShrinkLabels = false
//			}
		};

		public static ZoomLevel MaximumZoomLevel {
			get {
				return ZoomLevels.Where(z => z.MinHoursPerWindow == null).FirstOrDefault() 
					?? ZoomLevels.OrderBy(z => z.MinHoursPerWindow.Value).FirstOrDefault();
			}
		}

		public static ZoomLevel MinimumZoomLevel {
			get {
				return ZoomLevels.Where(z => z.MaxHoursPerWindow == null).FirstOrDefault() 
					?? ZoomLevels.OrderBy(z => z.MaxHoursPerWindow.Value).FirstOrDefault();
			}
		}
	}
}

