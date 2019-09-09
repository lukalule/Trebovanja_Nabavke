$(document).ready(function () {


    var table = $('#TabelarniPrikaz').DataTable({

        "ajax": {
            url: "/Marketing/ListaAktivnihTrebovanjaZaDT",
            type: 'POST'

        },
        "processing": true,
        //"serverSide": true,
        //"bPaginate": true,
        "sPaginationType": "full_numbers",
        "lengthMenu": [[10, 25, 50, 100, 200, -1], [10, 25, 50, 100, 200, "Prikaži sve"]],

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
        "columnDefs": [
            {
            "targets": 6,
            "data": null,
            "defaultContent": "<button class='btn btn-outline-primary mr-1 mb-2'>Naručeno</button>"
            },
            {
                //sakrijemo Id trebovanja
                "targets": [0],
                "visible": false,
                "searchable": false
            }
        ],
        "language": {
            "info": "_START_ - _END_ / _TOTAL_ trebovanja",
            "paginate": {
                "previous": "Prethodna",
                "next": "Sledeća",
                "last": "Zadnja",
                "first": "Prva"

            },
            "search": "Pretraga",
            "sLengthMenu": "Prikaži _MENU_ zapisa",
            'processing': '<i class= "fa fa-spinner fa-spin fa-3x fa-fw" ></i> <span class="sr-only">Loading..n.</span>',
            "zeroRecords": "Nema pronadjenjih trebovanja"
        },
        "columns": [
            //kada se dodaje nova kolona obavezno dodati i th u tabeli
            //{ "data": "SerijskiBroj", "targets": 0 },
            { "data": "TrebovanjeId", "targets": 0 },
            { "data": "SerijskiBroj", "targets": 1 },
            { "data": "SifraRadnika", "targets": 2 },
            { "data": "ImeIPrezimeRadnika", "targets": 3 },
            {
                "data": "DatumPodnesenogZahtjeva",
                "render": function (data) {
                    var re = /-?\d+/;
                    var m = re.exec(data);
                    var d = new Date(parseInt(m[0]));
                    moment.locale('pt');
                    var k = moment(d).format('L LTS');
                    return k;
                }
            },         
            {
                "data": "ListaArtikalaTrebovanja",
                "render": "[, ].Naziv"
            }


            


        ]
    });


    $('#TabelarniPrikaz tbody').on('click', 'button', function () {
        var data = table.row($(this).parents('tr')).data();
        var trebovanjeId = data.TrebovanjeId;
        NarucivanjeIzDTAjax(trebovanjeId);
    });

    function NarucivanjeIzDTAjax(trebovanjeId) {
        $.ajax({
            type: "POST",
            url: "/Marketing/Narucivanje?trebovanje=" + trebovanjeId,
            data: trebovanjeId,
            contentType: "application/json; charset=utf-8",
            success: function () {
                //dodati loader
                   
                $('#TabelarniPrikaz').DataTable().ajax.reload();
                toastr.success("Trebovanje uspješno naručeno", "Uspješno", { timeOut: 3000 });          

            }
        });
    }


});

