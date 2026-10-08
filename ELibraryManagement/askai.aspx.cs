using System;
using System.Web;
using ELibraryManagement.Infrastructure;
using ELibraryManagement.Services;

namespace ELibraryManagement
{
    public partial class askai : LoggedInPage
    {
        protected void btnAsk_Click(object sender, EventArgs e)
        {
            try
            {
                var result = AiService.Ask(txtQuestion.Text, Auth.UserId);
                // model output is untrusted: encode it before rendering
                lblAnswer.Text = HttpUtility.HtmlEncode(result.Answer).Replace("\n", "<br />");
                pnlAnswer.Visible = true;
            }
            catch (Exception ex) { Fail(ex); }
        }
    }
}
