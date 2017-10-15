using System;
using MyMood.DL;

namespace MyMood
{
	public class JumpToPromptEventArgs: EventArgs
	{
		public MoodPrompt Prompt { get; set; }
		
		public JumpToPromptEventArgs (MoodPrompt prompt) : base ()
		{
			this.Prompt = prompt;
		}
	}
}

