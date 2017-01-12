using StorageAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace S3ToBlobMigration
{
    public partial class AccessS3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            upload.ServerClick += Upload_ServerClick;
            btnGetS3Objects.ServerClick += BtnGetS3Objects_ServerClick;
            AccessStorageBlob objS3 = new AccessStorageBlob();
            objS3.CreateBucket("kudopi");
            Response.Write("Bucket is created");
        }

        private void BtnGetS3Objects_ServerClick(object sender, EventArgs e)
        {
            AccessStorageBlob objS3 = new AccessStorageBlob();
            var lst = objS3.ListObjects("kudopi");
            ListBox1.Items.Clear();
            foreach (var item in lst)
            {
                ListBox1.Items.Add(item);
            }
        }

        private void Upload_ServerClick(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                AccessStorageBlob objS3 = new AccessStorageBlob();
                objS3.uploadObject(FileUpload1.FileContent,
                    "kudopi", FileUpload1.FileName);
                    

                Response.Write("file is uploaded");
            }
        }

        protected void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtFileName.Text = ListBox1.SelectedValue;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            AccessStorageBlob objS3 = new AccessStorageBlob();
            Stream streamResponse = objS3.GetContent(
                "kudopi", txtFileName.Text);
            Response.Write(streamResponse.ToString());
        }
    }
}