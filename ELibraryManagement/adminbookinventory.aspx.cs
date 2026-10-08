using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class adminbookinventory : AdminPage
    {
        static readonly string[] AllowedImages = { ".jpg", ".jpeg", ".png", ".webp" };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;
            try
            {
                Bind(DropDownList2, "SELECT publisher_Id AS id, publisher_name AS name FROM publisher_master_tbl ORDER BY publisher_name");
                Bind(DropDownList3, "SELECT author_id AS id, author_name AS name FROM author_master_tbl ORDER BY author_name");
                BindGrid();
            }
            catch (Exception ex) { Fail(ex); }
        }

        static void Bind(DropDownList list, string sql)
        {
            list.DataSource = Db.Query(sql);
            list.DataValueField = "id";
            list.DataTextField = "name";
            list.DataBind();
        }

        void BindGrid()
        {
            GridView1.DataSource = Db.Query(
                "SELECT book_id AS [ID], book_name AS [Title], author_name AS [Author], publisher_name AS [Publisher], genre AS [Genre], " +
                "book_language AS [Language], actual_stock AS [Total], current_stock AS [Available] FROM vw_book_catalog ORDER BY book_name");
            GridView1.DataBind();
        }

        string BookId { get { return TextBox3.Text.Trim(); } }

        bool Exists() { return Convert.ToInt32(Db.Scalar("SELECT COUNT(1) FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId))) > 0; }

        /// <summary>Validates the form; returns false (after alerting) if anything is wrong.</summary>
        bool ReadForm(out DateTime? publishDate, out decimal cost, out int pages, out int stock)
        {
            publishDate = null; cost = 0; pages = 0; stock = 0;
            DateTime d;
            if (BookId.Length == 0 || TextBox2.Text.Trim().Length == 0) { Alert("Book ID and name are required."); return false; }
            if (TextBox1.Text.Trim().Length > 0) { if (!TryDate(TextBox1.Text, out d)) { Alert("Invalid publish date."); return false; } publishDate = d; }
            if (!decimal.TryParse(TextBox6.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out cost) || cost < 0) { Alert("Enter a valid book cost."); return false; }
            if (!int.TryParse(TextBox7.Text, out pages) || pages <= 0) { Alert("Enter a valid number of pages."); return false; }
            if (!int.TryParse(TextBox8.Text, out stock) || stock < 0) { Alert("Enter a valid actual stock."); return false; }
            if (DropDownList2.Items.Count == 0 || DropDownList3.Items.Count == 0) { Alert("Add at least one author and one publisher first."); return false; }
            return true;
        }

        string Genres { get { return string.Join(", ", ListBox1.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value)); } }

        /// <summary>Saves the uploaded cover (validated, random file name). Returns null when nothing was uploaded.</summary>
        string SaveCover()
        {
            if (!FileUpload1.HasFile) return null;
            string ext = Path.GetExtension(FileUpload1.FileName).ToLowerInvariant();
            if (!AllowedImages.Contains(ext) || FileUpload1.PostedFile.ContentLength > 2 * 1024 * 1024)
                throw new InvalidOperationException("Cover must be a JPG, PNG or WEBP under 2 MB.");
            string name = Guid.NewGuid().ToString("N") + ext;
            string dir = Server.MapPath("~/imgs/books");
            Directory.CreateDirectory(dir);
            FileUpload1.SaveAs(Path.Combine(dir, name));
            return "imgs/books/" + name;
        }

        // Go
        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = Db.Query("SELECT * FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId));
                if (dt.Rows.Count == 0) { Alert("Invalid Book ID."); return; }
                DataRow r = dt.Rows[0];
                TextBox2.Text = (string)r["book_name"];
                DropDownList1.SelectedIndex = Math.Max(0, DropDownList1.Items.IndexOf(DropDownList1.Items.FindByValue((string)r["book_language"])));
                var pub = DropDownList2.Items.FindByValue((string)r["publisher_id"]); if (pub != null) DropDownList2.SelectedValue = pub.Value;
                var aut = DropDownList3.Items.FindByValue((string)r["author_id"]); if (aut != null) DropDownList3.SelectedValue = aut.Value;
                TextBox1.Text = r["publish_date"] == DBNull.Value ? "" : ((DateTime)r["publish_date"]).ToString("yyyy-MM-dd");
                TextBox5.Text = r["edition"] == DBNull.Value ? "" : (string)r["edition"];
                TextBox6.Text = ((decimal)r["book_cost"]).ToString(CultureInfo.InvariantCulture);
                TextBox7.Text = r["no_of_pages"] == DBNull.Value ? "" : r["no_of_pages"].ToString();
                TextBox8.Text = r["actual_stock"].ToString();
                TextBox9.Text = r["current_stock"].ToString();
                TextBox11.Text = ((int)r["actual_stock"] - (int)r["current_stock"]).ToString();
                TextBox10.Text = r["book_description"] == DBNull.Value ? "" : (string)r["book_description"];
                var genres = (r["genre"] == DBNull.Value ? "" : (string)r["genre"]).Split(',').Select(g => g.Trim()).ToArray();
                foreach (ListItem i in ListBox1.Items) i.Selected = genres.Contains(i.Value);
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Add
        protected void Button2_Click(object sender, EventArgs e)
        {
            DateTime? pd; decimal cost; int pages, stock;
            if (!ReadForm(out pd, out cost, out pages, out stock)) return;
            try
            {
                if (Exists()) { Alert("A book with this ID already exists."); return; }
                Db.Exec("INSERT INTO book_master_tbl(book_id,book_name,genre,author_id,publisher_id,publish_date,book_language,edition,book_cost,no_of_pages,book_description,actual_stock,current_stock,book_img_link) " +
                        "VALUES(@id,@name,@genre,@author,@pub,@pd,@lang,@ed,@cost,@pages,@desc,@stock,@stock,@img)",
                    Db.P("@id", BookId), Db.P("@name", TextBox2.Text.Trim()), Db.P("@genre", Genres), Db.P("@author", DropDownList3.SelectedValue),
                    Db.P("@pub", DropDownList2.SelectedValue), Db.P("@pd", pd), Db.P("@lang", DropDownList1.SelectedValue), Db.P("@ed", TextBox5.Text.Trim()),
                    Db.P("@cost", cost), Db.P("@pages", pages), Db.P("@desc", TextBox10.Text.Trim()), Db.P("@stock", stock), Db.P("@img", SaveCover()));
                Alert("Book added."); BindGrid();
            }
            catch (InvalidOperationException ex) { Alert(ex.Message); }
            catch (Exception ex) { Fail(ex); }
        }

        // Update
        protected void Button1_Click(object sender, EventArgs e)
        {
            DateTime? pd; decimal cost; int pages, stock;
            if (!ReadForm(out pd, out cost, out pages, out stock)) return;
            try
            {
                if (!Exists()) { Alert("Book ID does not exist."); return; }
                int oldActual = Convert.ToInt32(Db.Scalar("SELECT actual_stock FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId)));
                int current = Convert.ToInt32(Db.Scalar("SELECT current_stock FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId)));
                int newCurrent = current + (stock - oldActual);   // keep the number of issued copies unchanged
                if (newCurrent < 0) { Alert("Actual stock cannot be lower than the number of copies currently issued."); return; }

                Db.Exec("UPDATE book_master_tbl SET book_name=@name, genre=@genre, author_id=@author, publisher_id=@pub, publish_date=@pd, book_language=@lang, " +
                        "edition=@ed, book_cost=@cost, no_of_pages=@pages, book_description=@desc, actual_stock=@stock, current_stock=@cur, " +
                        "book_img_link=ISNULL(@img, book_img_link) WHERE book_id=@id",
                    Db.P("@id", BookId), Db.P("@name", TextBox2.Text.Trim()), Db.P("@genre", Genres), Db.P("@author", DropDownList3.SelectedValue),
                    Db.P("@pub", DropDownList2.SelectedValue), Db.P("@pd", pd), Db.P("@lang", DropDownList1.SelectedValue), Db.P("@ed", TextBox5.Text.Trim()),
                    Db.P("@cost", cost), Db.P("@pages", pages), Db.P("@desc", TextBox10.Text.Trim()), Db.P("@stock", stock), Db.P("@cur", newCurrent), Db.P("@img", SaveCover()));
                Alert("Book updated."); BindGrid();
            }
            catch (InvalidOperationException ex) { Alert(ex.Message); }
            catch (Exception ex) { Fail(ex); }
        }

        // Delete
        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Exists()) { Alert("Book ID does not exist."); return; }
                Db.Exec("DELETE FROM book_master_tbl WHERE book_id=@id", Db.P("@id", BookId));
                Alert("Book deleted."); BindGrid();
            }
            catch (Exception ex)
            {
                if (IsFkViolation(ex)) Alert("This book has issue history and cannot be deleted.");
                else Fail(ex);
            }
        }
    }
}
