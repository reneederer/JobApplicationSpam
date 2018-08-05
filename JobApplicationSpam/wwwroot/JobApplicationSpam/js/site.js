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
            str += "\n" + pair[0] + ": " + pair[1];
        }
        alert(str);
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
        alert(data)
        var j = JSON.parse(data);
        alert(j.result)
        $("[id^='Employer_']").val("")
        $("[id^='Employer_Gender']").prop("checked", false)

    })
}

function addVariable() {
    var parent = $('#CustomVariables')
    var index = parent.children('div').length + 1
    alert(index)
    var elId = "CustomVariables_Text_" + index
    var el = $(' \
        <div class="form-group"> \
        <label for="' + elId + '"><b></b></label> \
        <textarea id="' + elId + '" name="' + elId + '" class="form-control field" style="min-height: 150px"></textarea>');
    el.insertBefore(parent.children().last())
}


function uploadFile(fileUpload) {
    if (fileUpload.files.length < 1) {
        return
    }
    var formData = new FormData();
    formData.append(fileUpload.files[0].name, fileUpload.files[0])
    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/UploadFile',
        data: formData
    }).done(function () {
        var table = $('#Documents_FileTable')
        var td = $(' \
            <tr> \
                <td style="width: 100%">' + fileUpload.files[0].name + '</td > \
                <td class="documentFile_Delete text-center mx-1"><a href="#"><i class="fa fa-trash"></i></a></td> \
                <td class="documentFile_Download text-center mx-1"><a href="#"><i class="fa fa-download"></i></a></td> \
                <td class="documentFile_MoveUp text-center mx-1"><a href="#"><i class="fa fa-arrow-up"></i></a></td> \
                <td class="documentFile_MoveDown text-center mx-1"><a href="#"><i class="fa fa-arrow-down"></i></a></td> \
            </tr>')
        table.append(td)
        $('#Documents_FileUploadButton').val("")

    })
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

