using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class MoodCategoryModel
	{
		public string Id {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public IEnumerable<MoodModel> Moods {
			get;
			set;
		}
	}
}

