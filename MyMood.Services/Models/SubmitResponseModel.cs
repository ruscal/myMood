using System;

namespace MyMood.Services
{
	public class SubmitResponseModel : RequestModelBase
	{
		public MoodResponseUpdateModel r {
			get;
			set;
		}
	}
}

