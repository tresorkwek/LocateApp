$(document).ready(function() {
    // L'indentation des listes hiérarchiques (organigramme) sert dans la liste ouverte, pas dans le champ fermé.
    $('select').not('.no-select2').select2({ templateSelection: function (choix) { return $.trim(choix.text || ''); } });
    
    $('#Specialites').select2({
        placeholder: 'Faites le choix de l\'hopital pour afficher ses services'
    });
       

    $("#CodeOrgane").on('change', function () {

        $.ajax({
            url: "/local/organe/" + $(this).val(),
            success: function (result) {

                if (result.success)
                {

                    var locaux = result.content;
                    var option = '<option value="" disabled selected>Choisissez le local</option>';

                    locaux.forEach(local => {

                        option += '<option value="' + local.id + '" > ' + local.designation + ' </option >';

                    });

                    $('li.select2-selection__choice').remove();
                    $('#select2-IdLocal-container').html('<font color="#9e9e9e">Choisissez le local</font>');
                    $('#IdLocal').html(option);

                }
                else
                {
                    $('#select2-IdLocal-container').html('');
                    $('#IdLocal').html('');
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


        $.ajax({
            url: "/agent/organe/" + $(this).val(),
            success: function (result) {

                if (result.success) {

                    var agents = result.content;
                    var option = '<option value="" selected>Personne</option>';

                    agents.forEach(agent => {

                        option += '<option value="' + agent.matricule.substring(0, 6) + '" > ' + agent.nom + ' ' + agent.postnom + ' ' + agent.prenom + ' (' + agent.matricule.substring(0, 6) + ') </option >';

                    });

                    $('li.select2-selection__choice').remove();
                    $('#select2-Responsable-container').html('<font color="#9e9e9e">Personne</font>');
                    $('#Responsable').html(option);

                }
                else {
                    $('#select2-Responsable-container').html('');
                    $('#Responsable').html('');
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

    });


    $("input[name='LastEtat']").change(function () {

        if (this.checked) {
            // the checkbox is now checked 

            $.ajax({
                url: "/observation/etat/" + this.value,
                success: function (result) {

                    if (result.success) {

                        var observations = result.content;
                        var option = '<option value="" disabled selected>Votre observation</option>';

                        observations.forEach(data => {

                            option += '<option value="' + data.id + '" > ' + data.observation + ' </option >';

                        });

                        $('li.select2-selection__choice').remove();
                        $('#select2-IdLastObservation-container').html('<font color="#9e9e9e">Votre observation</font>');
                        $('#IdLastObservation').html(option);

                    }
                    else {
                        $('#select2-IdLastObservation-container').html('');
                        $('#IdLastObservation').html('');
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
            // the checkbox is now no longer checked
        }

    });


    $(".js-example-basic-multiple-limit").select2({
    maximumSelectionLength: 2,
        placeholder: 'Limited Selection'
    });
    
    $(".js-example-tokenizer").select2({
        tags: true,
        tokenSeparators: [',', ' '],
        placeholder: 'With Tokenization'
    });
    
    var data = [{ id: 0, text: 'enhancement' }, { id: 1, text: 'bug' }, { id: 2, text: 'duplicate' }, { id: 3, text: 'invalid' }, { id: 4, text: 'wontfix' }];
 
    $(".js-example-data-array").select2({
        data: data
    });
    
});