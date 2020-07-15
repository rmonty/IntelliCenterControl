using System;
using System.Reflection;

namespace IntelliCenterControl.Services
{
    public interface ILogService
    {
        void Initialize(Assembly assembly, string assemblyName);

        void LogDebug(string message);

        void LogError(string message);

        void LogFatal(string message);

        void LogInfo(string message);

        void LogWarning(string message);
    }
}