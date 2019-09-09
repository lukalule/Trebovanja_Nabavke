$(function () {

        $('.btnOdobri').on('click', function () {
            var glavniDivTrebovanja = $(this).closest('.divZaTrazenjeIDa');
            $("#loader").removeClass('sakrij');
            $("#loader").modal({
                backdrop: "static",
                keyboard: false,
                show: true
            });
            var data = {
                odobreno: true,
                trebovanje: $(glavniDivTrebovanja).find('#idTrebovanja').attr('rel'),
                napomena: $(glavniDivTrebovanja).find('#NapomenaNadredjenog').val()
            }
            $.post('/Email/SlanjeMejlaZaOdobravanjeTrebovanja', data, function (result) {
                if (result.succses === true) {
                    toastr.success("Zahtjev je odobren!", "Uspjesno", { timeOut: 3000 });

                    window.setTimeout(function () {
                        window.location.href = '/Email/OdobravanjeListeTrebovanja';
                    }, 3000);
                }
                else {
                    alert(result.message);
                    window.location.href = '/Email/OdobravanjeListeTrebovanja';
                }

            });

        });

        $('.btnOdbij').on('click', function () {
                var odobreno = false;
            var glavniDivTrebovanja = $(this).closest('.divZaTrazenjeIDa');
            $("#loader").removeClass('sakrij');
            $("#loader").modal({
                backdrop: "static",
                keyboard: false,
                show: true
            });
                var trebovanje = $(glavniDivTrebovanja).find('#idTrebovanja').attr('rel');
                var napomena = $(glavniDivTrebovanja).find('#NapomenaNadredjenog').val();
            $.post('/Email/SlanjeMejlaZaOdobravanjeTrebovanja', { odobreno, trebovanje, napomena }, function (result) {
                if (result.succses === true) {
                    toastr.success("Zahtjev je odbijen!", "Uspjesno", { timeOut: 3000 });
                    window.setTimeout(function () {
                        window.location.href = '/Email/OdobravanjeListeTrebovanja';
                    }, 3000);
                        
                    }
                    else {
                        alert(result.message);
                        window.location.href = '/Email/OdobravanjeListeTrebovanja';
                    }
                });

        });

});


    