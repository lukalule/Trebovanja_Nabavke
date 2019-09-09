$(function () {


    $("#btnFilter").click(function () {
        //najstarijeg ka najnovijem  = 1
        //najnovijeg ka najstarijem = 2

        kupljenePodatakaIzFilteraIReload();

        $("#pretragaPoKorisnikuIArtiklu").val(sortiranjePoImenu);
        $("#inputSortiranjeDatum").val(sortiranjeDatum);
        $("#sortiranjeStatus").val(sortiranjeStatus);
    });

    $("#BtnObrisiPretragu").click(function () {
        $("#pretragaPoKorisnikuIArtiklu").val(" ");
    });

    $("#btnObrisiFilterVrijednosti").click(function () {

        $("#pretragaPoKorisnikuIArtiklu").val(" ");
        $("#sortiranjeStatus option:selected").text("Prikazi sve...");
        $("#inputSortiranjeDatum").val("Izaberi...");

        kupljenePodatakaIzFilteraIReload();



    });

    function kupljenePodatakaIzFilteraIReload() {
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();
        var sortiranjeStatus = $("#sortiranjeStatus option:selected").text();
        window.location.href = '/Nabavke/FilterMojeNabavke?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu + "&StatusTrebovanja=" + sortiranjeStatus;
    }


});