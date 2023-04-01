using Newtonsoft.Json;
using SimpleJournal.Dialogs;
using SimpleJournal;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Data;

namespace Helper
{
    public class UpdateHelper
    {
        private static UpdateInfo cache = null;
        private static DateTime lastTimeCachedVersion = DateTime.MinValue;

        /// <summary>
        /// Checks if there is a new SimpleJournal version available
        /// </summary>
        /// <returns><see cref="UpdateInfo"/></returns>
        public static async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            var now = DateTime.Now;
            bool isCacheExpired = (lastTimeCachedVersion > DateTime.MinValue && lastTimeCachedVersion.AddMinutes(30) < now);

            // Only return cache if it is valid and only if it is valid
            if (cache != null && cache.Result != SimpleJournal.Common.UpdateResult.Unknown && !isCacheExpired)
                return cache;

            if (!GeneralHelper.IsConnectedToInternet())
            {
                // Even if the cache is expired (if it has valid value) return it, to prevent overwriting valid results due to a lost connection!
                if (isCacheExpired && cache != null && cache.Result != SimpleJournal.Common.UpdateResult.Unknown)
                    return cache;

                cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.Unknown, null);
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
                    dynamic versions = JsonConvert.DeserializeObject(versionJSON);

#if !UWP
                    Version onlineVersion = Version.Parse(versions.current.normal.Value);
#else
                    Version onlineVersion = Version.Parse(versions.current.store.Value);
#endif

                    var result = onlineVersion.CompareTo(currentVersion);
                    if (result > 0)
                    {
                        // There is a new version
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.UpdateAvailable, onlineVersion);
                        lastTimeCachedVersion = now;
                        return cache;
                    }
                    else if (result < 0)
                    {
                        // Online version is older than this version (dev version)
#if UWP
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.DevVersion, Consts.StoreVersion);
#else
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.DevVersion, Consts.NormalVersion);
#endif

                        lastTimeCachedVersion = now;
                        return cache;
                    }
                    else
                    {
                        // equal
                        cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.NoUpdateAvaialble, onlineVersion);
                        lastTimeCachedVersion = now;
                        return cache;
                    }
                }
            }
            catch (Exception)
            {
                // ignore failed to get updates
            }

            cache = new UpdateInfo(SimpleJournal.Common.UpdateResult.Unknown, null);
            lastTimeCachedVersion = now;
            return cache;
        }

        public static void SearchForUpdates()
        {
            var result = Task.Run(() => CheckForUpdatesAsync()).Result;

            if (result.Result == SimpleJournal.Common.UpdateResult.UpdateAvailable && result.Version != null)
            {
                UpdateDialog ud = new UpdateDialog(result.Version);
                ud.ShowDialog();
            }
        }
    }
}
