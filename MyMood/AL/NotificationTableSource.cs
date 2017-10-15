using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MyMood.DL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyMood.Services;
using System.Linq;
using System.Collections.Generic;
using MonoTouch.Foundation;
using Discover.Core;

namespace MyMood
{
	public class NotificationTableSource : UITableViewSource
	{
		
		protected string _cellId = "NotificationCell";
		
		
		public IList<UILocalNotification> UILocalNotifications
		{
			get { return this._UILocalNotifications; }
			set { this._UILocalNotifications = value; }
		}
		protected IList<UILocalNotification> _UILocalNotifications = new List<UILocalNotification>();

		public NotificationTableSource () : base ()
		{
			var notifications = UIApplication.SharedApplication.ScheduledLocalNotifications;
			_UILocalNotifications = notifications.ToList();
		}
		
		public NotificationTableSource (IList<UILocalNotification> UILocalNotifications) : base ()
		{
			this._UILocalNotifications = UILocalNotifications;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
			
			if(cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
			}
			
			var prompt = this._UILocalNotifications[indexPath.Row];
			var detail = GetDetail(prompt);
			
			cell.TextLabel.Text = GetTitle(prompt);
			cell.DetailTextLabel.Text = detail;
			
			return cell;
		}
		
		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var prompt = _UILocalNotifications[indexPath.Row];
			new UIAlertView(GetTitle(prompt), GetDetail(prompt), null, "OK", null).Show();
		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return this._UILocalNotifications.Count;
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
		
		protected string GetTitle(UILocalNotification prompt){
			return string.Format("{0} :: {1}", prompt.FireDate.ToDateTime().ToString("dd MMM yyyy H:mm:ss"), prompt.AlertBody);
		}
		
		protected string GetDetail(UILocalNotification prompt){
			JObject promptJson = JObject.FromObject(prompt);
			return promptJson.ToString();
		}
	}
}

