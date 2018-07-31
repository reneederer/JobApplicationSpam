//import { setTimeout } from "core-js";
//import { clearTimeout } from "timers";

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

function setSaveTimer(jEl) {
    saveData.set(jEl.attr('name'), jEl.val())
    clearTimeout(saveTimer)
    saveTimer = setTimeout(function () {save(saveData)}, timeUntillSave)

}