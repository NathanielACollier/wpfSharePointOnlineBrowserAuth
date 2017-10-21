﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace SharePointOnlineWebBrowserAuth
{
    /// <summary>
    /// WinInet.dll wrapper
    /// </summary>
    internal static class CookieReader
    {
        /// <summary>
        /// Enables the retrieval of cookies that are marked as "HTTPOnly". 
        /// Do not use this flag if you expose a scriptable interface, 
        /// because this has security implications. It is imperative that 
        /// you use this flag only if you can guarantee that you will never 
        /// expose the cookie to third-party code by way of an 
        /// extensibility mechanism you provide. 
        /// Version:  Requires Internet Explorer 8.0 or later.
        /// </summary>
        private const int INTERNET_COOKIE_HTTPONLY = 0x00002000;

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(
            string url,
            string cookieName,
            StringBuilder cookieData,
            ref int size,
            int flags,
            IntPtr pReserved);

        /// <summary>
        /// Returns cookie contents as a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCookie(string url)
        {
            
            int size = 512;
            StringBuilder sb = new StringBuilder(size);
            if (!InternetGetCookieEx(url, null, sb, ref size, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero))
            {
                if (size < 0)
                {
                    return null;
                }
                sb = new StringBuilder(size);
                if (!InternetGetCookieEx(url, null, sb, ref size, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero))
                {
                    return null;
                }
            }
            return sb.ToString();
        }


        public static CookieCollection GetCookieCollection(string url)
        {
            Uri uriBase = new Uri(url);
            Uri uri = new Uri(uriBase, "/");
            return GetCookieCollection(uri);
        }

        public static CookieCollection GetCookieCollection(Uri uri)
        {

            return GetCookieContainer(uri).GetCookies(uri);
        }

        public static CookieContainer GetCookieContainer(Uri uri)
        {
            // call WinInet.dll to get cookie.
            string stringCookie = GetCookie(uri.ToString());
            if (string.IsNullOrEmpty(stringCookie)) return null;
            stringCookie = stringCookie.Replace("; ", ",").Replace(";", ",");
            // use CookieContainer to parse the string cookie to CookieCollection
            CookieContainer cookieContainer = new CookieContainer();
            cookieContainer.SetCookies(uri, stringCookie);
            return cookieContainer;
        }

    }
}
