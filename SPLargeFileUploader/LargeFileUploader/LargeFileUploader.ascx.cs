using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI.WebControls.WebParts;

namespace SPLargeFileUploader.LargeFileUploader
{
    [ToolboxItemAttribute(false)]
    public partial class LargeFileUploader : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public LargeFileUploader()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            hdnSiteUrl.Value = SPContext.Current.Web.Url;
            hdnLibraryName.Value = "Documents";

            if (!Page.IsPostBack)
            {
                HttpContext.Current.Session["FileRequests"] = null;
            }
        }

      
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            List<FileUploadInformation> lstFiles = (List<FileUploadInformation>) HttpContext.Current.Session["FileRequests"];
            HttpContext.Current.Session["FileRequests"] = null;

            //After file upload operation completes, you can set metadata of file here....

        }
    }
}
