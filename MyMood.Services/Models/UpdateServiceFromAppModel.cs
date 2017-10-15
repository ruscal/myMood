using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class UpdateServiceFromAppModel : RequestModelBase
	{
		public IEnumerable<MoodResponseUpdateModel> Responses {
			get;
			set;
		}

		public DateTime? LastUpdate {
			get;
			set;
		}

		public int ResTotal {
			get;
			set;
		}

	}
}

