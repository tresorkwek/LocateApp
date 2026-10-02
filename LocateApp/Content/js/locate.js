/* ============================================================
   Locate - initialisations communes (remplace alpha.js / Materialize)
   ============================================================ */

// Notification légère (Bootstrap Toast)
window.LocateToast = function (message, delay, variant) {
    var container = document.querySelector('.locate-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'locate-toast-container';
        document.body.appendChild(container);
    }

    var el = document.createElement('div');
    el.className = 'toast align-items-center text-bg-' + (variant || 'primary') + ' border-0';
    el.setAttribute('role', 'alert');
    el.setAttribute('aria-live', 'assertive');
    el.innerHTML = '<div class="d-flex"><div class="toast-body">' + message + '</div>' +
        '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fermer"></button></div>';
    container.appendChild(el);

    if (window.bootstrap && bootstrap.Toast) {
        var toast = new bootstrap.Toast(el, { delay: delay || 4000 });
        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
        toast.show();
    }
};

$(function () {

    // ---------- DataTables (jQuery) en français ----------
    if ($.fn.DataTable) {
        var dtLanguage = {
            sEmptyTable: 'Aucune donnée disponible',
            sInfo: 'Affichage de _START_ à _END_ sur _TOTAL_ éléments',
            sInfoEmpty: 'Aucun élément à afficher',
            sInfoFiltered: '(filtré à partir de _MAX_ éléments)',
            sLengthMenu: 'Afficher _MENU_',
            sLoadingRecords: 'Chargement...',
            sProcessing: 'Traitement...',
            sSearch: '',
            searchPlaceholder: 'Rechercher',
            sZeroRecords: 'Aucun résultat trouvé',
            oPaginate: {
                sFirst: '<i class="iconoir-fast-arrow-left"></i>',
                sPrevious: '<i class="iconoir-nav-arrow-left"></i>',
                sNext: '<i class="iconoir-nav-arrow-right"></i>',
                sLast: '<i class="iconoir-fast-arrow-right"></i>'
            },
            oAria: { sSortAscending: ': tri croissant', sSortDescending: ': tri décroissant' }
        };

        $('#example, table.datatable').each(function () {
            if (!$.fn.DataTable.isDataTable(this)) {
                $(this).DataTable({ language: dtLanguage, pageLength: 10, autoWidth: false });
            }
        });
    }

    // ---------- Plugins conservés ----------
    if ($.fn.inputmask) { $('.masked').inputmask(); }
    if ($.fn.nestable) { $('.dd').nestable(); }
    if ($.fn.counterUp) { $('.counter').counterUp({ delay: 10, time: 800 }); }

    // ---------- Infobulles héritées (data-tooltip) vers Bootstrap ----------
    if (window.bootstrap && bootstrap.Tooltip) {
        $('[data-tooltip]').each(function () {
            this.setAttribute('title', this.getAttribute('data-tooltip'));
            this.setAttribute('data-bs-toggle', 'tooltip');
            new bootstrap.Tooltip(this);
        });
    }

    // ---------- Panneaux dépliants hérités (ul.collapsible) ----------
    $(document).on('click', 'ul.collapsible > li > .collapsible-header', function () {
        var header = $(this);
        var accordion = header.closest('ul.collapsible').attr('data-collapsible') !== 'expandable';
        if (accordion) {
            header.closest('ul.collapsible').find('> li > .collapsible-header').not(header).removeClass('active');
        }
        header.toggleClass('active');
    });

    // ---------- Menus déroulants hérités (data-activates) ----------
    $('[data-activates]').each(function () {
        var trigger = $(this);
        var menu = $('#' + trigger.attr('data-activates'));
        if (!menu.length) { return; }
        menu.addClass('dropdown-menu legacy-dropdown');
        trigger.on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var isOpen = menu.hasClass('show');
            $('.legacy-dropdown.show').removeClass('show');
            if (!isOpen) {
                var off = trigger.offset();
                menu.addClass('show').css({ position: 'absolute', top: off.top + trigger.outerHeight(), left: off.left, zIndex: 1050 }).appendTo('body');
            }
        });
    });
    $(document).on('click', function () { $('.legacy-dropdown.show').removeClass('show'); });

    // ---------- Bouton flottant : ouverture au clic (écrans tactiles) ----------
    $('.fab-action > .btn').each(function () {
        var fab = $(this).closest('.fab-action');
        if (!fab.find('ul').length) { return; }
        $(this).on('click', function (e) {
            if (!this.getAttribute('href')) { e.preventDefault(); }
            fab.toggleClass('open');
        });
    });
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.fab-action').length) { $('.fab-action.open').removeClass('open'); }
    });

    // ---------- Photos : aperçu agrandi au survol (img.avatar-zoom) ----------
    var apercu = null;
    function apercuPhoto() {
        if (!apercu) {
            apercu = $('<div class="locate-avatar-preview"><div class="locate-avatar-legende"></div></div>').appendTo('body');
        }
        return apercu;
    }
    $(document).on('mouseenter', 'img.avatar-zoom', function () {
        var img = $(this);
        var source = img.attr('data-zoom-src') || img.attr('src');
        if (!source) { return; }
        var boite = apercuPhoto();
        var rect = this.getBoundingClientRect();
        var largeur = 240, hauteur = 240, marge = 12;
        var gauche = rect.right + marge;
        if (gauche + largeur > window.innerWidth - 8) { gauche = rect.left - largeur - marge; }
        if (gauche < 8) { gauche = 8; }
        var haut = rect.top + rect.height / 2 - hauteur / 2;
        haut = Math.max(8, Math.min(haut, window.innerHeight - hauteur - 8));
        boite.css({ left: gauche + 'px', top: haut + 'px', backgroundImage: 'url("' + source + '")' });
        boite.find('.locate-avatar-legende').text(img.attr('data-zoom-legende') || img.attr('alt') || '');
        boite.addClass('show');
    }).on('mouseleave', 'img.avatar-zoom', function () {
        if (apercu) { apercu.removeClass('show'); }
    });
    $(window).on('scroll', function () { if (apercu) { apercu.removeClass('show'); } });

    // ---------- Options de carte ----------
    $('.card-refresh').on('click', function (e) {
        e.preventDefault();
        var card = $(this).closest('.card');
        if ($.fn.block) {
            card.block({ message: '', overlayCSS: { backgroundColor: '#000', opacity: 0.35, cursor: 'wait' } });
            window.setTimeout(function () { card.unblock(); }, 1000);
        }
    });

    $('.card-remove').on('click', function (e) {
        e.preventDefault();
        $(this).closest('.card').fadeOut(300);
    });

    // ---------- Champ fichier : affiche le nom choisi ----------
    $(document).on('change', 'input[type=file]', function () {
        var label = $(this).closest('.form-group').find('.file-name');
        if (label.length && this.files && this.files.length) {
            label.text(this.files.length === 1 ? this.files[0].name : this.files.length + ' fichiers');
        }
    });
});
