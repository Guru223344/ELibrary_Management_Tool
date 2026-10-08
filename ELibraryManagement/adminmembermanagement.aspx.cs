using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class adminmembermanagement : AdminPage
    {
        string Id { get { return TextBox3.Text.Trim(); } }

        void Clear()
        {
            foreach (var t in new[] { TextBox1, TextBox2, TextBox3, TextBox4, TextBox5, TextBox6, TextBox7, TextBox8, TextBox9, TextBox10 }) t.Text = "";
        }

        // Go
        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = Db.Query("SELECT full_name, dob, contact_no, email, state, city, pincode, full_address, account_status FROM member_master_tbl WHERE member_id=@id", Db.P("@id", Id));
                if (dt.Rows.Count == 0) { Alert("Invalid Member ID."); return; }
                DataRow r = dt.Rows[0];
                TextBox2.Text = (string)r["full_name"];
                TextBox1.Text = ((DateTime)r["dob"]).ToString("yyyy-MM-dd");
                TextBox5.Text = (string)r["contact_no"];
                TextBox4.Text = (string)r["email"];
                TextBox6.Text = (string)r["state"];
                TextBox9.Text = (string)r["city"];
                TextBox8.Text = (string)r["pincode"];
                TextBox10.Text = (string)r["full_address"];
                TextBox7.Text = (string)r["account_status"];
            }
            catch (Exception ex) { Fail(ex); }
        }

        void SetStatus(string status)
        {
            try
            {
                int rows = Db.Exec("UPDATE member_master_tbl SET account_status=@s WHERE member_id=@id", Db.P("@s", status), Db.P("@id", Id));
                if (rows == 0) { Alert("Invalid Member ID."); return; }
                TextBox7.Text = status;
                GridView1.DataBind();
                Alert("Member status updated to " + status + ".");
            }
            catch (Exception ex) { Fail(ex); }
        }

        protected void Button4_Click(object sender, EventArgs e) { SetStatus("Active"); }
        protected void LinkButton1_Click(object sender, EventArgs e) { SetStatus("Pending"); }
        protected void LinkButton2_Click(object sender, EventArgs e) { SetStatus("Deactive"); }

        // Delete permanently
        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                int rows = Db.Exec("DELETE FROM member_master_tbl WHERE member_id=@id", Db.P("@id", Id));
                if (rows == 0) { Alert("Invalid Member ID."); return; }
                Clear(); GridView1.DataBind();
                Alert("Member deleted.");
            }
            catch (Exception ex)
            {
                if (IsFkViolation(ex)) Alert("This member has issue history and cannot be deleted. Deactivate the account instead.");
                else Fail(ex);
            }
        }
    }
}
