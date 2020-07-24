using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;

namespace IntelliCenterControl.Services
{
    public class CloudLogService : ICloudLogService
    {
        public void Initialize(string secretKey)
        {
            AppCenter.Start(secretKey, typeof(Analytics), typeof(Crashes));
        }

        public void LogEvent(string name, Dictionary<string, string> properties = null)
        {
            Analytics.TrackEvent(name, properties);
        }

        public void LogError(Exception ex, Dictionary<string, string> properties = null)
        {
            Crashes.TrackError(ex, properties);
        }
    }
}
