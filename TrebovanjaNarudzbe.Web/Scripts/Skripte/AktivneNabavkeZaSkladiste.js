$(function () {
    $(".btnSacuvajIzmjene").click(function () {
        var idNabavke = $(this).val();
        console.log(idNabavke);
        //objekat boji saljemo preko ajax-a u c#
        let nabavkaObject = {
            NabavkaId: $(this).val(),
            Stavke: [ ]
        };

        var id = "#" + idNabavke;
        $(id).find('ul li').each(function (i) {

            //provjera da li je cekirano true/false
            var spremno = $(this).find('.SpremnoCheckBox').is(':checked');
            var preuzeto = $(this).find('.PreuzetoCheckBox').is(':checked');

            if ($(this).attr('rel') !== undefined) {
                var artikl = { NabavkaVeznaId: $(this).attr('rel'), Spremno: spremno, Preuzeto: preuzeto };
                nabavkaObject.Stavke.push(artikl);
            }
        });

        AjaxCekiranjeTrebovanja(nabavkaObject);
    });


    $("#btnFilter").click(function () {
        //najstarijeg ka najnovijem  = 1
        //najnovijeg ka najstarijem = 2
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();

        window.location.href = '/Skladiste/FilterNabavke?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;
        $("#pretragaPoKorisnikuIArtiklu").val(sortiranjePoImenu);
    });

    $("#BtnObrisiPretragu").click(function () {
        $("#pretragaPoKorisnikuIArtiklu").val(" ");
    });


    function AjaxCekiranjeTrebovanja(data) {
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });
        var viewModel = data;
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: '/Skladiste/IzmjenaAktivneNabavke',
            data: JSON.stringify(viewModel),
            success: function (data) {
                if (data.success) {
                    //alert("Success: True");
                    location.reload();
                } else {
                    alert("greska");
                }
            },
            error: function (data) {
            }
        });
    }





    //jedno nabavku //f-ja se poziva kada skladistaru stigne mail pa otvori detalje jedne nabavke
    $("#btnSacuvajJednuNabavku").click(function () {

        //objekat boji saljemo preko ajax-a u c#
        let nabavkaObject = {
            NabavkaId: $(this).val(),
            Stavke: [  ]
        };
        $('#idNabavke').find('ul li').each(function (i) {

            //provjera da li je cekirano true/false
            var spremno = $(this).find('.SpremnoCheckBox').is(':checked');
            var preuzeto = $(this).find('.PreuzetoCheckBox').is(':checked');

            if ($(this).attr('rel') !== undefined) {
                var artikl = { NabavkaVeznaId: $(this).attr('rel'), Spremno: spremno, Preuzeto: preuzeto };
                nabavkaObject.Stavke.push(artikl);
            }
        });

        AjaxCekiranjeTrebovanja(nabavkaObject);
    });

});