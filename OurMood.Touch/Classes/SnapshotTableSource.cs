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
	public class SnapshotTableSource : UITableViewSource
	{
		protected string _cellId = "SnapshotCell";
		
		
		public IList<Snapshot> Snapshots
		{
			get { return this._snapshots; }
			set { this._snapshots = value; }
		}
		protected IList<Snapshot> _snapshots = new List<Snapshot>();
		
		public SnapshotTableSource(): base()
		{
			var report = MoodReport.CurrentReport;
			this._snapshots = report.Snapshots.OrderByDescending(l => l.TimeOfSnapshot).ToList();
		}
		
		public SnapshotTableSource (IList<Snapshot> activities) : base ()
		{
			this._snapshots = activities;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
			
			if(cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
			}
			
			var a = this._snapshots[indexPath.Row];
			var detail = GetDetail(a);
			
			cell.TextLabel.Text = GetTitle(a);
			cell.DetailTextLabel.Text = detail;
			
			return cell;
		}
		
		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var a = _snapshots[indexPath.Row];
			new UIAlertView(GetTitle(a), GetDetail(a), null, "OK", null).Show();
		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return this._snapshots.Count;
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
		
		protected string GetTitle(Snapshot Snapshot){
			return string.Format("{0} :: {1}", Snapshot.TimeOfSnapshotLocal.ToString("dd MMM yyyy H:mm:ss"), Snapshot.TotalResponses);
		}
		
		protected string GetDetail(Snapshot snapshot){
			JObject promptJson = JObject.FromObject(new MoodSnapshotReportModel(){
				t = snapshot.TimeOfSnapshotLocal,
				r = snapshot.TotalResponses,
				d = snapshot.Moods.Select(m => new MoodSnapshotDataModel(){
					i = m.Mood.DisplayIndex,
					c = m.ResponseCount,
					p = m.ResponsePercentage
				})
			});
			return promptJson.ToString();
		}
	}
}

