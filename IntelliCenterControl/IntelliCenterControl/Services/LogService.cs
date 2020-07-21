using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using NLog;
using NLog.Config;


namespace IntelliCenterControl.Services
{
    public class LogService : ILogService
    {
        private Logger logger;

        public void Initialize(Assembly assembly, string assemblyName)
        {
            var stream = GetEmbeddedResourceStream(assembly, "NLog.config");
            LogManager.Configuration = new XmlLoggingConfiguration(XmlReader.Create(stream), null);
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
        {
            var resourcePaths = assembly.GetManifestResourceNames()
                .Where(x => x.EndsWith(resourceFileName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (resourcePaths.Count == 1)
            {
                return assembly.GetManifestResourceStream(resourcePaths.Single());
            }
            return null;
        }

        public void LogDebug(string message)
        {
            this.logger?.Info(message);
        }

        public void LogError(string message)
        {
            this.logger?.Error(message);
        }

        public void LogFatal(string message)
        {
            this.logger?.Fatal(message);
        }

        public void LogInfo(string message)
        {
            this.logger?.Info(message);
        }

        public void LogWarning(string message)
        {
            this.logger?.Warn(message);
        }
    }
}
