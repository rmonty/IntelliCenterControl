using IntelliCenterControl.Annotations;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace IntelliCenterControl
{

    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public class Settings : INotifyPropertyChanged
    {
        private static ISettings AppSettings => CrossSettings.Current;

        private static Settings instance;
        private static readonly object mutex = new object();

        private Settings()
        {
        }

        public static Settings Instance
        {
            get
            {
                lock (mutex)
                {

                    instance ??= new Settings();
                }

                return instance;
            }
        }

        #region Setting Constants

        private const string ServerURLKey = "server_url_key";
        private readonly string ServerURLDefault = "http://192.168.0.130:5000/";

        private const string StorageAccessAskedKey = "storage_access_key";
        private readonly bool StorageAccessAskedDefault = false;

        private const string HomeURLKey = "home_url_key";
        private readonly string HomeURLDefault = "http://192.168.0.130:5000/";

        private const string AwayURLKey = "away_url_key";
        private readonly string AwayURLDefault = "http://192.168.0.130:5000/";

        private const string ShowSolarTempKey = "showSolarTemp_key";
        private readonly bool ShowSolarTempDefault = false;
        #endregion

        public string ServerURL
        {
            get => Preferences.Get(ServerURLKey, ServerURLDefault);
            set => Preferences.Set(ServerURLKey, value);
        }


        public bool StorageAccessAsked
        {
            get => Preferences.Get(StorageAccessAskedKey, StorageAccessAskedDefault);
            set => Preferences.Set(StorageAccessAskedKey, value);
        }

        public string HomeURL
        {
            get => Preferences.Get(HomeURLKey, HomeURLDefault);
            set
            {
                Preferences.Set(HomeURLKey, value);
                OnPropertyChanged();
            }
        }

        public string AwayURL
        {
            get => Preferences.Get(AwayURLKey, AwayURLDefault);
            set
            {
                Preferences.Set(AwayURLKey, value);
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get
            {
                var retVal = GetUserName().Result;
                return string.IsNullOrEmpty(retVal) ? string.Empty : retVal;
            }
            set
            {
                StoreUserName(value);
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get
            {
                var retVal = GetPassword().Result;
                return string.IsNullOrEmpty(retVal) ? string.Empty : retVal;
            }
            set
            {
                StorePassword(value);
                OnPropertyChanged();
            }
        }

        public bool ShowSolarTemp
        {
            get => Preferences.Get(ShowSolarTempKey, ShowSolarTempDefault);
            set
            {
                Preferences.Set(ShowSolarTempKey, value);
                OnPropertyChanged();
            }
        }

        public async Task<string> GetUserName()
        {
            try
            {
                return await SecureStorage.GetAsync("userName");
            }
            catch
            {
                //ignore
            }

            return null;
        }

        public async void StoreUserName(string userName)
        {
            try

            {
                await SecureStorage.SetAsync("userName", userName);
            }
            catch
            {
                //ignore
            }
        }

        public async Task<string> GetPassword()
        {
            try
            {
                return await SecureStorage.GetAsync("passWord");
            }
            catch
            {
                //ignore
            }

            return null;
        }

        public async void StorePassword(string passWord)
        {
            try

            {
                await SecureStorage.SetAsync("passWord", passWord);
            }
            catch
            {
                //ignore
            }
        }

        public void RemoveUserNameAndPassword()
        {
            SecureStorage.RemoveAll();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(sender: this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
