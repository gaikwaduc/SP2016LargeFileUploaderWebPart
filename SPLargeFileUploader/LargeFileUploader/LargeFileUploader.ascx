<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LargeFileUploader.ascx.cs" Inherits="SPLargeFileUploader.LargeFileUploader.LargeFileUploader" %>

<!-- Force latest IE rendering engine or ChromeFrame if installed -->
<!--[if IE]>
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1">
    <![endif]-->

<link href="../_layouts/15/POCSPLargeFileUploader/css/bootstrap.min.css" rel="stylesheet" />
<link rel="stylesheet" href="../_layouts/15/POCSPLargeFileUploader/css/style.css">

<link rel="stylesheet" href="../_layouts/15/POCSPLargeFileUploader/css/jquery.fileupload.css">

<asp:HiddenField ID="hdnSiteUrl" Value="" runat="server" />
<asp:HiddenField ID="hdnLibraryName" Value="" runat="server" />

<div class="container adjustWidth">
    
   
    <!-- The fileinput-button span is used to style the file input field as button -->
    <span class="btn btn-success fileinput-button">
        <i class="glyphicon glyphicon-plus"></i>
        <span>Select files...</span>
        <!-- The file input field used as target for the file upload widget -->
        <input id="fileupload" type="file" name="files[]" multiple>
    </span>
    <button type="button" id="uploadBtn" class="btn btn-primary start">
        <i class="glyphicon glyphicon-upload"></i>
        <span>Start upload</span>
    </button>

    <br>
    <br>
    <asp:Button style="display:none" ID="btnSubmit" runat="server" Text="Server Upload" OnClick="btnSubmit_Click" />
    
    <br />
    <!-- The global progress bar -->
    <div id="progress" class="progress">
        <div class="progress-bar progress-bar-success"></div>
    </div>

    <!-- The container for the uploaded files -->
    <div id="files" class="files"></div>
    <div id="dvFileMessage"></div>
    <br>
</div>



<script src="../_layouts/15/POCSPLargeFileUploader/js/vendor/jquery.js"></script>
<script src="../_layouts/15/POCSPLargeFileUploader/js/vendor/jquery.ui.widget.js"></script>

<script src="../_layouts/15/POCSPLargeFileUploader/js/jquery.iframe-transport.js"></script>


<script src="../_layouts/15/POCSPLargeFileUploader/js/jquery.fileupload.js"></script>
<script src="../_layouts/15/POCSPLargeFileUploader/js/vendor/tether.min.js"></script>
<script src="../_layouts/15/POCSPLargeFileUploader/js/bootstrap.min.js"></script>

<script src="../_layouts/15/POCSPLargeFileUploader/js/startUpload.js"></script>



