using System;
using System.Drawing;

using MonoTouch.ObjCRuntime;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace ApngPlayerBinding
{
	// The first step to creating a binding is to add your native library ("libNativeLibrary.a")
	// to the project by right-clicking (or Control-clicking) the folder containing this source
	// file and clicking "Add files..." and then simply select the native library (or libraries)
	// that you want to bind.
	//
	// When you do that, you'll notice that MonoDevelop generates a code-behind file for each
	// native library which will contain a [LinkWith] attribute. MonoDevelop auto-detects the
	// architectures that the native library supports and fills in that information for you,
	// however, it cannot auto-detect any Frameworks or other system libraries that the
	// native library may depend on, so you'll need to fill in that information yourself.
	//
	// Once you've done that, you're ready to move on to binding the API...
	//
	//
	// Here is where you'd define your API definition for the native Objective-C library.
	//
	// For example, to bind the following Objective-C class:
	//
	//     @interface Widget : NSObject {
	//     }
	//
	// The C# binding would look like this:
	//
	//     [BaseType (typeof (NSObject))]
	//     interface Widget {
	//     }
	//
	// To bind Objective-C properties, such as:
	//
	//     @property (nonatomic, readwrite, assign) CGPoint center;
	//
	// You would add a property definition in the C# interface like so:
	//
	//     [Export ("center")]
	//     PointF Center { get; set; }
	//
	// To bind an Objective-C method, such as:
	//
	//     -(void) doSomething:(NSObject *)object atIndex:(NSInteger)index;
	//
	// You would add a method definition to the C# interface like so:
	//
	//     [Export ("doSomething:atIndex:")]
	//     void DoSomething (NSObject object, int index);
	//
	// Objective-C "constructors" such as:
	//
	//     -(id)initWithElmo:(ElmoMuppet *)elmo;
	//
	// Can be bound as:
	//
	//     [Export ("initWithElmo:")]
	//     IntPtr Constructor (ElmoMuppet elmo);
	//
	// For more information, see http://docs.xamarin.com/ios/advanced_topics/binding_objective-c_types
	//




	[BaseType (typeof (NSObject))]
	interface AVFileUtil
	{

		[Static,Export ("getTmpDirPath:")]
		string getTmpDirPath(string filename);
		
		[Static,Export ("fileExists:")]
		bool fileExists(string path);
		
		[Static,Export ("getResourcePath:")]
		string getResourcePath(string resFilename);

		[Static,Export ("generateUniqueTmpPath")]
		string generateUniqueTmpPath();

		[Static,Export ("getQualifiedFilenameOrResource:")]
		string getQualifiedFilenameOrResource(string filename);

		[Static,Export ("renameFile::")]
		void renameFile(string path, string toPath);
	}

	[BaseType (typeof (NSObject))]
	interface AVResourceLoader
	{

		[Export ("isReady")]
		bool isReady();

		[Export ("load")]
		void load();

		[Export ("getResources")]
		NSArray getResources();

	}


	[BaseType (typeof (NSObject))]
	interface AVAnimatorMedia
	{
		[Export ("resourceLoader")]
		AVResourceLoader resourceLoader{ get;set; }

		[Export ("frameDecoder")]
		AVFrameDecoder frameDecoder{ get;set; }

		[Export ("animatorFrameDuration")]
		double animatorFrameDuration{ get;set; }

		[Export ("animatorNumFrames")]
		uint animatorNumFrames{ get;set; }

		[Export ("animatorRepeatCount")]
		uint animatorRepeatCount{ get;set; }

		[Export ("hasAudio")]
		bool hasAudio{ get;set; }

		[Export ("startAnimator")]
		void startAnimator();

		[Export ("stopAnimator")]
		void stopAnimator();

		[Export ("isAnimatorRunning")]
		bool isAnimatorRunning();

		[Export ("isInitializing")]
		bool isInitializing();

		[Export ("doneAnimator")]
		void doneAnimator();

		[Export ("pause")]
		void pause();

		[Export ("unpause")]
		void unpause();

		[Export ("rewind")]
		void rewind();

		[Export ("prepareToAnimate")]
		void prepareToAnimate();

		[Export ("showFrame:frame")]
		void showFrame(int frame);
	}


	[BaseType (typeof (UIImageView))]
	interface AVAnimatorView
	{
		[Export ("aVAnimatorViewWithFrame:")]
		//void aVAnimatorViewWithFrame(RectangleF viewFrame);
		IntPtr Constructor (RectangleF viewFrame);

		[Export ("media")]
		AVAnimatorMedia media{ get; }

		[Export ("animatorOrientation")]
		UIImageOrientation animatorOrientation{ get;set; }

		[Export ("attachMedia:")]
		void attachMedia(AVAnimatorMedia inMedia);

		[Export ("mediaAttached:")]
		void mediaAttached(bool worked);

		[Export ("dealloc:")]
		void dealloc();

	}

	[BaseType (typeof (NSObject))]
	interface AVFrameDecoder
	{
		
		[Export ("openForReading:")]
		bool openForReading(string path);

		[Export ("close")]
		void close();

		[Export ("rewind")]
		void rewind();

		[Export ("advanceToFrame:")]
		UIImage advanceToFrame(uint newFrameIndex);

		[Export ("allocateDecodeResources")]
		bool allocateDecodeResources();

		[Export ("releaseDecodeResources")]
		void releaseDecodeResources();

		[Export ("isResourceUsageLimit")]
		bool isResourceUsageLimit();

		[Export ("duplicateCurrentFrame")]
		UIImage duplicateCurrentFrame();

		[Export ("width")]
		uint width();

		[Export ("height")]
		uint height();

		[Export ("isOpen")]
		bool isOpen();

		[Export ("numFrames")]
		uint numFrames();
		
		[Export ("frameIndex")]
		int frameIndex();

		[Export ("frameDuration")]
		double frameDuration();

		[Export ("hasAlphaChannel")]
		bool hasAlphaChannel();


	}

	[BaseType (typeof (AVResourceLoader))]
	interface AVAppResourceLoader
	{
		
		[Export ("movieFilename")]
		string movieFilename{ get;set; }
		
		[Export ("audioFilename")]
		string audioFilename{ get;set; }

		[Export ("isReady:")][New]
		bool isReady();

		[Export ("load:")][New]
		void load();

	}

	[BaseType (typeof (AVAppResourceLoader))]
	interface AV7zAppResourceLoader
	{
		
		[Export ("archiveFilename")]
		string archiveFilename{ get;set; }

		[Export ("outPath")]
		string outPath{ get;set; }

	}
		
	
	[BaseType (typeof (AV7zAppResourceLoader))]
	interface AV7zQT2MvidResourceLoader
	{
		
		[Export ("alwaysGenerateAdler")]
		bool alwaysGenerateAdler{ get;set; }
		
	}

	[BaseType (typeof (AVFrameDecoder))]
	interface AVMvidFrameDecoder
	{
		
		[Export ("filePath")]
		string filePath{ get;set; }
		
		[Export ("mappedData")]
		NSData mappedData{ get;set; }
		
		//[Export ("currentFrameBuffer")]
		//currentFrameBuffer;
		
		[Export ("cgFrameBuffers")]
		NSArray cgFrameBuffers{ get;set; }
		
		[Export ("close:")][New]
		void close();
		
		[Export ("rewind:")][New]
		void rewind();
		
		[Export ("advanceToFrame:")][New]
		UIImage advanceToFrame(uint newFrameIndex);
		
		[Export ("allocateDecodeResources")][New]
		bool allocateDecodeResources();
		
		[Export ("releaseDecodeResources")][New]
		void releaseDecodeResources();
		
		[Export ("height")][New]
		uint height{ get;set; }
		
		[Export ("width")][New]
		uint width{ get;set; }
		
		[Export ("isOpen")][New]
		bool isOpen();
		
		[Export ("numFrames")][New]
		uint numFrames{ get;set; }
		
		[Export ("frameIndex")][New]
		uint frameIndex{ get;set; }
		
		[Export ("frameDuration")][New]
		double frameDuration();
	}

	[BaseType (typeof (AVMvidFrameDecoder))]
	interface AVQTAnimationFrameDecoder
	{
		
		[Export ("filePath")][New]
		string filePath{ get;set; }

		[Export ("mappedData")][New]
		NSData mappedData{ get;set; }

		//[Export ("currentFrameBuffer")]
		 //currentFrameBuffer;

		[Export ("cgFrameBuffers")][New]
		NSArray cgFrameBuffers{ get;set; }

		[Export ("close")][New]
		void close();

		[Export ("rewind")][New]
		void rewind();

		[Export ("advanceToFrame:")][New]
		UIImage advanceToFrame(uint newFrameIndex);

		[Export ("allocateDecodeResources")][New]
		bool allocateDecodeResources();

		[Export ("releaseDecodeResources")][New]
		void releaseDecodeResources();

		[Export ("height")][New]
		uint height{ get;set; }

		[Export ("width")][New]
		uint width{ get;set; }

		[Export ("isOpen")][New]
		bool isOpen();

		[Export ("numFrames")][New]
		uint numFrames{ get;set; }

		[Export ("frameIndex")][New]
		uint frameIndex{ get;set; }

		[Export ("frameDuration")][New]
		double frameDuration();

	}

}

