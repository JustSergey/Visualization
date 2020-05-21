var infections = getJson('GetInfections', 'infections');
var regions = getJson('GetInfections', 'regions');
var beginDate = parseDate(document.getElementById('trackBarValue').innerHTML);
var maxInfected = getMax('infected');
var maxRecovered = getMax('recovered');
var maxDeaths = getMax('deaths');
var type = 'infected';

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
                if (type == 'infected') {
                    let data = infections[date].infections[index].infected;
                    if (data > 0)
                        size = Math.log(data) / maxInfected * 20;
                }
                else if (type == 'recovered') {
                    let data = infections[date].infections[index].recovered;
                    if (data > 0)
                        size = Math.log(data) / maxRecovered * 20;
                }
                else if (type == 'deaths') {
                    let data = infections[date].infections[index].deaths;
                    if (data > 0)
                        size = Math.log(data) / maxDeaths * 20;
                }
                index++;
            }
        }
        $('#region_circle_' + i).css({ r: size });
    }
}

function onChangeType(t) {
    type = t;
    if (type == 'infected')
        $('circle').css({ 'fill': '#fb5f3d' });
    else if (type == 'recovered')
        $('circle').css({ 'fill': 'green' });
    else if (type == 'deaths')
        $('circle').css({ 'fill': 'black' });
    onChangeTrackBar();
}

function getMax(type) {
    let max = 0;
    for (i = 0; i < infections[infections.length - 1].infections.length; i++) {
        let data;
        if (type == 'infected')
            data = infections[infections.length - 1].infections[i].infected;
        else if (type == 'recovered')
            data = infections[infections.length - 1].infections[i].recovered;
        else if (type == 'deaths')
            data = infections[infections.length - 1].infections[i].deaths;
        if (data > max)
            max = data;
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