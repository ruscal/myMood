using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Diagnostics;
using System.Collections;


namespace MyMood
{

	public interface IPageLoopViewDelegate
	{
		int numberOfColumnsForTableView(HorizontalTableView tableView);
		UIView tableViewIndex(HorizontalTableView tableView, int index);
		float columnWidthForTableView(HorizontalTableView tableView);
	}

	[Register ("HorizontalTableView")]
	public class HorizontalTableView:UIView
	{ 
		ArrayList _pageViews;
		UIScrollView _scrollView;
		UIView _containerView;
		int _currentPageIndex;
		int _currentPhysicalPageIndex;
		int _visibleColumnCount;
		float _columnWidth;		
		ArrayList _columnPool;
		int kColumnPoolSize = 3;
		IPageLoopViewDelegate _delegate;

		public IPageLoopViewDelegate tableViewDelegate
		{
			set{ _delegate = value;}
		}

		public ArrayList pageViews
		{
			get{ return _pageViews;}
			set{ _pageViews = value;}
		}

		public UIScrollView scrollView
		{
			get{ return _scrollView;}
			set{ _scrollView = value;}
		}

		public UIView containerView
		{
			get{ return _containerView;}
			set{ _containerView = value;}
		}

		public int currentPageIndex
		{
			get{ return _currentPageIndex;}
			set{ _currentPageIndex = value;}
		}
		public int currentPhysicalPageIndex
		{
			get{ return _currentPhysicalPageIndex;}
			set{ _currentPhysicalPageIndex = value;}
		}

		public ArrayList columnPool
		{
			get{ return _columnPool;}
			set{ _columnPool = value;}
		}

		public HorizontalTableView()
		{
			this.init();
		}

		public HorizontalTableView(IntPtr ptr):base(ptr)
		{
			this.init();
		}

		void init()
		{
			base.Init();
			this.pageViews = new ArrayList(); 

			if (this !=null) {
				this.prepareView();
			}
		}
		
		void prepareView()
		{
			_columnPool = new ArrayList(kColumnPoolSize);
			_columnWidth = 0;
			this.ClipsToBounds=true;			
			this.AutosizesSubviews = true;			
			UIScrollView scroller = new UIScrollView();
			containerView = new UIView(this.Bounds);
			RectangleF zoomView  = new RectangleF(this.Bounds.X,this.Bounds.Y,this.Bounds.Width, this.Bounds.Height);
			scroller.Frame = zoomView;
			scroller.Delegate = new ScrollViewDelegate(this);
			scroller.AutosizesSubviews = true;
			scroller.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			scroller.ShowsHorizontalScrollIndicator = true;
			scroller.ShowsVerticalScrollIndicator = false;
			scroller.AlwaysBounceVertical = false;
			scroller.SetZoomScale(1.0f,true);
			scroller.MinimumZoomScale = 0.5f;
			scroller.MaximumZoomScale = 1.0f;

			this.scrollView = scroller;
			this.containerView.AddSubview(scroller);
			//containerView.BackgroundColor = UIColor.Red;
			UITapGestureRecognizer doubleTapRecogniser = new UITapGestureRecognizer(this,new MonoTouch.ObjCRuntime.Selector("scrollViewDoubleTapped"));
			doubleTapRecogniser.NumberOfTapsRequired = 2;
			doubleTapRecogniser.NumberOfTouchesRequired = 1;
			containerView.AddGestureRecognizer(doubleTapRecogniser);
			
			UITapGestureRecognizer twoFingerTapRecogniser = new UITapGestureRecognizer(this,new MonoTouch.ObjCRuntime.Selector("scrollViewTwoFingerTapped"));
			twoFingerTapRecogniser.NumberOfTapsRequired = 1;
			twoFingerTapRecogniser.NumberOfTouchesRequired = 2;
			containerView.AddGestureRecognizer(twoFingerTapRecogniser);
			containerView.AutosizesSubviews = true;
			containerView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			this.AddSubview(containerView);
			scroller = null;

		}

		public void refreshData() 
		{
			Console.WriteLine("refreshData");

			this.pageViews = new ArrayList(); 
			// to save time and memory, we won't load the page views immediately
			int numberOfPhysicalPages = this.numberOfPages();
			Console.WriteLine("numberOfPhysicalPages = {0}",numberOfPhysicalPages);

			for (int i = 0; i < numberOfPhysicalPages; ++i)
			{
				this.pageViews.Add(null);
			}
			this.SetNeedsLayout();
		}
		
		int numberOfPages() 
		{
			int numPages = 0;
			numPages =  _delegate.numberOfColumnsForTableView(this);
			Console.WriteLine("numberOfPages {0}",numPages);

			return numPages;
		}

		public UIView viewForPhysicalPage(int pageIndex)
		{
			Debug.Assert(pageIndex >= 0);
			Debug.Assert(pageIndex < this.pageViews.Count);
			
			UIView pageView = ((UIView)pageViews[pageIndex]);
			if (pageView == null) 
			{
					pageView = _delegate.tableViewIndex(this,pageIndex);
					this.pageViews[pageIndex]=pageView;
					this.scrollView.AddSubview(pageView);
				//Console.WriteLine("View loaded for page {0}", pageIndex);
			} else 
			{
				pageView = (UIView)this.pageViews[pageIndex];
			}
			return pageView;
		}

		public SizeF pageSize() {
			RectangleF rect =  this.scrollView.Bounds;
			return rect.Size;
		}
		
		public float columnWidth() 
		{
				float width = _delegate.columnWidthForTableView(this);
					_columnWidth = width;
			return _columnWidth;
			
		}
		
		public bool isPhysicalPageLoaded(int pageIndex) 
		{
			//Console.WriteLine("isPhysicalPageLoaded");

			return this.pageViews[pageIndex] != null;
		}
		
		void layoutPhysicalPage(int pageIndex)
		{
			UIView pageView = this.viewForPhysicalPage(pageIndex);
			float viewWidth = pageView.Bounds.Size.Width;
			SizeF pageSize = this.pageSize();

			RectangleF rect = new RectangleF(viewWidth * pageIndex, 0, viewWidth, pageSize.Height);
			pageView.Frame = rect;
		}
		
		void awakeFromNib() 
		{
			//Console.WriteLine("awakeFromNib");

			this.prepareView();
		}
		
		void queueColumnView(UIView vw)
		{
			//Console.WriteLine("queueColumnView");

			if (this.columnPool.Count >= kColumnPoolSize) 
			{
				return;
			}
			this.columnPool.Add(vw);
		}
		
		public UIView dequeueColumnView() 
		{
			//Console.WriteLine("dequeueColumnView");
			if (columnPool.Count >0)
			{
			var vw = this.columnPool[columnPool.Count-1];
			if (vw !=null) {
				this.columnPool.Remove(vw);
				//Console.WriteLine("Supply from reuse pool");
			}
			return (UIView)vw;
			}
			else
			{
				return null;
			}
		}
		
		public void removeColumn(int index)
		{
			if (this.pageViews[index] != null) 
			    {
				//Console.WriteLine("Removing view at position {0}", index);
				UIView vw = (UIView)this.pageViews[index];
				this.queueColumnView(vw);
				vw.RemoveFromSuperview();
				this.pageViews[index]=null;
			}
		}

		public void clearViews()
		{
			if(this.pageViews !=null)
			{
			foreach (UIView vw in this.pageViews)
			{
					if(vw!= null)
					{
						vw.RemoveFromSuperview();
					}
			}
				pageViews.Clear();
			}
		}

		public void currentPageIndexDidChange() 
		{
			//Console.WriteLine("currentPageIndexDidChange");

			SizeF pageSize = this.pageSize();
			float columnWidth = this.columnWidth();
			_visibleColumnCount = (int)( pageSize.Width / columnWidth)+2;

			int leftMostPageIndex = -1;
			int rightMostPageIndex = 0;
			
			for (int i = -2; i < _visibleColumnCount; i++) 
			{
				int index = _currentPhysicalPageIndex + i;
				if (index < this.pageViews.Count && (index >= 0)) {
					this.layoutPhysicalPage(index);
					if (leftMostPageIndex < 0)
						leftMostPageIndex = index;
					rightMostPageIndex = index;
				}
			}
			
			// clear out views to the left
			for (int i = 0; i < leftMostPageIndex; i++) {
				this.removeColumn(i);
			}
			
			// clear out views to the right
			for (int i = rightMostPageIndex + 1; i < this.pageViews.Count; i++) {
				this.removeColumn(i);
			}
		}

		public void layoutPages() 
		{
			//Console.WriteLine("layoutPages");

			SizeF pageSize = this.Bounds.Size;
			this.scrollView.ContentSize = new SizeF(this.pageViews.Count * this.columnWidth(), pageSize.Height);
		}
		

		public int physicalPageIndex()
		{
			int page = (int)(this.scrollView.ContentOffset.X / this.columnWidth());
			return page;
		}

		public void setPhysicalPageIndex(int newIndex)
		{
			this.scrollView.ContentOffset = new PointF((float)(newIndex * this.pageSize().Width), 0.0f);
		}

		public override void LayoutSubviews() 
		{
			base.LayoutSubviews();
			this.layoutPages();
			this.currentPageIndexDidChange();
			//this.centerScrollViewContents();
		}


		private void centerScrollViewContents()
		{
			SizeF boundsSize = scrollView.Bounds.Size;
			RectangleF contentsFrame = containerView.Frame;
			
			//if (contentsFrame.Size.Width < boundsSize.Width) {
			//	contentsFrame.X = (boundsSize.Width - contentsFrame.Size.Width) / 2.0f;
			//} else {
			//contentsFrame.X = 0.0f;
			//}
			Console.WriteLine("contentsFrame {0} boundsSize {1}",contentsFrame,boundsSize);
			
			//contentsFrame.Size.Width = oParentController.Frame.Width;
			//contentsFrame.X = 0.0f;
			if (contentsFrame.Size.Height < boundsSize.Height) {
				contentsFrame.Y = (boundsSize.Height - contentsFrame.Size.Height) / 2.0f;
			} else {
				contentsFrame.Y = 0.0f;
			}
			contentsFrame.Width = 1024;
			//Console.WriteLine("frame width {0} frame position {1}",oParentController.Frame.Width,contentsFrame.Y);
			containerView.Frame = contentsFrame;
			
			
		}

		//Move to a particular mood
		public void SnapToPage(int page, bool animate)
		{	
			Console.WriteLine("Snap to page - {0}", page);
			int pageToSnapTo = page;
			int totpages = this.numberOfPages();

			if (totpages <8) return;

			if (page > totpages-7) pageToSnapTo = totpages-7;
				PointF point = new PointF(pageToSnapTo * 140,0);
			//RectangleF pos = new RectangleF(page*140,0,this.pageSize().Width,0);
			//this.scrollView.ScrollRectToVisible(pos,true);
			//this.scrollView.SetContentOffset(point,animate);
			this.SetContentXOffset(point.X, animate);
		}

		internal void SetContentXOffset (float xOffset, bool animate)
		{
			var contentW = this.scrollView.ContentSize.Width;
			var scrollW = this.scrollView.Bounds.Width;
			if (contentW <= scrollW) {
				this.scrollView.SetContentOffset (new PointF (0f, 0f), false);
			} else {
				if(xOffset+scrollW > contentW)
				{
					xOffset = contentW-scrollW;
				}
				this.scrollView.SetContentOffset (new PointF(xOffset, 0), animate);
			}
		}

		[Export("scrollViewTwoFingerTapped")]
		public void scrollViewTwoFingerTapped(UITapGestureRecognizer recogniser)
		{
			// Zoom out slightly, capping at the minimum zoom scale specified by the scroll view
			float newZoomScale = scrollView.ZoomScale / 1.5f;
			newZoomScale = Math.Max(newZoomScale,scrollView.MinimumZoomScale);
			scrollView.SetZoomScale(newZoomScale, true);
		}

		[Export("scrollViewDoubleTapped")]
		public void scrollViewDoubleTapped(UITapGestureRecognizer recogniser)
		{
			PointF pointInView = recogniser.LocationInView(containerView);
			float newZoomScale = scrollView.ZoomScale * 1.5f;
			newZoomScale = Math.Min(newZoomScale, scrollView.MaximumZoomScale);
			SizeF scrollViewSize = scrollView.Bounds.Size;
			
			float w = scrollViewSize.Width / newZoomScale;
			float h = scrollViewSize.Height / newZoomScale;
			float x = pointInView.X - (w / 2.0f);
			float y = pointInView.Y - (h / 2.0f);
			
			RectangleF rectToZoomTo = new RectangleF(x, y, w, h);			
			scrollView.ZoomToRect(rectToZoomTo,true);
		}

	}



	//UIScrollView Delegates		

	public class ScrollViewDelegate:UIScrollViewDelegate
	{
		private HorizontalTableView oParentController;
		private PointF myScrollViewOffset;
		private float scrollOffsetPercentage;

		public ScrollViewDelegate(HorizontalTableView oParentController):base()
		{
			this.oParentController = oParentController;
		}

		public override void Scrolled(UIScrollView scrollView)
		{
			int newPageIndex = oParentController.physicalPageIndex();
			if (newPageIndex == oParentController.currentPhysicalPageIndex) return;
			oParentController.currentPhysicalPageIndex = newPageIndex;
			oParentController.currentPageIndexDidChange();			
			//SizeF rect = oParentController.scrollView.ContentSize;
		}
		
		public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
		{
		}
		
		public override void DecelerationEnded(UIScrollView scrollView)
		{
		}
		
		public bool shouldAutorotateToInterfaceOrientation(UIInterfaceOrientation interfaceOrientation)
		{
			//return (interfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
			return false;
		}

		
		public void didRotateFromInterfaceOrientation(UIInterfaceOrientation fromInterfaceOrientation)
		{
			// adjust frames according to the new page size - this does not cause any visible changes
//			oParentController.layoutPages();
//			oParentController.setPhysicalPageIndex(oParentController.currentPhysicalPageIndex);

			// unhide
//			for (int pageIndex = 0; pageIndex < oParentController.pageViews.Count; ++pageIndex)
//				if (oParentController.isPhysicalPageLoaded(pageIndex))
//			{
//				oParentController.viewForPhysicalPage(pageIndex).Hidden = false;
//			}
//			float f = (float)oParentController.pageViews.Count;
//			oParentController.scrollView.ContentSize = new SizeF(f * oParentController.columnWidth(), oParentController.pageSize().Height);
//			oParentController.currentPageIndexDidChange();
		}

		public override UIView ViewForZoomingInScrollView(UIScrollView scrollView)			
		{
			
			Console.WriteLine("ViewForZoomingInScrollView {0} ",scrollView.Bounds);
			return oParentController.containerView;
			
		}


		public override void ZoomingStarted (UIScrollView scrollView, UIView view)
		{
			myScrollViewOffset = oParentController.scrollView.ContentOffset;
			scrollOffsetPercentage = oParentController.scrollView.ContentOffset.X / oParentController.scrollView.ContentSize.Width;
		}

		public override void DidZoom (UIScrollView scrollView)
		{

			Console.WriteLine("Zoomscale {0} frame position {1}",scrollView.ZoomScale,560.0f - (280.0f * scrollView.ZoomScale));
//			centerScrollViewContents();
//			oParentController.scrollView.SetContentOffset(myScrollViewOffset,false);


			//oParentController.containerView.Frame.X = 560.0f - (280.0f * scrollView.ZoomScale);
			//oParentController.containerView.Center.X = 560.0f;
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
		}

		public override void ZoomingEnded (UIScrollView scrollView, UIView withView, float atScale)
		{
			//oParentController.containerView.Frame.X = 560.0f - (280.0f * scrollView.ZoomScale);
			centerScrollViewContents();
			//oParentController.scrollView.SetContentOffset(myScrollViewOffset,false);
		}

		public override bool ShouldScrollToTop (UIScrollView scrollView)
		{
			return false;
		}

		private void centerScrollViewContents ()
		{
			SizeF boundsSize = oParentController.scrollView.Bounds.Size;
			RectangleF contentsFrame = oParentController.containerView.Frame;
			var contentW = oParentController.scrollView.ContentSize.Width;

			
			//if (contentsFrame.Size.Width < boundsSize.Width) {
			//	contentsFrame.X = (boundsSize.Width - contentsFrame.Size.Width) / 2.0f;
			//} else {
			//contentsFrame.X = 0.0f;
			//}
			Console.WriteLine ("contentsFrame {0} boundsSize {1}", contentsFrame, boundsSize);

			//contentsFrame.Size.Width = oParentController.Frame.Width;
			//contentsFrame.X = 0.0f;
			if (contentsFrame.Size.Height < boundsSize.Height) {
				contentsFrame.Y = (boundsSize.Height - contentsFrame.Size.Height) / 2.0f;
			} else {
				contentsFrame.Y = 0.0f;
			}
			contentsFrame.Width = oParentController.Bounds.Width;
			Console.WriteLine ("frame width {0} frame position {1}", oParentController.Frame.Width, contentsFrame.Y);
			oParentController.containerView.Frame = contentsFrame;

			oParentController.SetContentXOffset(myScrollViewOffset.X, false);

				Console.WriteLine("Scrolloffset {0} {1}", myScrollViewOffset, oParentController.scrollView.ContentOffset);
		}


	}
}

