using System;
using System.IO;
using System.Web;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using Microsoft.SharePoint;

namespace SPLargeFileUploader.Layouts.POCSPLargeFileUploader
{
    public partial class FileHandler : IHttpHandler, System.Web.SessionState.IRequiresSessionState
    {
        public bool IsReusable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private static readonly int ATTEMPTS_TO_WRITE = 3;

        private static readonly int ATTEMPT_WAIT = 100;

        private static readonly double CHUNK_SIZE = 20000.00;

        private static class HttpMethods
        {
            public static readonly string GET = "GET";
            public static readonly string POST = "POST";
            public static readonly string DELETE = "DELETE";
        }

        [DataContract]
        private class FileResponse
        {
            [DataMember]
            public string name;
            [DataMember]
            public long size;
            [DataMember]
            public string type;
            [DataMember]
            public string url;
            [DataMember]
            public string error;
            [DataMember]
            public string deleteUrl;
            [DataMember]
            public string deleteType;
        }

        [DataContract]
        private class UploaderResponse
        {
            [DataMember]
            public FileResponse[] files;

            public UploaderResponse(FileResponse[] fileResponses)
            {
                files = fileResponses;
            }
        }

        public void ProcessRequest(HttpContext context)
        {

            if (context.Session["FileRequests"] != null && context.Request.HttpMethod.ToUpper() == HttpMethods.GET)
            {
                context.Session["FileRequests"] = null;
            }

            if (context.Request.HttpMethod.ToUpper() == HttpMethods.POST && context.Session["FileRequests"] == null && context.Request.Files != null && context.Request.Files.Count > 0)
            {
                var serializer = new JavaScriptSerializer();

                List<FileUploadInformation> lstFiles = serializer.Deserialize<List<FileUploadInformation>>(Convert.ToString(context.Request.Form["FileLists"]));

                foreach (var f in lstFiles)
                {
                    f.LoopCounter = getLoopCounter(f.FileSize);
                    f.CurrentLoopCount = f.LoopCounter;
                    f.FileId = Guid.NewGuid();
                    f.IsUploaded = false;
                }

                context.Session["FileRequests"] = lstFiles;
            }

            if (context.Request.HttpMethod.ToUpper() == HttpMethods.GET)
            {

                List<FileResponse> FileResponseList = new List<FileResponse>();

                string[] FileNames = null;
                
                //In this POC we have not used GET requests

                if (FileNames != null)
                {
                    foreach (string FileName in FileNames)
                    {
                        FileResponseList.Add(CreateFileResponse(FileName, new FileInfo(FileName).Length, String.Empty, context));
                    }
                }

                SerializeUploaderResponse(FileResponseList, context);

            }
            else if (context.Request.HttpMethod.ToUpper() == HttpMethods.POST)
            {
                List<FileResponse> FileResponseList = new List<FileResponse>();

                for (int FileIndex = 0; FileIndex < context.Request.Files.Count; FileIndex++)
                {
                    HttpPostedFile File = context.Request.Files[FileIndex];
                    long fileLength = 0;
                    string tt = string.Empty;
                    
                    string ErrorMessage = String.Empty;

                    for (int Attempts = 0; Attempts < ATTEMPTS_TO_WRITE; Attempts++)
                    {
                        ErrorMessage = String.Empty;
                        try
                        {
                            List<FileUploadInformation> lstFiles = (List<FileUploadInformation>)context.Session["FileRequests"];
                           
                            for (int i = 0; i < lstFiles.Count; i++)
                            {
                                if (lstFiles[i].FileName.Equals(File.FileName))
                                {
                                    lstFiles[i] = FileUploadService.UploadFile(lstFiles[i], File);
                                    fileLength = lstFiles[i].FileSize;
                                    break;
                                }
                            }
                            context.Session["FileRequests"] = lstFiles;
                        }
                        catch (Exception exception)
                        {
                            ErrorMessage = exception.Message;
                            System.Threading.Thread.Sleep(ATTEMPT_WAIT);
                            continue;
                        }

                        break;
                    }

                    FileResponseList.Add(CreateFileResponse(File.FileName, fileLength, ErrorMessage, context));

                }

                SerializeUploaderResponse(FileResponseList, context);
            }
            else if (context.Request.HttpMethod.ToUpper() == HttpMethods.DELETE)
            {
                bool SuccessfullyDeleted = true;

                try
                {
                    //In this POC DELETE call has not been implemented and not used anywhere
                }
                catch
                {
                    SuccessfullyDeleted = false;
                }

                context.Response.Write(String.Format("{{\"{0}\":{1}}}", "file", SuccessfullyDeleted.ToString().ToLower()));
            }
            else
            {
                context.Response.StatusCode = 405;
                context.Response.StatusDescription = "Method not allowed";
                context.Response.End();
                return;
            }
            context.Response.End();
        }

        private FileResponse CreateFileResponse(string fileName, long size, string error, HttpContext context)
        {
            return new FileResponse()
            {
                name = fileName,
                size = size,
                type = String.Empty,
                url = string.Empty,
                error = error,
                deleteUrl = string.Empty,
                deleteType = HttpMethods.DELETE
            };
        }

        private void SerializeUploaderResponse(List<FileResponse> fileResponses, HttpContext context)
        {
            DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(UploaderResponse));

            Serializer.WriteObject(context.Response.OutputStream, new UploaderResponse(fileResponses.ToArray()));
        }
       
        private int getLoopCounter(long fileLength)
        {
            double c = fileLength / CHUNK_SIZE;
            var count = Math.Ceiling((double)c / 10);
            return Convert.ToInt32(count);
        }
    }
}
