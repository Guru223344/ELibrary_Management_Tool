using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class adminpublishermanagement : AdminPage
    {
        string Id { get { return TextBox3.Text.Trim(); } }
        string Name { get { return TextBox2.Text.Trim(); } }

        bool Exists()
        {
            return Convert.ToInt32(Db.Scalar("SELECT COUNT(1) FROM publisher_master_tbl WHERE publisher_Id=@id", Db.P("@id", Id))) > 0;
        }

        void Clear() { TextBox2.Text = ""; TextBox3.Text = ""; }

        bool Valid()
        {
            if (Id.Length == 0 || Name.Length == 0) { Alert("Publisher ID and name are required."); return false; }
            return true;
        }

        // Go
        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = Db.Query("SELECT publisher_name FROM publisher_master_tbl WHERE publisher_Id=@id", Db.P("@id", Id));
                if (dt.Rows.Count == 0) { Alert("Invalid Publisher ID."); Clear(); return; }
                TextBox2.Text = (string)dt.Rows[0][0];
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Add
        protected void Button2_Click(object sender, EventArgs e)
        {
            if (!Valid()) return;
            try
            {
                if (Exists()) { Alert("Publisher ID already exists. Try another one."); return; }
                Db.Exec("INSERT INTO publisher_master_tbl(publisher_Id,publisher_name) VALUES(@id,@name)", Db.P("@id", Id), Db.P("@name", Name));
                Alert("Publisher added."); Clear(); GridView1.DataBind();
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Update
        protected void Button3_Click(object sender, EventArgs e)
        {
            if (!Valid()) return;
            try
            {
                if (!Exists()) { Alert("Publisher ID does not exist."); return; }
                Db.Exec("UPDATE publisher_master_tbl SET publisher_name=@name WHERE publisher_Id=@id", Db.P("@id", Id), Db.P("@name", Name));
                Alert("Publisher updated."); Clear(); GridView1.DataBind();
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Delete
        protected void Button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Exists()) { Alert("Publisher ID does not exist."); return; }
                Db.Exec("DELETE FROM publisher_master_tbl WHERE publisher_Id=@id", Db.P("@id", Id));
                Alert("Publisher deleted."); Clear(); GridView1.DataBind();
            }
            catch (Exception ex)
            {
                if (IsFkViolation(ex)) Alert("This Publisher is linked to books and cannot be deleted.");
                else Fail(ex);
            }
        }
    }
}
