$(function () {

    //$("#btnPretraga").click(function () {
    //    window.location.href = '/Skladiste/AktivnaTrebovanja?txtPretraga=' + $("#inputPretraga").val();
    //});



    $(".btnSacuvajIzmjene").click(function () {
        var idTrebovanja = $(this).val();
             
       //objekat boji saljemo preko ajax-a u c#
        let trebovanjeObject = {
            TrebovanjeId : $(this).val(),
            ListaArtikalaTrebovanja : []             
            
        };       
        var id = "#" + idTrebovanja;
        $(id).find('ul li').each(function (i) { 

            //provjera da li je cekirano true/false
            var spremno = $(this).find('.SpremnoCheckBox').is(':checked');
            var preuzeto = $(this).find('.PreuzetoCheckBox').is(':checked');            

            if ($(this).attr('rel') !== undefined) {
                var artikl = { ArtiklId: $(this).attr('rel'), Spremno: spremno, Preuzeto: preuzeto};
                trebovanjeObject.ListaArtikalaTrebovanja.push(artikl);
            }
        });

        AjaxCekiranjeTrebovanja(trebovanjeObject);
     
    });


    $("#btnFilter").click(function () {
         //najstarijeg ka najnovijem  = 1
        //najnovijeg ka najstarijem = 2
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();
     

        window.location.href = '/Skladiste/FilterTrebovanje?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;
        //$("#inputSortiranjeDatum").val() = sortiranjeDatum;
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
        $.ajax({
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            type: 'POST',
            url: '/Skladiste/IzmjenaAktivnogTrebovanja',
            data: JSON.stringify(data),
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





    //jedno Trebovanje //f-ja se poziva kada skladistaru stigne mail pa otvori detalje jednog trebovanja
    $("#btnSacuvajJednoTrebovanje").click(function () {

        //objekat boji saljemo preko ajax-a u c#
        let trebovanjeObject = {
            TrebovanjeId: $(this).val(),
            ListaArtikalaTrebovanja: [

            ]


        };
        //var id = "#" + idTrebovanja;
        $('#idTrebovanja').find('ul li').each(function (i) {

            //provjera da li je cekirano true/false
            var spremno = $(this).find('.SpremnoCheckBox').is(':checked');
            var preuzeto = $(this).find('.PreuzetoCheckBox').is(':checked');

            if ($(this).attr('rel') !== undefined) {
                var artikl = { ArtiklId: $(this).attr('rel'), Spremno: spremno, Preuzeto: preuzeto };
                trebovanjeObject.ListaArtikalaTrebovanja.push(artikl);
            }
        });

        AjaxCekiranjeTrebovanja(trebovanjeObject);
    });


    $("#btnObrisiFilterVrijednosti").click(function () {

        $("#pretragaPoKorisnikuIArtiklu").val(" ");
        //$("#inputSortiranjeDatum").val("Izaberi...");
        kupljenePodatakaIzFilteraIReload();

    });

    function kupljenePodatakaIzFilteraIReload() {
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();

        window.location.href = '/Skladiste/FilterZavrsenaTrebovanja?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;

    }

   
});