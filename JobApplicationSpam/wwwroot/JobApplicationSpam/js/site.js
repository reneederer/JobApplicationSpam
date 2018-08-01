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




/* #####################################################################
   #
   #   Project       : Modal Login with jQuery Effects
   #   Author        : Rodrigo Amarante (rodrigockamarante)
   #   Version       : 1.0
   #   Created       : 07/29/2015
   #   Last Change   : 08/04/2015
   #
   ##################################################################### */

$(function () {

    var $formLogin = $('#login-form');
    var $formLost = $('#lost-form');
    var $formRegister = $('#register-form');
    var $divForms = $('#div-forms');
    var $modalAnimateTime = 300;
    var $msgAnimateTime = 150;
    var $msgShowTime = 2000;


    $('#login_register_btn').click(function () { modalAnimate($formLogin, $formRegister) });
    $('#register_login_btn').click(function () { modalAnimate($formRegister, $formLogin); });
    $('#login_lost_btn').click(function () { modalAnimate($formLogin, $formLost); });
    $('#lost_login_btn').click(function () { modalAnimate($formLost, $formLogin); });
    $('#lost_register_btn').click(function () { modalAnimate($formLost, $formRegister); });
    $('#register_lost_btn').click(function () { modalAnimate($formRegister, $formLost); });

    function modalAnimate($oldForm, $newForm) {
        var $oldH = $oldForm.height();
        var $newH = $newForm.height();
        $divForms.css("height", $oldH);
        $oldForm.fadeToggle($modalAnimateTime, function () {
            $divForms.animate({ height: $newH }, $modalAnimateTime, function () {
                $newForm.fadeToggle($modalAnimateTime);
            });
        });
    }

    function msgFade($msgId, $msgText) {
        $msgId.fadeOut($msgAnimateTime, function () {
            $(this).text($msgText).fadeIn($msgAnimateTime);
        });
    }

    function msgChange($divTag, $iconTag, $textTag, $divClass, $iconClass, $msgText) {
        var $msgOld = $divTag.text();
        msgFade($textTag, $msgText);
        $divTag.addClass($divClass);
        $iconTag.removeClass("glyphicon-chevron-right");
        $iconTag.addClass($iconClass + " " + $divClass);
        setTimeout(function () {
            msgFade($textTag, $msgOld);
            $divTag.removeClass($divClass);
            $iconTag.addClass("glyphicon-chevron-right");
            $iconTag.removeClass($iconClass + " " + $divClass);
        }, $msgShowTime);
    }
});
