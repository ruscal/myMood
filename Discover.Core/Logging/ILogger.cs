using System;

namespace Discover.Logging
{
	public interface ILogger
	{
		void Log(string message, string detail, int logLevel);
		void Error(string message, Exception ex, int logLevel);
	}
}

