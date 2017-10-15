using System;
using Vici.CoolStorage;
using System.Linq;
using System.Collections.Generic;

namespace MyMood.DL
{
	[MapTo("MoodCategory")]
	public class MoodCategory : CSObject<MoodCategory,string>
	{
			
		public string Id {
			get { return (string)GetField ("Id"); }
			set{ SetField ("Id", value); }
		}

		[OneToMany(LocalKey="Id", ForeignKey="MoodCategoryId")]
		public CSList<Mood> Moods {
			get {
				return (CSList<Mood>)GetField ("Moods");
			} 
		}
			
		public string Name {
			get { return (string)GetField("Name"); }
			set{ SetField("Name", value); }
		}
			
		public static string DefaultCategoryName = "Default";
		//should be 0000-00000-0000 etc
		public static string DefaultCategoryId = new System.Guid().ToString();	
	}
}

