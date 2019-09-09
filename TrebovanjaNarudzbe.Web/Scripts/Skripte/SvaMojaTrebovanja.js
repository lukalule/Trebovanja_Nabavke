$(document).ready(function () {

    $("#inputSortiranjeDatum").change(function () {

        var metoda = $('#Metoda').text();

        var vrijednost = $('#inputSortiranjeDatum option:selected').val();
        if (vrijednost === "1" || vrijednost === "2") {
            window.location.href = "/Trebovanje/" + metoda + "?page=0;&vrijednost=" + vrijednost;
        }

    });
});