using System;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MyMood.DL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyMood.Services;
using System.Linq;

namespace MyMood
{
	public class MoodPromptTableSource : UITableViewSource
	{
		
		protected string _cellId = "MoodPromptCell";
		
		
		public IList<MoodPrompt> MoodPrompts
		{
			get { return this._MoodPrompts; }
			set { this._MoodPrompts = value; }
		}
		protected IList<MoodPrompt> _MoodPrompts = new List<MoodPrompt>();

		public MoodPromptTableSource(): base()
		{
			this._MoodPrompts = MoodPrompt.All().OrderByDescending(l => l.ActiveFrom).ToList();
		}
		
		public MoodPromptTableSource (IList<MoodPrompt> MoodPrompts) : base ()
		{
			this._MoodPrompts = MoodPrompts;
		}
		
		public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell(this._cellId);
			
			if(cell == null)
			{
				cell = new UITableViewCell(UITableViewCellStyle.Value1, this._cellId);
			}
			
			var prompt = this._MoodPrompts[indexPath.Row];
			var detail = GetDetail(prompt);

			cell.TextLabel.Text = GetTitle(prompt);
			cell.DetailTextLabel.Text = detail;

			return cell;
		}
		
		public override void RowSelected (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
		{
			var prompt = _MoodPrompts[indexPath.Row];
			new UIAlertView(GetTitle(prompt), GetDetail(prompt), null, "OK", null).Show();
		}
		
		public override int RowsInSection (UITableView tableview, int section)
		{
			return this._MoodPrompts.Count;
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

		protected string GetTitle(MoodPrompt prompt){
			return string.Format("{0} :: {1}", prompt.TimeStamp.ToLocalTime().ToString("dd MMM yyyy H:mm:ss"), prompt.Title);
		}

		protected string GetDetail(MoodPrompt prompt){
			JObject promptJson = JObject.FromObject(new MoodPromptModel(){
				Id = prompt.Id,
				Name = prompt.Name,
				NotificationText = prompt.NotificationText,
				ActiveFrom = prompt.ActiveFrom,
				ActiveTil = prompt.ActiveTil,
				Activity = new ActivityModel(){
					Title = prompt.Title,
					TimeStamp = prompt.TimeStamp
				}
			});
			return promptJson.ToString();
		}
	}
}

