using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Mail;
using System.Web;
using System.Web.UI;

namespace ELibraryManagement.Infrastructure
{
    /// <summary>Common helpers: safe alerts, error handling, parsing.</summary>
    public class BasePage : Page
    {
        protected void Alert(string message)
        {
            ClientScript.RegisterStartupScript(GetType(), Guid.NewGuid().ToString("N"),
                "alert(" + HttpUtility.JavaScriptStringEncode(message, true) + ");", true);
        }

        /// <summary>Logs the real error server-side and shows the user a generic message.</summary>
        protected void Fail(Exception ex)
        {
            System.Diagnostics.Trace.TraceError(ex.ToString());
            Alert("Something went wrong. Please try again.");
        }

        protected static bool IsFkViolation(Exception ex)
        {
            var s = ex as SqlException;
            return s != null && s.Number == 547;
        }

        protected static bool TryDate(string text, out DateTime date)
        {
            return DateTime.TryParseExact((text ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        protected static bool IsEmail(string text)
        {
            try { return new MailAddress(text).Address == text; } catch { return false; }
        }
    }

    /// <summary>Only signed-in admins may load pages that derive from this.</summary>
    public class AdminPage : BasePage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!Auth.IsAdmin) Response.Redirect("AdminLogin.aspx", true);
        }
    }

    /// <summary>Only signed-in members.</summary>
    public class MemberPage : BasePage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (!Auth.IsMember) Response.Redirect("UserLogin.aspx", true);
        }
    }

    /// <summary>Any signed-in user (member or admin).</summary>
    public class LoggedInPage : BasePage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (Auth.Role == "") Response.Redirect("UserLogin.aspx", true);
        }
    }
}
