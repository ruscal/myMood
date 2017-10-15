using System;

namespace MyMood.Services
{
	public class GetServiceUpdatesModel : RequestModelBase
	{
		public DateTime? LastUpdate {
			get;
			set;
		}
	}
}

