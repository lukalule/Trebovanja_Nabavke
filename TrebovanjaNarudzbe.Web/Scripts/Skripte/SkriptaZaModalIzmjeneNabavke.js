$(document).ready(function () {
    //alert("test");
    var vrijednostReda;
    
    $(".btnPrikaziModal").click(function () {

        vrijednostReda = $(this).closest('tr').attr('rel');

        //kupimo vrijednosti iz tabele za odredjeni red u zavisnosti od btn na koji kliknemo
        var artikl = {
            opis : $(this).closest('tr').find('.opis').text(),
            kolicina : $(this).closest('tr').find('.kolicina').text(),
            cijena : $(this).closest('tr').find('.cijena').text(),
            dobavljac : $(this).closest('tr').find('.dobavljac').text()
        }

        //vrijednosti iz reda u tabeli upisujemo u input polja u modal-u
        $("#exampleModal").find(".inputOpis").val(artikl.opis)
        $("#exampleModal").find(".inputKolicina").val(artikl.kolicina)
        $("#exampleModal").find(".inputCijena").val(artikl.cijena)
        $("#exampleModal").find(".inputDobavljac").val(artikl.dobavljac)



    });

    $("#btnSacuvajIzmjene").click(function () {

        var inputCijena = $(".inputCijena").val();       
        var provjera = $.isNumeric(inputCijena);

        if (provjera) {

            //spremimo u varijable vrijednosti iz inputa
            var inputOpis = $(".inputOpis").val();
            var inputDobavljac = $(".inputDobavljac").val();

            //svaki red u tabeli ima svoju klasu 
            var klasaReda = "." + vrijednostReda

            $(klasaReda).find('.cijena').text(inputCijena);
            $(klasaReda).find('.opis').text(inputOpis);
            $(klasaReda).find('.dobavljac').text(inputDobavljac);

            $(function () {
                $('#exampleModal').modal('toggle');
            });

        } else {
            $(".inputCijena").val(" ");
            toastr.warning("Cijena mora biti broj", "Upozorenje", { timeOut: 2500 });
        }

    });

    
});