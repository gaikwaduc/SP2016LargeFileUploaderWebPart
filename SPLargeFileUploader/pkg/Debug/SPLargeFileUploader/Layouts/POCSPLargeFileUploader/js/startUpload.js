/*jslint unparam: true */
/*global window, $ */
$(function () {
    'use strict';

    $('#fileupload').bind('fileuploadsubmit', function (e, data) {
        var fileArray = [];

        var libName = $("input[id*='hdnLibraryName']").val();
        var siteUrl = $("input[id*='hdnSiteUrl']").val();

        for (var i = 0; i < data.originalFiles.length; i++) {
            var fileInfo = { FileName: "", FileSize: 0, LibraryName: "", SiteUrl: "" };
            fileInfo.FileName = data.originalFiles[i].name;
            fileInfo.FileSize = data.originalFiles[i].size;
            fileInfo.LibraryName = libName;
            fileInfo.SiteUrl = siteUrl;
            fileArray.push(fileInfo);
        }

        data.formData = { FileLists: JSON.stringify(fileArray) };

        if (!data.formData.FileLists) {
            data.context.find('button').prop('disabled', false);
            input.focus();
            return false;
        }
    });

    // Change this to the location of your server-side upload handler:
    var url = '../_layouts/15/POCSPLargeFileUploader/FileHandler.ashx';

    $('#fileupload').fileupload({
        maxChunkSize: 200000, // 2 MB
        sequentialUploads: true,
        url: url,
        dataType: 'json',
        add: function (e, data) {
            data.context = $('#uploadBtn')
                .click(function () {
                    data.submit();
                });
        },
        done: function (e, data) {
            $.each(data.result.files, function (index, file) {

                var isActive = $("#fileupload").fileupload("active");
                if (isActive === 1) {
                    $("input[id*='btnSubmit']").click();
                }
                $('<p/>').text(file.name).appendTo('#files');
            });
        },
        progressall: function (e, data) {
            var progress = parseInt(data.loaded / data.total * 100, 10);
            $('#progress .progress-bar').css(
                'width',
                progress + '%'
            );
        },
        fail: function (e, data) {

            $("#dvFileMessage").html("<div style='color:red'>" + data.errorThrown + "</div>")

            $('#progress .progress-bar').css(
            'width',
            0 + '%'
            );
            $("#progressCount").html('');
            $("#progressContainer").hide();
        }
    }).prop('disabled', !$.support.fileInput)
        .parent().addClass($.support.fileInput ? undefined : 'disabled');

});