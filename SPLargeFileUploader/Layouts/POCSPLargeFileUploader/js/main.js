$(function () {
    'use strict';

    // Initialize the jQuery File Upload widget:
    $('#fileupload').fileupload({
        maxChunkSize: 10000000, // 10 MB
        url: '../_layouts/15/SPLargeFileUploader/FileHandler.ashx'
        

    });

     $(document).ready(function () {

        
         
         setTimeout(function () {

            


             $('#fileupload').bind('fileuploadsubmit', function (e, data) {
                 var fileArray = [];
                 for (var i = 0; i < data.originalFiles.length; i++) {
                     var fileInfo = { FileName: "", FileSize: 0 };
                     fileInfo.FileName = data.originalFiles[i].name;
                     fileInfo.FileSize = data.originalFiles[i].size;
                     fileArray.push(fileInfo);
                 }

                 data.formData = { FileLists: JSON.stringify(fileArray) };

                 if (!data.formData.FileLists) {
                     data.context.find('button').prop('disabled', false);
                     input.focus();
                     return false;
                 }
             });


             // Enable iframe cross-domain access via redirect option:
             $('#fileupload').fileupload(
                'option',
                'redirect',
                window.location.href.replace(
                    /\/[^\/]*$/,
                    '/cors/result.html?%s'
                )
            );

             if (window.location.hostname === 'blueimp.github.io') {

                 $('#fileupload').fileupload('option', {
                     url: '../_layouts/15/SPLargeFileUploader/FileHandler.ashx',
                     // Enable image resizing, except for Android and Opera,
                     // which actually support image resizing, but fail to
                     // send Blob objects via XHR requests:
                     disableImageResize: /Android(?!.*Chrome)|Opera/
                         .test(window.navigator.userAgent),
                     maxFileSize: 999000,
                     acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i
                 });

                 if ($.support.cors) {
                     $.ajax({
                         url: '../_layouts/15/SPLargeFileUploader/FileHandler.ashx',
                         type: 'HEAD'
                     }).fail(function () {
                         $('<div class="alert alert-danger"/>')
                             .text('Upload server currently unavailable - ' +
                                     new Date())
                             .appendTo('#fileupload');
                     });
                 }
             } else {
                 $('#fileupload').addClass('fileupload-processing');
                 $.ajax({

                     // Uncomment the following to send cross-domain cookies:
                     //xhrFields: {withCredentials: true},
                     url: '../_layouts/15/SPLargeFileUploader/FileHandler.ashx',
                     //url: $('#fileupload').fileupload('option', 'url'),
                     dataType: 'json',
                     context: $('#fileupload')[0]
                 }).always(function () {
                     $(this).removeClass('fileupload-processing');
                 }).done(function (result) {
                     $(this).fileupload('option', 'done')
                         .call(this, $.Event('done'), {
                             result: result
                         });
                 });
             }
             
         }, 1000);

         

     });
  


   
});
