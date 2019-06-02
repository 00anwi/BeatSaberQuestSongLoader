window.LoadDataTables = () => {

    if (!$.fn.DataTable.isDataTable('#AvailableSongsTable') ) {
        AvailableSongstable = $('#AvailableSongsTable').DataTable({
            "dom": 'Bfrtip',
            "processing": true,
            "ajax": {
                "url": "/api/DataTables/GetAvailableSongs",
                "method": "POST"
            },
            "columns": [
                { "data": "songFolder" },
                { "data": "songName" }
            ],
            "tabIndex": -1,
            "ordering": true,
            "order": [[1, "asc"]],
            "paging": false,
            "select": true,
            "scrollY": 500
        });
        AvailableSongstable.column(0).visible(false);
    }
    else {
        AvailableSongstable.ajax.reload();
    }


    if (!$.fn.DataTable.isDataTable('#SongsToLoadTable') ) {
        SongsToLoadtable = $('#SongsToLoadTable').DataTable({
            "dom": 'Bfrtip',
            "processing": true,
            "ajax": {
                "url": "/api/DataTables/GetSongsToLoad",
                "method": "POST"
            },
            "columns": [
                { "data": "songFolder" },
                { "data": "songName" }
            ],
            "tabIndex": -1,
            "ordering": true,
            "order": [[1, "asc"]],
            "paging": false,
            "select": true,
            "scrollY": 500,
        });
        SongsToLoadtable.column(0).visible(false);
    }
    else {
        SongsToLoadtable.ajax.reload();
    }
}

window.GetSelectedSongsInAvailableSongsTable = () => {
    var selecedSongs = [];
    var rows = AvailableSongstable.rows({ selected: true }).count();

    for (i = 0; i < rows; i++) {
        var AvailableSongstableRow = AvailableSongstable.rows({ selected: true }).data()[i];
        selecedSongs.push(AvailableSongstableRow.songName);
    };

    return selecedSongs;
}

