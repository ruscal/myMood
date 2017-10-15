using System;
using Vici.CoolStorage;

namespace MyMood.DL
{
	[MapTo("Mood")]
	public class Mood : CSObject<Mood,string>
	{

		public string Id {
			get { return (string)GetField("Id"); }
			set{ SetField("Id", value); }
		}

		[ManyToOne(LocalKey="MoodCategoryId", ForeignKey="Id")]
		public MoodCategory Category {
			get{ return (MoodCategory) GetField("Category");} set{ SetField("Category", value);}
		}

		public string Name {
			get { return (string)GetField("Name"); }
			set{ SetField("Name", value); }
		}

		public int DisplayIndex {
			get { return (int)GetField("DisplayIndex"); }
			set{ SetField("DisplayIndex", value); }
		}

		public string DisplayColor {
			get { return (string)GetField("DisplayColor"); }
			set{ SetField("DisplayColor", value); }
		}

		public MoodType MoodType {
			get { 
				var val = GetField("MoodType");
				return (MoodType)Enum.Parse(typeof(MoodType), val.ToString()); }
			set{ SetField("MoodType", value); }
		}

		public static Mood GetMoodByName(string name){
			return Mood.ReadFirst("Name = @name", new{ Name = name });
		}
	}
}

