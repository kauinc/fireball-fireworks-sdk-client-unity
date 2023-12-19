using System;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
using AOT;
#endif

namespace Fireball.Game.Client.Modules
{
    public static class WebBrowser
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")] private static extern string userAgent();
        [DllImport("__Internal")] private static extern string platform();
        [DllImport("__Internal")] private static extern string vendor();

        [DllImport("__Internal")] private static extern bool isiPhone();
        [DllImport("__Internal")] private static extern bool isiPad();
        [DllImport("__Internal")] private static extern bool isiPod();

        [DllImport("__Internal")] private static extern bool isAndroid();
        [DllImport("__Internal")] private static extern bool isAndroidPhone();
        [DllImport("__Internal")] private static extern bool isAndroidTablet();

        [DllImport("__Internal")] private static extern bool isWindows();
        [DllImport("__Internal")] private static extern bool isWindowsPhone();
        [DllImport("__Internal")] private static extern bool isWindowsTablet();

        [DllImport("__Internal")] private static extern bool isBlackberry();

        [DllImport("__Internal")] private static extern bool isIE();
        [DllImport("__Internal")] private static extern bool isEdge();
        [DllImport("__Internal")] private static extern bool isChrome();
        [DllImport("__Internal")] private static extern bool isSafari();
        [DllImport("__Internal")] private static extern bool isFirefox();
        [DllImport("__Internal")] private static extern bool isOpera();
        [DllImport("__Internal")] private static extern bool isOperaMini();
        [DllImport("__Internal")] private static extern bool isOnline();

        [DllImport("__Internal")] private static extern void sendBeacon(string url, string json);
        [DllImport("__Internal")] private static extern void postMessage(string msg);
        [DllImport("__Internal")] private static extern void setLocation(string url);

        [DllImport("__Internal")] private static extern bool inIFrame();
        [DllImport("__Internal")] private static extern bool isFullScreen();
        [DllImport("__Internal")] private static extern bool isTabActive();
        [DllImport("__Internal")] private static extern void onTabVisibility(Action<int> callback);
        [DllImport("__Internal")] private static extern void enterFullScreen();
        [DllImport("__Internal")] private static extern void exitFullScreen();

        [DllImport("__Internal")] private static extern void reloadPage();

        public static string UserAgent => userAgent();
        public static string Platform => platform();
        public static string Vendor => vendor();

        public static bool IsDesktop => !IsMobile && !IsTablet;
        public static bool IsMobile => IsiPhone || IsiPod || IsAndroidPhone || IsBlackberry || IsWindowsPhone;
        public static bool IsTablet => IsiPad || IsAndroidTablet || IsWindowsTablet;

        public static bool IsiOS => IsiPad || IsiPod || IsiPhone;
        public static bool IsiPhone => isiPhone() && !IsiPad;
        public static bool IsiPad => isiPad();
        public static bool IsiPod => isiPod();

        public static bool IsAndroid => isAndroid();
        public static bool IsAndroidPhone => isAndroidPhone();
        public static bool IsAndroidTablet => isAndroidTablet();

        public static bool IsWindows => isWindows();
        public static bool IsWindowsPhone => IsWindows && isWindowsPhone();
        public static bool IsWindowsTablet => IsWindows && !IsWindowsPhone && isWindowsTablet();

        public static bool IsBlackberry => isBlackberry();

        public static bool IsIE => isIE();
        public static bool IsEdge => isEdge();
        public static bool IsChrome => isChrome() && !IsOpera;
        public static bool IsSafari => isSafari();
        public static bool IsFirefox => isFirefox();
        public static bool IsOpera => isOpera();
        public static bool IsOperaMini => isOperaMini();
        public static bool IsOnline => isOnline();


        public static void SendBeacon(string url, string json)
        {
            sendBeacon(url, json);
        }

        public static void PostMessage(string msg)
        {
            postMessage(msg);
        }

        public static void SetLocation(string url)
        {
            setLocation(url);
        }

        public static bool InIFrame => inIFrame();
        public static bool IsFullScreen => isFullScreen();
        public static bool IsTabActive => isTabActive();

        private static Action<bool> _tabVisibilityCallback;

        [MonoPInvokeCallback(typeof(Action<int>))]
        public static void TabVisibilityCallback(int visible)
        {
            _tabVisibilityCallback?.Invoke(visible == 1);
        }
        public static void OnTabVisibility(Action<bool> callback)
        {
            _tabVisibilityCallback += callback;
            onTabVisibility(TabVisibilityCallback);
        }
        public static void RemoveTabVisibility(Action<bool> callback)
        {
            _tabVisibilityCallback -= callback;
        }
        public static void ToggleFullScreen()
        {
            if (IsFullScreen) ExitFullScreen();
            else EnterFullScreen();
        }
        public static void EnterFullScreen()
        {
            enterFullScreen();
        }
        public static void ExitFullScreen()
        {
            exitFullScreen();
        }
        public static void ReloadPage()
        {
            reloadPage();
        }

#else

        public static string UserAgent => "";
        public static string Platform => "";
        public static string Vendor => "";

        public static bool IsDesktop => false;
        public static bool IsMobile => false;
        public static bool IsTablet => false;

        public static bool IsiOS => false;
        public static bool IsiPhone => false;
        public static bool IsiPad => false;
        public static bool IsiPod => false;

        public static bool IsAndroid => false;
        public static bool IsAndroidPhone => false;
        public static bool IsAndroidTablet => false;

        public static bool IsWindows => false;
        public static bool IsWindowsPhone => false;
        public static bool IsWindowsTablet => false;

        public static bool IsBlackberry => false;

        public static bool IsIE => false;
        public static bool IsEdge => false;
        public static bool IsChrome => false;
        public static bool IsSafari => false;
        public static bool IsFirefox => false;
        public static bool IsOpera => false;
        public static bool IsOperaMini => false;
        public static bool IsOnline => UnityEngine.Application.internetReachability != UnityEngine.NetworkReachability.NotReachable;

        public static void SendBeacon(string url, string json) { }
        public static void PostMessage(string msg) { }
        public static void SetLocation(string url) { }

        public static bool InIFrame => false;
        public static bool IsFullScreen => false;
        public static bool IsTabActive => true;
        public static void OnTabVisibility(Action<bool> callback) { }
        public static void RemoveTabVisibility(Action<bool> callback) { }
        public static void ToggleFullScreen() { }
        public static void EnterFullScreen() { }
        public static void ExitFullScreen() { }

        public static void ReloadPage() { }

#endif

    }
}
