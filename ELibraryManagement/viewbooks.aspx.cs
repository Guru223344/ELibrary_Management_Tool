using System;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class viewbooks : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try
            {
                GridView1.DataSource = Db.Query(
                    "SELECT book_id AS [ID], book_name AS [Title], author_name AS [Author], publisher_name AS [Publisher], genre AS [Genre], " +
                    "book_language AS [Language], available AS [Available] FROM vw_book_catalog ORDER BY book_name");
                GridView1.DataBind();
            }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
