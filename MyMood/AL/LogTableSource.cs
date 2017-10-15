using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MyMood.DL;
using System.Linq;


namespace MyMood
{
	public class LogTableSource : UITableViewSource
	{
		
		protected string _cellId = "LogCell";
		

		public IList<Log> Logs
		{
			get { return this._logs; }
			set { this._logs = value; }
		}
		protected IList<Log> _logs = new List<Log>();

		public LogTableSource(): base()
		{
			this._logs = Log.All().OrderByDescending(l => l.TimeStamp).ToList();
		}

		public LogTableSource (IList<Log> logs) : base ()
		{
			this._logs = logs;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
			
			if(cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
			}
			
			var log = this._logs[indexPath.Row];
			var detail = log.Detail;

			
			cell.TextLabel.Text = string.Format("{0} :: {1}", log.TimeStamp.ToLocalTime().ToString("dd MMM yyyy H:mm:ss"), log.Message);
			cell.DetailTextLabel.Text = detail;

			

			return cell;
		}

		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var log = _logs[indexPath.Row];
			var popup = new UIAlertView(string.Format("{0} :: {1}", log.TimeStamp.ToLocalTime().ToString("dd MMM yyyy H:mm:ss"), log.Message)
			                              , log.Detail, null, "OK", null);
			popup.Show();

		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return this._logs.Count;
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

