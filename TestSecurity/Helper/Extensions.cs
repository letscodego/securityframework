using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace TestSecurity.Helper
{
    public static class Extensions
    {
        public static string GetDomain(this string identity)
        {
            string s = identity;
            int stop = s.IndexOf("\\");
            return (stop > -1) ? s.Substring(0, stop).ToLower() : string.Empty;
        }

        public static string GetLogin(this string identity)
        {
            string s = identity;
            int stop = s.IndexOf("\\");
            return (stop > -1) ? s.Substring(stop + 1, s.Length - stop - 1).ToLower() : string.Empty;
        }
    }
}