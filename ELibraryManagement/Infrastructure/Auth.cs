using System;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;

namespace ELibraryManagement.Infrastructure
{
    /// <summary>Session-based identity plus a simple brute-force lockout.</summary>
    public static class Auth
    {
        static HttpSessionState S { get { return HttpContext.Current.Session; } }
        static string Get(string key) { return (S[key] as string) ?? ""; }

        public static string Role { get { return Get("role"); } }
        public static string UserId { get { return Get("username"); } }
        public static string FullName { get { return Get("full_name"); } }
        public static bool IsAdmin { get { return Role == "admin"; } }
        public static bool IsMember { get { return Role == "user"; } }

        public static void SignIn(string role, string userId, string fullName)
        {
            S.Clear();
            S["role"] = role;
            S["username"] = userId;
            S["full_name"] = fullName;
        }

        public static void SignOut() { S.Clear(); S.Abandon(); }

        const int MaxFailures = 5;
        public static bool IsLocked(string key)
        {
            int n = (HttpRuntime.Cache["fail:" + key] as int?) ?? 0;
            return n >= MaxFailures;
        }

        public static void RecordFailure(string key)
        {
            int n = (HttpRuntime.Cache["fail:" + key] as int?) ?? 0;
            HttpRuntime.Cache.Insert("fail:" + key, n + 1, null, DateTime.UtcNow.AddMinutes(10), Cache.NoSlidingExpiration);
        }

        public static void ClearFailures(string key) { HttpRuntime.Cache.Remove("fail:" + key); }
    }
}
