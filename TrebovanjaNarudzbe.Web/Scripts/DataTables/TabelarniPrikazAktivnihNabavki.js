$(document).ready(function () {
    $('#TabelarniPrikaz').DataTable({

        "ajax": {
            url: "/Skladiste/ListaAktivnihNabavkiZaDT",
            type: 'POST'

        },
        "processing": true,
        //"serverSide": true,
        //"bPaginate": true,
        "sPaginationType": "full_numbers",
        "lengthMenu": [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "Prikaži sve"]],
        // "iDisplayLength": 10,

        "dom": 'lBfrtip',
        "buttons": [
            {
                extend: 'copyHtml5',
                text: '<i class="fas fa-copy align-items-center">&ensp;Kopiraj</i>',
                titleAttr: 'Copy'
            },
            {
                extend: 'excelHtml5',
                text: '<i class="fa fa-file-excel align-items-center" aria-hidden="true">&ensp;Excel</i>',
                titleAttr: 'Excel'
            },
            {
                extend: 'csvHtml5',
                text: '<i class="fas fa-file-csv align-items-center">&ensp;CSV</i>',
                titleAttr: 'CSV'
            },
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf align-items-center">&ensp;PDF</i>',
                titleAttr: 'PDF'
            },
            {

                extend: 'print',
                text: '<i class="fas fa-print align-items-center">&ensp;Štampaj</i>',
                autoPrint: false
            }
        ],


        "language": {
            "info": "_START_ - _END_ / _TOTAL_ nabavki",
            "paginate": {
                "previous": "Prethodna",
                "next": "Sljedeća",
                "last": "Zadnja",
                "first": "Prva"

            },
            "search": "Pretraga",
            "sLengthMenu": "Prikaži _MENU_ zapisa",
            'processing': '<i class= "fa fa-spinner fa-spin fa-3x fa-fw" ></i> <span class="sr-only">Loading..n.</span>',
            "zeroRecords": "Nema pronadjenjih nabavki"
        },
        "columns": [
            //kada se dodaje nova kolona obavezno dodati i th u tabeli
            {
                "data": "SerijskiBroj",
                "className": "serijskiBroj",
                "targets": 0
            },
            { "data": "SifraRadnika", "targets": 1 },
            { "data": "ImeIPrezimeRadnika", "targets": 2 },
            {
                "data": "DatumPodnosenjaZahtjeva",
                "targets": 3,
                "render": function (data) {
                    var re = /-?\d+/;
                    var m = re.exec(data);
                    var d = new Date(parseInt(m[0]));
                    moment.locale('pt');
                    var k = moment(d).format('ll');
                    return k;
                }
            }
            //{
            //    "data": "Stavke",
            //    "render": "[, ].Opis",
            //    "targets": 4
            //}

        ]
    });


    $('#TabelarniPrikaz tbody').on('click', 'tr', function () {
        //$('#loader').removeClass('sakrij');
        //$("#loader").modal({
        //    backdrop: "static",
        //    keyboard: false,
        //    show: true
        //});
        var data = { serijskiBroj: $(this).find('.serijskiBroj').html() };

        $.get('/Skladiste/AktivnaNabavkaPartial', data, function (partial) { //vracam objekat trebovanja
          
                $("#myModal").find("div.modal-body").html(partial);
                $("#myModal").modal("show");
                $('#loader').addClass('sakrij');
          
        });

    });



});

