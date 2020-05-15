var infections = getJson("GetInfections", "infections");
var regions = getJson("GetInfections", "regions");
var beginDate = parseDate(document.getElementById('trackBarValue').innerHTML);

function onChangeTrackBar() {
    let index = $('#trackbar').val();
    let newDate = addDays(beginDate, index);
    document.getElementById('trackBarValue').innerHTML = newDate.toLocaleDateString();
    let max = Math.log(infections[infections.length - 1].infections[0].infected);
    for (i = 0; i < infections[index].infections.length; i++) {
        let size = Math.log(infections[index].infections[i].infected) / max * 100;
        let region = infections[index].infections[i].region;
        $('#region_circle_' + region).css({ r: size });
    }
}

function parseDate(input) {
    let parts = input.split('.');
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