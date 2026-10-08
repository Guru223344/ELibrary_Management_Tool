using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class UserLogin : BasePage
    {
        protected void Button1_Click(object sender, EventArgs e)
        {
            string id = TextBox1.Text.Trim();
            string lockKey = "user:" + id.ToLowerInvariant();
            if (Auth.IsLocked(lockKey)) { Alert("Too many failed attempts. Please try again in 10 minutes."); return; }
            try
            {
                DataTable dt = Db.Query("SELECT member_id, full_name, password_hash, account_status FROM member_master_tbl WHERE member_id=@id", Db.P("@id", id));
                if (dt.Rows.Count != 1 || !PasswordHasher.Verify(TextBox2.Text, (string)dt.Rows[0]["password_hash"]))
                {
                    Auth.RecordFailure(lockKey);
                    Alert("Invalid credentials.");
                    return;
                }
                string status = (string)dt.Rows[0]["account_status"];
                if (status == "Pending") { Alert("Your account is awaiting admin approval."); return; }
                if (status != "Active") { Alert("Your account is deactivated. Please contact the library."); return; }

                Auth.ClearFailures(lockKey);
                Auth.SignIn("user", (string)dt.Rows[0]["member_id"], (string)dt.Rows[0]["full_name"]);
                Response.Redirect("homepage.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
