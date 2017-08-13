using Microsoft.SharePoint.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SPLargeFileUploader
{
    [Serializable]
    public class FileUploadInformation
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FilePath { get; set; }
        public int LoopCounter { get; set; }
        public int CurrentLoopCount { get; set; }
        public Guid FileId { get; set; }
        public long FileOffsetValue { get; set; }
        public bool IsUploaded { get; set; }
        public int ItemID { get; set; }
        public string LibraryName { get; set; }
        public string SiteUrl { get; set; }
    }

    /// <summary>
    /// Encapsulate file upload services for demo purposes
    /// </summary>
    public class FileUploadService
    {
        public static FileUploadInformation UploadFile(FileUploadInformation fileInfo, HttpPostedFile postedFile)
        {
            using (ClientContext ctx = ClientContextProvider.GetSPContext(fileInfo.SiteUrl))
            {
                //Web web = ctx.Web;
                //ctx.Load(web);
                string libraryName = fileInfo.LibraryName;
                string fileName = fileInfo.FileName;
                // Each sliced upload requires a unique id
                Guid uploadId = fileInfo.FileId;
                string uniqueFileName = fileName;
                // File object 
                Microsoft.SharePoint.Client.File uploadFile;

                // Use large file upload approach
                ClientResult<long> bytesUploaded = null;

                if (fileInfo.LoopCounter == 1)
                {
                    // Get to folder to upload into 
                    List docs = ctx.Web.Lists.GetByTitle(libraryName);
                    ctx.Load(docs, l => l.RootFolder);
                    // Get the information about the folder that will hold the file
                    ctx.Load(docs.RootFolder, f => f.ServerRelativeUrl);
                    ctx.ExecuteQuery();

                    fileInfo.FilePath = docs.RootFolder.ServerRelativeUrl + System.IO.Path.AltDirectorySeparatorChar + uniqueFileName;

                    FileCreationInformation newFile = new FileCreationInformation();
                    newFile.ContentStream = postedFile.InputStream;
                    newFile.Url = uniqueFileName;
                    newFile.Overwrite = true;
                    uploadFile = docs.RootFolder.Files.Add(newFile);
                    ctx.Load(uploadFile);
                    ctx.ExecuteQuery();
                    fileInfo.IsUploaded = true;
                    ctx.Load(uploadFile.ListItemAllFields);
                    ctx.ExecuteQuery();
                    fileInfo.ItemID = uploadFile.ListItemAllFields.Id;
                    fileInfo.LibraryName = libraryName;
                    fileInfo.SiteUrl = docs.RootFolder.ServerRelativeUrl;
                }
                else
                {
                    if (fileInfo.LoopCounter == fileInfo.CurrentLoopCount) //First chunk
                    {
                        // Get to folder to upload into 
                        List docs = ctx.Web.Lists.GetByTitle(libraryName);
                        ctx.Load(docs, l => l.RootFolder);
                        // Get the information about the folder that will hold the file
                        ctx.Load(docs.RootFolder, f => f.ServerRelativeUrl);
                        ctx.ExecuteQuery();

                        fileInfo.FilePath = docs.RootFolder.ServerRelativeUrl + System.IO.Path.AltDirectorySeparatorChar + uniqueFileName;

                        FileCreationInformation newFile = new FileCreationInformation();
                        newFile.ContentStream = postedFile.InputStream;
                        newFile.Url = uniqueFileName;
                        newFile.Overwrite = true;
                        uploadFile = docs.RootFolder.Files.Add(newFile);

                        bytesUploaded = uploadFile.StartUpload(fileInfo.FileId, postedFile.InputStream);
                        ctx.ExecuteQuery();
                        // fileoffset is the pointer where the next slice will be added

                        fileInfo.FileOffsetValue = bytesUploaded.Value;

                        fileInfo.CurrentLoopCount = fileInfo.CurrentLoopCount - 1;
                        fileInfo.SiteUrl = docs.RootFolder.ServerRelativeUrl;
                        ctx.Load(uploadFile.ListItemAllFields);
                        ctx.ExecuteQuery();
                        fileInfo.ItemID = uploadFile.ListItemAllFields.Id;
                        fileInfo.LibraryName = libraryName;

                    }
                    else if (fileInfo.CurrentLoopCount == 1) // Last chunk
                    {
                        uploadFile = ctx.Web.GetFileByServerRelativeUrl(fileInfo.FilePath);
                        // End sliced upload by calling FinishUpload
                        uploadFile = uploadFile.FinishUpload(fileInfo.FileId, fileInfo.FileOffsetValue, postedFile.InputStream);
                        ctx.ExecuteQuery();
                        fileInfo.CurrentLoopCount = fileInfo.CurrentLoopCount - 1;
                        fileInfo.IsUploaded = true;
                       
                       
                    }
                    else // continue upload
                    {
                        // Get a reference to our file
                        uploadFile = ctx.Web.GetFileByServerRelativeUrl(fileInfo.FilePath);
                        // Continue sliced upload
                        bytesUploaded = uploadFile.ContinueUpload(fileInfo.FileId, fileInfo.FileOffsetValue, postedFile.InputStream);
                        ctx.ExecuteQuery();
                        // update fileoffset for the next slice
                        fileInfo.FileOffsetValue = bytesUploaded.Value;

                        fileInfo.CurrentLoopCount = fileInfo.CurrentLoopCount - 1;
                    }
                }
            }

            return fileInfo;
        }
    }


}
