$(document).on('click', '.menuItem', function (ev) {
    $('.menuItem').removeClass('active1')
    $('.page').hide()
    for (var pageToActivate of JSON.parse($(this).attr('data-activatesPages'))) {
        $('#' + pageToActivate).show()
    }
    $(this).addClass('active1')
})

var timeUntillSave = 2000
var saveTimer = null
var saveData = new FormData()
var fileActions = new FormData()
var fileActionsLength = 0

function save(saveData) {
    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/Save',
        data: saveData
    }).done(function () {
        saveData = new FormData()
        var str = ""
        for (var pair of fileActions.entries()) {
            str += "\n" + pair[0] + ": " + pair[1]
        }
        if (fileActionsLength > 0) {
            $.ajax({
                method: 'post',
                contentType: false,
                processData: false,
                cache: false,
                url: '/Home/PerformFileActions',
                data: fileActions
            }).done(function () {
                fileActions = new FormData()
            })
        }
    })

}

$(document).on('input', '.field', function (ev) {
    saveData.set($(this).attr('name'), $(this).val())
    setSaveTimer()
})

$(document).on('change', '.field', function (ev) {
    saveData.set($(this).attr('name'), $(this).val())
    setSaveTimer()
})

$(document).on('click', '.field', function (ev) {
    saveData.set($(this).attr('name'), $(this).val())
    setSaveTimer()
})

function setSaveTimer() {
    clearTimeout(saveTimer)
    saveTimer = setTimeout(function () {save(saveData)}, timeUntillSave)
}

$(document).on('click', '.applyNow', function () {
    applyNow()
})

$(document).on('change', '.uploadFile', function () {
    uploadFile($(this).get(0))
})

$(document).on('click', '.addVariable', function () {
    addVariable()
})

function applyNow() {
    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/ApplyNow',
        data: new FormData()
    }).done(function (data) {
        var j = JSON.parse(data)
        $("[id^='Employer_']").val("")
        $("[id^='Employer_Gender']").prop("checked", false)

    })
}

function addVariable() {
    var parent = $('#CustomVariables')
    var index = parent.children('div').length + 1
    var elId = "CustomVariables_Text_" + index
    var el = $(' \
        <div class="form-group"> \
        <label for="' + elId + '"><b></b></label> \
        <textarea id="' + elId + '" name="' + elId + '" class="form-control field" style="min-height: 150px; overflow: auto"></textarea>')
    el.insertBefore(parent.children().last())
}


function uploadFile(fileUpload) {
//TODO disable upload button while files are being uploaded
    if (fileUpload.files.length < 1) {
        return
    }
    var formData = new FormData()
    formData.append("files", fileUpload.files[0])
    var table = $('#Documents_FileTable')
    var td = $(' \
        <tr> \
            <td style="width: 100%" colspan="5"> \
                <div style="position:relative;width:296px;background:#0000f0" > <div id="progress" style="background: blue; height: 20px;width:0"></div></div> \
            </td> \
        </tr>')
    table.append(td)
    var progressEle = table.find('tr:last').find('td > div').first()
    $('#Documents_FileUploadButton').val("")

    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/UploadFile',
        data: formData,
        xhr: function() {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var progress = Math.round((evt.loaded / evt.total) * 100);
                    progressEle.width(progress + "%");
                }
            }, false);
            return xhr;
        },
        success: function (data) {
            if (data.state === 0) { //Upload succeeded
                progressEle.parent().replaceWith('\
                <td style="width: 100%"> \
                    ' + data.message + ' \
                </td> \
                <td class="documentFile_Delete text-center mx-1"><a href="#"><i class="fa fa-trash"></i></a></td> \
                <td class="documentFile_Download text-center mx-1"><a href="#"><i class="fa fa-download"></i></a></td> \
                <td class="documentFile_MoveUp text-center mx-1"><a href="#"><i class="fa fa-arrow-up"></i></a></td> \
                <td class="documentFile_MoveDown text-center mx-1"><a href="#"><i class="fa fa-arrow-down"></i></a></td> \
                ')
            }
            else if (data.state === 1) { //Upload failed
               progressEle.replaceWith('<div style="color: red">' + data.message + '<a class="float-right" onClick="removeRow(this)"><i class="fa fa-window-close"></i></a></div>')
            }
        },
        error: function (data) {
            progressEle.replaceWith('<div style="color: red">Sorry, the upload failed<a href="#" onClick="removeRow(this)"><i class="fa fa-close"></i></a></div>')
        }

    })
}

function removeRow(el) {
    var jEl = $(el)
    jEl.parents("tr:first").remove()

}

$(document).on('click', '.documentFile_MoveUp', function (ev) {
    var el = $(this).parent()
    fileActions.append('MoveUp_' + fileActionsLength, el.index())
    ++fileActionsLength
    var prevEl = el.prev()
    prevEl.insertAfter(el)
    setSaveTimer()
})

$(document).on('click', '.documentFile_MoveDown', function (ev) {
    var el = $(this).parent()
    fileActions.append('MoveDown_' + fileActionsLength, el.index())
    ++fileActionsLength
    var nextEl = el.next()
    nextEl.insertBefore(el)
    setSaveTimer()
})

$(document).on('click', '.documentFile_Delete', function (ev) {
    var el = $(this).parent()
    fileActions.append('Delete_' + fileActionsLength, el.index())
    ++fileActionsLength
    setSaveTimer()
    el.remove()
})


$(document).on('click', '.documentFile_Download', function (ev) {
    var el = $(this).parent()
})

