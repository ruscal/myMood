using System;

using MyMood.DL;

namespace MyMood
{
	public class MyMoodSetEventArgs: EventArgs
	{
		public MoodResponse MoodResponse { get; set; }
		
		public MyMoodSetEventArgs (MoodResponse moodResponse) : base ()
		{
			this.MoodResponse = moodResponse;
		}
	}


	public class MoodPromptEventArgs: EventArgs
	{
		public MoodPrompt MoodPrompt { get; set; }
		
		public MoodPromptEventArgs (MoodPrompt prompt) : base ()
		{
			this.MoodPrompt = prompt;
		}
	}
}

