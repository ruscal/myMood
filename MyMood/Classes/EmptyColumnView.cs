using System;

namespace MyMood
{
	public class PlaceholderColumnView : TimelineColumnView
	{
		public PlaceholderColumnView (int columnIndex)
			:base(columnIndex)
		{
			this.backgroundImage.Image = Resources.EmptyNode;
		}
	}
}

