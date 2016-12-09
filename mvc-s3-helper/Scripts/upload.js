var $thatDropZone;

//File Upload response from the server
Dropzone.options.dropzoneForm = {
    maxFiles: 6,
    autoProcessQueue: false,
    uploadMultiple: true,
    init: function () {
        $thatDropZone = this;
        this.on("maxfilesexceeded", function (data) {
            var res = eval('(' + data.xhr.responseText + ')');

        });
        this.on("addedfile", function (file) {

            // Create the remove button
            var removeButton = Dropzone.createElement("<button>Remove file</button>");


            // Capture the Dropzone instance as closure.
            var _this = this;

            // Listen to the click event
            removeButton.addEventListener("click", function (e) {
                // Make sure the button click doesn't submit the form:
                e.preventDefault();
                e.stopPropagation();
                // Remove the file preview.
                _this.removeFile(file);
                // If you want to the delete the file on the server as well,
                // you can do the AJAX request here.
            });

            // Add the button to the file preview element.
            file.previewElement.appendChild(removeButton);
        });
    }
};

$(document).ready(function(){  
    $('#btnUpload').click(function () {  
  
        // Checking whether FormData is available in browser  
        if (window.FormData !== undefined) { 
              
            var files = $thatDropZone.files;
            if (files.length == 0) {
                alert("Please upload at least one Image");
                return;
            }

            // Create FormData object  
            var fileData = new FormData();
  
            // Looping over all files and add it to FormData object  
            for (var i = 0; i < files.length; i++) {  
                fileData.append(files[i].name, files[i]);  
            }  
              
            //// Adding one more key to FormData object  
            //fileData.append('username', "");  
  
            $.ajax({  
                url: '/Image/Create',  
                type: "POST",  
                contentType: false, // Not to set any content header  
                processData: false, // Not to process data  
                data: fileData,  
                success: function (result) {  
                    alert(result.Message);  
                },  
                error: function (err) {  
                    alert(err.statusText);  
                }  
            });  
        } else {  
            alert("FormData is not supported.");  
        }  
    });  
});  