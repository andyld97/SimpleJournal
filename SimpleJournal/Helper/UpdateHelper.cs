using Newtonsoft.Json;
using SimpleJournal.Dialogs;
using SimpleJournal;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Data;
using SimpleJournal.Common;
using System.Linq;
using System.IO;

namespace Helper
{
    public class UpdateHelper
    {
        private static UpdateInfo cache = null;
   
        /// <summary>
        /// Checks if there is a new SimpleJournal version available
        /// </summary>
        /// <returns><see cref="UpdateInfo"/></returns>
        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            var now = DateTime.Now;
      
            // Load cache from file!
            if (System.IO.File.Exists(Consts.UpdateCacheFilePath))
            {
                try
                {
                    cache = Serialization.Read<UpdateInfo>(Consts.UpdateCacheFilePath);
                }
                catch
                {

                }
            }

            var dt = cache?.LastUpdated ?? DateTime.MinValue;
            bool isCacheExpired = (dt > DateTime.MinValue && dt.AddMinutes(30) < now);

            // Only return cache if it is valid and only if it is valid
            if (cache != null && cache.Result != SimpleJournal.Common.UpdateResult.Unknown && !isCacheExpired)
                return cache;

            if (!GeneralHelper.IsConnectedToInternet())
            {
                // Even if the cache is expired (if it has valid value) return it, to prevent overwriting valid results due to a lost connection!
                if (isCacheExpired && cache != null && cache.Result != SimpleJournal.Common.UpdateResult.Unknown)
                    return cache;

                cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.Unknown, null, string.Empty, now);
                return cache;
            }

            // If a new update is available
            // Display all changes from current version till new version (changelog is enough)

            // 1) Get current version
#if !UWP
            var currentVersion = Consts.NormalVersion;
#else 
            var currentVersion = Consts.StoreVersion;
#endif

            // 2) Download version
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    bool debug = false;
#if DEBUG
                    debug = true;
#endif
                    string url = $"{Consts.VersionUrl}&lang={System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName}&debug={debug.ToString().ToLower()}";
                    string versionJSON = await client.GetStringAsync(url);
                    var versionInfo = JsonConvert.DeserializeObject<VersionInfo>(versionJSON);

                    string onlineVersionString;
#if !UWP
                    onlineVersionString = versionInfo.Current.Normal);
#else
                    onlineVersionString = versionInfo.Current.Store;
#endif
                    Version onlineVersion = Version.Parse(onlineVersionString);

                    // Find hash
                    var longVersionInfo = versionInfo.Versions.FirstOrDefault(p => p.Version == versionInfo.Current.Normal);
                    string hash = longVersionInfo?.Hash;

#if UWP
                    // Clear hash (there are no hashes for UWP - since it's managed via MS Store)
                    hash = string.Empty;
#endif

                    var result = onlineVersion.CompareTo(currentVersion);
                    if (result > 0)
                    {
                        // There is a new version
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.UpdateAvailable, onlineVersion, hash, now);
                        SaveCache();
                        return cache;
                    }
                    else if (result < 0)
                    {
                        // Online version is older than this version (dev version)
#if UWP
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.DevVersion, Consts.StoreVersion, hash, now);
#else
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.DevVersion, Consts.NormalVersion, hash, now);
#endif

                        cache.LastUpdated = now;
                        SaveCache();
                        return cache;
                    }
                    else
                    {
                        // equal
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.NoUpdateAvaialble, onlineVersion, hash, now);
                        SaveCache();
                        return cache;
                    }
                }
            }
            catch (Exception)
            {
                // ignore failed to get updates
            }

            cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.Unknown, null, string.Empty, now);
            SaveCache();
            return cache;
        }

        private static void SaveCache()
        {
            try
            {
                Serialization.Save(Consts.UpdateCacheFilePath, cache);
            }
            catch { }
        }

        public static void SearchForUpdates()
        {
            var result = Task.Run(() => CheckForUpdatesAsync()).Result;

            if (result.Result == SimpleJournal.Common.UpdateResult.UpdateAvailable && result.Version != null)
            {
                UpdateDialog ud = new UpdateDialog(result.Version, result.SHA256Hash);
                ud.ShowDialog();
            }
        }

        internal static string GetLastHash() => cache?.SHA256Hash;
    }
}