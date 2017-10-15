using System;
using MyMood.DL;

namespace MyMood
{
	public class AddNewResponseEventArgs: EventArgs
	{
		public MoodPrompt CurrentPrompt { get; set; }
		
		public AddNewResponseEventArgs (MoodPrompt currentPrompt) : base ()
		{
			this.CurrentPrompt = currentPrompt;
		}
	}
}

