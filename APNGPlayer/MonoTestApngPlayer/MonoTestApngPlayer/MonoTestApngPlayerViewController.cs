using System;
using System.Drawing;
using System.IO;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using ApngPlayerBinding;

namespace MonoTestApngPlayer
{
	public partial class MonoTestApngPlayerViewController : UIViewController
	{
		AVAnimatorMedia animatorMedia;
		ApngPlayerBinding.AVAnimatorView animatorView;

		public MonoTestApngPlayerViewController (IntPtr handle) : base (handle)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		#region View lifecycle
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.btnPlay.TouchUpInside += delegate(object sender, EventArgs e) {
				testPlay1();
			};
			this.btnPlay2.TouchUpInside += delegate(object sender, EventArgs e) {
				testPlay2();
			};
			this.btnPlay3.TouchUpInside += delegate(object sender, EventArgs e) {
				testPlay3();
			};
			this.View.BackgroundColor = UIColor.Blue;

			//UITextField tf = new UITextField(new RectangleF(10,10,200,20));
			//tf.Text = "Hello World";
			//this.View.Add(tf);

			animatorView = new ApngPlayerBinding.AVAnimatorView();//  (new RectangleF(100,100,200,200));
			animatorView.Frame = new RectangleF(40,148,240,240);
			//animatorView.Bounds = new RectangleF(40,148,240,240);
			//animatorView.BackgroundColor = UIColor.Red;
			this.View.Add(animatorView);

			// Perform any additional setup after loading the view, typically from a nib.
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}
		
		#endregion
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}

		public void testPlay1()
		{
			playMedia("Matrix_480_320_10FPS_16BPP",false);

		}

		public void testPlay2()
		{
			playMedia("Bounce_24BPP_15FPS",false);
			
		}
		public void testPlay3()
		{
			playMedia("Sweep15FPS_ANI",true);
			
		}
		public void playMedia(string filename,bool convert)
		{

			AVAnimatorMedia media = new AVAnimatorMedia();
			media.animatorRepeatCount=10;
			animatorMedia = media;
			genericResourceLoader(filename,convert,media);

			this.animatorView.attachMedia(media);
			this.animatorView.media.startAnimator();

		}

		void genericResourceLoader(string resourcePrefix, bool convertToMvid, AVAnimatorMedia media)
			
		{
			string videoResourceArchiveName;
			string videoResourceEntryName;
			string videoResourceOutName;
			string videoResourceOutPath;

	
			string mvidResFilename = string.Format("{0}.mvid.7z", resourcePrefix  );
			string mvidResPath = NSBundle.MainBundle.PathForResource(mvidResFilename, null);

			bool convertToMvidLoader = true;
			if (convertToMvid == false) {
				convertToMvidLoader = false;
			}
			
			if (convertToMvid && (mvidResPath != null)) {
				// Extract existing FILENAME.mvid from FILENAME.mvid.7z attached as app resource
				videoResourceArchiveName = string.Format("{0}.mvid.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mvid", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mvid", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
				convertToMvidLoader = false;
			}  else if (convertToMvid) {
				// Extract to /tmp/FILENAME.mvid
				videoResourceArchiveName = string.Format("{0}.mov.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mov", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mvid", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
			}  else {
				// Extract to /tmp/FILENAME.mov
				videoResourceArchiveName = string.Format("{0}.mov.7z", resourcePrefix);
				videoResourceEntryName = string.Format("{0}.mov", resourcePrefix);
				string resourceTail = resourcePrefix;
				videoResourceOutName = string.Format("{0}.mov", resourceTail);
				videoResourceOutPath = AVFileUtil.getTmpDirPath(videoResourceOutName);
			}
			
			if (convertToMvidLoader) {
				AV7zQT2MvidResourceLoader resLoader = new AV7zQT2MvidResourceLoader();
				resLoader.archiveFilename = videoResourceArchiveName;
				resLoader.movieFilename = videoResourceEntryName;
				resLoader.outPath = videoResourceOutPath;
				media.resourceLoader = resLoader;
			}  else {
				AV7zAppResourceLoader resLoader = new AV7zAppResourceLoader();
				resLoader.archiveFilename = videoResourceArchiveName;
				resLoader.movieFilename = videoResourceEntryName;
				resLoader.outPath = videoResourceOutPath;
				
				media.resourceLoader = resLoader;
			}
			
			if (convertToMvid) {
				AVMvidFrameDecoder frameDecoder = new AVMvidFrameDecoder();
				media.frameDecoder = frameDecoder;
			}  else {
				AVQTAnimationFrameDecoder frameDecoder = new AVQTAnimationFrameDecoder();
				media.frameDecoder = frameDecoder;
			}
			
			return;
		}
		
	}


	}

