using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class userprofile : MemberPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) LoadProfile();
        }

        void LoadProfile()
        {
            try
            {
                DataTable dt = Db.Query("SELECT full_name, dob, contact_no, email, state, city, pincode, full_address, account_status FROM member_master_tbl WHERE member_id=@id", Db.P("@id", Auth.UserId));
                if (dt.Rows.Count == 0) { Auth.SignOut(); Response.Redirect("UserLogin.aspx"); return; }
                DataRow r = dt.Rows[0];
                TextBox3.Text = (string)r["full_name"];
                TextBox2.Text = ((DateTime)r["dob"]).ToString("yyyy-MM-dd");
                TextBox1.Text = (string)r["contact_no"];
                TextBox4.Text = (string)r["email"];
                var state = DropDownList1.Items.FindByValue((string)r["state"]); if (state != null) DropDownList1.SelectedValue = state.Value;
                TextBox6.Text = (string)r["city"];
                TextBox7.Text = (string)r["pincode"];
                TextBox5.Text = (string)r["full_address"];
                TextBox8.Text = Auth.UserId;
                Label1.Text = "Account status: " + (string)r["account_status"];

                DataTable books = Db.Query(
                    "SELECT book_id AS [Book ID], book_name AS [Title], issue_date AS [Issued], due_date AS [Due], return_date AS [Returned], fine_amount AS [Fine], status AS [Status] " +
                    "FROM vw_issued_books WHERE member_id=@id ORDER BY issue_id DESC", Db.P("@id", Auth.UserId));
                GridView1.DataSource = books;
                GridView1.DataBind();
                Label2.Text = books.Select("Status <> 'Returned'").Length + " book(s) currently with you";
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Update
        protected void Button2_Click(object sender, EventArgs e)
        {
            DateTime dob;
            if (TextBox3.Text.Trim().Length < 2) { Alert("Please enter your full name."); return; }
            if (!TryDate(TextBox2.Text, out dob)) { Alert("Please enter a valid date of birth."); return; }
            if (!Regex.IsMatch(TextBox1.Text.Trim(), @"^\d{10}$")) { Alert("Contact number must be 10 digits."); return; }
            if (!IsEmail(TextBox4.Text.Trim())) { Alert("Please enter a valid email address."); return; }
            if (DropDownList1.SelectedValue == "select") { Alert("Please select your state."); return; }
            if (!Regex.IsMatch(TextBox7.Text.Trim(), @"^\d{6}$")) { Alert("Pincode must be 6 digits."); return; }

            string newPwd = TextBox10.Text;
            try
            {
                if (newPwd.Length > 0)
                {
                    string stored = (string)Db.Scalar("SELECT password_hash FROM member_master_tbl WHERE member_id=@id", Db.P("@id", Auth.UserId));
                    if (!PasswordHasher.Verify(TextBox9.Text, stored)) { Alert("Old password is incorrect."); return; }
                    if (newPwd.Length < 8 || !Regex.IsMatch(newPwd, "[A-Za-z]") || !Regex.IsMatch(newPwd, "[0-9]")) { Alert("New password needs at least 8 characters with letters and digits."); return; }
                }
                Db.Exec("UPDATE member_master_tbl SET full_name=@name, dob=@dob, contact_no=@contact, email=@email, state=@state, city=@city, pincode=@pin, full_address=@addr, " +
                        "password_hash=CASE WHEN @hash IS NULL THEN password_hash ELSE @hash END WHERE member_id=@id",
                    Db.P("@name", TextBox3.Text.Trim()), Db.P("@dob", dob), Db.P("@contact", TextBox1.Text.Trim()), Db.P("@email", TextBox4.Text.Trim()),
                    Db.P("@state", DropDownList1.SelectedValue), Db.P("@city", TextBox6.Text.Trim()), Db.P("@pin", TextBox7.Text.Trim()),
                    Db.P("@addr", TextBox5.Text.Trim()), Db.P("@hash", newPwd.Length > 0 ? PasswordHasher.Hash(newPwd) : null), Db.P("@id", Auth.UserId));
                Auth.SignIn("user", Auth.UserId, TextBox3.Text.Trim());
                LoadProfile();
                Alert("Profile updated.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) { Alert("That email is already used by another member."); }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
