using System;
using System.Collections.Generic;

namespace MyMood.Services
{
	public class UpdateAppFromServiceModel
	{
		public bool SyncSuccess {
			get;
			set;
		}

		public DateTime? SyncTimestamp {
			get;
			set;
		}

		public bool HasPromptUpdates {
			get;
			set;
		}

		//server has different total of responses
		public bool ResError {
			get;
			set;
		}

		public DateTime UpdatedOn {
			get;
			set;
		}

		public ApplicationStateModel Application {
			get;
			set;
		}

		public IEnumerable<MoodCategoryModel> Categories{
			get;
			set;
		}

		public IEnumerable<MoodPromptModel> Prompts{
			get;
			set;
		}
	}
}

