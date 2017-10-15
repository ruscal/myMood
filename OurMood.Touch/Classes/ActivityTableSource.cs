using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MyMood.DL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyMood.Services;
using System.Linq;

namespace OurMood.Touch
{
	public class ActivityTableSource : UITableViewSource
	{
		
		protected string _cellId = "ActivityCell";
		

		public IList<Activity> Activities
		{
			get { return this._activities; }
			set { this._activities = value; }
		}
		protected IList<Activity> _activities = new List<Activity>();

		public ActivityTableSource(): base()
		{
			this._activities = Activity.All().OrderByDescending(l => l.TimeStamp).ToList();
		}
		
		public ActivityTableSource (IList<Activity> activities) : base ()
		{
			this._activities = activities;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
			
			if(cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
			}

			var a = this._activities[indexPath.Row];
			var detail = GetDetail(a);

			cell.TextLabel.Text = GetTitle(a);
			cell.DetailTextLabel.Text = detail;

			return cell;
		}
		
		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var a = _activities[indexPath.Row];
			new UIAlertView(GetTitle(a), GetDetail(a), null, "OK", null).Show();
		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return this._activities.Count;
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

		protected string GetTitle(Activity activity){
			return string.Format("{0} :: {1} :: {2}", activity.TimeStampLocal.ToString("dd MMM yyyy H:mm:ss"), activity.ActivityType.ToString(), activity.Title);
		}

		protected string GetDetail(Activity activity){
			JObject promptJson = JObject.FromObject(new ActivityModel(){
				Id = activity.Id,
				Title = activity.Title,
				TimeStamp = activity.TimeStamp,

			});
			return promptJson.ToString();
		}
	}
}

