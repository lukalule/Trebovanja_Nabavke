$(function () {

    $('.btnOdobri').on('click', function () {
        var glavniDivNabavka = $(this).closest('.divZaTrazenjeIDa');
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });
        var data = {
            odobreno: true,
            nabavka: $(glavniDivNabavka).find('#idNabavke').attr('rel'),
            napomena: $('#NapomenaNadredjenog').val()//dodati div
        };

        $.post('/OdobravanjeNabavke/SlanjeMejlaZaOdobravanjeNabavke', data, function (result) {
            //zamjeniti sa ajax pozivom za reload
            if (result.succses === true) {               
                toastr.success("Zahtjev je odobren!", "Uspješno", { timeOut: 3000 });

                window.setTimeout(function () {
                    window.location.href = '/OdobravanjeNabavke/OdobravanjeListeNabavki';
                }, 3000);

            }
            else {
                alert('Došlo je do greške, pokušajte ponovo!')
                window.location.reload();
            }

        });
    });

    $('.btnOdbij').on('click', function () {
        var glavniDivNabavka = $(this).closest('.divZaTrazenjeIDa');
        $("#loader").removeClass('sakrij');
        $("#loader").modal({
            backdrop: "static",
            keyboard: false,
            show: true
        });
        var data = {
            odobreno: false,
            nabavka: $(glavniDivNabavka).find('#idNabavke').attr('rel'),
            napomena: $(glavniDivNabavka).find('#NapomenaNadredjenog').val()//dodati div
        }
        $.post('/OdobravanjeNabavke/SlanjeMejlaZaOdobravanjeNabavke', data, function (result) {
                        //zamjeniti sa ajax pozivom za reload

            if (result.succses === true) {
                toastr.success("Zahtjev je odbijen!", "Uspješno", { timeOut: 3000 });

                window.setTimeout(function () {
                    window.location.href = '/OdobravanjeNabavke/OdobravanjeListeNabavki';
                }, 3000);
            }
            else {
                alert('Došlo je do greške, pokušajte ponovo!')
                window.location.reload();
            }
        });

    });

});


