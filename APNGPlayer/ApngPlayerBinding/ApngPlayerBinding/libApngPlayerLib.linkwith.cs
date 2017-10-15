using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libApngPlayerLib.a", LinkTarget.ArmV7 | LinkTarget.Simulator, ForceLoad = true)]
