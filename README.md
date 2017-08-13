![Visual WebPart for Large File Upload](https://github.com/gaikwaduc/SP2016LargeFileUploaderWebPart/blob/master/img/1.png)

Existing version of SharePoint 2013 supports and allows to upload the files up to 2 GB. Now it is possible to upload larger files like more than 2 GB in SharePoint 2016. However, large file upload is not directly supported by OOTB, that we need to do some configuration on the farm. Let's, call it as **prerequisite configurations**. 

This prerequisites are applicable to only SharePoint 2016 on premise farm. SharePoint online supports large file uploads by default so no need to do any changes in case of SharePoint online. In this blog, we have covered file upload with SharePoint Visual WebPart farm solution. In case of SharePoint online, provider hosted app model can be used. Let's have a look into high level information of the areas used for uploading large file.

Large file handling managed CSOM API's -  StartUpload, ContinueUpload, and FinishUpload

For more information about this API and core logic you can refer below link,

[https://github.com/SharePoint/PnP/tree/dev/Samples/Core.LargeFileUpload#large-file-handling---option-3-startupload-continueupload-and-finishupload](https://github.com/SharePoint/PnP/tree/dev/Samples/Core.LargeFileUpload#large-file-handling---option-3-startupload-continueupload-and-finishupload)

These API's are introduced and applicable for SharePoint 2016 and SharePoint Online.

In this approach, large files are broken into the chunks and uploaded to the server. We have to define chunk size considering network bandwidth and standards. In our case, we have defined 2MB of chunk size for uploads. We can resume upload operation if it gets failed in case of any network disturbances, but for that we have to **maintain the state** of each and every uploaded chunks. However, retrying of  failed chunks functionality has not been covered in this blog.

In order to handle all these chunks we have write a **handler **or web service at the server side using managed CSOM file upload API's.

It is very important to select a file upload control that will help us to break the file in chunks and post it  to the server for uploading. For this we have used **blueimp file upload J-Query control**. This is really good control and has lot of features like, progress bar, statistics of file upload operations, sequential and parallel uploads, single file upload, multi file upload, drag and drop, resume uploads, cancel uploads, retry failed chunk operations. Similarly there are lot of helpful features and good documentation on github.

Now, we are going to see in details each and every operations which are highlighted above. The use case is that we are going to create visual WebPart to upload large files in the SharePoint document library. It supports multi file upload, drag and drop and progress bar indicator. ADFS is configured for authentication.

### 1.	Prerequisite for SharePoint 2016 to enable large file upload

You can follow below blog written by Viorel Iftode for configuration to support large file uploads
[https://www.vioreliftode.com/index.php/how-to-make-sharepoint-2016-not-fail-long-running-uploads-large-files/](https://www.vioreliftode.com/index.php/how-to-make-sharepoint-2016-not-fail-long-running-uploads-large-files/)

Apart from above configuration, we need to configure maximum file size for a web application as shown below in steps,

1.	In Central Administration, in Application Management, click Manage web applications.
2.	Select the application (for example, SharePoint - 80).
3.	On the Web Applications ribbon, click the down arrow on the General Settings button.
4.	Click General Settings.
5.	Scroll to Maximum Upload Size.
6.	Set the property to the same number, or larger as the Maximum Workbook Size in Excel Services.
7.	Click OK.

![Web Application Configuration](https://github.com/gaikwaduc/SP2016LargeFileUploaderWebPart/blob/master/img/2.png)
 
And also I would suggest to perform large file upload operation in the OOTB document library to make sure everything is in place.

### 2.	Enable session state to maintain the state of each file chunk

We need to maintain state of each file chunk. It is not recommended to use session management in SharePoint,  so this can be replaced with another state management methodology. 

You can refer this blog to enable session on farm,
[https://nikpatel.net/2012/02/12/enable-asp-net-session-state-on-sharepoint-2010-application/](https://nikpatel.net/2012/02/12/enable-asp-net-session-state-on-sharepoint-2010-application/)

### 3.	Handler to serve and upload chunk requests

Core business logic resides into this handler, and it supports multiple file upload functionality.
We have used new SharePoint 2016 API's for file upload into this handler. The logic is to identify first chunk, middle chunks and last chunk to follow StartUpload, ContinueUpload, and FinishUpload execution. 

Please note that in this demo we have handled only POST request.  GET and DELETE can be added for further improvements and functionality.

If you have ADFS authentication then, in that case, you need to create client context as per provided guidelines in the below blog,
[https://mikesmode.wordpress.com/2016/08/14/using-csom-with-saml-authentication-adfs/](https://mikesmode.wordpress.com/2016/08/14/using-csom-with-saml-authentication-adfs/)

Please make sure that the token constants are modified according to your environment. 

![Token Constants](https://github.com/gaikwaduc/SP2016LargeFileUploaderWebPart/blob/master/img/3.png)

In case you are not having ADFS configured, then you can use client context creation directly as suggested in below link.
[https://msdn.microsoft.com/en-us/library/office/ee537538(v=office.14).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1](https://msdn.microsoft.com/en-us/library/office/ee537538(v=office.14).aspx?cs-save-lang=1&cs-lang=csharp#code-snippet-1)

### 4.	blueimp file upload J-Query

We have used blueimp file upload control for uploading files. For this POC, only few of features have been utilized from this control. This control has lot of features which can be explored by below link,
[https://github.com/blueimp/jQuery-File-Upload](https://github.com/blueimp/jQuery-File-Upload)
