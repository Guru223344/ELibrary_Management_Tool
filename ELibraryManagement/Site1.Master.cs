using System;
using ELibraryManagement.Infrastructure;

namespace ELibraryManagement
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool guest = Auth.Role == "", admin = Auth.IsAdmin;

            LinkButton1.Visible = LinkButton2.Visible = guest;          // user login, sign up
            LinkButton3.Visible = LinkButton7.Visible = !guest;         // logout, hello
            LinkButton6.Visible = !admin;                               // admin login
            LinkButton8.Visible = LinkButton9.Visible = LinkButton10.Visible =
                LinkButton11.Visible = LinkButton12.Visible = admin;    // admin menu

            if (!guest) LinkButton7.Text = "Hello " + Server.HtmlEncode(Auth.FullName);
        }

        protected void LinkButton1_Click(object sender, EventArgs e) { Response.Redirect("UserLogin.aspx"); }
        protected void LinkButton2_Click(object sender, EventArgs e) { Response.Redirect("usersignup.aspx"); }
        protected void LinkButton4_Click(object sender, EventArgs e) { Response.Redirect("viewbooks.aspx"); }
        protected void LinkButton6_Click(object sender, EventArgs e) { Response.Redirect("AdminLogin.aspx"); }
        protected void LinkButton7_Click(object sender, EventArgs e) { if (Auth.IsMember) Response.Redirect("userprofile.aspx"); }
        protected void LinkButton8_Click(object sender, EventArgs e) { Response.Redirect("adminbookinventory.aspx"); }
        protected void LinkButton9_Click(object sender, EventArgs e) { Response.Redirect("adminbookissuing.aspx"); }
        protected void LinkButton10_Click(object sender, EventArgs e) { Response.Redirect("adminmembermanagement.aspx"); }
        protected void LinkButton11_Click(object sender, EventArgs e) { Response.Redirect("adminauthormanagement.aspx"); }
        protected void LinkButton12_Click(object sender, EventArgs e) { Response.Redirect("adminpublishermanagement.aspx"); }

        protected void LinkButton3_Click(object sender, EventArgs e)
        {
            Auth.SignOut();
            Response.Redirect("homepage.aspx");
        }
    }
}
