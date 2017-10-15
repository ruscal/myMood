using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MyMood.DL;

namespace MyMood.AL
{
	public class MoodResponseTableSource : UITableViewSource
		{
			
			protected string _cellId = "MoodResponseCell";
			
			
			public IList<MoodResponse> Responses
			{
			get { return this._responses; }
			set { this._responses = value; }
			}
		protected IList<MoodResponse> _responses = new List<MoodResponse>();
			
		public MoodResponseTableSource (IList<MoodResponse> tasks) : base ()
			{
			this._responses = tasks;
			}
			
			public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
				
				if(cell == null)
				{
					cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
				}

			var response = this._responses[indexPath.Row];
			var detail = response.Mood.Name;
			if(response.Prompt != null)
				detail += " (" + response.Prompt.NotificationText + ")";
				
			cell.TextLabel.Text = response.TimeStamp.ToLocalTime().ToString();
			cell.DetailTextLabel.Text = detail;
				
				return cell;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
			return this._responses.Count;
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}
			

			
			public override bool CanEditRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				return false;
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.Delete;
			}

			
			
		}
}

