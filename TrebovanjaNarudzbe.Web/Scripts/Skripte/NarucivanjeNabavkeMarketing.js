$(function () {

    $('.btnNaruceno').on('click', function () {
        var glavniDivNabavka = $(this).closest('.divZaTrazenjeIDa');
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });
        var data = {
            nabavka: $(glavniDivNabavka).find('#idNabavke').attr('rel'),
            napomena: $(glavniDivNabavka).find('.napomenaReferanta').val()//dodati div
        };

        $.post('/Marketing/NabavkaNarucivanje', data, function (result) {
            //zamjeniti sa ajax pozivom za reload
            if (result == "True") {               
                toastr.success("Nabavka je naručena!", "Uspješno", { timeOut: 3000 });

                window.setTimeout(function () { window.location.href = '/Marketing/PrikazSvihAktivnihNabavki';}, 2000);
            }
            else {
                alert('Došlo je do greške, pokušajte ponovo!');
                window.location.reload();
            }

        });
    });


    $("#btnFilter").click(function () {
        //najstarijeg ka najnovijem  = 1
        //najnovijeg ka najstarijem = 2
        // var sortiranjeDatum = $("#inputSortiranjeDatum option:selected").text();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();
        var sortiranjeDatum = $("#inputSortiranjeDatum option:selected").val();

        window.location.href = '/Marketing/FilterAktivneNabavke?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;


    });

    $("#BtnObrisiPretragu").click(function () {
        $("#pretragaPoKorisnikuIArtiklu").val(" ");
    });

    $("#btnObrisiFilterVrijednosti").click(function () {

        $("#pretragaPoKorisnikuIArtiklu").val(" ");
        //$("#inputSortiranjeDatum").val("Izaberi...");
        kupljenePodatakaIzFilteraIReload();

    });

    function kupljenePodatakaIzFilteraIReload() {
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();

        window.location.href = '/Marketing/FilterAktivneNabavke?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;

    }
});