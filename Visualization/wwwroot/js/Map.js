var infections = getJson('GetInfections', 'infections');
var regions = getJson('GetInfections', 'regions');
var beginDate = parseDate(document.getElementById('trackBarValue').innerHTML);
var maxInfected = getMax('infected');
var maxRecovered = getMax('recovered');
var maxDeaths = getMax('deaths');
var type = 'infected';
var moving = false;
var increase = getJson('GetIncrease', 'daily');
var ave_increase = getJson('GetIncrease', 'average');

$(function () {
    zooming();
    var playButton = $("#play-button");
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

function animation() {
    let button = document.getElementById("play-button");
    if (moving) {
        button.value = "Play";
        moving = false;
        clearInterval(timer);
    }
    else {
        button.value = "Pause";
        moving = true;
        timer = setInterval(step, 300);
    }
}

function step() {
    var trackbar = document.getElementById("trackbar");
    if (trackbar.value == trackbar.max) {
        trackbar.value = 0;
        moving = false;
        clearInterval(timer);
        let button = document.getElementById("play-button");
        button.value = "Play";
    }
    else
        trackbar.value++;
    onChangeTrackBar();
}

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
        if (index < infections[date].infections.length && i == infections[date].infections[index].region) {
            if (type == 'infected') {
                let data = infections[date].infections[index].infected;
                if (data > 0)
                    size = Math.log(data) / maxInfected * 20;
                let dailyInc = increase[date].increases[index].inf;
                document.getElementById('increase_' + i).innerHTML = dailyInc + '%';
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].inf + '%';
                addColor(dailyInc, i);
            }
            else if (type == 'recovered') {
                let data = infections[date].infections[index].recovered;
                if (data > 0)
                    size = Math.log(data) / maxRecovered * 20;
                let dailyInc = increase[date].increases[index].rec;
                document.getElementById('increase_' + i).innerHTML = dailyInc + '%';
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].rec + '%';
                addColor(dailyInc, i);
            }
            else if (type == 'deaths') {
                let data = infections[date].infections[index].deaths;
                if (data > 0)
                    size = Math.log(data) / maxDeaths * 20;
                let dailyInc = increase[date].increases[index].dea;
                document.getElementById('increase_' + i).innerHTML = dailyInc + '%';
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].dea + '%';
                addColor(dailyInc, i);
            }
            index++;
        }
        else {
            document.getElementById('increase_' + i).innerHTML = '0%';
            if (type == 'infected')
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].inf + '%';
            else if (type == 'recovered')
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].rec + '%';
            else if (type == 'deaths')
                document.getElementById('ave_increase_' + i).innerHTML = ave_increase[i - 1].dea + '%';
            addColor(0, i);
        }
        $('#region_circle_' + i).css({ r: size });
    }
}

function addColor(value, id) {
    $("#region-table-" + id).css({ 'background-color' : 'rgba(255, 255, 255, .6)' });
    if (value != 0 && type == 'infected') {
        let red = Math.round(value * 5.1);
        if (red > 255)
            red = 255;
        if (red < 0)
            red = 0;
        let green = 255 - Math.round((value - 50) * 5.1);
        if (green > 255)
            green = 255;
        if (green < 0)
            green = 0;
        $("#region-table-" + id).css({ 'background-color': 'rgba(' + red + ', ' + green + ', 0, .6)' });
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
    if (moving) {
        moving = false;
        clearInterval(timer);
        let button = document.getElementById("play-button");
        button.value = "Play";
    }
    for (i = 1; i < regions.length + 1; i++) {
        $("#region-table-" + i).css({ 'background-color': 'rgba(255, 255, 255, .6)' });
    }
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

function zooming() {
    const svg = d3.select("svg");
    const g = d3.selectAll(".d-map__region, circle");

    svg.call(d3.zoom()
        .extent([[0, 0], [1500, 900]])
        .scaleExtent([1, 4])
        .on("zoom", function () { g.attr("transform", d3.event.transform) }))

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