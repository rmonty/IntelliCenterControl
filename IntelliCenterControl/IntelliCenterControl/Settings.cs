using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace IntelliCenterControl
{
    
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region Setting Constants

        private const string ServerURLKey = "server_url_key";
        //private static readonly string ServerURLDefault = "ws://192.168.0.114:6680/";
        private static readonly string ServerURLDefault = "http://192.168.0.130:5000/stream";

        private const string StorageAccessAskedKey = "storage_access_key";
        private static readonly bool StorageAccessAskedDefault = false;
        #endregion


        public static string ServerURL
        {
            get => AppSettings.GetValueOrDefault(ServerURLKey, ServerURLDefault);
            set => AppSettings.AddOrUpdateValue(ServerURLKey, value);
        }


        public static bool StorageAccessAsked
        {
            get => AppSettings.GetValueOrDefault(StorageAccessAskedKey, StorageAccessAskedDefault);
            set => AppSettings.AddOrUpdateValue(StorageAccessAskedKey, value);
        }

    }
}
