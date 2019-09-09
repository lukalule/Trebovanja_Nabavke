$(document).ready(function () {
    //Lista stavki/artikala za popunjavanje tabele
    var listaArtikala = [];

    //Cuvanje objekat dugmeta za brisanje(pri pojedinacnom brisanju)
    var dugmeX;

    //funkcija za poziv select2 scripte
    $('.js-example-basic-single').select2({
        width: 'resolve',
        allowClear: false,
        escapeMarkup: function (markup) { return markup; },
        templateResult: function (result) {
            return result.htmlmatch ? result.htmlmatch : result.text;
        },

        //Funkcija za pretragu i boldovanje unesenog teksta u polje za pretragu
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

    //Funkcija za unos stavki u tabelu
    function UpisPodatakaUTabelu(listaArtikala) {

        //Brisanje redova iz tabele
        $("#tbody").empty();

        //Ukupna cijena svih stavki
        var cijenaZaPrikaz = 0;

        //Popunjvanje tabele sa informacijama o unesenim stavkama
        for (var i = 0; i < listaArtikala.length; i++) {
            var tr = "<tr class='red'>";
            tr += "<td class='redniBrArtikla'>" + (i + 1) + "</td>" +
                `<td class="klasaArtiklId" hidden>` + listaArtikala[i].ArtiklId + "</td>" +
                `<td class="klasaOpis">` + listaArtikala[i].Opis + "</td>" +
                `<td class="klasaKolicina">` + listaArtikala[i].Kolicina.toString() + "</td>" +
                `<td class="klasaDobavljac">` + listaArtikala[i].Dobavljac + "</td>" +
                `<td class="klasaCijena">` + listaArtikala[i].Cijena.toFixed(2) + "</td>" +
                `<td class="klasaUkupnaCijena">` + listaArtikala[i].UkupnaCijenaStavke.toFixed(2) + "</td>" +
                "<td>" +
                "<span class='obrisi text-gray' style='cursor:pointer'><i class='fas fa-times'></i></span> </td > </tr > ";
            tbody.innerHTML += tr;
            cijenaZaPrikaz += listaArtikala[i].UkupnaCijenaStavke;
        }

        //Upisivanje ukupne cijene svih stavki u polje u tabeli
        $('#UkupnaCijenaNabavke').val(cijenaZaPrikaz.toFixed(2) + " KM");
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

        toastr.error("Uspješno ste obrisali stavku iz tabele!", "Uspješno", { timeOut: 3000 });
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

    //Funkcija za vraćanje cijene izabranog artikla i unos cijene u polje
    $('#ArtiklOpcije').change(function () {
        var data = $('#ArtiklOpcije').select2('data');
        if (data[0] !== null && data[0] !== undefined) {
            var nazivArtiklaISifra = data[0].text;

            //Čuvanje šifre artikla
            var sifraArtikla = nazivArtiklaISifra.substring(
                nazivArtiklaISifra.lastIndexOf("[") + 1,
                nazivArtiklaISifra.lastIndexOf("]")).trim();

            //Metoda iz Nabavke kontrolera za vraćanje cijene
            $.get('/Nabavke/VratiCijenuArtikla', { sifraArtikla }, function (cijena) {
                if (cijena !== null && cijena !== undefined) {
                    $('#CijenaArtikla').val(cijena.cijena.toFixed(2));
                }
                else
                    toastr.warning("Artikl nije pronađen!", "Upozorenje", { timeOut: 3000 });
            });
        }
    });
    
    $('#ArtiklOpcije').trigger("change");

    function provjeraValidsnotiArtikla() {
        var greska = false;
        if ($('#CijenaArtikla').val() === "" || $("#CijenaArtikla").val() < 0.1) {
            toastr.warning("Cijena nije validna!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }
        
        if ($("#KolicinaArtikla").val() < 1) {
            toastr.warning("Kolicina željenog aktikla mora biti 1 ili više!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        if ($("#ArtiklOpcije").val() === null || $("#ArtiklOpcije").val() === "") {
            toastr.warning("Morate izabrati artikal!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        return greska;
    }

    //Funkcija za dodavanje postojećeg ARTIKLA iz dropdown liste u listu artikala
    $("#btnDodajUKorpu").click(function () {

        if (provjeraValidsnotiArtikla())
            return false;

        //Kupimo vrijednost iz select2 inputa
        var data = $('#ArtiklOpcije').select2('data');
        var kolicina = $("#KolicinaArtikla").val();
        var nazivArtiklaISifra = data[0].text;
        var sifraArtikla = nazivArtiklaISifra.substring(
            nazivArtiklaISifra.lastIndexOf("[") + 1,
            nazivArtiklaISifra.lastIndexOf("]")).trim();
        var nazivArtikla = nazivArtiklaISifra.substr(nazivArtiklaISifra.indexOf("]") + 1);
        var artikl = { ArtiklId: sifraArtikla };
        var cijena = $('#CijenaArtikla').val();
        if (!containsObject(artikl, listaArtikala)) {
            listaArtikala.push(
                {
                    "Opis": nazivArtikla.trim(),
                    "Kolicina": parseInt(kolicina),
                    "Cijena": Number(cijena),
                    "Dobavljac": "",
                    "UkupnaCijenaStavke": parseInt(kolicina) * cijena,
                    "ArtiklId": sifraArtikla
                });

            //nakon klika na dugme brisemo vrijednost select2 input-a #ArtiklOpcije
            $("#KolicinaArtikla").val("1");
            $('#CijenaArtikla').val("1");
            $('#ArtiklOpcije').value = "";
            //Pupunjavanje tabele 
            UpisPodatakaUTabelu(listaArtikala);
            toastr.success("Uspjesno ste dodali artikl u korpu!", "Uspjesno", { timeOut: 3000 });
        }
        else {
            toastr.warning("Artikl ste vec  dodali u korpu!", "Upozorenje", { timeOut: 3000 });
        }

    });

    function provjeraValidnostiNovogProizvoda() {
        var greska = false;
        if ($('#Cijena').val() === "" || $("#Cijena").val() < 0.1) {
            toastr.warning("Cijena nije validna!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        if ($('#Dobavljac').val() === "") {
            toastr.warning("Dobavljač polje je prazno!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        if ($("#Kolicina").val() < 1) {
            toastr.warning("Kolicina željenog aktikla mora biti 1 ili više!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        if ($("#Opis").val() === null || $("#Opis").val() === "") {
            toastr.warning("Morate popuniti opis!", "Upozorenje", { timeOut: 3000 });
            greska = true;
        }

        return greska;
    }

    //Funkcija za dodavanje ARTIKLA iz Opis polja
    $("#btnDodajUTabelu").click(function () {
        if (provjeraValidnostiNovogProizvoda())
            return false;

        var kolicina = $("#Kolicina").val().trim();
        var opis = $("#Opis").val().trim();
        var cijena = $('#Cijena').val();
        var dobavljac = $('#Dobavljac').val();

        listaArtikala.push(
            {
                "Opis": opis,
                "Kolicina": parseInt(kolicina),
                "Cijena": Number(cijena),
                "Dobavljac": dobavljac,
                "UkupnaCijenaStavke": parseInt(kolicina) * cijena,
                "ArtiklId": null
            });

        $("#Kolicina").val("1");
        $("#Opis").val("");
        $('#Cijena').val("1");
        $('#Dobavljac').val("");

        toastr.success("Uspjesno ste dodali stavku u tabelu!", "Uspjesno", { timeOut: 3000 });
            UpisPodatakaUTabelu(listaArtikala);
    });

    $("#BtnPrikaziModal").click(function () {
        $('#PotvrdiNabavku').modal('show');
    });

    var files = [];
    
    function addIconsToDiv() {

        $('#Ikone').empty();
        $(files).each(function (index, element) {
            var file = element.name;
            if (file.endsWith(".pdf")) {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/pdf-1.png'/><span class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
            else if (file.endsWith(".docx")) {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/word-1.png'/><span class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
            else if (file.endsWith(".xlsx") || file.endsWith(".xls")) {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/excel.png'/><span class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
            else if (file.endsWith(".png") || file.endsWith(".jpg") || file.endsWith(".jpeg")) {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/polaroid-1.png'/><span class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
            else if (file.endsWith(".zip") || file.endsWith(".rar")) {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/zip.png'/><span class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
            else if (file.endsWith(".exe") || file.endsWith(".app") || file.endsWith(".bat")) {
                toastr.warning("Ne možete dodati izvršne fajlove!!!", "Upozorenje", { timeOut: 3000 });
                files.splice(index, 1);
            }
            else {
                $('#Ikone').append("<div class='col-md-3 m-2'><span style='cursor:pointer ' class='brisiDatoteku text-danger' data-index='" + index + "'>&Chi;</span><img class='img-ikone' style='width:50px; height:auto;' src='/Content/img/default.png'/><span  class='text-muted text-truncate ikone'>" + file + "</span></div>")
            }
        });

        $('.brisiDatoteku').click(function () {
            var index = $(this).attr('data-index');

            files.splice(index, 1);

            addIconsToDiv();
        });
    };

    $('#brisanjeSvihFajlova').click(function () {
        files = [];
        
        addIconsToDiv();
    });

    $('#Dokumenti').on('change', prepareUpload);

    var dropZone = document.getElementById('drop_zone');
    dropZone.addEventListener('dragover', handleDragOver, false);
    dropZone.addEventListener('drop', appendDroppedFiles, false);
    dropZone.addEventListener('dragenter', dragenterHandler, false);
    dropZone.addEventListener('dragleave', dragleaveHandler, false);

    window.addEventListener("dragover", function (e) {
        e = e || event; //za starije browsere
        e.preventDefault();
    }, false);
    window.addEventListener("drop", function (e) {
        e = e || event;
        e.preventDefault();
    }, false);

    function appendDroppedFiles(event) {
        event.stopPropagation();
        event.preventDefault();
        var fajlovi = [];
        $(event.dataTransfer.items).each(function (i, file) {
            fajlovi.push(file.getAsFile());
        });

        if (files == undefined) //prvi put
        {
            files = fajlovi;
        }
        else {
            files.push.apply(files, fajlovi);
        }
        addIconsToDiv();

        $('#drop_zone').removeClass('hover');
        $('#drop_zone').removeClass('mojaklasa');
    }
    // Grab the files and set them to our variable
    function prepareUpload(event) {
        $('#Ikone').empty();

        //puni listu fajlovima
        if (files == undefined) //prvi put
            files = event.target.files;
        else
            files.push.apply(files, event.target.files);//svaki put poslije toga

        addIconsToDiv();
    }

    $('#formaZaNabavku').on('submit', uploadFiles);
    function uploadFiles(event) {
       
        $('#PotvrdiNabavku').modal('hide');

        if ($('#ReferentId').val() === "") {
            toastr.warning("Morate izabrati referenta nabavke!", "Upozorenje", { timeOut: 3000 });
            return false;
        }

        if (listaArtikala.length === 0) {
            toastr.warning("Morate dodati minimalno 1 stavku!", "Upozorenje", { timeOut: 3000 });
            return false;
        }
        
        event.stopPropagation(); // Stop stuff happening
        event.preventDefault(); // Totally stop stuff happening
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });

        // Create a formdata object and add the files
        var data = new FormData();

        data.append("TipId", $('#TipId').val());
        data.append("Obrazlozenja", $('#Obrazlozenja').val());

        data.append("SifraReferentaNabavke", $('#ReferentId').val());

        data.append("Odgovoran", $('#OdgovorniId').val());
        
        for (var i = 0; i < files.length; i++) {
            data.append("Files[" + i + "]", files[i]);
        }
        $("#tabelaArtikala tr.red").each(function (i) {
            data.append("Stavke[" + i + "].Opis", $(this).find('.klasaOpis').html());
            data.append("Stavke[" + i + "].Kolicina", $(this).find('.klasaKolicina').html());
            data.append("Stavke[" + i + "].Cijena", $(this).find('.klasaCijena').html());
            data.append("Stavke[" + i + "].ArtiklId", $(this).find('.klasaArtiklId').html());
            data.append("Stavke[" + i + "].Dobavljac", $(this).find('.klasaDobavljac').html());
        });


        $.ajax({
            url: '/Nabavke/NovaNabavkaZaOsnovnaSredstva',
            type: 'POST',
            data: data,
            dataType: 'json',
            processData: false, // Don't process the files
            contentType: false, // Set content type to false as jQuery will tell the server its a query string request
            success: function (data) {
                if (data.success == true) {
                    files = undefined;

                    $('#BtnPrikaziModal').attr('disabled', 'disabled');
                    $('#BtnPrikaziModal').addClass('disabled');
                    $('#btnDodajUTabelu').attr('disabled', 'disabled');
                    $('#btnDodajUTabelu').addClass('disabled');
                    toastr.success("Zahtjev za nabavku poslat!", "Uspjeh!", { timeOut: 3000 });
                   
                    window.setTimeout(function () {
                        window.location.href = '/Nabavke/IstorijaNabavkiKorisnika';
                    }, 3000);
                }
                else {
                    toastr.warning("Nabavka nije kreirana!", "Upozorenje", { timeOut: 3000 });
                    return false;
                }
            }
        });

    }

    function handleDragOver(evt) {
        evt.preventDefault();
        evt.dataTransfer.effectAllowed = 'copy';
        evt.dataTransfer.dropEffect = 'copy';
        $('#drop_zone').addClass('hover');
        $('#drop_zone').addClass('mojaklasa');
    }

    function dragenterHandler() {
        //$('#drop_zone').removeClass('drop_zone');
        $('#drop_zone').addClass('hover');
        $('#drop_zone').addClass('mojaklasa');
    }

    function dragleaveHandler() {
        $('#drop_zone').removeClass('hover');
        $('#drop_zone').removeClass('mojaklasa');
    }
});