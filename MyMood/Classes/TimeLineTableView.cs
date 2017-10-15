using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MyMood.DL;
using MyMood.AL;
using MyMood.Services;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using MonoTouch.ObjCRuntime;
using Discover.Core;

namespace MyMood
{
	[Register ("TimeLineTableView")]
	public class TimeLineTableView : HorizontalTableView, IPageLoopViewDelegate
	{
		public event EventHandler<AddNewResponseEventArgs> AddNewMood;

		public DateTime StartDate {
			get;
			set;
		}

		protected List<TimelineColumnView> timelineColumns;

		public TimeLineTableView ()
			:base()
		{
			this.initTimeline ();
		}

		public TimeLineTableView (IntPtr ptr):base(ptr)
		{
			this.initTimeline ();
		}

		protected void initTimeline ()
		{
			this.tableViewDelegate = this;
			this.GenerateTimelineColumns ();

		}

		public void Refresh ()
		{
			Console.WriteLine ("Refreshing Timeline");
			GenerateTimelineColumns ();
			this.clearViews ();
			this.refreshData ();


		}

		public void SnapToEnd ()
		{
			this.SnapToPage (this.timelineColumns.Count (), true);
		}

		public void SnapToPrompt (MoodPrompt prompt)
		{
			if (this.timelineColumns == null)
				Refresh ();
			var promptCol = this.timelineColumns.OfType<MoodPromptColumnView> ().FirstOrDefault (c => c.Prompt.Id == prompt.Id);
			if (promptCol != null) {
				// translate index so prompt appears in center if poss
				var pageIndex = promptCol.ColumnIndex - 3;
				this.SnapToPage (pageIndex > 0 ? pageIndex : 0, true);
			}
		}

		private void GenerateTimelineColumns ()
		{
			Console.WriteLine ("Generating timeline");


			//Console.WriteLine("Get current prompt");

			var currentPrompt = MoodPrompt.GetCurrentPrompt ();

			//Console.WriteLine("Get unprompted responses");

			var responses = MoodResponse.OrderedList ("TimeStamp", "TimeStamp <= @Now", new{Now = DateTime.UtcNow}).ToList ();

			//Console.WriteLine("Got responses");

			timelineColumns = new List<TimelineColumnView> ();
			int index = 0;

			StartDate = responses.Any () 
				? responses.First ().TimeStamp.ToLocalTime (ApplicationState.Current.EventTimeOffset).Date 
					: DateTime.UtcNow.ToLocalTime (ApplicationState.Current.EventTimeOffset).Date;
			var date = StartDate;

			timelineColumns.Add (new DayMarkerColumnView (index, StartDate, 1));
			index++;


			foreach (var i in responses) {
				//Console.WriteLine("Add column - " + i.TimeStamp.ToString());
				if (i.TimeStamp.Date > date) {
					date = i.TimeStamp.Date;
					var dayIndex = date.Subtract (StartDate).Days + 1;
					timelineColumns.Add (new DayMarkerColumnView (index, date, dayIndex));
				}
				if (i.Mood == null) {
					if (i.Prompt != null) {
						var respondToPromptCol = new MoodPromptColumnView (index, i.Prompt);
						respondToPromptCol.AddNewMood += (object sender, AddNewResponseEventArgs e) => {
							if (this.AddNewMood != null) {
								AddNewMood (sender, e);
							}
						};
						timelineColumns.Add (respondToPromptCol);
					}
				} else {
					timelineColumns.Add (new MoodResponseColumnView (index, i));
				}
				index++;
			}

			var addNewMoodCol = new AddResponseColumnView (index, currentPrompt == null ? null : currentPrompt);
			addNewMoodCol.AddNewMood += (object sender, AddNewResponseEventArgs e) => {
				if (this.AddNewMood != null) {
					AddNewMood (sender, e);
				}
			};
			timelineColumns.Add (addNewMoodCol);

			Console.WriteLine ("Timline columns done");

		
		}


		#region "implement IPageLoopViewDelegate"

		//Total number of columns in the scrollview
		public int numberOfColumnsForTableView (HorizontalTableView tableView)
		{
			return this.timelineColumns.Count;
		}
		
		//set up and display each view from a generic nib ColumnView
		//each control is accessed from its tag property
		//back ground image tag = 10
		//button tag = 96
		public UIView tableViewIndex (HorizontalTableView aTableView, int index)
		{
			var vw = this.timelineColumns [index];
			this.Add (vw);
			return vw;
		}

		// Width of each column in the scrollview
		public float columnWidthForTableView (HorizontalTableView tableView)
		{
			return 140.0f;			
		}

		#endregion
	}

	public class TimelineReponseItem
	{
		public MoodPrompt Prompt {
			get;
			set;
		}
						
		public MoodResponse Response {
			get;
			set;
		}

		public DateTime TimeStamp {
			get;
			set;
		}

	}
}

