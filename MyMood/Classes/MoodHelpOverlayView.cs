using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

namespace MyMood
{
//	public class MoodHelpOverlayView :NSObject
//	{
//		private UIPageControl helpPageControl;
//		public UIScrollView overlayHelpScrollView;
//		private UIViewController _parentView;
//		private UIView _helpPage1;
//		private UIView _helpPage2;
//		private UIView _helpPage3;
//		private UIView _helpPage4;
//		private UIView _helpPage5;
//		private UIImageView overlayImage;
//		UIButton closeButton1;
//		private Object[] _pageViews;
//
//		public int _currentPageIndex;
//
//		public MoodHelpOverlayView (UIViewController parentView)
//		{
//			_parentView = parentView;
//			_currentPageIndex = 0;
//
//			overlayHelpScrollView = new UIScrollView(parentView.View.Frame);
//			helpPageControl = new UIPageControl(parentView.View.Frame);
//			overlayHelpScrollView.PagingEnabled = true;
//			overlayHelpScrollView.Delegate = new ScrollViewHelpDelegate(this);
//			overlayHelpScrollView.AutosizesSubviews = true;
//			overlayHelpScrollView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
//			overlayHelpScrollView.ShowsHorizontalScrollIndicator = false;
//			overlayHelpScrollView.ShowsVerticalScrollIndicator = false;
//			overlayHelpScrollView.AlwaysBounceVertical = false;
//			
//			overlayHelpScrollView.AddSubview(helpPageControl);
//			
//			closeButton1 = new UIButton();
//			closeButton1.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Normal);
//			closeButton1.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Selected);			
//			closeButton1.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton1.Frame = new RectangleF(30,25,78,32);
//			closeButton1.Alpha = 0;
//			
//			UIButton closeButton2 = new UIButton();
//			closeButton2.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Normal);
//			closeButton2.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Selected);			
//			closeButton2.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton2.Frame = new RectangleF(30,25,78,32);
//			closeButton2.Alpha = 1;
//			
//			UIButton closeButton3 = new UIButton();
//			closeButton3.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Normal);
//			closeButton3.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Selected);			
//			closeButton3.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton3.Frame = new RectangleF(30,25,78,32);
//			closeButton3.Alpha = 1;
//			
//			UIButton closeButton4 = new UIButton();
//			closeButton4.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Normal);
//			closeButton4.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Selected);			
//			closeButton4.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton4.Frame = new RectangleF(30,25,78,32);
//			closeButton4.Alpha = 1;
//			
//			UIButton closeButton5 = new UIButton();
//			closeButton5.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Normal);
//			closeButton5.SetBackgroundImage(Resources.CloseIntroButton,UIControlState.Selected);			
//			closeButton5.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton5.Frame = new RectangleF(30,25,78,32);
//			closeButton5.Alpha = 1;
//			
//			_helpPage1 = new UIView(parentView.View.Frame);
//			UIImageView iv1  = new UIImageView(parentView.View.Frame);
//			iv1.Image = Resources.TimelineHelp1;
//			_helpPage1.AddSubview(iv1);
//			this._helpPage1.AddSubview(closeButton1);
//			UIButton whoaButton1 = new UIButton();
//			whoaButton1.TouchUpInside += (object sender, EventArgs e) => {
//				whoa(Resources.WhoaTimelineHelp1);
//			};
//			
//			whoaButton1.Frame = new RectangleF(153,370,250,102);
//			whoaButton1.Alpha = 1;
//			this._helpPage1.AddSubview(whoaButton1);
//			
//			
//			
//			_helpPage2 = new UIView(parentView.View.Frame);
//			UIImageView iv2  = new UIImageView(parentView.View.Frame);
//			iv2.Image = Resources.TimelineHelp2;
//			_helpPage2.AddSubview(iv2);
//			this._helpPage2.AddSubview(closeButton2);
//			UIButton whoaButton2 = new UIButton();
//			whoaButton2.TouchUpInside += (object sender, EventArgs e) => {
//				whoa(Resources.WhoaTimelineHelp2);
//			};
//			
//			whoaButton2.Frame = new RectangleF(707,370,121,115);
//			whoaButton2.Alpha = 1;
//			this._helpPage2.AddSubview(whoaButton2);
//			
//			
//			
//			_helpPage3 = new UIView(parentView.View.Frame);
//			UIImageView iv3  = new UIImageView(parentView.View.Frame);
//			iv3.Image = Resources.TimelineHelp3;
//			_helpPage3.AddSubview(iv3);
//			this._helpPage3.AddSubview(closeButton3);
//			
//			UIButton whoaButton3 = new UIButton();
//			whoaButton3.TouchUpInside += (object sender, EventArgs e) => {
//				whoa(Resources.WhoaTimelineHelp3);
//			};
//			
//			whoaButton3.Frame = new RectangleF(438,362,247,102);
//			whoaButton3.Alpha = 1;
//			this._helpPage3.AddSubview(whoaButton3);
//			
//			
//			_helpPage4 = new UIView(parentView.View.Frame);
//			UIImageView iv4  = new UIImageView(parentView.View.Frame);
//			iv4.Image = Resources.TimelineHelp4;
//			_helpPage4.AddSubview(iv4);
//			this._helpPage4.AddSubview(closeButton4);
//			
//			UIButton whoaButton4 = new UIButton();
//			whoaButton4.TouchUpInside += (object sender, EventArgs e) => {
//				whoa(Resources.WhoaTimelineHelp4);
//			};
//			
//			whoaButton4.Frame = new RectangleF(340,695,343,54);
//			whoaButton4.Alpha = 1;
//			this._helpPage4.AddSubview(whoaButton4);
//			
//			_helpPage5 = new UIView(parentView.View.Frame);
//			UIImageView iv5  = new UIImageView(parentView.View.Frame);
//			iv5.Image = Resources.TimelineHelp5;
//			_helpPage5.AddSubview(iv5);
//			this._helpPage5.AddSubview(closeButton5);
//			
//			UIButton closeButton5a = new UIButton();
//			closeButton5a.TouchUpInside += (object sender, EventArgs e) => {
//				dismissHelpScreen();
//			};
//			closeButton5a.Frame = new RectangleF(898,595,126,157);
//			closeButton5a.Alpha = 1;
//			this._helpPage5.AddSubview(closeButton5a);
//			
//			
//			overlayHelpScrollView.Alpha = 0;
//			this._pageViews = new object[]{_helpPage1,_helpPage2,_helpPage3,_helpPage4,_helpPage5};
//			
//			//overlayHelpScrollView.CanCancelContentTouches = false;
//			//overlayHelpScrollView.DelaysContentTouches = false;
//			SizeF pageSize = this.pageSize();
//			this.overlayHelpScrollView.ContentSize = new SizeF(this.numberOfPages() * pageSize.Width,pageSize.Height);
//			for (int pageIndex = 0; pageIndex < this.numberOfPages();pageIndex++)
//			{
//				if(this.isPageLoaded(pageIndex)) this.layoutPage(pageIndex);
//			}
//
//			parentView.View.AddSubview(overlayHelpScrollView);
//			
//
//
//		}
//
//		public void show()
//		{
//			UIView.Animate(0.5,()=>{overlayHelpScrollView.Alpha = 1;closeButton1.Alpha = 1;});
//
//		}
//
//
//		private void whoa(UIImage whoaImage)
//		{
//			UITapGestureRecognizer singleTap = new UITapGestureRecognizer(this,new MonoTouch.ObjCRuntime.Selector("dismissWhoa"));
//			overlayImage = new UIImageView();
//			overlayImage.Image = whoaImage;
//			overlayImage.Bounds = _parentView.View.Bounds;
//			overlayImage.Frame = _parentView.View.Frame;
//			overlayImage.Alpha = 0;
//			//overlayImage.CanBecomeFirstResponder = true;
//			overlayImage.UserInteractionEnabled = true;
//			overlayImage.AddGestureRecognizer(singleTap);
//			_parentView.View.AddSubview(overlayImage);
//			UIView.Animate(0.5,()=>{overlayImage.Alpha = 1;});
//			
//		}
//
//		[Export("dismissWhoa")]
//		private void dismissWhoa()
//		{
//			UIView.Animate(0.5,0,UIViewAnimationOptions.TransitionNone,()=>{
//				overlayImage.Alpha=0;},
//			() =>{
//				overlayImage.RemoveFromSuperview();
//			}
//			);
//		}
//
//		private void dismissHelpScreen()
//		{
//			UIView.Animate(0.5,0,UIViewAnimationOptions.TransitionNone,()=>{
//				overlayHelpScrollView.Alpha=0;},
//			() =>{
//				overlayHelpScrollView.RemoveFromSuperview();
//			}
//			);
//		}
//
//
//		// Help scrollview page code
//		
//		private void UpdatePageControl(UIPageControl cont, int current, int pages, UIView showed)
//		{
//			cont.CurrentPage = current;
//			cont.Pages = pages;
//			cont.UpdateCurrentPageDisplay();
//			UIPageControl.AnimationsEnabled = true;
//			cont.Frame = new RectangleF(showed.Frame.Location.X,cont.Frame.Location.Y,pageSize().Width,cont.Frame.Height);
//			UIPageControl.CommitAnimations();			
//		}
//		
//		
//		private UIView loadViewForPage(int pageindex)
//		{
//			UIView _view = null;
//			switch(pageindex)
//			{
//			case 1:
//				_view = this._helpPage1;
//				break;
//			case 2:
//				_view = this._helpPage2;
//				break;
//			case 3:
//				_view = this._helpPage3;
//				break;
//			case 4:
//				_view = this._helpPage4;
//				break;
//			case 5:
//				_view = this._helpPage5;
//				break;
//			}
//			return _view;
//		}
//		
//		private void layoutPage(int pageIndex)
//		{
//			SizeF pageSize = this.pageSize();
//			((UIView)this._pageViews[pageIndex]).Frame = new RectangleF(pageIndex * pageSize.Width,0,pageSize.Width,pageSize.Height);
//			this.viewForPage(pageIndex);
//		}
//		
//		private int numberOfPages()
//		{
//			return (int)this._pageViews.Count();
//		}
//		
//		private UIView viewForPage(int pageindex)
//		{
//			UIView pageView;
//			if(_pageViews.ElementAt(pageindex)==null)
//			{
//				pageView = loadViewForPage(pageindex);
//				_pageViews[pageindex]=pageView;
//			}
//			else
//			{
//				pageView = (UIView)_pageViews[pageindex];
//			}
//			overlayHelpScrollView.AddSubview(pageView);
//			return pageView;
//		}
//		
//		
//		public SizeF pageSize()
//		{
//			return overlayHelpScrollView.Frame.Size;
//		}
//		
//		private bool isPageLoaded(int pageindex)
//		{
//			return _pageViews.ElementAt(pageindex) !=null;
//		}
//		
//		public void currentPageIndexDidChange(){
//			this.layoutPage(_currentPageIndex);
//			if(_currentPageIndex + 1 < this.numberOfPages()){
//				this.layoutPage(_currentPageIndex +1);
//			}
//			if (_currentPageIndex > 0){
//				this.layoutPage(_currentPageIndex -1);
//			}
//			this.UpdatePageControl(helpPageControl,_currentPageIndex,this.numberOfPages(),((UIView)this._pageViews[_currentPageIndex]));
//		}
//
//
//	}
//
//	//UIScrollView Delegates		
//	
//	public class ScrollViewHelpDelegate:UIScrollViewDelegate
//	{
//		private MoodHelpOverlayView oParentController;
//		
//		public ScrollViewHelpDelegate(MoodHelpOverlayView oParentController):base()
//		{
//			this.oParentController = oParentController;
//		}
//		
//		public override void Scrolled(UIScrollView scrollView)
//		{
//			SizeF pageSize = oParentController.pageSize();
//			int newPageIndex = ((int)oParentController.overlayHelpScrollView.ContentOffset.X + (int)pageSize.Width /2)/(int)pageSize.Width;
//			if (newPageIndex == oParentController._currentPageIndex) return;
//			oParentController._currentPageIndex = newPageIndex;
//			oParentController.currentPageIndexDidChange();			
//		}
//		
//		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
//		{
//		}
//		
//		public override void DecelerationEnded(UIScrollView scrollView)
//		{
//		}
//		
//		public bool shouldAutorotateToInterfaceOrientation(UIInterfaceOrientation interfaceOrientation)
//		{
//			return (interfaceOrientation != UIInterfaceOrientation.LandscapeRight);
//		}
//		
//		public void didRotateFromInterfaceOrientation(UIInterfaceOrientation fromInterfaceOrientation)
//		{
//			
//		}
//		
//	}

}

