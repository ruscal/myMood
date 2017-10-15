using System;
using ApngPlayerBinding;
using MonoTouch.Foundation;
namespace MyMood
{
	[Register ("AVAnimatorViewClass")]

	public partial class AVAnimatorViewClass:AVAnimatorView
	{
		public AVAnimatorViewClass (IntPtr handle) : base(handle)
		{
		}
	}
}

