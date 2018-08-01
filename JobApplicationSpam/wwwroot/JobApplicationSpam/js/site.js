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
    });
}

$(document).on('input', '.field', function (ev) {
    setSaveTimer($(this))
})

$(document).on('change', '.field', function (ev) {
    setSaveTimer($(this))
})

$(document).on('click', '.field', function (ev) {
    setSaveTimer($(this))
})

function setSaveTimer(jEl) {
    saveData.set(jEl.attr('name'), jEl.val())
    clearTimeout(saveTimer)
    saveTimer = setTimeout(function () {save(saveData)}, timeUntillSave)
}

$(document).on('click', '.applyNow', function () {
    applyNow()
})

$(document).on('change', '.uploadFile', function () {
    uploadFile($(this).get(0))
    window.location.reload()
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
    }).done(function () {
        $("[id^='Employer_']").val("")
        $("[id^='Employer_Gender']").prop("checked", false)

    })
}

function addVariable() {
    var formData = new FormData();
    formData.append('path', 'c:/users/rene/Downloads/hallo.odt')
    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/ConvertToPdf',
        data: formData
    }).done(function () {
        alert("asdf")
    });
}


function uploadFile(fileUpload) {
    var formData = new FormData();
    for (var i = 0; i < fileUpload.files.length; ++i) {
        formData.append(fileUpload.files[i].name, fileUpload.files[i])
    }
    $.ajax({
        method: 'post',
        contentType: false,
        processData: false,
        cache: false,
        url: '/Home/UploadFile',
        data: formData
    }).done(function () {
    });
}
