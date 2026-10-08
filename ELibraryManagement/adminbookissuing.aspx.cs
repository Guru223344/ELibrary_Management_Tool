using System;
using System.Data;
using System.Data.SqlClient;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class adminbookissuing : AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            TextBox5.Text = DateTime.Today.ToString("yyyy-MM-dd");
            TextBox6.Text = DateTime.Today.AddDays(14).ToString("yyyy-MM-dd");
            BindGrid();
        }

        void BindGrid()
        {
            try
            {
                GridView1.DataSource = Db.Query(
                    "SELECT TOP 200 member_id AS [Member], member_name AS [Name], book_id AS [Book ID], book_name AS [Title], issue_date AS [Issued], " +
                    "due_date AS [Due], return_date AS [Returned], fine_amount AS [Fine], status AS [Status] FROM vw_issued_books ORDER BY issue_id DESC");
                GridView1.DataBind();
            }
            catch (Exception ex) { Fail(ex); }
        }

        string MemberId { get { return TextBox2.Text.Trim(); } }
        string BookId { get { return TextBox3.Text.Trim(); } }

        // Go: look up names
        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                object m = Db.Scalar("SELECT full_name FROM member_master_tbl WHERE member_id=@id", Db.P("@id", MemberId));
                object b = Db.Scalar("SELECT book_name FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId));
                TextBox1.Text = m == null ? "" : (string)m;
                TextBox4.Text = b == null ? "" : (string)b;
                if (m == null || b == null) Alert("Invalid " + (m == null ? "Member ID" : "Book ID") + ".");
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Issue
        protected void Button2_Click(object sender, EventArgs e)
        {
            DateTime issue, due;
            if (MemberId.Length == 0 || BookId.Length == 0) { Alert("Member ID and Book ID are required."); return; }
            if (!TryDate(TextBox5.Text, out issue) || !TryDate(TextBox6.Text, out due)) { Alert("Please choose valid start and end dates."); return; }
            try
            {
                Db.Proc("sp_IssueBook", Db.P("@member_id", MemberId), Db.P("@book_id", BookId), Db.P("@issue_date", issue), Db.P("@due_date", due));
                Alert("Book issued."); BindGrid();
            }
            catch (SqlException ex) when (ex.Number >= 50000) { Alert(ex.Message); }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627) { Alert("This member already has this book issued."); }
            catch (Exception ex) { Fail(ex); }
        }

        // Return
        protected void Button3_Click(object sender, EventArgs e)
        {
            if (MemberId.Length == 0 || BookId.Length == 0) { Alert("Member ID and Book ID are required."); return; }
            try
            {
                DataTable dt = Db.Proc("sp_ReturnBook", Db.P("@member_id", MemberId), Db.P("@book_id", BookId), Db.P("@return_date", DateTime.Today));
                decimal fine = Convert.ToDecimal(dt.Rows[0]["fine_amount"]);
                Alert(fine > 0 ? "Book returned. Late fine: Rs " + fine.ToString("0.00") : "Book returned. No fine.");
                BindGrid();
            }
            catch (SqlException ex) when (ex.Number >= 50000) { Alert(ex.Message); }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
