using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class AdminLogin : BasePage
    {
        protected void Button1_Click(object sender, EventArgs e)
        {
            string user = TextBox1.Text.Trim();
            string lockKey = "admin:" + user.ToLowerInvariant();
            if (Auth.IsLocked(lockKey)) { Alert("Too many failed attempts. Please try again in 10 minutes."); return; }
            try
            {
                DataTable dt = Db.Query("SELECT username, full_name, password_hash FROM admin_login_tbl WHERE username=@u", Db.P("@u", user));
                if (dt.Rows.Count == 1 && PasswordHasher.Verify(TextBox2.Text, (string)dt.Rows[0]["password_hash"]))
                {
                    Auth.ClearFailures(lockKey);
                    Auth.SignIn("admin", (string)dt.Rows[0]["username"], (string)dt.Rows[0]["full_name"]);
                    Response.Redirect("homepage.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    Auth.RecordFailure(lockKey);
                    Alert("Invalid credentials.");
                }
            }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
