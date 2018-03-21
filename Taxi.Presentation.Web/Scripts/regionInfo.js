$(document).ready(function () {
    $("#Countries").change(function () {
        let value = $("#Countries").val();
        $("#Cities").empty();
        if (value.length > 0) {            
            $.ajax({
                type: 'GET',
                url: `/Home/GetCities/${value}`,
                dataType: 'json',
                success: function (states) {
                    $("#Cities").append('<option value="" selected>Город не выбран</option>');

                    $.each(states, function (i, state) {
                        $("#Cities").append('<option value="' + state.Value + '">' +
                            state.Text + '</option>');

                    });
                },
                error: function (ex) {
                    console.error('Failed to retrieve country cities.');
                }
            });
        } else
            $("#Cities").append('<option value="" disabled selected>Город не выбран</option>');
        return false;
    });

    $("#Cities").change(function () {
        let value = $("#Cities").val();
        if (value.length > 0) {
            $('#region-info-detail').load(`/Home/GetRegionInfoDetail/${value}`);
        }
    });
});