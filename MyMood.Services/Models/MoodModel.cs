using System;
using MyMood.DL;

namespace MyMood.Services
{
	public class MoodModel
	{
		public string Id {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public int DisplayIndex {
			get;
			set;
		}

		public string DisplayColor {
			get;
			set;
		}

		public MoodType MoodType {
			get;
			set;
		}
	}
}

