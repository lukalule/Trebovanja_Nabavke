$(document).ready(function () {


    var listaArtikala = [];
    var artikl;

    //funkcija za poziv select2 scripte
    $('.js-example-basic-single').select2({
        width: 'resolve',
        allowClear: false,
        escapeMarkup: function (markup) { return markup; },
        templateResult: function (result) {
            return result.htmlmatch ? result.htmlmatch : result.text;
        },
        matcher: function (params, data) {
            if ($.trim(params.term) === '') {
                return data;
            }
            if (typeof data.text === 'undefined') {
                return null;
            }

            var idx = data.text.toLowerCase().indexOf(params.term.toLowerCase());
            if (idx > -1) {
                var modifiedData = $.extend({
                    'htmlmatch': data.text.replace(new RegExp("(" + params.term + ")", "gi"), "<b style='font-weight: bold;'>$1</b>")
                }, data, true);

                return modifiedData;
            }
            return null;
        }

    });

    function UpisPodatakaUTabelu(listaArtikala) {
        $("#tbody").empty();
        for (var i = 0; i < listaArtikala.length; i++) {
            var tr = "<tr class='red'>";
            // if (listaArtikala[i].value.toString().substring(listaArtikala[i].value.toString().indexOf('.'), listaArtikala[i].value.toString().length) < 2) listaArtikala[i].value += "";

            tr += "<td class='redniBrArtikla'>" + (i + 1) + "</td>" +
                "<td class='klasaArtiklId' hidden>" + listaArtikala[i].ArtiklId + "</td>" +
                "<td class='klasaOpis'>" + listaArtikala[i].Opis + "</td>" +
                "<td class='klasaKolicina'>" + listaArtikala[i].TrebovanaKolicina + "</td>" +
                "<td>" +
                "<span class='obrisi text-gray' style='cursor:pointer'><i class='fas fa-times'></i></span> </td > </tr > ";
            tbody.innerHTML += tr;

        }
    }

    //Prikazivanje modala za potvrdu brisanja
    $(document).on('click', '.obrisi', function () {
        dugmeX = $(this);
        $('#BrisanjeArtiklaModal').modal('show');
    });

    //Brisanje artikla/stavke iz liste artikala i upis stavki u tabelu
    $("#btnObrisi").click(function () {
        var red = dugmeX.closest(".red");
        var polje = $(red).find(".redniBrArtikla").html();
        listaArtikala.splice(polje - 1, 1);

        $('#BrisanjeArtiklaModal').modal('hide');

        toastr.error("Uspješno ste obrisali stavku iz korpe!", "Uspješno", { timeOut: 3000 });
        UpisPodatakaUTabelu(listaArtikala);
    });

    //Funkcija za provjeru da li artikl već postoji u listi artikala
    function containsObject(obj, list) {
        var i;
        for (i = 0; i < list.length; i++) {
            if (list[i].ArtiklId === obj.ArtiklId) {
                return true;
            }
        }
        return false;
    }

    function provjeraValidsnotiArtikla() {
        var greska = false;

        if ($("#Kolicina").val() < 1 || $("#Kolicina").val() === "") {
            toastr.warning("Kolicina željenog aktikla mora biti 1 ili više!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        if ($("#ArtiklOpcije").val() === null || $("#ArtiklOpcije").val() === "") {
            toastr.warning("Morate izabrati artikal!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        return greska;
    }

    $("#btnDodajUKorpu").click(function () {

        if (provjeraValidsnotiArtikla())
            return false;

        //Kupimo vrijednost iz dropdown inputa
        var data = $('#ArtiklOpcije').select2('data');
        var kolicina = $("#Kolicina").val();
        var nazivArtiklaISifra = data[0].text;
        var sifraArtikla = nazivArtiklaISifra.substring(
            nazivArtiklaISifra.lastIndexOf("[") + 1,
            nazivArtiklaISifra.lastIndexOf("]")).trim();
        var nazivArtikla = nazivArtiklaISifra.substr(nazivArtiklaISifra.indexOf("]") + 1);
        var artikl = { ArtiklId: sifraArtikla };

        if (!containsObject(artikl, listaArtikala)) {
            listaArtikala.push(
                {
                    "Opis": nazivArtikla.trim(),
                    "TrebovanaKolicina": kolicina,
                    "ArtiklId": sifraArtikla
                });
            
            //nakon klika na dugme brisemo vrijednost select2 input-a
            $("#Kolicina").val("1");

            //pupunjavanje tabele 
            UpisPodatakaUTabelu(listaArtikala);
            toastr.success("Uspjesno ste dodali artikl u korpu", "Uspjesno", { timeOut: 3000 });
        }
        else {
            toastr.warning("Artikl ste vec  dodali u korpu", "Upozorenje", { timeOut: 3000 });
        }

    });


    $("#BtnKreirajTrebovanje").click(function () {
        if (listaArtikala.length > 0) {
            $('#PotvrdiTrebovanje').modal('show');

        } else {
            toastr.warning("Morate izabrati artikle iz liste prilikom kreiranja novog trebovanja", "Upozorenje", { timeOut: 3000 });
        }
    });

    $("#btnPotvrdi").click(function () {
        $('#PotvrdiTrebovanje').modal('hide');
        AjaxPozivZaKreiranjeNovogTrebovannja();
    });

    function AjaxPozivZaKreiranjeNovogTrebovannja() {
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });

        var data = {
            artikliZaTrebovanje: listaArtikala,
            napomenaRadnika: $('#napomenaRadnika').val()
        };

        $.ajax({
            url: '/Trebovanje/KreiranjeNovogTrebovanja',
            type: 'POST',
            //dataType: "json",
            data: data,
            success: function (response) {
                toastr.success("Uspjesno ste kreirali trebovanje", "Uspjesno", { timeOut: 3000 });


                window.location.href = '/Trebovanje/IstorijaTrebovanjaKorisnika';
            }
        });
    }
});



