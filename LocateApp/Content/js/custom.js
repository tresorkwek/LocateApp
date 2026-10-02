function fetchFammilly(legend = false) {

    var famille = null;

    $.ajax({
        type: "get",
        url: legend ? "/famille/stat/pie/legend/" : "/famille/stat/pie/",
        async: false,
        dataType: "json",
        success: function (result) {

            if (result.success) {

                famille = result.content;

            }
            else {
                console.log("aucun resultat");
            }

        },
        failure: function (result) {

            swal({
                title: "Echec",
                text: result.message,
                type: "warning",
                timer: 5000,
                html: true
            });

        },
        error: function (result) {

            swal({
                title: "Erreur",
                text: result.message,
                type: "error",
                timer: 5000,
                html: true
            });

        }
    });

    return famille;
}


$(document).ready(function () {


    $.fn.extend(
    {
            choose: function (name) {

                var detailName = this.val().split("_");
                var id = detailName[0];
                var idProfil = detailName[1];
                var agentName = detailName[2];
                var agentPhoto = detailName[3];
                var pathPhotoUser = $("#pathPhotoUser").val();

                var input = '<input type="hidden" name="' + name + '" id="' + id + '" value="' + this.val() + '" />';
                var inputMatricule = '<input type="hidden" name="Matricule" id="send' + id + '" value="' + this.val() + '" />';
                var checkboxMatricule = '<input type="checkbox"  name="MatriculeToSelect" value="' + id + '" id="Matricule' + id + '" />';
                var agentSelected = '<div class="chip" id="chipAgent' + id + '"> <img src="' + pathPhotoUser + agentPhoto + '" alt = "' + agentName + '" /> ' + agentName + ' </div >';
                

                if ($("#" + id).length)
                {
                    $("#" + id).remove();
                    $("#send" + id).remove();
                    $("#Matricule" + id).remove();
                    $("#chipAgent" + id).remove();
                }  

                if (idProfil != "0")
                {                 
                    var contenu = $("#insertValues").html();
                    contenu += input + inputMatricule;

                    var contenuIllistration = $("#AgentSelected").html();
                    contenuIllistration += agentSelected;


                    var td = checkboxMatricule + $("#td" + id).html();

                    $("#insertValues").html(contenu);
                    $("#AgentSelected").html(contenuIllistration);
                    $("#td" + id).html(td);

                    $("#Matricule" + id).attr('checked', true);
                }
                
                
            },

            checkData: function (name) {

                var detailValue = this.val().split("_");
                var id = detailValue[0];
                var valueName = detailValue[1];

                var input = '<input type="hidden" name="' + name + '" id="' + name + id + '" value="' + id + '" />';
                var valueChecked = '<div class="chip" id="Chip' + name + id + '"> ' + valueName + ' </div >';

                if ($("#" + name + id).length) {

                    $("#" + name + id).remove();
                    $("#Chip" + name + id).remove();
                }
                
                if (this.is(":checked")) {

                    var contenu = $("#" + name + "Selected").html();
                    contenu += input + valueChecked;
                    
                    $("#" + name + "Selected").html(contenu);
                }

            },

            promptUrl: function (title,url, message, legende, AcceptNullValue = true) { 

                swal({
                    title: title,
                    text: message,
                    html: true,
                    type: "input",                   
                    showCancelButton: true,
                    closeOnConfirm: false,
                    animation: "slide-from-top",
                    inputPlaceholder: legende
                }, function (inputValue) {
                    if (inputValue === false) return false;

                    if (!AcceptNullValue) {

                        if (inputValue == "") {
                            swal.showInputError("Vous devez �crire quelque chose");
                            return false
                        }

                    }

                    document.location = url + inputValue;

                });

            },

            confirmUrl: function (title, url, message, messageAnnule = "")
            {

                swal({
                    title: title,
                    text: message,
                    html: true,
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Oui, Continuer !",
                    cancelButtonText: "Non, Annuler svp !",
                    closeOnConfirm: false,
                    closeOnCancel: false
                }, function (isConfirm) {
                    if (isConfirm) {
                        document.location = url;
                    } else {

                        messageAnnule = messageAnnule == "" ? title + "annul&eacute;(e)" : messageAnnule;

                        swal({
                            title: "Annuler " + title,
                            text: messageAnnule,
                            timer: 5000,
                            html: true,
                            type: "error"
                        });                        

                    }
                });
                   

            },

            printAndCconfirmUrl: function (urlToRefresh,title, urlToPrint, message, messageAnnule = "") {                                             

                window.print();

                swal({
                    title: title,
                    text: message,
                    html: true,
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Oui, Continuer !",
                    cancelButtonText: "Non, Annuler svp !",
                    closeOnConfirm: false,
                    closeOnCancel: false
                }, function (isConfirm) {
                    if (isConfirm) {

                        $.ajax({
                            url: urlToPrint,
                            success: function (result) {

                                if (result.success) {

                                    window.opener.location.href = urlToRefresh;
                                    window.close();                                  

                                }
                            },
                            failure: function (result) {

                                swal({
                                    title: "Echec",
                                    text: result.message,
                                    type: "warning",
                                    timer: 5000,
                                    html: true
                                });

                            },
                            error: function (result) {

                                swal({
                                    title: "Erreur",
                                    text: result.message,
                                    type: "error",
                                    timer: 5000,
                                    html: true
                                });

                            }
                        });

                                                
                    } else {

                        messageAnnule = messageAnnule == "" ? title + "annul&eacute;(e)" : messageAnnule;

                        swal({
                            title: "Annuler " + title,
                            text: messageAnnule,
                            timer: 5000,
                            html: true,
                            type: "error"
                        });

                    }
                });

               
               
            },

            insertHTML: function (idSource,idDestination) {

                var source = $("#" + idSource).html();
                var destination = $("#" + idDestination).html();

                destination += source;

                $("#" + idDestination).html(destination);

            }

    });

    $(".masked").inputmask();
   


    if ($('#Alerte').val()) {

        var AlertColor = $('#AlertColor').val();
        var TypeAlert = $('#TypeAlert').val();
        var AlerteTitle = $('#AlerteTitle').val()? $('#AlerteTitle').val() : "";
        var Alerte = $('#Alerte').val();


        swal({
            title: AlerteTitle,
            text: Alerte,
            type: TypeAlert,
            timer: 5000,
            html: true
        });
    }      


    


    //CODE POUR LE DASHBOARD (graphiques : voir pages/dashboard.js)

    if ($('#AppName').val()) {
        setTimeout(function () { LocateToast('Bienvenue sur ' + $('#AppName').val(), 4000) }, 1500);
    }

    // Compteurs animés
    $('.counter').each(function () {
        $(this).prop('Counter', 0).animate({
            Counter: $(this).text()
        }, {
            duration: 2500,
            easing: 'swing',
            step: function (now) {
                $(this).text(Math.ceil(now));
                $(this).text($(this).text().replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1 "));
            }
        });
    });

    // Mini camemberts de progression (tableau des entités)
    if ($.fn.peity) {
        $.fn.peity.defaults.pie = {
            delimiter: null,
            fill: ["#3167f3", "rgba(132, 145, 183, 0.25)"],
            height: null,
            radius: 8,
            width: null
        };
        $("span.pie").peity("pie");
    }

});
