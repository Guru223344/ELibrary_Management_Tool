using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

using System.Text.RegularExpressions;

namespace ELibraryManagement
{
    public partial class usersignup : BasePage
    {
        protected void Button1_Click(object sender, EventArgs e)
        {
            string name = TextBox3.Text.Trim(), id = TextBox8.Text.Trim(), email = TextBox4.Text.Trim(), pwd = TextBox9.Text;
            DateTime dob;
            if (name.Length < 2) { Alert("Please enter your full name."); return; }
            if (!TryDate(TextBox2.Text, out dob) || dob > DateTime.Today.AddYears(-5)) { Alert("Please enter a valid date of birth."); return; }
            if (!Regex.IsMatch(TextBox1.Text.Trim(), @"^\d{10}$")) { Alert("Contact number must be 10 digits."); return; }
            if (!IsEmail(email)) { Alert("Please enter a valid email address."); return; }
            if (DropDownList1.SelectedValue == "select") { Alert("Please select your state."); return; }
            if (TextBox6.Text.Trim().Length == 0 || TextBox5.Text.Trim().Length == 0) { Alert("City and address are required."); return; }
            if (!Regex.IsMatch(TextBox7.Text.Trim(), @"^\d{6}$")) { Alert("Pincode must be 6 digits."); return; }
            if (!Regex.IsMatch(id, @"^[A-Za-z0-9_.-]{4,30}$")) { Alert("Member ID must be 4-30 characters: letters, digits, '.', '_' or '-'."); return; }
            if (pwd.Length < 8 || !Regex.IsMatch(pwd, "[A-Za-z]") || !Regex.IsMatch(pwd, "[0-9]")) { Alert("Password needs at least 8 characters with letters and digits."); return; }

            try
            {
                Db.Exec("INSERT INTO member_master_tbl(full_name,dob,contact_no,email,state,city,pincode,full_address,member_id,password_hash,account_status) " +
                        "VALUES(@name,@dob,@contact,@email,@state,@city,@pin,@addr,@id,@hash,'Pending')",
                    Db.P("@name", name), Db.P("@dob", dob), Db.P("@contact", TextBox1.Text.Trim()), Db.P("@email", email),
                    Db.P("@state", DropDownList1.SelectedValue), Db.P("@city", TextBox6.Text.Trim()), Db.P("@pin", TextBox7.Text.Trim()),
                    Db.P("@addr", TextBox5.Text.Trim()), Db.P("@id", id), Db.P("@hash", PasswordHasher.Hash(pwd)));
                Alert("Sign-up successful! An admin will activate your account, then you can log in.");
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                Alert("That Member ID or email is already registered.");
            }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
