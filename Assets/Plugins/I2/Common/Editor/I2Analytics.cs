using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace I2
{
    public static class I2Analytics
	{
        public static Dictionary<string, string> PluginsVersion = new Dictionary<string, string>(StringComparer.Ordinal);

        public static List<WWW> wwwAnalytics = new List<WWW>();

        public static string GetPluginVersion_AssetStore( string pluginName )
        {
            if (I2AboutWindow.PluginsData == null)
                return null;
            for (int i = 0; i < I2AboutWindow.PluginsData.Plugins.Length; i++)
                if (I2AboutWindow.PluginsData.Plugins[i].Name == pluginName)
                    return I2AboutWindow.PluginsData.Plugins[i].AssetStoreVersion;
            return null;
        }
        public static string GetPluginVersion_Beta(string pluginName)
        {
            if (I2AboutWindow.PluginsData == null)
                return null;
            for (int i = 0; i < I2AboutWindow.PluginsData.Plugins.Length; i++)
                if (I2AboutWindow.PluginsData.Plugins[i].Name == pluginName)
                    return I2AboutWindow.PluginsData.Plugins[i].BetaVersion;
            return null;
        }

        public static bool HasNewVersion( string pluginName )
        {
            if (I2AboutWindow.PluginsData == null)
                return false;
            for (int i = 0; i < I2AboutWindow.PluginsData.Plugins.Length; i++)
                if (I2AboutWindow.PluginsData.Plugins[i].Name == pluginName)
                {
                    string InstalledVersion;
                    bool ShouldUpgrade, HasNewBeta, ShouldSkip;
                    I2AboutWindow.GetShouldUpgrade(I2AboutWindow.PluginsData.Plugins[i], out InstalledVersion, out ShouldUpgrade, out HasNewBeta, out ShouldSkip);
                    if (!ShouldUpgrade && !ShouldSkip && HasNewBeta && I2AboutWindow.bNotifyOfNewBetas)
                        ShouldUpgrade = true;
                    return ShouldUpgrade;
                }
            return false;
        }


        public static void CheckAnalyticsResult()
        {
            wwwAnalytics.RemoveAll(a => 
                {
                    if (!a.isDone)
                        return false;

                    if (string.IsNullOrEmpty(a.error))
                    {
                        int i1 = a.url.IndexOf("&ec=");
                        int i2 = a.url.IndexOf("&ea=", i1);
                        if (i1 < 0 || i2 < 0)
                            return false;

                        i1 += "&ec=".Length;
                        string pluginName = Uri.UnescapeDataString(a.url.Substring(i1, i2 - i1));
                        string uVersion = GetUnityVersion();
                        string key = "LastUsed " + uVersion + pluginName;
                        EditorPrefs.SetString(key, DateTime.Now.ToString());
                    }
                    return true;
                });
            if (wwwAnalytics.Count <= 0)
                EditorApplication.update -= CheckAnalyticsResult;
        }

        public static void SendAnalytics( string pluginName, string version )
        {
            string uVersion = GetUnityVersion();
            string key = "LastUsed " +uVersion + pluginName;
            string LastTime = EditorPrefs.GetString(key, "");

            bool ShouldSend = true;
            if (!string.IsNullOrEmpty(LastTime))
            {
                DateTime LastDate;
                if (!DateTime.TryParse(LastTime, out LastDate))
                    ShouldSend = true;
                else
                {
                    //double days = (DateTime.Now - LastDate).TotalDays;
                    //ShouldSend = (days >= 1);
                    ShouldSend = DateTime.Now.DayOfYear != LastDate.DayOfYear;
                }
            }

            if (!ShouldSend)
                return;

            string userID = SystemInfo.deviceUniqueIdentifier;
            string unityVersion = "Unity " + uVersion;
            string url = string.Format("http://www.google-analytics.com/collect?v=1&tid=UA-40895130-2&t=event&cid={0}&ec={1}&ea={2}&el={3}", Uri.EscapeUriString(userID), Uri.EscapeUriString(pluginName), Uri.EscapeUriString(unityVersion), Uri.EscapeUriString(version));
            wwwAnalytics.Add( new WWW(url) );
            EditorApplication.update += CheckAnalyticsResult;
        }

        public static string GetUnityVersion()
        {
            string unityVersion = Application.unityVersion;
            for (int i=0; i<unityVersion.Length; ++i)
                if (!char.IsDigit(unityVersion[i]) && unityVersion[i]!='.')
                    return unityVersion.Substring(0, i);
            return unityVersion;
        }


    }
}