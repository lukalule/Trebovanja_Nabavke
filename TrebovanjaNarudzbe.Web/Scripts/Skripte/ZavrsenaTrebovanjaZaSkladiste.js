$(function () {

    $("#btnFilter").click(function () {
        //najstarijeg ka najnovijem  = 1
        //najnovijeg ka najstarijem = 2
       // var sortiranjeDatum = $("#inputSortiranjeDatum option:selected").text();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();
        var sortiranjeDatum = $("#inputSortiranjeDatum option:selected").val();

        window.location.href = '/Skladiste/FilterZavrsenaTrebovanja?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;
      
      
    });

    $("#BtnObrisiPretragu").click(function () {
        $("#pretragaPoKorisnikuIArtiklu").val("");
    });

    $("#btnObrisiFilterVrijednosti").click(function () {
      
        $("#pretragaPoKorisnikuIArtiklu").val("");
        //$("#inputSortiranjeDatum").val("Izaberi...");
        kupljenePodatakaIzFilteraIReload();

    });

    function kupljenePodatakaIzFilteraIReload() {
        var sortiranjeDatum = $("#inputSortiranjeDatum").val();
        var sortiranjePoImenu = $("#pretragaPoKorisnikuIArtiklu").val();

        window.location.href = '/Skladiste/FilterZavrsenaTrebovanja?SortiranjeDatum=' + sortiranjeDatum + "&SortiranjePoImenu=" + sortiranjePoImenu;

    }

});