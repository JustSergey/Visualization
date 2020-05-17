var infections = getJson('GetInfections', 'infections');
var regions = getJson('GetInfections', 'regions');
var beginDate = parseDate(document.getElementById('trackBarValue').innerHTML);
var max = getMax();

$(function () {
    onChangeTrackBar();
    for (i = 1; i < regions.length + 1; i++) {
        $('#region_' + i)
            .data('region', regions[i - 1]);
    }
    $('.d-map__region-path > path, .d-map__region-path > g').mouseover(function () {
        let region = $(this).data('region');
        mouseOver(region);
    }).mouseleave(function () {
        mouseLeave();
    }).mousemove(function (e) {
        mouseMove(e.pageX, e.pageY);
    });
    var color;
    $('.d-map__region-path path').mouseover(function () {
        color = $(this).css('fill');
        $(this).css({ 'fill': '#fb2' });
    }).mouseleave(function () {
        $(this).css({ fill: color });
    });
})

function mouseOver(region) {
    $('<div class="info_panel">' +
        region.title + 
        '</div>'
    ).appendTo('body');
}

function mouseLeave() {
    $('.info_panel').remove();
}

function mouseMove(mouseX, mouseY) {
    $('.info_panel').css({
        top: mouseY - ($('.info_panel').height()),
        left: mouseX + 20
    });
}

function onChangeTrackBar() {
    let date = $('#trackbar').val();
    let newDate = addDays(beginDate, date);
    document.getElementById('trackBarValue').innerHTML = newDate.toLocaleDateString();
    let index = 0;
    for (i = 1; i < regions.length + 1; i++) {
        let size = 0;
        if (index < infections[date].infections.length) {
            if (i == infections[date].infections[index].region) {
                size = Math.log(infections[date].infections[index].infected) / max * 20;
                index++;
            }
        }
        $('#region_circle_' + i).css({ r: size });
    }
}

function getMax() {
    let max = 0;
    for (i = 0; i < infections[infections.length - 1].infections.length; i++) {
        let inf = infections[infections.length - 1].infections[i].infected;
        if (inf > max)
            max = inf;
    }
    return Math.log(max);
}

function parseDate(date) {
    let parts = date.split('.');
    return new Date(parts[2], parts[1] - 1, parts[0]);
}

function addDays(date, days) {
    let result = new Date(date);
    result.setDate(result.getDate() + parseInt(days));
    return result;
}

function getJson(url, type) {
    let result = [];
    $.ajax({
        type: 'POST',
        url: url,
        data: 'type=' + type,
        dataType: 'json',
        async: false,
        success: function (data) {
            result = data;
        }
    });
    return result;
}