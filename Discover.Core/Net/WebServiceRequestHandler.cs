using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;
using Discover.Logging;
using MonoTouch.Foundation;

namespace Discover.Net
{
	public delegate void WebServiceJsonRequestCompleted (WebServiceJsonRequestStatus status);

	public class WebServiceRequestHandler
	{
		ILogger _logger;
		int _retryRequestDelaySeconds = 120;
		protected int RequestTimeout = 20000;

		public WebServiceRequestHandler (ILogger logger)
		{
			this._logger = logger;
		}

		public void JsonRequestWithRetry (string requestUrl, object postData, int retries, WebServiceJsonRequestCompleted completed)
		{
			WebServiceJsonRequestStatus status = JsonRequest (requestUrl, postData);
			if (!status.Success && retries > 0) {
				NSTimer.CreateScheduledTimer(this._retryRequestDelaySeconds, delegate {
					JsonRequestWithRetry(requestUrl, postData, retries-1, completed);
				});
			}
			completed.Invoke(status);
		}

		public WebServiceJsonRequestStatus JsonRequest(string requestUrl, object postData){
			
			JObject value = null;
			try
			{
				var req = HttpWebRequest.Create(requestUrl);
				req.ContentType = "application/json";
				req.Method = "POST";
				req.Timeout = RequestTimeout;

				 
				JObject postJson = JObject.FromObject(postData);
				
				string postJsonString = postJson.ToString();

				Log ("Web request - " + requestUrl, postJsonString, 3);
				
				byte[] data = new ASCIIEncoding().GetBytes(postJsonString);

				req.ContentLength = data.Length;

				Stream dataStream = req.GetRequestStream();
				dataStream.Write(data, 0, data.Length);
				dataStream.Close();
				
				using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse()) 
				{
					using (StreamReader reader = new StreamReader (resp.GetResponseStream ())) 
					{
						value = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
						Log ("Web response - " + requestUrl, value.ToString(), 3);
					}
				}
			}
			catch (Exception ex)
			{
				Error(String.Format("Exception on JsonRequest::{0}:{1}", requestUrl, ex.Message), ex); 
				return new WebServiceJsonRequestStatus(){
					Success = false,
					Exception = ex
				};
			}
			return new WebServiceJsonRequestStatus(){
				Success = true,
				Response = value
			};
		}

		protected void Log (string message, string detail, int logLevel)
		{
			this._logger.Log(message, detail, logLevel);
		}

		protected void Error (string message,  Exception ex)
		{
			this._logger.Error(message, ex, 1);
		}
	}

	public class WebServiceJsonRequestStatus
	{
		public bool Success {
			get;
			set;
		}
		public Exception Exception {
			get;
			set;
		}

		public JObject Response {
			get;
			set;
		}
	}
}

