/* ============================================================================
   Locate Terrain : application mobile des agents d'inventaire.
   Application 100 % navigateur : elle n'utilise que les routes déjà exposées
   par le serveur (réponses JSON quand l'en-tête X-Requested-With est présent).
   ============================================================================ */
(function () {
    'use strict';

    /* ------------------------------------------------------------------ utilitaires */
    var $ = function (s, r) { return (r || document).querySelector(s); };
    var $$ = function (s, r) { return Array.prototype.slice.call((r || document).querySelectorAll(s)); };
    function el(html) { var t = document.createElement('template'); t.innerHTML = html.trim(); return t.content.firstElementChild; }
    function esc(s) { return String(s == null ? '' : s).replace(/[&<>"']/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]; }); }
    /* Lit une propriété quelle que soit la casse renvoyée par le serveur (camelCase ou PascalCase). */
    function p(o, nom) {
        if (o == null) { return undefined; }
        if (o[nom] !== undefined) { return o[nom]; }
        var camel = nom.charAt(0).toLowerCase() + nom.slice(1);
        if (o[camel] !== undefined) { return o[camel]; }
        var pascal = nom.charAt(0).toUpperCase() + nom.slice(1);
        return o[pascal];
    }
    var GUID = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;
    function dateOk(s) { return !!s && String(s).indexOf('0001-01-01') !== 0 && String(s).indexOf('1900-01-01') !== 0; }
    function fmtDate(s, heure) {
        if (!dateOk(s)) { return null; }
        var d = new Date(s);
        if (isNaN(d.getTime())) { return null; }
        var o = { day: '2-digit', month: 'short', year: 'numeric' };
        if (heure) { o.hour = '2-digit'; o.minute = '2-digit'; }
        return d.toLocaleDateString('fr-FR', o);
    }
    function nombre(n) { return Number(n || 0).toLocaleString('fr-FR'); }
    function pourcent(a, b) { return b ? Math.round(a * 100 / b) : 0; }
    function minuscule(s) { return String(s == null ? '' : s).trim().toLowerCase(); }
    function heureCourte() { return new Date().toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' }); }
    function libelleEtat(b) {
        var e = p(b, 'lastEtat') || '';
        return p(b, 'lastEtatString') || (e === 'B' ? 'Bon état' : e === 'M' ? 'Mauvais état' : 'État inconnu');
    }

    /* Retour sensoriel : vibration et bip court (utile en extérieur ou avec des gants). */
    function vibrer(motif) { try { if (navigator.vibrate) { navigator.vibrate(motif); } } catch (e) { /* non pris en charge */ } }
    var audioCtx = null;
    function bip(type) {
        try {
            var AC = window.AudioContext || window.webkitAudioContext;
            if (!AC) { return; }
            audioCtx = audioCtx || new AC();
            if (audioCtx.state === 'suspended') { audioCtx.resume(); }
            var t0 = audioCtx.currentTime, duree = type === 'ok' ? 0.12 : 0.28;
            var o = audioCtx.createOscillator(), g = audioCtx.createGain();
            o.type = type === 'err' ? 'square' : 'sine';
            o.frequency.value = type === 'ok' ? 1320 : type === 'att' ? 760 : 260;
            g.gain.setValueAtTime(0.0001, t0);
            g.gain.exponentialRampToValueAtTime(type === 'err' ? 0.12 : 0.22, t0 + 0.01);
            g.gain.exponentialRampToValueAtTime(0.0001, t0 + duree);
            o.connect(g); g.connect(audioCtx.destination);
            o.start(t0); o.stop(t0 + duree + 0.02);
        } catch (e) { /* audio indisponible */ }
    }
    function signal(type) {
        if (type === 'ok') { vibrer(70); } else if (type === 'att') { vibrer([50, 70, 50]); } else { vibrer([220]); }
        bip(type);
    }

    /* ------------------------------------------------------------------ état */
    var etat = {
        user: null,          // identité connectée (/profile/)
        annee: null,         // année d'inventaire en cours
        arbre: null,         // organigramme de l'agent (/local/organe/list/{IdInstitution}/)
        plat: null,          // organigramme aplati [{organe, niveau}]
        cacheLocaux: {},     // designation des locaux par id
        obs: {},             // observations par état ('B', 'M'), chargées à la demande
        titre: 'Locate',
        jeton: null,         // jeton JWT reçu à la connexion (Authorization: Bearer), gardé en sessionStorage
        identifiants: null   // compte / mot de passe gardés en mémoire seulement, pour renouveler le jeton expiré
    };
    try { etat.jeton = window.sessionStorage.getItem('locate.jeton'); } catch (e) { etat.jeton = null; }

    function enregistrerJeton(token) {
        var acces = token && (token.access_token || token.accessToken || token.AccessToken);
        if (!acces) { return; }
        etat.jeton = acces;
        try { window.sessionStorage.setItem('locate.jeton', acces); } catch (e) { /* stockage indisponible */ }
    }
    function oublierJeton() {
        etat.jeton = null; etat.identifiants = null;
        try { window.sessionStorage.removeItem('locate.jeton'); } catch (e) { /* ignoré */ }
    }

    /* ------------------------------------------------------------------ accès au serveur */
    function normaliser(d) {
        if (d && typeof d === 'object' && !Array.isArray(d) && (('success' in d) || ('Success' in d))) {
            return { success: Number(p(d, 'success')), message: p(d, 'message') || '', content: p(d, 'content'), token: p(d, 'token') };
        }
        return { success: 1, message: '', content: d };
    }

    /* Renouvelle le jeton avec les identifiants en mémoire ; vrai si réussi. */
    async function reconnecter() {
        if (!etat.identifiants) { return false; }
        try {
            var r = await api('/auth/login', { form: { UserName: etat.identifiants.u, Password: etat.identifiants.p, RememberMe: etat.identifiants.r === 0 ? 0 : 1 }, sansReessai: true });
            if ((r.success === 1 || r.success === 4) && r.token) { enregistrerJeton(r.token); return true; }
        } catch (e) { /* refus : on retombe sur l'écran de connexion */ }
        return false;
    }

    function sessionPerdue() {
        oublierJeton();
        etat.user = null;
        afficherLogin();
        var err = new Error('Connectez-vous pour continuer.');
        err.session = true;
        throw err;
    }

    async function api(url, options) {
        options = options || {};
        var opts = {
            method: options.method || 'GET',
            credentials: 'same-origin',
            headers: { 'Accept': 'application/json', 'X-Requested-With': 'XMLHttpRequest' }
        };
        if (etat.jeton) { opts.headers['Authorization'] = 'Bearer ' + etat.jeton; }
        if (options.form) {
            opts.method = 'POST';
            opts.headers['Content-Type'] = 'application/x-www-form-urlencoded; charset=UTF-8';
            opts.body = new URLSearchParams(options.form).toString();
        }
        if (options.multipart) {
            // Envoi de fichier (multipart/form-data) : le navigateur fixe lui-même l'en-tête Content-Type et sa frontière.
            opts.method = 'POST';
            opts.body = options.multipart;
        }
        var resp;
        try { resp = await fetch(url, opts); }
        catch (e) {
            // fetch rejeté (TypeError) : pas de réseau ou serveur injoignable. Marqué pour la file hors ligne.
            var errReseau = new Error('Serveur injoignable. Vérifiez votre connexion.');
            errReseau.reseau = true;
            throw errReseau;
        }

        if (resp.status === 401) {
            if (!options.sansReessai && await reconnecter()) { return api(url, Object.assign({}, options, { sansReessai: true })); }
            sessionPerdue();
        }
        var texte = await resp.text();
        var data = null;
        try { data = texte ? JSON.parse(texte) : null; } catch (e) { data = null; }
        if (resp.status === 403) { throw new Error((data && (p(data, 'message'))) || "Vous n'avez pas le droit d'utiliser cette fonction."); }
        if (!resp.ok) { throw new Error((data && p(data, 'message')) || ('Erreur ' + resp.status)); }
        if (data === null) { throw new Error('Réponse inattendue du serveur (page HTML au lieu de données).'); }
        // Les codes 401/403/500 sont réécrits par le serveur en réponse 200 { Success:0, Message } (clés en PascalCase) :
        // on les distingue ainsi des réponses normales (clés en camelCase).
        if (typeof data === 'object' && !Array.isArray(data) && ('Success' in data) && !('success' in data) && Number(data.Success) === 0) {
            var msg = data.Message || '';
            if (/authentification/i.test(msg)) {
                if (!options.sansReessai && await reconnecter()) { return api(url, Object.assign({}, options, { sansReessai: true })); }
                sessionPerdue();
            }
            throw new Error(msg || 'Le serveur a refusé la demande.');
        }
        var r = normaliser(data);
        if (r.token) { enregistrerJeton(r.token); }
        return r;
    }
    function reussi(r) { return !!r && (r.success === 1 || r.success === 4); }

    /* Appel d'une action serveur (RedirectUrl côté serveur) : succès = 1, avertissement = 2, échec = 0. */
    async function action(url, form) {
        var r = await api(url, form ? { form: form } : {});
        if (reussi(r)) { toast(r.message || 'Opération effectuée.', 'success'); return true; }
        toast(r.message || "L'opération a échoué.", r.success === 2 ? 'warning' : 'danger');
        return false;
    }

    /* ------------------------------------------------------------------ mode hors ligne
       Les identifications et déclarations « non vu » faites sans réseau sont gardées dans localStorage
       (clé « locate.fileHorsLigne », entrées { chemin, champs, libelle, date }) puis envoyées dans l'ordre
       au retour du réseau (événement « online », ouverture de l'accueil ou bouton « Envoyer »). */
    var CLE_FILE = 'locate.fileHorsLigne', CLE_REFUS = 'locate.fileHorsLigne.refus';
    var MSG_HORS_LIGNE = 'Hors ligne : enregistré, sera envoyé au retour du réseau.';
    var fileMemoire = [], refusMemoire = [], synchro = { enCours: false };
    function lireListe(cle, memoire) {
        try {
            var t = window.localStorage.getItem(cle);
            if (t == null) { return memoire.slice(); }
            var l = JSON.parse(t);
            return Array.isArray(l) ? l : [];
        } catch (e) { return memoire.slice(); }
    }
    function ecrireListe(cle, liste) {
        try {
            if (liste.length) { window.localStorage.setItem(cle, JSON.stringify(liste)); } else { window.localStorage.removeItem(cle); }
        } catch (e) { /* stockage indisponible ou plein : la liste reste en mémoire pour cette session */ }
    }
    function lireFile() { return lireListe(CLE_FILE, fileMemoire); }
    function ecrireFile(l) { fileMemoire = l.slice(); ecrireListe(CLE_FILE, l); majCarteFile(); }
    function lireRefus() { return lireListe(CLE_REFUS, refusMemoire); }
    function ecrireRefus(l) { refusMemoire = l.slice(); ecrireListe(CLE_REFUS, l); majCarteFile(); }
    function ajouterFile(chemin, champs, libelle) {
        var l = lireFile();
        l.push({ chemin: chemin, champs: champs, libelle: libelle, date: new Date().toISOString() });
        ecrireFile(l);
    }
    function retirerFile(op) {
        var cle = JSON.stringify(op), l = lireFile();
        for (var i = 0; i < l.length; i++) { if (JSON.stringify(l[i]) === cle) { l.splice(i, 1); break; } }
        ecrireFile(l);
    }
    function horsLigne() { return navigator.onLine === false; }

    /* Envoie une opération d'inventaire ; sans réseau, la met en file. Résout avec { ok, attente, message }.
       silencieux : pas de notification (traitements en lot qui font leur propre bilan). */
    async function envoyerOuMettreEnAttente(chemin, champs, libelle, silencieux) {
        var attente = function () {
            ajouterFile(chemin, champs, libelle);
            if (!silencieux) { toast(MSG_HORS_LIGNE + (libelle ? ' (' + libelle + ')' : ''), 'warning'); }
            return { ok: true, attente: true, message: MSG_HORS_LIGNE };
        };
        if (horsLigne()) { return attente(); }
        try {
            var r = await api(chemin, { form: champs });
            var ok = reussi(r);
            if (!silencieux) { toast(r.message || (ok ? 'Opération effectuée.' : "L'opération a échoué."), ok ? 'success' : (r.success === 2 ? 'warning' : 'danger')); }
            return { ok: ok, attente: false, message: r.message || '' };
        } catch (e) {
            if (e.reseau) { return attente(); }
            if (!silencieux && !e.session) { toast(e.message, 'danger'); }
            return { ok: false, attente: false, message: e.message };
        }
    }

    /* Envoie la file dans l'ordre : chaque entrée envoyée (ou refusée par le serveur, dont on garde le message) est retirée ;
       arrêt à la première erreur réseau (ou session expirée). */
    async function synchroniserFile(manuel) {
        if (synchro.enCours || !etat.user) { return; }
        if (!lireFile().length) { if (manuel) { toast('Aucune opération en attente.', 'info'); } return; }
        if (horsLigne()) { if (manuel) { toast('Toujours hors ligne : les opérations restent en attente.', 'warning'); } return; }
        synchro.enCours = true;
        majCarteFile();
        var envoyees = 0, refusees = 0, coupure = false;
        try {
            // Nombre de tours borné : une entrée ajoutée pendant l'envoi partira au prochain passage.
            for (var tour = lireFile().length; tour > 0; tour--) {
                var l = lireFile();
                if (!l.length) { break; }
                var op = l[0], message = null;
                try {
                    var r = await api(op.chemin, { form: op.champs || {} });
                    if (reussi(r)) { envoyees++; } else { message = r.message || 'refusée par le serveur'; }
                } catch (e) {
                    if (e.reseau || e.session) { coupure = true; break; }
                    message = e.message;
                }
                if (message !== null) {
                    refusees++;
                    var refus = lireRefus();
                    refus.unshift({ libelle: op.libelle, message: message, date: op.date });
                    ecrireRefus(refus.slice(0, 20));
                }
                retirerFile(op);
            }
        } finally {
            synchro.enCours = false;
            majCarteFile();
        }
        var restantes = lireFile().length;
        if (envoyees || refusees) {
            toast(nombre(envoyees) + ' opération(s) hors ligne envoyée(s)' + (refusees ? ', ' + nombre(refusees) + ' refusée(s) par le serveur' : '') +
                (restantes ? ', ' + nombre(restantes) + ' encore en attente' : '') + '.', refusees ? 'warning' : 'success');
        } else if (manuel && coupure) {
            toast('Serveur injoignable : les opérations restent en attente.', 'warning');
        }
    }

    /* Carte « N opération(s) en attente d'envoi » de l'accueil (mise à jour à chaque changement de la file). */
    function majCarteFile() {
        var zone = document.getElementById('carteFile');
        if (!zone) { return; }
        var file = lireFile(), refus = lireRefus();
        if (!file.length && !refus.length) { zone.innerHTML = ''; return; }
        var html = '';
        if (file.length) {
            var plusAncienne = fmtDate(file[0].date, true);
            html += '<div class="m-card m-carte-file"><div class="m-entete-carte"><span class="ico-gros ico-orange"><i class="iconoir-cloud-upload"></i></span>' +
                '<div class="min-w-0"><h6>' + nombre(file.length) + ' opération(s) en attente d\'envoi</h6>' +
                '<div class="m-sous">Enregistrée(s) sans réseau' + (plusAncienne ? ' depuis le ' + esc(plusAncienne) : '') + '. Elles partent automatiquement au retour du réseau.</div></div></div>' +
                '<button type="button" class="m-btn m-btn-primaire m-btn-bloc mt-3" id="btnEnvoyerFile"' + (synchro.enCours ? ' disabled' : '') + '>' +
                '<i class="' + (synchro.enCours ? 'iconoir-refresh' : 'iconoir-send') + '"></i>' + (synchro.enCours ? 'Envoi en cours…' : 'Envoyer') + '</button></div>';
        }
        if (refus.length) {
            html += '<div class="m-card m-carte-refus"><div class="d-flex align-items-center gap-2"><i class="iconoir-warning-triangle text-danger"></i>' +
                '<b class="flex-grow-1">' + nombre(refus.length) + ' opération(s) refusée(s) par le serveur</b>' +
                '<button type="button" class="m-lien border-0 bg-transparent" id="btnEffacerRefus">Effacer</button></div><ul>' +
                refus.slice(0, 5).map(function (x) { return '<li><b>' + esc(x.libelle || 'Opération') + '</b> : ' + esc(x.message) + '</li>'; }).join('') + '</ul></div>';
        }
        zone.innerHTML = html;
        var b = document.getElementById('btnEnvoyerFile');
        if (b) { b.onclick = function () { synchroniserFile(true); }; }
        var eff = document.getElementById('btnEffacerRefus');
        if (eff) { eff.onclick = function () { ecrireRefus([]); }; }
    }
    window.addEventListener('online', function () {
        if (!etat.user) { return; }
        if (lireFile().length) { toast('Réseau rétabli : envoi des opérations en attente.', 'info'); }
        synchroniserFile(false);
    });
    window.addEventListener('offline', function () {
        if (etat.user) { toast('Vous êtes hors ligne : les identifications et déclarations « non vu » seront gardées sur l\'appareil.', 'warning'); }
    });

    /* ------------------------------------------------------------------ notifications, confirmation, feuille, barre d'actions */
    var ICONES_TOAST = { success: 'iconoir-check-circle', warning: 'iconoir-warning-triangle', danger: 'iconoir-xmark-circle', info: 'iconoir-info-circle', primary: 'iconoir-info-circle' };
    function toast(message, type) {
        type = type || 'primary';
        var zone = $('#toasts');
        var t = el('<div class="m-toast ' + type + '" role="alert"><i class="' + (ICONES_TOAST[type] || ICONES_TOAST.info) + '"></i><div class="msg">' + esc(message) + '</div><button type="button" aria-label="Fermer"><i class="iconoir-xmark"></i></button></div>');
        var fermer = function () { if (!t.parentNode) { return; } t.classList.add('sortie'); setTimeout(function () { t.remove(); }, 220); };
        $('button', t).onclick = fermer;
        zone.appendChild(t);
        while (zone.children.length > 3) { zone.firstElementChild.remove(); }
        setTimeout(fermer, type === 'danger' ? 6000 : 3500);
    }

    /* Attend la fin de la fermeture d'une boîte de confirmation précédente (deux confirmations à la suite). */
    function modalLibre() {
        var m = $('#confirmModal');
        if (m.style.display !== 'block') { return Promise.resolve(); }
        return new Promise(function (res) {
            var fait = false;
            var ok = function () { if (fait) { return; } fait = true; m.removeEventListener('hidden.bs.modal', ok); res(); };
            m.addEventListener('hidden.bs.modal', ok);
            setTimeout(ok, 800);
        });
    }
    /* classeOui : style du bouton de validation ('m-btn-rouge' pour une opération définitive). */
    async function confirmer(titre, texte, oui, non, classeOui) {
        await modalLibre();
        return new Promise(function (resolve) {
            $('#confirmTitre').textContent = titre;
            $('#confirmTexte').innerHTML = texte;
            $('#confirmOui').className = 'm-btn ' + (classeOui || 'm-btn-primaire');
            $('#confirmOui').textContent = oui || 'Oui';
            $('#confirmNon').textContent = non || 'Non';
            var m = bootstrap.Modal.getOrCreateInstance($('#confirmModal'), { backdrop: 'static' });
            var fini = false;
            function repondre(v) { if (fini) { return; } fini = true; m.hide(); resolve(v); }
            $('#confirmOui').onclick = function () { repondre(true); };
            $('#confirmNon').onclick = function () { repondre(false); };
            $('#confirmModal').addEventListener('hidden.bs.modal', function h() { $('#confirmModal').removeEventListener('hidden.bs.modal', h); repondre(false); });
            m.show();
        });
    }

    var sheetInstance = null;
    function ouvrirSheet(titre, html) {
        $('#sheetTitre').textContent = titre;
        $('#sheetCorps').innerHTML = html;
        sheetInstance = bootstrap.Offcanvas.getOrCreateInstance($('#sheet'));
        sheetInstance.show();
        return $('#sheetCorps');
    }
    function fermerSheet() { if (sheetInstance) { sheetInstance.hide(); } }

    /* Résout quand la feuille du bas est entièrement fermée (une feuille ouverte juste après une autre
       ne doit ni être refermée par la fin de l'animation précédente, ni recevoir son événement « hidden »). */
    function sheetFermee() {
        var s = $('#sheet');
        var ouverte = s.classList.contains('show') || s.classList.contains('showing') || s.classList.contains('hiding');
        if (!ouverte) { return Promise.resolve(); }
        return new Promise(function (res) {
            var fait = false;
            var ok = function () { if (fait) { return; } fait = true; s.removeEventListener('hidden.bs.offcanvas', ok); res(); };
            s.addEventListener('hidden.bs.offcanvas', ok);
            setTimeout(ok, 900);
            if (!s.classList.contains('hiding')) { fermerSheet(); }
        });
    }
    async function ouvrirFeuille(titre, html) { await sheetFermee(); return ouvrirSheet(titre, html); }
    /* Appelle surFermeture une seule fois quand la feuille se ferme (par l'utilisateur ou par le code). */
    function surFermetureSheet(surFermeture) {
        var s = $('#sheet');
        s.addEventListener('hidden.bs.offcanvas', function h() { s.removeEventListener('hidden.bs.offcanvas', h); surFermeture(); });
    }

    /* Feuille de choix : items [{id, icone, libelle, sous, classe}] ou {section: 'Titre'} ; résout avec l'id choisi ou null. */
    async function choisir(titre, items, intro) {
        var html = intro ? '<div class="m-feuille-intro">' + intro + '</div>' : '';
        var ouvert = false;
        items.forEach(function (it) {
            if (it.section) {
                html += (ouvert ? '</div>' : '') + '<div class="m-feuille-section">' + esc(it.section) + '</div><div class="m-choix">';
                ouvert = true;
                return;
            }
            if (!ouvert) { html += '<div class="m-choix">'; ouvert = true; }
            html += '<button type="button" data-choix="' + esc(it.id) + '"' + (it.classe ? ' class="' + it.classe + '"' : '') + '><i class="' + it.icone + '"></i>' +
                '<span>' + esc(it.libelle) + (it.sous ? '<br><small class="m-sous">' + esc(it.sous) + '</small>' : '') + '</span></button>';
        });
        html += ouvert ? '</div>' : '';
        var corps = await ouvrirFeuille(titre, html);
        return new Promise(function (resolve) {
            var fini = false;
            var fin = function (v) { if (fini) { return; } fini = true; resolve(v); };
            $$('button[data-choix]', corps).forEach(function (b) { b.onclick = function () { fin(b.dataset.choix); fermerSheet(); }; });
            surFermetureSheet(function () { fin(null); });
        });
    }

    /* Saisie d'un texte dans la feuille du bas ; résout avec le texte (éventuellement vide) ou null si l'utilisateur annule.
       o : {titre, texte (HTML), libelle, placeholder, bouton, icone, classe, multiligne, maxlength, requis} */
    async function demanderTexte(o) {
        var champ = o.multiligne
            ? '<textarea class="form-control" id="txtSaisie" rows="3" maxlength="' + (o.maxlength || 200) + '" placeholder="' + esc(o.placeholder || '') + '"></textarea>'
            : '<input type="text" class="form-control" id="txtSaisie" maxlength="' + (o.maxlength || 200) + '" placeholder="' + esc(o.placeholder || '') + '" autocomplete="off" autocapitalize="' + (o.majuscules ? 'characters' : 'sentences') + '" spellcheck="false">';
        var corps = await ouvrirFeuille(o.titre,
            (o.texte ? '<div class="m-feuille-intro">' + o.texte + '</div>' : '') +
            '<form id="frmTexte" autocomplete="off" novalidate>' + (o.libelle ? '<label class="m-libelle" for="txtSaisie">' + esc(o.libelle) + '</label>' : '') + champ +
            '<button type="submit" class="m-btn ' + (o.classe || 'm-btn-primaire') + ' m-btn-bloc mt-3"><i class="' + (o.icone || 'iconoir-check') + '"></i>' + esc(o.bouton || 'Valider') + '</button>' +
            '<button type="button" class="m-btn m-btn-clair m-btn-bloc mt-2" id="txtAnnuler">Annuler</button></form>');
        return new Promise(function (resolve) {
            var fini = false;
            var fin = function (v) { if (fini) { return; } fini = true; resolve(v); };
            $('#frmTexte', corps).onsubmit = function (e) {
                e.preventDefault();
                var v = $('#txtSaisie', corps).value;
                if (o.requis && !v.trim()) { toast(o.requis === true ? 'Ce champ est obligatoire.' : o.requis, 'warning'); return; }
                fin(v);
                fermerSheet();
            };
            $('#txtAnnuler', corps).onclick = function () { fin(null); fermerSheet(); };
            surFermetureSheet(function () { fin(null); });
        });
    }

    /* Barre d'actions au-dessus de la navigation : [{icone, libelle, court, action, principal}] */
    function setActions(items) {
        var zone = $('#barreActions');
        zone.innerHTML = '';
        var ok = !!(items && items.length);
        zone.hidden = !ok;
        document.body.classList.toggle('a-actions', ok);
        if (!ok) { return; }
        var ligne = el('<div></div>');
        items.forEach(function (it) {
            var b = it.principal
                ? el('<button type="button" class="m-btn m-btn-primaire"><i class="' + it.icone + '"></i><span>' + esc(it.libelle) + '</span></button>')
                : el('<button type="button" class="m-btn m-btn-secondaire" aria-label="' + esc(it.libelle) + '"><i class="' + it.icone + '"></i><span>' + esc(it.court || it.libelle) + '</span></button>');
            b.onclick = it.action;
            ligne.appendChild(b);
        });
        zone.appendChild(ligne);
    }

    /* ------------------------------------------------------------------ fragments d'interface */
    function vide(icone, titre, texte, variante, bouton) {
        return '<div class="m-vide ' + (variante || '') + '"><div class="cercle"><i class="' + icone + '"></i></div>' +
            '<div class="titre">' + esc(titre) + '</div>' + (texte ? '<p>' + esc(texte) + '</p>' : '') +
            (bouton ? '<button type="button" class="m-btn m-btn-primaire" id="' + bouton.id + '"><i class="' + (bouton.icone || 'iconoir-refresh') + '"></i>' + esc(bouton.libelle) + '</button>' : '') + '</div>';
    }
    function squelette(n, avecHero) {
        var s = avecHero ? '<div class="m-sq m-sq-hero"></div>' : '';
        s += '<div class="m-squelette">';
        for (var i = 0; i < (n || 6); i++) {
            s += '<div class="m-sq-carte"><div class="m-sq m-sq-rond"></div><div class="m-sq-lignes"><div class="m-sq m-sq-l1"></div><div class="m-sq m-sq-l2"></div><div class="m-sq m-sq-l3"></div></div></div>';
        }
        return s + '</div>';
    }
    function recherche(id, placeholder) {
        return '<div class="m-recherche"><i class="iconoir-search"></i><input type="search" id="' + id + '" placeholder="' + esc(placeholder) + '" autocomplete="off" enterkeyhint="search"></div>';
    }
    function couleurTheme(c) { var m = $('meta[name=theme-color]'); if (m) { m.setAttribute('content', c); } }

    /* ------------------------------------------------------------------ dessin d'un QR code (SVG), sans bibliothèque externe
       Encodeur minimal : mode octet, correction d'erreur M, versions 1 à 6 (jusqu'à 106 octets ; un GUID tient en version 3).
       Algorithme de la norme ISO/IEC 18004 (Reed-Solomon GF(256), placement en zigzag, choix du masque par pénalités). */
    /* QR-DEBUT */
    function qrMatrice(texte) {
        var octets = Array.prototype.slice.call(new TextEncoder().encode(texte));
        var ECC = [-1, 10, 16, 26, 18, 24, 16], BLOCS = [-1, 1, 1, 1, 2, 2, 4];
        var mul = function (x, y) {
            var z = 0;
            for (var i = 7; i >= 0; i--) { z = (z << 1) ^ ((z >>> 7) * 0x11D); z ^= ((y >>> i) & 1) * x; }
            return z & 0xFF;
        };
        var modulesBruts = function (v) {
            var n = (16 * v + 128) * v + 64;
            if (v >= 2) { var na = Math.floor(v / 7) + 2; n -= (25 * na - 10) * na - 55; }
            return n;
        };
        var version = 0, capacite = 0;
        for (var v = 1; v <= 6; v++) {
            capacite = Math.floor(modulesBruts(v) / 8) - ECC[v] * BLOCS[v];
            if (4 + 8 + octets.length * 8 <= capacite * 8) { version = v; break; }
        }
        if (!version) { throw new Error('Texte trop long pour le QR code.'); }
        // Flux de bits : mode octet (0100), longueur sur 8 bits, données, terminaison, bourrage.
        var bits = [];
        var ajouter = function (val, n) { for (var i = n - 1; i >= 0; i--) { bits.push((val >>> i) & 1); } };
        ajouter(4, 4); ajouter(octets.length, 8);
        octets.forEach(function (o) { ajouter(o, 8); });
        ajouter(0, Math.min(4, capacite * 8 - bits.length));
        ajouter(0, (8 - bits.length % 8) % 8);
        for (var pad = 0xEC; bits.length < capacite * 8; pad ^= 0xEC ^ 0x11) { ajouter(pad, 8); }
        var donnees = [];
        for (var bi = 0; bi < bits.length; bi += 8) { var o8 = 0; for (var k = 0; k < 8; k++) { o8 = (o8 << 1) | bits[bi + k]; } donnees.push(o8); }
        // Correction d'erreur Reed-Solomon par bloc, puis entrelacement.
        var nbBlocs = BLOCS[version], eccLen = ECC[version], brut = Math.floor(modulesBruts(version) / 8);
        var diviseur = [];
        for (var d = 0; d < eccLen - 1; d++) { diviseur.push(0); }
        diviseur.push(1);
        for (var racine = 1, r0 = 0; r0 < eccLen; r0++) {
            for (var j = 0; j < diviseur.length; j++) { diviseur[j] = mul(diviseur[j], racine); if (j + 1 < diviseur.length) { diviseur[j] ^= diviseur[j + 1]; } }
            racine = mul(racine, 2);
        }
        var reste = function (data) {
            var res = diviseur.map(function () { return 0; });
            data.forEach(function (b) {
                var f = b ^ res.shift();
                res.push(0);
                diviseur.forEach(function (c, i) { res[i] ^= mul(c, f); });
            });
            return res;
        };
        var nbCourts = nbBlocs - brut % nbBlocs, lgCourt = Math.floor(brut / nbBlocs), blocs = [];
        for (var b = 0, pos = 0; b < nbBlocs; b++) {
            var dat = donnees.slice(pos, pos + lgCourt - eccLen + (b < nbCourts ? 0 : 1));
            pos += dat.length;
            var ecc = reste(dat);
            if (b < nbCourts) { dat.push(0); }
            blocs.push(dat.concat(ecc));
        }
        var flux = [];
        for (var i2 = 0; i2 < blocs[0].length; i2++) {
            for (var j2 = 0; j2 < blocs.length; j2++) { if (i2 !== lgCourt - eccLen || j2 >= nbCourts) { flux.push(blocs[j2][i2]); } }
        }
        // Motifs fixes : repères, synchronisation, alignement, informations de format.
        var taille = version * 4 + 17, m = [], fixe = [];
        for (var y = 0; y < taille; y++) { m.push(new Array(taille).fill(false)); fixe.push(new Array(taille).fill(false)); }
        var poser = function (x, y, noir) { m[y][x] = noir; fixe[y][x] = true; };
        for (var t = 0; t < taille; t++) { poser(6, t, t % 2 === 0); poser(t, 6, t % 2 === 0); }
        [[3, 3], [taille - 4, 3], [3, taille - 4]].forEach(function (c) {
            for (var dy = -4; dy <= 4; dy++) {
                for (var dx = -4; dx <= 4; dx++) {
                    var dist = Math.max(Math.abs(dx), Math.abs(dy)), xx = c[0] + dx, yy = c[1] + dy;
                    if (xx >= 0 && xx < taille && yy >= 0 && yy < taille) { poser(xx, yy, dist !== 2 && dist !== 4); }
                }
            }
        });
        if (version >= 2) {
            var na = Math.floor(version / 7) + 2, pas = Math.ceil((version * 4 + 4) / (na * 2 - 2)) * 2, posA = [6];
            for (var pa = taille - 7; posA.length < na; pa -= pas) { posA.splice(1, 0, pa); }
            for (var ia = 0; ia < na; ia++) {
                for (var ja = 0; ja < na; ja++) {
                    if ((ia === 0 && ja === 0) || (ia === 0 && ja === na - 1) || (ia === na - 1 && ja === 0)) { continue; }
                    for (var ay = -2; ay <= 2; ay++) { for (var ax = -2; ax <= 2; ax++) { poser(posA[ia] + ax, posA[ja] + ay, Math.max(Math.abs(ax), Math.abs(ay)) !== 1); } }
                }
            }
        }
        var format = function (masque) {
            var dataF = (0 << 3) | masque, rem = dataF; // niveau M : bits de format 00
            for (var i = 0; i < 10; i++) { rem = (rem << 1) ^ ((rem >>> 9) * 0x537); }
            var fb = ((dataF << 10) | rem) ^ 0x5412;
            var bit = function (i) { return ((fb >>> i) & 1) !== 0; };
            for (var i1 = 0; i1 <= 5; i1++) { poser(8, i1, bit(i1)); }
            poser(8, 7, bit(6)); poser(8, 8, bit(7)); poser(7, 8, bit(8));
            for (var i3 = 9; i3 < 15; i3++) { poser(14 - i3, 8, bit(i3)); }
            for (var i4 = 0; i4 < 8; i4++) { poser(taille - 1 - i4, 8, bit(i4)); }
            for (var i5 = 8; i5 < 15; i5++) { poser(8, taille - 15 + i5, bit(i5)); }
            poser(8, taille - 8, true);
        };
        format(0);
        // Placement des données en zigzag (colonnes de deux modules, de droite à gauche).
        var n = 0;
        for (var droite = taille - 1; droite >= 1; droite -= 2) {
            if (droite === 6) { droite = 5; }
            for (var vert = 0; vert < taille; vert++) {
                for (var jj = 0; jj < 2; jj++) {
                    var x = droite - jj, monte = ((droite + 1) & 2) === 0, yv = monte ? taille - 1 - vert : vert;
                    if (!fixe[yv][x] && n < flux.length * 8) { m[yv][x] = ((flux[n >>> 3] >>> (7 - (n & 7))) & 1) !== 0; n++; }
                }
            }
        }
        var inverser = function (masque, x, y) {
            switch (masque) {
                case 0: return (x + y) % 2 === 0;
                case 1: return y % 2 === 0;
                case 2: return x % 3 === 0;
                case 3: return (x + y) % 3 === 0;
                case 4: return (Math.floor(x / 3) + Math.floor(y / 2)) % 2 === 0;
                case 5: return x * y % 2 + x * y % 3 === 0;
                case 6: return (x * y % 2 + x * y % 3) % 2 === 0;
                default: return ((x + y) % 2 + x * y % 3) % 2 === 0;
            }
        };
        var appliquer = function (masque) {
            for (var y = 0; y < taille; y++) { for (var x = 0; x < taille; x++) { if (!fixe[y][x] && inverser(masque, x, y)) { m[y][x] = !m[y][x]; } } }
        };
        // Pénalités simplifiées (suites de 5 modules ou plus, blocs 2x2, équilibre noir/blanc) pour choisir le masque.
        var penalite = function () {
            var p = 0, noirs = 0;
            for (var a = 0; a < taille; a++) {
                for (var sens = 0; sens < 2; sens++) {
                    var suite = 0, prec = null;
                    for (var c = 0; c < taille; c++) {
                        var val = sens ? m[c][a] : m[a][c];
                        if (val === prec) { suite++; if (suite === 5) { p += 3; } else if (suite > 5) { p++; } } else { suite = 1; prec = val; }
                    }
                }
            }
            for (var y = 0; y < taille; y++) {
                for (var x = 0; x < taille; x++) {
                    if (m[y][x]) { noirs++; }
                    if (x < taille - 1 && y < taille - 1 && m[y][x] === m[y][x + 1] && m[y][x] === m[y + 1][x] && m[y][x] === m[y + 1][x + 1]) { p += 3; }
                }
            }
            return p + Math.floor(Math.abs(noirs * 20 - taille * taille * 10) / (taille * taille)) * 10;
        };
        var meilleur = 0, min = Infinity;
        for (var mq = 0; mq < 8; mq++) {
            appliquer(mq); format(mq);
            var pen = penalite();
            if (pen < min) { min = pen; meilleur = mq; }
            appliquer(mq); // annule le masque (opération involutive)
        }
        appliquer(meilleur); format(meilleur);
        return m;
    }
    function qrSvg(texte) {
        var m = qrMatrice(texte), marge = 4, cote = m.length + marge * 2, chemin = '';
        for (var y = 0; y < m.length; y++) { for (var x = 0; x < m.length; x++) { if (m[y][x]) { chemin += 'M' + (x + marge) + ' ' + (y + marge) + 'h1v1h-1z'; } } }
        return '<svg viewBox="0 0 ' + cote + ' ' + cote + '" xmlns="http://www.w3.org/2000/svg" shape-rendering="crispEdges" role="img" aria-label="QR code ' + esc(texte) + '">' +
            '<rect width="' + cote + '" height="' + cote + '" fill="#fff"/><path d="' + chemin + '" fill="#000"/></svg>';
    }
    /* QR-FIN */

    /* ------------------------------------------------------------------ caméra et lecture de QR codes */
    var cam = { flux: null, timer: null, actif: false, jeton: null, video: null, piste: null, lampe: false };

    function extraireCode(texte) {
        if (!texte) { return null; }
        var m = String(texte).match(GUID);
        return m ? m[0].toLowerCase() : String(texte).trim();
    }
    function detecteurQr() {
        if (!('BarcodeDetector' in window)) { return null; }
        try { return new window.BarcodeDetector({ formats: ['qr_code'] }); } catch (e) { return null; }
    }
    function arreterCamera() {
        cam.actif = false; cam.jeton = null;
        if (cam.timer) { clearTimeout(cam.timer); cam.timer = null; }
        if (cam.flux) { cam.flux.getTracks().forEach(function (t) { t.stop(); }); cam.flux = null; }
        if (cam.video) { try { cam.video.srcObject = null; } catch (e) { /* ignoré */ } cam.video = null; }
        cam.piste = null; cam.lampe = false;
    }
    /* Démarre la caméra arrière dans `video` ; surCode(code) est appelé pour chaque QR lu et renvoie true pour arrêter.
       Résout avec 'ok', 'indisponible', 'refus', 'sans-decodeur' ou 'annule'. */
    async function demarrerCamera(video, surCode) {
        arreterCamera();
        var jeton = {};
        cam.actif = true; cam.jeton = jeton;
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) { return 'indisponible'; }
        var flux;
        try {
            flux = await navigator.mediaDevices.getUserMedia({ video: { facingMode: { ideal: 'environment' }, width: { ideal: 1280 }, height: { ideal: 720 } }, audio: false });
        } catch (e) { return cam.jeton === jeton ? 'refus' : 'annule'; }
        if (cam.jeton !== jeton) { flux.getTracks().forEach(function (t) { t.stop(); }); return 'annule'; }
        cam.flux = flux; cam.video = video; cam.piste = flux.getVideoTracks()[0] || null;
        video.srcObject = flux;
        try { await video.play(); } catch (e) { /* lecture bloquée : la saisie manuelle reste possible */ }
        var detecteur = detecteurQr();
        if (!detecteur) { return 'sans-decodeur'; }
        var boucle = async function () {
            if (cam.jeton !== jeton) { return; }
            var pause = 180;
            try {
                if (video.readyState >= 2) {
                    var codes = await detecteur.detect(video);
                    for (var i = 0; codes && i < codes.length; i++) {
                        if (cam.jeton !== jeton) { return; }
                        var code = extraireCode(codes[i].rawValue);
                        if (code && surCode(code) === true) { return; }
                        pause = 300;
                    }
                }
            } catch (e) { /* image pas encore prête */ }
            if (cam.jeton === jeton) { cam.timer = setTimeout(boucle, pause); }
        };
        boucle();
        return 'ok';
    }
    function lampeDisponible() {
        try { var c = cam.piste && cam.piste.getCapabilities ? cam.piste.getCapabilities() : null; return !!(c && c.torch); } catch (e) { return false; }
    }
    async function basculerLampe(bouton) {
        if (!cam.piste) { return; }
        try { cam.lampe = !cam.lampe; await cam.piste.applyConstraints({ advanced: [{ torch: cam.lampe }] }); }
        catch (e) { cam.lampe = false; }
        if (bouton) { bouton.classList.toggle('allume', cam.lampe); }
    }
    function messageCamera(statut) {
        if (statut === 'indisponible') { return 'Caméra indisponible (le site doit être en HTTPS). Saisissez ou lisez le code avec une douchette.'; }
        if (statut === 'refus') { return 'Accès à la caméra refusé. Saisissez ou lisez le code avec une douchette.'; }
        if (statut === 'sans-decodeur') { return 'Ce navigateur ne décode pas les QR codes : saisissez le code ci-dessous.'; }
        return null;
    }

    /* Scanner plein écran à lecture unique ; résout avec le code lu (GUID en minuscules) ou null si l'utilisateur ferme. */
    var scan = { resoudre: null };
    function fermerScanner(valeur) {
        arreterCamera();
        $('#scanner').hidden = true;
        $('#scanLampe').hidden = true; $('#scanLampe').classList.remove('allume');
        var msg = $('#scanner .m-camera-msg'); if (msg) { msg.remove(); }
        $('#scanner .m-scanner-video').classList.remove('sans-camera');
        var r = scan.resoudre; scan.resoudre = null;
        if (r) { r(valeur || null); }
    }
    function scanner(titre, aide) {
        return new Promise(async function (resolve) {
            if (scan.resoudre) { fermerScanner(null); }
            scan.resoudre = resolve;
            $('#scanTitre').textContent = titre || 'Scanner un QR code';
            $('#scanAide').textContent = aide || 'Placez le QR code dans le cadre.';
            $('#scanSaisie').value = '';
            $('#scanner').hidden = false;
            var statut = await demarrerCamera($('#scanVideo'), function (code) { signal('ok'); fermerScanner(code); return true; });
            if (scan.resoudre !== resolve) { return; }
            if (statut === 'ok') { $('#scanLampe').hidden = !lampeDisponible(); return; }
            var texte = messageCamera(statut);
            if (texte) {
                $('#scanner .m-scanner-video').classList.add('sans-camera');
                $('#scanner .m-scanner-video').appendChild(el('<div class="m-camera-msg"><i class="iconoir-camera"></i>' + esc(texte) + '</div>'));
                $('#scanSaisie').focus();
            }
        });
    }
    $('#scanFermer').onclick = function () { fermerScanner(null); };
    $('#scanLampe').onclick = function () { basculerLampe(this); };
    $('#scanFormManuel').onsubmit = function (e) {
        e.preventDefault();
        var code = extraireCode($('#scanSaisie').value);
        if (code) { fermerScanner(code); }
    };

    /* ------------------------------------------------------------------ données métier */
    async function chargerProfil() {
        var r = await api('/profile/');
        // Session valide mais identité introuvable (content null) : on continue avec une identité vide plutôt que de redemander le profil à chaque page.
        etat.user = r.content || {};
        try {
            var inv = await api('/inventaire/encours/');
            etat.inventaireOuvert = Number(p(inv.content, 'annee')) > 0;
            etat.annee = Number(p(inv.content, 'annee')) || new Date().getFullYear();
        } catch (e) { etat.inventaireOuvert = false; etat.annee = new Date().getFullYear(); }
        return etat.user;
    }

    async function chargerArbre(force) {
        if (etat.arbre && !force) { return etat.arbre; }
        var inst = etat.user ? p(etat.user, 'idInstitution') : null;
        var r = await api(inst ? '/local/organe/list/' + encodeURIComponent(inst) + '/' : '/local/organe/list/');
        etat.arbre = Array.isArray(r.content) ? r.content : [];
        etat.plat = [];
        (function aplatir(liste, niveau) {
            (liste || []).forEach(function (o) {
                etat.plat.push({ organe: o, niveau: niveau });
                aplatir(p(o, 'organes'), niveau + 1);
            });
        })(etat.arbre, 0);
        return etat.arbre;
    }
    /* Organe d'inventaire affecté à l'utilisateur (idInstitution) : racine de son périmètre ; sans affectation, toute l'institution. */
    function nomOrganeInventaire() {
        var id = etat.user ? p(etat.user, 'idInstitution') : null;
        if (!id) { return 'toute l\'institution'; }
        var o = organeParId(id);
        return o ? String(p(o, 'nom') || id).trim() : id;
    }
    function organeParId(id) {
        var t = (etat.plat || []).filter(function (x) { return String(p(x.organe, 'id')) === String(id); })[0];
        return t ? t.organe : null;
    }
    function totaux() {
        var s = { total: 0, identifies: 0, inventories: 0 };
        (etat.plat || []).forEach(function (x) {
            s.total += Number(p(x.organe, 'nbreBien') || 0);
            s.identifies += Number(p(x.organe, 'nbreBienIdentifie') || 0);
            s.inventories += Number(p(x.organe, 'nbreBienInventorie') || 0);
        });
        return s;
    }
    async function chargerLocal(id) {
        var r = await api('/local/id/' + id);
        var local = Array.isArray(r.content) ? r.content[0] : r.content;
        if (local) { etat.cacheLocaux[String(p(local, 'id'))] = p(local, 'designation'); }
        return local || null;
    }
    async function nomLocal(id) {
        if (!id) { return null; }
        if (etat.cacheLocaux[String(id)]) { return etat.cacheLocaux[String(id)]; }
        try { var l = await chargerLocal(id); return l ? p(l, 'designation') : null; } catch (e) { return null; }
    }
    /* Bien identifié (« vu ») dans la campagne en cours : UserVu renseigné (le serveur le remet à NULL au lancement d'un inventaire).
       L'année de DateVu ne convient pas : l'année d'inventaire est une année comptable, l'identification a lieu les années suivantes. */
    function bienVu(immo) { return String(p(immo, 'userVu') || '').trim() !== ''; }
    /* Bien inventorié pour l'année en cours (clôture du local) : drapeau Inventorier, ou inventorieur renseigné sur l'année comptable courante. */
    /* On n'inventorie que ce qu'on a vu : identifié (userVu), inventorieur renseigné et dernière année d'inventaire = année en cours. */
    function bienInventorie(immo) {
        return bienVu(immo) && String(p(immo, 'inventorieur') || '').trim() !== '' && Number(p(immo, 'lastAnneeComptable')) === Number(etat.annee);
    }
    /* État d'inventaire d'un bien : 'inv' (inventorié), 'vu' (identifié, local à clôturer) ou '' (pas encore vu). */
    function statutBien(immo) { return bienInventorie(immo) ? 'inv' : (bienVu(immo) ? 'vu' : ''); }
    /* Pastille d'état d'inventaire d'un bien (liste ou fiche) ; detail : date avec l'heure, auteur, et mention « pas encore vu ». */
    function badgeStatutBien(b, detail) {
        var s = statutBien(b);
        if (s === 'inv') {
            var di = fmtDate(p(b, 'dateInventaire'), detail) || fmtDate(p(b, 'dateVu'), detail);
            return '<span class="m-badge m-badge-inv"><i class="iconoir-check"></i>inventorié' + (di ? ' le ' + esc(di) : '') + (detail && p(b, 'inventorieur') ? ' par ' + esc(p(b, 'inventorieur')) : '') + '</span>';
        }
        if (s === 'vu') {
            var dv = fmtDate(p(b, 'dateVu'), detail);
            return '<span class="m-badge m-badge-ident"><i class="iconoir-eye"></i>vu' + (dv ? ' le ' + esc(dv) : '') + (detail && p(b, 'userVu') ? ' par ' + esc(p(b, 'userVu')) : '') + ' · à clôturer</span>';
        }
        return detail ? '<span class="m-badge m-badge-total">pas encore vu (inventaire ' + esc(etat.annee) + ')</span>' : '';
    }
    /* Marque localement un bien comme identifié (affichage immédiat en bleu, avant rechargement depuis le serveur). */
    function marquerVuLocalement(immo) {
        immo.dateVu = new Date().toISOString();
        immo.userVu = String(p(etat.user, 'userName') || '').trim() || 'moi';
    }

    /* Étape d'avancement d'un local ou d'un organe, d'après ses compteurs, testée dans cet ordre :
       Vide (gris), À clôturer (bleu : des biens identifiés ne sont pas encore inventoriés), Clôturé (vert : tous inventoriés),
       Incomplet (orange : clôturé, mais des biens ne sont ni vus ni déclarés non vus), À faire (rouge). */
    function etapeInventaire(total, identifies, inventories) {
        total = Number(total || 0); identifies = Number(identifies || 0); inventories = Number(inventories || 0);
        if (total === 0 && inventories === 0) { return { cle: 'vide', libelle: 'Vide', icone: 'iconoir-box-iso' }; }
        if (identifies > inventories) { return { cle: 'cloturer', libelle: 'À clôturer', icone: 'iconoir-eye' }; }
        if (inventories >= total) { return { cle: 'cloture', libelle: 'Clôturé', icone: 'iconoir-check-circle' }; }
        if (inventories > 0 || identifies > 0) { return { cle: 'incomplet', libelle: 'Incomplet', icone: 'iconoir-warning-triangle' }; }
        return { cle: 'afaire', libelle: 'À faire', icone: 'iconoir-scan-qr-code' };
    }
    function compteursLocal(l) { return { total: p(l, 'quantiteImmo'), identifies: p(l, 'quantiteImmoIdentifier'), inventories: p(l, 'quantiteImmoInventorier') }; }
    function compteursOrgane(o) { return { total: p(o, 'nbreBien'), identifies: p(o, 'nbreBienIdentifie'), inventories: p(o, 'nbreBienInventorie') }; }
    function etapeDe(c) { return etapeInventaire(c.total, c.identifies, c.inventories); }
    /* Pastille de l'étape et compteur « total / identifiés / inventoriés » d'un local ou d'un organe. */
    function puceEtape(c) {
        var e = etapeDe(c);
        return '<span class="m-etape m-etape-' + e.cle + '"><i class="' + e.icone + '"></i>' + esc(e.libelle) + '</span>' +
            '<span class="m-compteur-inv" title="Total / identifiés / inventoriés"><b>' + nombre(c.total) + '</b>/<span class="ident">' + nombre(c.identifies) + '</span>/<span class="inv">' + nombre(c.inventories) + '</span></span>';
    }

    /* Observations d'un état ('B' ou 'M') : GET /observation/etat/{etat}, mises en cache. */
    async function observations(etatB) {
        if (etat.obs[etatB]) { return etat.obs[etatB]; }
        var r = await api('/observation/etat/' + etatB);
        var liste = (Array.isArray(r.content) ? r.content : []).map(function (o) { return { id: p(o, 'id'), libelle: p(o, 'observation') }; });
        if (liste.length) { etat.obs[etatB] = liste; }
        return liste;
    }
    /* Observation proposée par défaut pour un état : la plus « neutre » si elle existe, sinon la première. */
    async function observationParDefaut(etatB) {
        var liste = await observations(etatB);
        var motif = etatB === 'B' ? /^\s*(bon|ras\b|r\.a\.s|rien|normal|fonctionn|en service|ok\b)/i : /(mauvais|d[ée]fect|panne|endommag|cass|hors|ab[iî]m)/i;
        return liste.filter(function (o) { return motif.test(o.libelle || ''); })[0] || liste[0] || null;
    }

    /* ------------------------------------------------------------------ en-tête et navigation */
    function entete(titre, retour, onglet, sousTitre) {
        var zoneTitre = $('#topTitle');
        zoneTitre.textContent = titre;
        zoneTitre.classList.toggle('m-title-double', !!sousTitre);
        if (sousTitre) {
            var sous = document.createElement('small');
            sous.className = 'm-title-sous';
            sous.textContent = sousTitre;
            zoneTitre.appendChild(sous);
        }
        $('#btnRetour').hidden = !retour;
        $('#topLogo').hidden = !!retour;
        $('#btnCompte').hidden = !etat.user;
        document.title = titre + ' - Locate Terrain';
        document.body.classList.toggle('page-accueil', onglet === 'accueil');
        couleurTheme(onglet === 'accueil' ? '#0b1f3a' : '#ffffff');
        $$('#tabbar [data-onglet]').forEach(function (a) { a.classList.toggle('actif', a.dataset.onglet === onglet); });
        window.scrollTo(0, 0);
    }
    function aller(hash) { if (location.hash === hash) { rendre(); } else { location.hash = hash; } }
    $('#btnRetour').onclick = function () { if (history.length > 1) { history.back(); } else { aller('#/'); } };
    $('#btnRafraichir').onclick = function () { etat.arbre = null; rendre(); };
    $('#tabScanner').onclick = function () { demarrerRapide(); };
    $('#btnCompte').onclick = function () {
        if (!etat.user) { return; }
        var u = etat.user;
        var corps = ouvrirSheet('Mon compte',
            '<div class="m-compte-tete">' +
            '<img class="m-avatar" src="/Content/images/photos/' + esc(p(u, 'photo') || '') + '" alt="" onerror="this.style.visibility=\'hidden\'">' +
            '<div class="min-w-0"><div class="fw-semibold">' + esc([p(u, 'nom'), p(u, 'postnom'), p(u, 'prenom')].filter(Boolean).join(' ') || 'Utilisateur connecté') + '</div>' +
            '<div class="m-sous">' + esc(p(u, 'userName')) + (p(u, 'profil') ? ' · ' + esc(p(p(u, 'profil'), 'nom')) : '') + '</div>' +
            '<div class="m-sous">Inventaire ' + esc(etat.annee) + '</div></div></div>' +
            '<div class="m-choix">' +
            '<button type="button" id="cptWeb"><i class="iconoir-laptop"></i>Ouvrir la version web</button>' +
            '<button type="button" id="cptRefresh"><i class="iconoir-refresh"></i>Recharger les données</button>' +
            '<button type="button" class="danger" id="cptLogout"><i class="iconoir-log-out"></i>Se déconnecter</button>' +
            '</div>');
        $('#cptWeb', corps).onclick = function () { window.location.href = '/'; };
        $('#cptRefresh', corps).onclick = function () { fermerSheet(); etat.arbre = null; etat.obs = {}; rendre(); };
        $('#cptLogout', corps).onclick = async function () {
            fermerSheet();
            try { await fetch('/auth/logout', { credentials: 'same-origin', headers: etat.jeton ? { 'Authorization': 'Bearer ' + etat.jeton, 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' } : {} }); } catch (e) { /* ignoré */ }
            oublierJeton();
            etat.user = null; etat.arbre = null;
            afficherLogin();
        };
    };

    function afficherChargement(accueil) { $('#page').innerHTML = squelette(accueil ? 3 : 6, accueil); }
    function afficherErreur(message, reessayer) {
        $('#page').innerHTML = vide('iconoir-warning-triangle', 'Impossible d\'afficher cette page', message, 'attention', { id: 'btnReessayer', libelle: 'Réessayer' });
        $('#btnReessayer').onclick = reessayer || rendre;
    }

    var navigation = 0; // incrémenté à chaque rendu : une page lente abandonnée ne reprend pas la main
    async function rendre() {
        navigation++;
        if (scan.resoudre) { fermerScanner(null); }
        quitterRapide();
        fermerSheet();
        setActions(null);
        document.body.classList.remove('mode-login', 'mode-rapide');
        if (!etat.user) {
            document.body.classList.add('mode-demarrage');
            try { await chargerProfil(); }
            catch (e) {
                // Session perdue : afficherLogin() a déjà pris la main. Sinon (réseau...), proposer de réessayer.
                if (!document.body.classList.contains('mode-login')) {
                    $('#page').innerHTML = vide('iconoir-wifi-off', 'Connexion impossible', e.message, 'attention', { id: 'btnReessayer', libelle: 'Réessayer' });
                    $('#btnReessayer').onclick = rendre;
                }
                return;
            }
        }
        document.body.classList.remove('mode-demarrage');
        var h = location.hash.replace(/^#\/?/, '');
        var partie = h.split('?')[0].split('/').filter(Boolean);
        var params = new URLSearchParams(h.split('?')[1] || '');
        try {
            if (partie.length === 0) { return await pageDashboard(); }
            if (partie[0] === 'organes') { return await pageOrganes(); }
            if (partie[0] === 'organe' && partie[1]) { return await pageOrgane(decodeURIComponent(partie[1])); }
            if (partie[0] === 'local' && partie[1]) {
                var idArticle = partie[2] === 'article' ? partie[3] : null;
                return await pageLocal(partie[1], idArticle ? 'details' : (params.get('mode') || 'articles'), idArticle);
            }
            if (partie[0] === 'bien' && partie[1]) { return await pageBien(partie[1], params.get('depuis') === 'declasses'); }
            if (partie[0] === 'nonvu') { return await pageSpeciale('nonvu'); }
            if (partie[0] === 'transit') { return await pageSpeciale('transit'); }
            if (partie[0] === 'declasses') { return await pageSpeciale('declasses'); }
            if (partie[0] === 'rapide' && partie[1]) { return await pageRapide(partie[1]); }
            aller('#/');
        } catch (e) {
            if (etat.user) {
                document.body.classList.remove('mode-rapide');
                quitterRapide();
                afficherErreur(e.message || 'Erreur inattendue.');
            }
        }
    }
    window.addEventListener('hashchange', rendre);

    /* ------------------------------------------------------------------ page : connexion (même présentation que le site) */
    function afficherLogin() {
        if (scan.resoudre) { fermerScanner(null); }
        quitterRapide();
        setActions(null);
        document.body.classList.remove('mode-demarrage', 'mode-rapide', 'page-accueil');
        document.body.classList.add('mode-login');
        document.title = 'Connexion - Locate Terrain';
        couleurTheme('#000000');
        $('#btnCompte').hidden = true;
        $('#page').innerHTML =
            '<div class="m-login"><div class="card">' +
            '<div class="card-body p-0 bg-black auth-header-box rounded-top"><div class="text-center p-3">' +
            '<img src="/Content/images/locate-logo.png" alt="Locate" class="auth-logo">' +
            '<h4 class="mt-3 mb-1 fw-semibold text-white fs-18">Bienvenue sur Locate</h4>' +
            '<p class="text-muted fw-medium mb-0">Connectez-vous pour continuer.</p>' +
            '</div></div>' +
            '<div class="card-body pt-0">' +
            '<div class="alert alert-danger mt-3 mb-0 d-flex align-items-center gap-2" role="alert" id="lgErr" hidden><i class="iconoir-warning-triangle"></i><div></div></div>' +
            '<form class="my-4" id="frmLogin" autocomplete="on">' +
            '<div class="form-group mb-2"><label class="form-label" for="lgUser">Compte</label><input type="text" class="form-control" id="lgUser" name="UserName" placeholder="Votre identifiant" autocomplete="username" autocapitalize="none" spellcheck="false" required></div>' +
            '<div class="form-group"><label class="form-label" for="lgPwd">Mot de passe</label><input type="password" class="form-control" id="lgPwd" name="Password" placeholder="Votre mot de passe" autocomplete="current-password" required></div>' +
            '<div class="form-group row mt-3"><div class="col-sm-12"><div class="form-check form-switch form-switch-success">' +
            '<input class="form-check-input" type="checkbox" id="lgRemember" checked><label class="form-check-label" for="lgRemember">Se souvenir de moi</label></div></div></div>' +
            '<div class="form-group mb-0 row"><div class="col-12"><div class="d-grid mt-3"><button class="btn btn-primary" type="submit" id="lgBtn">Connexion <i class="fas fa-sign-in-alt ms-1"></i></button></div></div></div>' +
            '</form>' +
            '<div class="text-center mb-2"><p class="text-muted mb-0">&copy; ' + new Date().getFullYear() + ' RandareCx</p></div>' +
            '</div></div></div>';
        var erreur = function (m) { $('#lgErr').hidden = !m; $('#lgErr div').textContent = m || ''; };
        $('#frmLogin').onsubmit = async function (e) {
            e.preventDefault();
            var btn = $('#lgBtn'); btn.disabled = true; erreur('');
            var souvenir = $('#lgRemember').checked ? 1 : 0;
            try {
                var r = await api('/auth/login', { form: { UserName: $('#lgUser').value.trim(), Password: $('#lgPwd').value, RememberMe: souvenir }, sansReessai: true });
                if (reussi(r)) {
                    etat.identifiants = { u: $('#lgUser').value.trim(), p: $('#lgPwd').value, r: souvenir };
                    if (r.token) { enregistrerJeton(r.token); }
                    await chargerProfil();
                    document.body.classList.remove('mode-login');
                    $('#btnCompte').hidden = false;
                    if (!location.hash || location.hash === '#' || location.hash === '#/') { aller('#/'); } else { rendre(); }
                } else {
                    erreur(r.message || 'Connexion refusée.');
                }
            } catch (err) {
                erreur(err.message);
            } finally { btn.disabled = false; }
        };
    }

    /* ------------------------------------------------------------------ page : accueil */
    async function pageDashboard() {
        entete('Locate Terrain', false, 'accueil');
        afficherChargement(true);
        await chargerArbre();
        var t = totaux();
        var pct = pourcent(t.inventories, t.total), pctId = pourcent(t.identifies, t.total);
        var u = etat.user;
        var nomUser = p(u, 'prenom') || p(u, 'nom') || p(u, 'userName') || '';
        var circ = 2 * Math.PI * 46;
        $('#page').innerHTML =
            '<section class="m-hero">' +
            '<div class="m-hero-haut"><div class="m-hero-texte">' +
            (nomUser ? '<div class="m-hero-salut">Bonjour,</div><div class="m-hero-nom">' + esc(nomUser) + '</div>' : '<div class="m-hero-salut">Bonjour</div><div class="m-hero-nom">Bienvenue sur Locate</div>') +
            '<div class="m-hero-organe"><i class="iconoir-community"></i><span>Organe d\'inventaire : <b>' + esc(nomOrganeInventaire()) + '</b></span></div>' +
            (etat.inventaireOuvert
                ? '<span class="m-puce-annee"><i class="iconoir-calendar"></i>Inventaire ' + esc(etat.annee) + ' en cours</span>'
                : '<span class="m-puce-annee m-puce-alerte"><i class="iconoir-warning-triangle"></i>Aucun inventaire en cours</span>') + '</div>' +
            '<div class="m-anneau" role="img" aria-label="' + pct + ' % inventorié"><svg viewBox="0 0 108 108"><circle class="fond" cx="54" cy="54" r="46" fill="none" stroke-width="10"></circle>' +
            '<circle class="val" id="anneauVal" cx="54" cy="54" r="46" fill="none" stroke-width="10" stroke-dasharray="' + circ.toFixed(2) + '" stroke-dashoffset="' + circ.toFixed(2) + '"></circle></svg>' +
            '<div class="centre"><b>' + pct + '%</b><small>inventorié</small></div></div></div>' +
            '<div class="m-hero-stats"><div><b>' + nombre(t.total) + '</b><span>Biens</span></div><div><b>' + nombre(t.identifies) + '</b><span>Vus (identifiés)</span></div><div><b>' + nombre(t.inventories) + '</b><span>Inventoriés</span></div></div>' +
            '<div class="m-hero-barre"><div class="d-flex justify-content-between"><span>Biens identifiés (vus)</span><b>' + pctId + '%</b></div><div class="m-progress"><div style="width:' + pctId + '%"></div></div></div>' +
            '<button type="button" class="m-btn m-btn-blanc" id="actRapide"><span class="ico"><i class="iconoir-scan-qr-code"></i></span>Démarrer l\'inventaire rapide</button>' +
            '</section>' +
            '<div id="carteFile"></div>' +
            '<div class="m-accueil-grille"><div>' +
            '<div class="m-section"><h2>Parcourir</h2><span>' + nombre(etat.plat.length) + ' organe(s)</span></div><div class="m-actions">' +
            '<button type="button" class="m-action" id="actOrganes"><span class="ico ico-bleu"><i class="iconoir-city"></i></span><span class="txt">Lister les organes<small>Vos organes, leurs locaux et leurs biens</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '<button type="button" class="m-action" id="actScanLocal"><span class="ico ico-sarcelle"><i class="iconoir-home-alt"></i></span><span class="txt">Scanner un local<small>Affiche le contenu du local scanné</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '<button type="button" class="m-action" id="actScanBien"><span class="ico ico-orange"><i class="iconoir-qr-code"></i></span><span class="txt">Scanner un bien<small>Affiche la fiche du bien scanné</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '</div></div><div>' +
            '<div class="m-section"><h2>Mouvements</h2><span>scannez le bien concerné</span></div><div class="m-tuiles">' +
            '<button type="button" class="m-tuile" id="mvExpedier"><span class="ico ico-cyan"><i class="iconoir-send"></i></span><b>Expédier un bien</b><small>Vers un autre organe (transit)</small></button>' +
            '<button type="button" class="m-tuile" id="mvService"><span class="ico ico-vert"><i class="iconoir-log-in"></i></span><b>Mettre en service</b><small>Réceptionner un bien dans un local</small></button>' +
            '<button type="button" class="m-tuile" id="mvNonVu"><span class="ico ico-rouge"><i class="iconoir-eye-closed"></i></span><b>Déclarer non vu</b><small>Bien introuvable</small></button>' +
            '<button type="button" class="m-tuile" id="mvDeclasser"><span class="ico ico-gris"><i class="iconoir-archive"></i></span><b>Déclasser un bien</b><small>Vers le local des déclassés</small></button>' +
            '</div></div><div>' +
            '<div class="m-section"><h2>Locaux particuliers</h2></div><div class="m-actions">' +
            '<button type="button" class="m-action" id="actNonVu"><span class="ico ico-violet"><i class="iconoir-eye-closed"></i></span><span class="txt">Biens non vus<small>Local « non vu » de votre organe</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '<button type="button" class="m-action" id="actTransit"><span class="ico ico-orange"><i class="iconoir-delivery-truck"></i></span><span class="txt">Biens en transit<small>Local de transit de votre organe</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '<button type="button" class="m-action" id="actDeclasses"><span class="ico ico-gris"><i class="iconoir-archive"></i></span><span class="txt">Biens déclassés<small>Local des déclassés de votre entité, cession</small></span><i class="iconoir-nav-arrow-right fleche"></i></button>' +
            '</div></div></div>';
        requestAnimationFrame(function () { requestAnimationFrame(function () { var a = $('#anneauVal'); if (a) { a.setAttribute('stroke-dashoffset', (circ * (1 - pct / 100)).toFixed(2)); } }); });
        $('#actRapide').onclick = demarrerRapide;
        $('#actOrganes').onclick = function () { aller('#/organes'); };
        $('#actScanLocal').onclick = scanLocal;
        $('#actScanBien').onclick = scanBien;
        $('#actNonVu').onclick = function () { aller('#/nonvu'); };
        $('#actTransit').onclick = function () { aller('#/transit'); };
        $('#actDeclasses').onclick = function () { aller('#/declasses'); };
        $('#mvExpedier').onclick = function () { scannerPuis('Expédier un bien', expedierBien); };
        $('#mvService').onclick = function () { scannerPuis('Mettre en service', mettreEnService); };
        $('#mvNonVu').onclick = function () { scannerPuis('Déclarer non vu', declarerNonVu); };
        $('#mvDeclasser').onclick = function () { scannerPuis('Déclasser un bien', declasserBien); };
        majCarteFile();
        synchroniserFile(false);
    }

    /* ------------------------------------------------------------------ filtre de liste (recherche collante) */
    function brancherFiltre(input, conteneur, selecteur, zoneVide, filtreSup) {
        var appliquer = function () {
            var q = minuscule(input.value), visibles = 0;
            $$(selecteur, conteneur).forEach(function (it) {
                var ok = (!q || it.dataset.recherche.indexOf(q) !== -1) && (!filtreSup || filtreSup(it));
                it.hidden = !ok;
                if (ok) { visibles++; }
            });
            if (zoneVide) { zoneVide.hidden = visibles > 0 || !$$(selecteur, conteneur).length; }
        };
        input.oninput = appliquer;
        return appliquer;
    }

    /* ------------------------------------------------------------------ page : liste des organes */
    function badgesOrgane(o) { return puceEtape(compteursOrgane(o)); }
    /* Légende des cartes organe / local : compteur « total / identifiés / inventoriés » et étapes. */
    function legendeBadges() {
        return '<div class="m-legende"><span><span class="m-compteur-inv"><b>total</b>/<span class="ident">identifiés</span>/<span class="inv">inventoriés</span></span></span>' +
            '<span><span class="m-etape m-etape-afaire">À faire</span><span class="m-etape m-etape-cloturer">À clôturer</span><span class="m-etape m-etape-incomplet">Incomplet</span><span class="m-etape m-etape-cloture">Clôturé</span></span></div>';
    }
    async function pageOrganes() {
        entete('Organes', true, 'organes');
        afficherChargement();
        await chargerArbre();
        var page = $('#page');
        page.innerHTML = '<div class="m-collant">' + recherche('filtreOrg', 'Rechercher un organe (nom, sigle, code)') + legendeBadges() + '</div>' +
            '<div class="m-liste m-arbre" id="listeOrg"></div><div id="videOrg" hidden>' + vide('iconoir-search', 'Aucun organe trouvé', 'Essayez un autre nom, sigle ou code.') + '</div>';
        var liste = $('#listeOrg');
        if (!etat.plat.length) { liste.innerHTML = vide('iconoir-city', 'Aucun organe', 'Aucun organe dans votre périmètre.'); }
        etat.plat.forEach(function (x) {
            var o = x.organe;
            var it = el('<div class="m-item m-cliquable m-etape-bord bord-' + etapeDe(compteursOrgane(o)).cle + ' niveau-' + x.niveau + '" style="--niveau:' + Math.min(x.niveau, 4) + '"><div class="corps"><div class="titre">' + esc(p(o, 'nom')) + '</div>' +
                '<div class="detail">' + esc(p(o, 'id')) + (p(o, 'sigle') ? ' · ' + esc(p(o, 'sigle')) : '') + '</div><div class="badges">' + badgesOrgane(o) + '</div></div><i class="iconoir-nav-arrow-right fleche"></i></div>');
            it.dataset.recherche = minuscule((p(o, 'nom') || '') + ' ' + (p(o, 'sigle') || '') + ' ' + (p(o, 'id') || ''));
            it.onclick = function () { aller('#/organe/' + encodeURIComponent(p(o, 'id'))); };
            liste.appendChild(it);
        });
        brancherFiltre($('#filtreOrg'), liste, '.m-item', $('#videOrg'));
        setActions([{ icone: 'iconoir-scan-qr-code', libelle: 'Scanner un local', action: scanLocal, principal: true }]);
    }

    /* ------------------------------------------------------------------ page : locaux d'un organe */
    async function pageOrgane(code) {
        await chargerArbre();
        var organe = organeParId(code);
        entete(organe ? p(organe, 'nom') : 'Locaux', true, 'organes');
        afficherChargement();
        var r = await api('/local/organe/' + encodeURIComponent(code));
        var locaux = Array.isArray(r.content) ? r.content : [];
        var page = $('#page');
        page.innerHTML = (organe ? '<div class="m-card m-etape-bord bord-' + etapeDe(compteursOrgane(organe)).cle + '"><div class="m-entete-carte"><span class="ico-gros ico-bleu"><i class="iconoir-city"></i></span><div class="min-w-0"><h6>' + esc(p(organe, 'nom')) + '</h6><div class="m-sous">' + esc(p(organe, 'id')) + (p(organe, 'sigle') ? ' · ' + esc(p(organe, 'sigle')) : '') + '</div><div class="d-flex flex-wrap gap-1 mt-2">' + badgesOrgane(organe) + '</div></div></div></div>' : '') +
            '<div class="m-collant">' + recherche('filtreLoc', 'Rechercher un local (nom, code)') + legendeBadges() +
            '<div class="m-legende"><span><span class="m-qr non petit" style="width:22px;height:22px;font-size:.8rem;border-radius:6px"><i class="iconoir-qr-code"></i></span>sans QR : touchez pour en affecter un</span><span>appui long sur le QR : changer / détacher</span></div></div>' +
            '<div class="m-section"><h2>Locaux</h2><span>' + nombre(locaux.length) + '</span></div>' +
            '<div class="m-liste grille" id="listeLocaux"></div><div id="videLoc" hidden>' + vide('iconoir-search', 'Aucun local trouvé', 'Essayez un autre nom ou code.') + '</div>';
        var liste = $('#listeLocaux');
        etat.locauxOrgane = { code: String(code), locaux: locaux };
        if (!locaux.length) {
            liste.outerHTML = vide('iconoir-home-alt', 'Aucun local', 'Cet organe n\'a pas encore de local.', '', { id: 'btnCreerLocalVide', libelle: 'Créer un local', icone: 'iconoir-plus' });
            $('#btnCreerLocalVide').onclick = function () { creerLocal(code); };
        }
        else { locaux.forEach(function (l) { liste.appendChild(ligneLocal(l, code)); }); brancherFiltre($('#filtreLoc'), liste, '.m-item', $('#videLoc')); }
        setActions([
            { icone: 'iconoir-scan-qr-code', libelle: 'Scanner un local', action: scanLocal, principal: true },
            { icone: 'iconoir-plus', libelle: 'Créer un local', court: 'Nouveau', action: function () { creerLocal(code); } }
        ]);
    }

    /* Création d'un local dans l'organe : POST /local/add/ { Code, Designation, CodeOrgane, IsSpace, IdTypeLocal=1 },
       puis rechargement de la liste et proposition de scanner l'étiquette QR du nouveau local. */
    async function creerLocal(codeOrgane) {
        var organe = organeParId(codeOrgane);
        var corps = await ouvrirFeuille('Nouveau local',
            (organe ? '<div class="m-feuille-intro">Organe : <b>' + esc(p(organe, 'nom')) + '</b></div>' : '') +
            '<form id="frmLocal" autocomplete="off" novalidate>' +
            '<div class="mb-3"><label class="m-libelle" for="nlDesignation">Désignation <span class="text-danger">*</span></label>' +
            '<input type="text" class="form-control" id="nlDesignation" maxlength="150" placeholder="Ex. Bureau 204 - Secrétariat" required></div>' +
            '<div class="mb-3"><label class="m-libelle" for="nlCode">Code (facultatif)</label>' +
            '<input type="text" class="form-control" id="nlCode" maxlength="50" placeholder="Ex. B2-204" autocapitalize="characters" spellcheck="false"></div>' +
            '<label class="m-interrupteur" for="nlEspace"><span>C\'est un espace<small>Couloir, hall, cour, parking… plutôt qu\'une pièce fermée</small></span>' +
            '<span class="form-check form-switch m-0"><input class="form-check-input" type="checkbox" role="switch" id="nlEspace"></span></label>' +
            '<button type="submit" class="m-btn m-btn-primaire m-btn-bloc mt-3" id="nlValider"><i class="iconoir-plus"></i>Créer le local</button></form>');
        $('#frmLocal', corps).onsubmit = async function (e) {
            e.preventDefault();
            var designation = $('#nlDesignation', corps).value.trim(), codeLocal = $('#nlCode', corps).value.trim();
            if (!designation) { toast('La désignation du local est obligatoire.', 'warning'); $('#nlDesignation', corps).focus(); return; }
            var bouton = $('#nlValider', corps);
            bouton.disabled = true;
            var ok = false;
            try {
                ok = await action('/local/add/', { Code: codeLocal, Designation: designation, CodeOrgane: codeOrgane, IsSpace: $('#nlEspace', corps).checked ? 'true' : 'false', IdTypeLocal: 1 });
            } catch (err) { toast(err.message, 'danger'); }
            bouton.disabled = false;
            if (!ok) { return; }
            fermerSheet();
            etat.arbre = null;
            await rendre(); // recharge la liste des locaux de l'organe (page courante)
            // Le nouveau local : même désignation (et même code), identifiant le plus grand.
            var trouves = ((etat.locauxOrgane && etat.locauxOrgane.code === String(codeOrgane)) ? etat.locauxOrgane.locaux : []).filter(function (l) {
                return minuscule(p(l, 'designation')) === minuscule(designation) && (!codeLocal || minuscule(p(l, 'code')) === minuscule(codeLocal));
            }).sort(function (a, b) { return Number(p(b, 'id')) - Number(p(a, 'id')); });
            var nouveau = trouves[0];
            if (!nouveau || p(nouveau, 'qrCode')) { return; }
            var suite = await confirmer('Local créé', 'Voulez-vous scanner maintenant l\'étiquette QR à coller sur <strong>' + esc(designation) + '</strong> ?', 'Scanner', 'Plus tard');
            if (suite) { await affecterQrLocal(nouveau, codeOrgane); }
        };
    }

    function ligneLocal(l, codeOrgane) {
        var id = p(l, 'id'), qr = p(l, 'qrCode');
        etat.cacheLocaux[String(id)] = p(l, 'designation');
        var it = el('<div class="m-item m-etape-bord bord-' + etapeDe(compteursLocal(l)).cle + '">' +
            '<div class="m-qr ' + (qr ? 'oui' : 'non') + '" role="button" aria-label="' + (qr ? 'QR code affecté (appui long pour changer)' : 'Sans QR code : toucher pour en affecter un') + '"><i class="iconoir-qr-code"></i></div>' +
            '<div class="corps m-cliquable"><div class="titre">' + esc(p(l, 'designation')) + '</div>' +
            '<div class="detail">' + esc(p(l, 'code') || '') + (p(l, 'isSpace') ? ' · espace' : '') + '</div>' +
            '<div class="badges">' + (qr ? '' : '<span class="m-badge m-badge-att"><i class="iconoir-warning-triangle"></i>sans QR</span>') +
            puceEtape(compteursLocal(l)) + '</div></div>' +
            '<button type="button" class="m-kebab" aria-label="Actions sur le local"><i class="iconoir-more-vert"></i></button></div>');
        it.dataset.recherche = minuscule((p(l, 'designation') || '') + ' ' + (p(l, 'code') || ''));
        $('.corps', it).onclick = function () { ouvrirLocal(l, codeOrgane); };
        $('.m-kebab', it).onclick = function () { menuLocal(l, codeOrgane); };
        appuiLong($('.m-qr', it), function () { changerQrLocal(l, codeOrgane); }, function () { ouvrirLocal(l, codeOrgane); });
        return it;
    }

    /* Appui long (600 ms) sur un élément, avec action courte de repli. */
    function appuiLong(elem, longue, courte) {
        var timer = null, declenche = false;
        var debut = function () { declenche = false; timer = setTimeout(function () { declenche = true; vibrer(40); longue(); }, 600); };
        var fin = function (e) { if (timer) { clearTimeout(timer); timer = null; } if (!declenche && e.type === 'pointerup' && courte) { courte(); } };
        elem.addEventListener('pointerdown', debut);
        elem.addEventListener('pointerup', fin);
        elem.addEventListener('pointerleave', fin);
        elem.addEventListener('pointercancel', fin);
        elem.addEventListener('contextmenu', function (e) { e.preventDefault(); });
    }

    async function ouvrirLocal(l, codeOrgane) {
        var id = p(l, 'id');
        if (p(l, 'qrCode')) { aller('#/local/' + id); return; }
        var ok = await confirmer('Local sans QR code', 'Le local <strong>' + esc(p(l, 'designation')) + '</strong> n\'a pas de QR code.<br>Voulez-vous en scanner un pour le lui affecter ?', 'Scanner', 'Plus tard');
        if (!ok) { return; }
        await affecterQrLocal(l, codeOrgane);
    }

    async function affecterQrLocal(l, codeOrgane) {
        var code = await scanner('Affecter un QR code au local', 'Scannez l\'étiquette QR à coller sur « ' + (p(l, 'designation') || '') + ' ».');
        if (!code) { return false; }
        if (!GUID.test(code)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return false; }
        var ok = await action('/local/qrcode/', { Id: p(l, 'id'), QrCode: code });
        if (ok) { if (codeOrgane) { aller('#/organe/' + encodeURIComponent(codeOrgane)); } else { rendre(); } }
        return ok;
    }

    async function changerQrLocal(l, codeOrgane) {
        if (!p(l, 'qrCode')) { return affecterQrLocal(l, codeOrgane); }
        var ok = await confirmer('Changer le QR code', 'Voulez-vous changer le QR code du local <strong>' + esc(p(l, 'designation')) + '</strong> ?');
        if (!ok) { return; }
        var corps = ouvrirSheet('Retirer le QR code actuel',
            '<p class="m-sous">Comme dans la version web, choisissez ce que devient l\'étiquette actuelle.</p><div class="m-choix">' +
            '<button type="button" data-url="/local/resetqrcode/' + p(l, 'id') + '"><i class="iconoir-refresh-double"></i><span>Détacher pour l\'affecter ailleurs<br><small class="m-sous">L\'étiquette reste utilisable</small></span></button>' +
            '<button type="button" class="danger" data-url="/local/liveqrcode/' + p(l, 'id') + '"><i class="iconoir-trash"></i><span>Jeter le QR code<br><small class="m-sous">L\'étiquette est abîmée ou perdue</small></span></button>' +
            '</div>');
        $$('button[data-url]', corps).forEach(function (b) {
            b.onclick = async function () {
                fermerSheet();
                if (!(await action(b.dataset.url))) { return; }
                var suite = await confirmer('QR code retiré', 'Scanner le nouveau QR code maintenant ?', 'Scanner', 'Plus tard');
                var copie = Object.assign({}, l); copie.qrCode = null; copie.QrCode = null;
                if (suite) { await affecterQrLocal(copie, codeOrgane); } else if (codeOrgane) { aller('#/organe/' + encodeURIComponent(codeOrgane)); } else { rendre(); }
            };
        });
    }

    function menuLocal(l, codeOrgane) {
        var id = p(l, 'id');
        var corps = ouvrirSheet(p(l, 'designation') || 'Local',
            '<div class="m-choix">' +
            '<button type="button" id="mlBiens"><i class="iconoir-box-iso"></i>Voir les biens</button>' +
            (p(l, 'qrCode') ? '<button type="button" id="mlRapide"><i class="iconoir-scan-qr-code"></i>Inventaire rapide de ce local</button>' : '') +
            '<button type="button" id="mlModifier"><i class="iconoir-edit-pencil"></i>Modifier le local</button>' +
            '<button type="button" id="mlDeplacer"><i class="iconoir-arrow-right"></i>Déplacer vers un autre organe</button>' +
            (p(l, 'qrCode') ? '<button type="button" id="mlQr"><i class="iconoir-qr-code"></i>Changer le QR code</button>' : '<button type="button" id="mlQr"><i class="iconoir-scan-qr-code"></i>Affecter un QR code</button>') +
            '</div>');
        $('#mlBiens', corps).onclick = function () { fermerSheet(); aller('#/local/' + id); };
        if ($('#mlRapide', corps)) { $('#mlRapide', corps).onclick = function () { fermerSheet(); aller('#/rapide/' + id); }; }
        $('#mlModifier', corps).onclick = function () { window.location.href = '/local/modify/' + id; };
        $('#mlDeplacer', corps).onclick = function () { fermerSheet(); deplacerLocal(l, codeOrgane); };
        $('#mlQr', corps).onclick = function () { fermerSheet(); changerQrLocal(l, codeOrgane); };
    }

    async function deplacerLocal(l, codeOrgane) {
        await chargerArbre();
        var corps = ouvrirSheet('Déplacer « ' + (p(l, 'designation') || '') + ' »',
            '<div class="mb-2">' + recherche('dlFiltre', 'Rechercher l\'organe de destination') + '</div><div class="m-choix" id="dlListe" style="max-height:55vh;overflow:auto"></div>');
        var liste = $('#dlListe', corps);
        etat.plat.forEach(function (x) {
            var o = x.organe, oid = p(o, 'id');
            if (String(oid) === String(p(l, 'codeOrgane'))) { return; }
            var b = el('<button type="button" style="padding-left:' + (0.9 + Math.min(x.niveau, 4) * 0.9) + 'rem"><i class="iconoir-city"></i><span>' + esc(p(o, 'nom')) + '<br><small class="m-sous">' + esc(oid) + '</small></span></button>');
            b.dataset.recherche = minuscule((p(o, 'nom') || '') + ' ' + oid);
            b.onclick = async function () {
                fermerSheet();
                var ok = await confirmer('Déplacer le local', 'Déplacer <strong>' + esc(p(l, 'designation')) + '</strong> vers <strong>' + esc(p(o, 'nom')) + '</strong> ?');
                if (!ok) { return; }
                var fait = await action('/local/modify/', {
                    Id: p(l, 'id'), Code: p(l, 'code') || '', Designation: p(l, 'designation') || '', CodeOrgane: oid,
                    IsSpace: p(l, 'isSpace') ? 'true' : 'false', IsActive: p(l, 'isActive') === false ? 'false' : 'true', IdTypeLocal: p(l, 'idTypeLocal') || ''
                });
                if (fait) { etat.arbre = null; aller('#/organe/' + encodeURIComponent(codeOrgane || p(l, 'codeOrgane'))); }
            };
            liste.appendChild(b);
        });
        brancherFiltre($('#dlFiltre', corps), liste, 'button');
    }

    /* ------------------------------------------------------------------ scans globaux */
    async function localParQr(code) {
        var r = await api('/local/qrcode/' + code);
        var l = Array.isArray(r.content) ? r.content[0] : r.content;
        if (l && p(l, 'id') != null) { etat.cacheLocaux[String(p(l, 'id'))] = p(l, 'designation'); return l; }
        return null;
    }
    async function scanLocal() {
        var code = await scanner('Scanner un local', 'Scannez le QR code collé sur la porte du local.');
        if (!code) { return; }
        if (!GUID.test(code)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return; }
        try {
            var l = await localParQr(code);
            if (!l) { toast('Aucun local ne porte ce QR code.', 'warning'); return; }
            aller('#/local/' + p(l, 'id'));
        } catch (e) { toast(e.message, 'danger'); }
    }

    async function bienParQr(code) {
        var r = await api('/immo/qrcode/' + code);
        var b = Array.isArray(r.content) ? r.content[0] : r.content;
        return b && p(b, 'id') != null ? b : null;
    }
    async function trouverBienParQr(titre, aide) {
        var code = await scanner(titre || 'Scanner un bien', aide || 'Scannez l\'étiquette QR du bien.');
        if (!code) { return null; }
        if (!GUID.test(code)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return null; }
        var b = await bienParQr(code);
        if (!b) { toast('Aucun bien ne porte ce QR code.', 'warning'); return null; }
        return b;
    }
    async function scanBien() {
        try {
            var b = await trouverBienParQr();
            if (b) { aller('#/bien/' + p(b, 'id')); }
        } catch (e) { toast(e.message, 'danger'); }
    }

    /* ------------------------------------------------------------------ liste de biens « en détails » (local, non vu, transit) */
    /* opts : { menu: false } pour ne pas proposer le bouton ⋮, { menu: function (b) } pour une feuille ⋮ particulière (page Déclassés),
       { declasse: true } si les biens sont dans un local des déclassés (le menu ⋮ propose la cession au lieu du déclassement),
       { lienFiche: '?depuis=declasses' } suffixe du lien vers la fiche, { avancement: false } pour masquer la progression « vus ». */
    function rendreListeBiens(zone, biens, avant, opts) {
        opts = opts || {};
        var avecAvancement = opts.avancement !== false;
        // Vus = identifiés (à clôturer) + inventoriés ; la barre montre les inventoriés (vert) puis les identifiés à clôturer (bleu).
        var nbIdent = 0, nbInv = 0;
        biens.forEach(function (b) { var s = statutBien(b); if (s === 'inv') { nbInv++; } else if (s === 'vu') { nbIdent++; } });
        var vus = nbIdent + nbInv, pct = pourcent(vus, biens.length);
        zone.innerHTML = (avant || '') +
            (avecAvancement ? '<div class="m-card"><div class="m-avancement"><span><b class="text-success">' + nombre(vus) + '</b> / ' + nombre(biens.length) + ' bien(s) vus · ' + nombre(nbInv) + ' inventorié(s)</span><b>' + pct + '%</b></div>' +
            '<div class="m-progress double"><div style="width:' + pourcent(nbInv, biens.length) + '%"></div><div class="ident" style="width:' + pourcent(nbIdent, biens.length) + '%"></div></div></div>' : '') +
            (biens.length ? '<div class="m-collant">' + recherche('filtreBiens', 'Rechercher (désignation, code ADM, code immo)') +
                (avecAvancement ? '<div class="m-filtres" id="filtresVu"><button type="button" class="actif" data-f="tous">Tous <b>' + biens.length + '</b></button><button type="button" data-f="">À voir <b>' + (biens.length - vus) + '</b></button>' +
                    '<button type="button" data-f="vu">À clôturer <b>' + nbIdent + '</b></button><button type="button" data-f="inv">Inventoriés <b>' + nbInv + '</b></button></div>' : '') + '</div>' : '') +
            '<div class="m-liste grille" id="listeBiens"></div><div id="videBiens" hidden>' + vide('iconoir-search', 'Aucun bien trouvé', 'Modifiez la recherche ou le filtre.') + '</div>';
        var lb = $('#listeBiens', zone);
        if (!biens.length) { lb.outerHTML = vide('iconoir-box-iso', 'Aucun bien', 'Aucun bien n\'est enregistré ici.'); return; }
        // Ordre : pas encore vus, puis identifiés (à clôturer), puis inventoriés.
        var rang = { '': 0, vu: 1, inv: 2 };
        if (avecAvancement) { biens.sort(function (a, b) { return rang[statutBien(a)] - rang[statutBien(b)]; }); }
        biens.forEach(function (b) { lb.appendChild(ligneBien(b, opts)); });
        var filtre = 'tous';
        var appliquer = brancherFiltre($('#filtreBiens', zone), lb, '.m-item', $('#videBiens', zone), function (it) {
            return filtre === 'tous' || it.dataset.statut === filtre;
        });
        $$('#filtresVu button', zone).forEach(function (b) {
            b.onclick = function () {
                filtre = b.dataset.f;
                $$('#filtresVu button', zone).forEach(function (x) { x.classList.toggle('actif', x === b); });
                appliquer();
            };
        });
    }

    function ligneBien(b, opts) {
        opts = opts || {};
        var avecMenu = opts.menu !== false;
        // Trois états : inventorié (vert), identifié mais pas encore inventorié (bleu, local à clôturer), pas encore vu (neutre).
        var statut = statutBien(b), etatB = p(b, 'lastEtat') || '';
        var it = el('<div class="m-item m-cliquable' + (statut === 'inv' ? ' m-vu' : statut === 'vu' ? ' m-ident' : '') + '">' +
            '<div class="m-qr petit ' + (statut === 'inv' ? 'vu' : statut === 'vu' ? 'ident' : (p(b, 'qrCode') ? 'oui bleu' : 'non')) + '" title="' + (p(b, 'qrCode') ? 'QR code affecté' : 'Sans QR code') + '">' +
            '<i class="' + (statut === 'inv' ? 'iconoir-check' : statut === 'vu' ? 'iconoir-eye' : 'iconoir-qr-code') + '"></i></div>' +
            '<div class="corps"><div class="titre">' + esc(p(b, 'designation') || ('Bien ' + p(b, 'id'))) + '</div>' +
            '<div class="detail">' + esc(p(b, 'codeADM') || '') + (p(b, 'codeImmo') ? ' · ' + esc(p(b, 'codeImmo')) : '') + '</div>' +
            '<div class="badges"><span class="m-badge m-badge-etat-' + esc(etatB) + '">' + esc(libelleEtat(b)) + '</span>' +
            (p(b, 'lastObservation') ? '<span class="m-badge m-badge-obs">' + esc(p(b, 'lastObservation')) + '</span>' : '') +
            badgeStatutBien(b, false) +
            (p(b, 'qrCode') ? '' : '<span class="m-badge m-badge-att">sans QR</span>') + '</div></div>' +
            (avecMenu ? '<button type="button" class="m-kebab" aria-label="Actions sur le bien"><i class="iconoir-more-vert"></i></button>' : '<i class="iconoir-nav-arrow-right fleche"></i>') + '</div>');
        it.dataset.recherche = minuscule([p(b, 'designation'), p(b, 'codeADM'), p(b, 'codeImmo'), p(b, 'lastObservation')].filter(Boolean).join(' '));
        it.dataset.statut = statut;
        it.onclick = function () { aller('#/bien/' + p(b, 'id') + (opts.lienFiche || '')); };
        if (avecMenu) {
            $('.m-kebab', it).onclick = function (e) {
                e.stopPropagation();
                if (typeof opts.menu === 'function') { opts.menu(b); } else { menuBien(b, false, !!opts.declasse); }
            };
        }
        return it;
    }

    /* ------------------------------------------------------------------ page : contenu d'un local */
    async function pageLocal(id, mode, idArticle) {
        entete('Local', true, 'organes');
        afficherChargement();
        var local = await chargerLocal(id);
        if (!local) { afficherErreur('Local introuvable.'); return; }
        await chargerArbre();
        var organe = organeParId(p(local, 'codeOrgane'));
        entete(p(local, 'designation'), true, 'organes');
        // Compteurs relus à chaque affichage (GET /local/id/) : l'étape et le bouton de clôture sont donc à jour.
        var cpt = compteursLocal(local), etape = etapeDe(cpt);
        var page = $('#page');
        page.innerHTML =
            '<div class="m-card m-etape-bord bord-' + etape.cle + '"><div class="m-entete-carte"><div class="m-qr ' + (p(local, 'qrCode') ? 'oui' : 'non') + '" style="width:52px;height:52px;font-size:1.6rem;border-radius:14px" role="button" aria-label="QR code du local"><i class="iconoir-qr-code"></i></div>' +
            '<div class="min-w-0"><h6>' + esc(p(local, 'designation')) + '</h6><div class="m-sous">' + esc(p(local, 'code') || '') + (organe ? ' · ' + esc(p(organe, 'nom')) : '') + '</div>' +
            '<div class="d-flex flex-wrap gap-1 mt-2">' + puceEtape(cpt) + '</div>' +
            (p(local, 'qrCode') ? '' : '<div class="mt-1"><span class="m-badge m-badge-att"><i class="iconoir-warning-triangle"></i>sans QR code : touchez l\'icône pour en affecter un</span></div>') + '</div></div></div>' +
            (etape.cle === 'cloturer' ? '<div class="m-card m-carte-cloture"><div class="m-entete-carte"><span class="ico-gros ico-bleu"><i class="iconoir-eye"></i></span>' +
                '<div class="min-w-0"><h6>Inventaire à clôturer</h6><div class="m-sous">' + nombre(cpt.identifies) + ' bien(s) identifié(s), ' + nombre(cpt.inventories) + ' inventorié(s) pour l\'inventaire ' + esc(etat.annee) + '.</div></div></div>' +
                '<button type="button" class="m-btn m-btn-primaire m-btn-bloc mt-3" id="btnCloturerLocal"><i class="iconoir-check-circle"></i>Clôturer l\'inventaire du local</button></div>' : '') +
            '<div class="m-seg"><button type="button" id="segArticles"' + (mode === 'articles' ? ' class="actif"' : '') + '><i class="iconoir-view-grid"></i>Par article</button><button type="button" id="segDetails"' + (mode !== 'articles' ? ' class="actif"' : '') + '><i class="iconoir-list"></i>En détails</button></div>' +
            '<div id="zoneContenu">' + squelette(4) + '</div>';
        $('#segArticles').onclick = function () { aller('#/local/' + id + '?mode=articles'); };
        $('#segDetails').onclick = function () { aller('#/local/' + id + '?mode=details'); };
        appuiLong($('.m-entete-carte .m-qr'), function () { changerQrLocal(local, null); }, function () { if (!p(local, 'qrCode')) { affecterQrLocal(local, null); } });
        if ($('#btnCloturerLocal')) {
            $('#btnCloturerLocal').onclick = async function () {
                var bouton = this;
                bouton.disabled = true;
                try {
                    var res = await cloturerLocal(local);
                    // Envoyée : la page est rechargée (compteurs, couleurs des biens) ; hors ligne, elle reste en file d'attente.
                    if (res && res.ok && !res.attente) { await rendre(); return; }
                } catch (e) { if (!e.session) { toast(e.message, 'danger'); } }
                if (bouton.isConnected) { bouton.disabled = false; }
            };
        }

        setActions([
            { icone: 'iconoir-scan-qr-code', libelle: 'Inventorier', action: function () { lancerRapideLocal(local); }, principal: true },
            { icone: 'iconoir-check-circle', libelle: 'Identifier un bien', court: 'Identifier', action: function () { identifierDepuisLocal(local); } },
            { icone: 'iconoir-qr-code', libelle: 'Scanner un bien', court: 'Scanner', action: scanBien },
            { icone: 'iconoir-download', libelle: 'Déplacer un bien ici', court: 'Déplacer ici', action: function () { deplacerBienIci(local); } }
        ]);

        var zone = $('#zoneContenu');
        if (mode === 'articles') {
            var ra = await api('/article/local/' + id);
            var articles = Array.isArray(ra.content) ? ra.content : [];
            if (!articles.length) { zone.innerHTML = vide('iconoir-box-iso', 'Aucun bien dans ce local', 'Scannez un bien pour le déplacer ici.'); return; }
            zone.innerHTML = '<div class="m-collant">' + recherche('filtreArt', 'Rechercher un article') + '</div>' +
                '<div class="m-section"><h2>Articles</h2><span>' + articles.length + ' · touchez pour voir les biens</span></div><div class="m-liste grille" id="listeArt"></div><div id="videArt" hidden>' + vide('iconoir-search', 'Aucun article trouvé', '') + '</div>';
            var la = $('#listeArt');
            articles.forEach(function (a) {
                var it = el('<div class="m-item m-cliquable"><span class="m-vignette"><i class="iconoir-box-iso"></i>' +
                    (p(a, 'photo') ? '<img src="/Content/images/articles/' + esc(p(a, 'photo')) + '" alt="" onerror="this.remove()">' : '') + '</span>' +
                    '<div class="corps"><div class="titre">' + esc(p(a, 'designation')) + '</div><div class="detail">' + esc([p(a, 'marque'), p(a, 'modele')].filter(Boolean).join(' ') || p(a, 'code') || '') + '</div></div>' +
                    '<span class="m-compte">' + nombre(p(a, 'nbreImmo')) + '</span><i class="iconoir-nav-arrow-right fleche"></i></div>');
                it.dataset.recherche = minuscule([p(a, 'designation'), p(a, 'marque'), p(a, 'modele'), p(a, 'code')].filter(Boolean).join(' '));
                it.onclick = function () { aller('#/local/' + id + '/article/' + p(a, 'id')); };
                la.appendChild(it);
            });
            brancherFiltre($('#filtreArt'), la, '.m-item', $('#videArt'));
        } else {
            var rb = await api(idArticle ? '/immo/local/' + id + '/article/' + idArticle : '/immo/local/' + id);
            var biens = Array.isArray(rb.content) ? rb.content : [];
            rendreListeBiens(zone, biens, idArticle ? '<a class="m-lien mb-1" href="#/local/' + id + '?mode=articles"><i class="iconoir-nav-arrow-left"></i>Tous les articles</a>' : '', { declasse: localDeclasse(local) });
        }
    }

    function lancerRapideLocal(local) {
        if (!p(local, 'qrCode')) {
            toast('Ce local n\'a pas de QR code : les biens pourront être identifiés, mais pas déplacés ici. Affectez-lui une étiquette si besoin.', 'warning');
        }
        aller('#/rapide/' + p(local, 'id'));
    }

    /* Clôture de l'inventaire d'un local : POST /inventaire/details/local/ { IdLocal }.
       Le serveur enregistre comme inventoriés les biens actifs du local, étiquetés et identifiés, pas encore inventoriés cette année.
       Sans réseau, la clôture part dans la file hors ligne (après les identifications déjà en file). Résout avec { ok, attente, message } ou null si annulé. */
    async function cloturerLocal(local) {
        var id = p(local, 'id'), nom = p(local, 'designation') || ('local ' + id);
        var ok = await confirmer('Clôturer l\'inventaire du local', '<strong>' + esc(nom) + '</strong><br>' +
            'Les biens identifiés et étiquetés de ce local seront enregistrés comme inventoriés pour l\'inventaire ' + esc(etat.annee) + '. ' +
            'Les biens non identifiés ne sont pas concernés : déclarez-les non vus.', 'Clôturer', 'Annuler', 'm-btn-vert');
        if (!ok) { return null; }
        return envoyerOuMettreEnAttente('/inventaire/details/local/', { IdLocal: id }, 'Clôture du local ' + nom);
    }

    /* Identification (le bien est marqué « vu » avec son état et une observation), via POST /inventaire/identifier/. */
    function sheetIdentifier(bien, apres) {
        return new Promise(function (resolve) {
            var corps = ouvrirSheet('Identifier « ' + (p(bien, 'designation') || 'ce bien') + ' »',
                '<form id="frmIdent"><div class="mb-3"><span class="m-libelle">État constaté</span><div class="m-etat">' +
                '<label><input type="radio" name="LastEtat" value="B"' + (p(bien, 'lastEtat') !== 'M' ? ' checked' : '') + '><span class="B"><i class="iconoir-check-circle"></i>Bon état</span></label>' +
                '<label><input type="radio" name="LastEtat" value="M"' + (p(bien, 'lastEtat') === 'M' ? ' checked' : '') + '><span class="M"><i class="iconoir-warning-triangle"></i>Mauvais état</span></label></div></div>' +
                '<div class="mb-3"><label class="m-libelle" for="idObs">Observation</label><select class="form-select" id="idObs" name="IdLastObservation" required><option value="">Chargement...</option></select></div>' +
                '<button type="submit" class="m-btn m-btn-vert m-btn-bloc"><i class="iconoir-check-circle"></i>Marquer comme vu</button></form>');
            var sel = $('#idObs', corps);
            var chargerObs = async function (etatB) {
                sel.innerHTML = '<option value="">Chargement...</option>';
                try {
                    var obs = await observations(etatB);
                    sel.innerHTML = '<option value="">Choisissez une observation</option>' + obs.map(function (o) {
                        return '<option value="' + esc(o.id) + '"' + (String(o.id) === String(p(bien, 'idLastObservation')) ? ' selected' : '') + '>' + esc(o.libelle) + '</option>';
                    }).join('');
                } catch (e) { sel.innerHTML = '<option value="">Observations indisponibles</option>'; toast(e.message, 'danger'); }
            };
            $$('input[name=LastEtat]', corps).forEach(function (r) { r.onchange = function () { chargerObs(r.value); }; });
            chargerObs(p(bien, 'lastEtat') === 'M' ? 'M' : 'B');
            $('#frmIdent', corps).onsubmit = async function (e) {
                e.preventDefault();
                var fd = new FormData(e.target);
                if (!fd.get('IdLastObservation')) { toast('Choisissez une observation.', 'warning'); return; }
                fermerSheet();
                // Sans réseau, l'identification est mise en file (mode hors ligne) et envoyée plus tard.
                var res = await envoyerOuMettreEnAttente('/inventaire/identifier/', { IdImmo: p(bien, 'id'), LastEtat: fd.get('LastEtat'), IdLastObservation: fd.get('IdLastObservation') },
                    'Identification : ' + (p(bien, 'designation') || ('bien ' + p(bien, 'id'))));
                if (res.ok) { marquerVuLocalement(bien); } // le bien passe aussitôt en bleu (identifié, à clôturer)
                if (res.ok && !res.attente && apres) { apres(); }
                resolve(res.ok);
            };
            $('#sheet').addEventListener('hidden.bs.offcanvas', function h() { $('#sheet').removeEventListener('hidden.bs.offcanvas', h); resolve(false); });
        });
    }

    async function identifierDepuisLocal(local) {
        try {
            var bien = await trouverBienParQr('Identifier un bien', 'Scannez l\'étiquette du bien à identifier.');
            if (!bien) { return; }
            var idLocalBien = p(bien, 'idLocal');
            if (String(idLocalBien) !== String(p(local, 'id'))) {
                var ancien = p(p(bien, 'local'), 'designation') || (await nomLocal(idLocalBien)) || 'un autre local';
                var deplacer = await confirmer('Bien hors de ce local', '<strong>' + esc(p(bien, 'designation')) + '</strong> est enregistré dans <strong>' + esc(ancien) + '</strong>.<br>Voulez-vous le déplacer ?', 'Déplacer', 'Non');
                if (deplacer) {
                    var codeLocal = await scanner('Local de destination', 'Scannez le QR code du local dans lequel se trouve le bien.');
                    if (codeLocal) {
                        if (!GUID.test(codeLocal)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); }
                        else { await action('/immo/changelocal/', { IdImmo: p(bien, 'id'), QrCodeLocal: codeLocal }); }
                    }
                }
            }
            await sheetIdentifier(bien, rendre);
        } catch (e) { toast(e.message, 'danger'); }
    }

    async function deplacerBienIci(local) {
        if (!p(local, 'qrCode')) { toast('Ce local n\'a pas de QR code : affectez-lui d\'abord une étiquette.', 'warning'); return; }
        try {
            var bien = await trouverBienParQr('Déplacer un bien ici', 'Scannez l\'étiquette du bien à placer dans « ' + p(local, 'designation') + ' ».');
            if (!bien) { return; }
            if (String(p(bien, 'idLocal')) === String(p(local, 'id'))) { toast('Ce bien est déjà dans ce local.', 'info'); return; }
            var ancien = p(p(bien, 'local'), 'designation') || (await nomLocal(p(bien, 'idLocal'))) || 'son local actuel';
            var ok = await confirmer('Déplacer le bien', 'Déplacer <strong>' + esc(p(bien, 'designation')) + '</strong> de <strong>' + esc(ancien) + '</strong> vers <strong>' + esc(p(local, 'designation')) + '</strong> ?');
            if (!ok) { return; }
            if (await action('/immo/changelocal/', { IdImmo: p(bien, 'id'), QrCodeLocal: p(local, 'qrCode') })) { rendre(); }
        } catch (e) { toast(e.message, 'danger'); }
    }

    /* Local de type « Déclassé » (IdTypeLocal = 4) : ses biens peuvent être cédés. */
    var TYPE_LOCAL_DECLASSE = 4;
    function localDeclasse(local) { return !!local && Number(p(local, 'idTypeLocal')) === TYPE_LOCAL_DECLASSE; }

    /* ------------------------------------------------------------------ page : fiche d'un bien
       depuisDeclasses : fiche ouverte depuis la page Déclassés (sert quand le type du local est inconnu). */
    async function pageBien(id, depuisDeclasses) {
        entete('Bien', true, 'organes');
        afficherChargement();
        var r = await api('/immo/id/' + id);
        var b = Array.isArray(r.content) ? r.content[0] : r.content;
        if (!b) { afficherErreur('Bien introuvable.'); return; }
        var local = p(b, 'idLocal') ? await chargerLocal(p(b, 'idLocal')).catch(function () { return null; }) : null;
        await chargerArbre();
        var organe = local ? organeParId(p(local, 'codeOrgane')) : null;
        var photos = Array.isArray(p(b, 'immoPhotos')) ? p(b, 'immoPhotos') : [];
        var principale = '/Content/images/equipements/' + (p(b, 'photo') || 'defaultImmo.jpg');
        var statut = statutBien(b), etatB = p(b, 'lastEtat') || '';
        var typeLocal = local ? p(local, 'idTypeLocal') : null;
        var declasse = typeLocal != null && typeLocal !== '' ? localDeclasse(local) : !!depuisDeclasses;
        // En-tête : où l'on est (local et organe) ; le nom du bien est déjà en tête de la fiche.
        entete(local ? String(p(local, 'designation') || 'Local').trim() : 'Bien sans local', true, 'organes',
            organe ? String(p(organe, 'nom') || '').trim() : (local ? String(p(local, 'codeOrgane') || '') : ''));

        function prop(k, v, large) { return v == null || v === '' ? '' : '<div' + (large ? ' class="large"' : '') + '><div class="k">' + k + '</div><div class="v">' + v + '</div></div>'; }
        $('#page').innerHTML =
            '<div class="m-bien-grille"><div>' +
            '<img class="m-photo-principale" src="' + principale + '" alt="" onerror="this.onerror=null;this.src=\'/Content/images/equipements/defaultImmo.jpg\'">' +
            // Galerie : photos de constat autres que la photo principale (une photo ajoutée depuis le terrain y apparaît).
            (photos.filter(function (ph) { return String(p(ph, 'id')) + '.jpg' !== String(p(b, 'photo')); }).length ? '<div class="m-galerie">' + photos.map(function (ph) { return '<img src="/Content/images/equipements/' + esc(p(ph, 'id')) + '.jpg" alt="' + esc(p(ph, 'constat') || '') + '" title="' + esc(p(ph, 'constat') || '') + '">'; }).join('') + '</div>' : '') +
            '<div style="height:12px"></div></div><div>' +
            '<div class="m-card' + (statut === 'inv' ? ' m-vu' : statut === 'vu' ? ' m-ident' : '') + '"><h6>' + esc(p(b, 'designation')) + '</h6><div class="m-sous">' + esc(p(b, 'codeADM') || '') + (p(b, 'codeImmo') ? ' · ' + esc(p(b, 'codeImmo')) : '') + '</div>' +
            '<div class="d-flex flex-wrap gap-1 mt-2"><span class="m-badge m-badge-etat-' + esc(etatB) + '">' + esc(libelleEtat(b)) + '</span>' +
            badgeStatutBien(b, true) +
            (p(b, 'qrCode') ? '<span class="m-badge m-badge-ident"><i class="iconoir-qr-code"></i>QR code</span>' : '<span class="m-badge m-badge-att">sans QR code</span>') + '</div></div>' +
            '<div class="m-props">' +
            prop('Local', local ? esc(p(local, 'designation')) + (organe ? '<div class="m-sous">' + esc(p(organe, 'nom')) + '</div>' : '') : '<span class="text-warning">Aucun local</span>', true) +
            prop('Dernière observation', esc(p(b, 'lastObservation'))) +
            prop('Responsable', esc(p(b, 'responsable'))) +
            prop('Mise en service', esc(fmtDate(p(b, 'dateMisEnService')))) +
            prop('Inventorié', statut === 'inv' ? 'Oui' + (fmtDate(p(b, 'dateInventaire')) ? ' · ' + esc(fmtDate(p(b, 'dateInventaire'))) : '') + (p(b, 'inventorieur') ? ' · ' + esc(p(b, 'inventorieur')) : '') : (statut === 'vu' ? 'Non · local à clôturer' : 'Non')) +
            prop('Année comptable', p(b, 'lastAnneeComptable') ? esc(p(b, 'lastAnneeComptable')) : null) +
            prop('Bien principal', p(b, 'idImmoParent') ? esc(p(b, 'designationImmoPrincipal')) : null, true) +
            prop('Observation', esc(p(b, 'observation')), true) +
            '</div>' +
            (p(b, 'qrCode') ? carteEtiquette(b) : '') +
            actionsFiche(b, local, declasse) +
            '</div></div>';

        var deplacer = function () { executerActionBien('deplacer', b); };
        $$('[data-action-bien]').forEach(function (bt) { bt.onclick = function () { executerActionBien(bt.dataset.actionBien, b); }; });
        setActions([
            { icone: 'iconoir-check-circle', libelle: statut ? 'Identifier à nouveau' : 'Identifier ce bien', action: function () { executerActionBien('identifier', b); }, principal: true },
            { icone: 'iconoir-arrow-right', libelle: 'Déplacer ce bien', court: 'Déplacer', action: deplacer },
            { icone: 'iconoir-qr-code', libelle: 'Scanner un autre bien', court: 'Autre bien', action: scanBien }
        ]);
    }

    /* Carte « Étiquette QR » de la fiche : image du QR code (dessinée localement, sans bibliothèque) et GUID. */
    function carteEtiquette(b) {
        var guid = String(p(b, 'qrCode'));
        var svg = '';
        try { svg = qrSvg(guid.toLowerCase()); } catch (e) { svg = ''; }
        return '<div class="m-card m-etiquette">' +
            '<div class="m-etiquette-qr">' + (svg || '<i class="iconoir-qr-code"></i>') + '</div>' +
            '<div class="min-w-0"><div class="m-etiquette-titre"><i class="iconoir-qr-code"></i>Étiquette QR</div>' +
            '<div class="m-guid">' + esc(guid) + '</div>' +
            '<div class="m-sous">' + esc([p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ')) + '</div></div></div>';
    }

    /* Actions de la fiche, regroupées par thème (les opérations rares ou définitives en dernier).
       declasse : le bien est dans le local des déclassés, on propose sa cession au lieu du déclassement. */
    function actionsFiche(b, local, declasse) {
        var bouton = function (id, icone, couleur, titre, sous, classe) {
            return '<button type="button" class="m-action' + (classe ? ' ' + classe : '') + '" data-action-bien="' + id + '"><span class="ico ' + couleur + '"><i class="' + icone + '"></i></span>' +
                '<span class="txt">' + esc(titre) + (sous ? '<small>' + esc(sous) + '</small>' : '') + '</span><i class="iconoir-nav-arrow-right fleche"></i></button>';
        };
        var vu = bienVu(b);
        return '<div class="m-section"><h2>Inventaire</h2></div><div class="m-actions">' +
            bouton('identifier', 'iconoir-check-circle', 'ico-vert', vu ? 'Identifier à nouveau' : 'Identifier ce bien', 'Le marquer comme vu avec son état') +
            bouton('nonvu', 'iconoir-eye-closed', 'ico-rouge', 'Déclarer non vu', 'Le bien est introuvable') +
            '</div><div class="m-section"><h2>Mouvements</h2></div><div class="m-actions">' +
            bouton('deplacer', 'iconoir-arrow-right', 'ico-violet', 'Déplacer ce bien', 'Scanner le QR code du local de destination') +
            bouton('expedier', 'iconoir-send', 'ico-cyan', 'Expédier vers un autre organe', 'Le bien passe en transit') +
            bouton('service', 'iconoir-log-in', 'ico-vert', 'Mettre en service', 'Réceptionner le bien dans un local') +
            (local ? bouton('local', 'iconoir-home-alt', 'ico-bleu', 'Voir le local', p(local, 'designation') || '') : '') +
            '</div><div class="m-section"><h2>Étiquette et photos</h2></div><div class="m-actions">' +
            (p(b, 'qrCode')
                ? bouton('detacherqr', 'iconoir-link-xmark', 'ico-orange', 'Détacher le QR code', 'Pour le réaffecter ou le remplacer')
                : bouton('affecterqr', 'iconoir-scan-qr-code', 'ico-orange', 'Affecter un QR code', 'Coller et scanner une étiquette')) +
            bouton('photo', 'iconoir-camera', 'ico-rose', 'Ajouter une photo', 'Photographier le bien avec un constat') +
            '</div><div class="m-section"><h2>Sortie du patrimoine</h2></div><div class="m-actions">' +
            (declasse
                ? bouton('ceder', 'iconoir-log-out', 'ico-rouge', 'Céder le bien', 'Sortie définitive du patrimoine, double confirmation', 'm-action-discret')
                : bouton('declasser', 'iconoir-archive', 'ico-gris', 'Déclasser le bien', 'Le ranger dans le local des déclassés de l\'entité', 'm-action-discret')) +
            '</div>';
    }

    /* ==================================================================
       ACTIONS SUR UN BIEN (fiche, bouton ⋮ des listes, tuiles « Mouvements » de l'accueil)
       Chaque action renvoie vrai si le bien a changé côté serveur (la page est alors rechargée).
       ================================================================== */
    var ACTIONS_BIEN = {
        identifier: function (b) { return sheetIdentifier(b, rendre).then(function () { return false; }); }, // sheetIdentifier recharge elle-même
        nonvu: function (b) { return declarerNonVu(b); },
        deplacer: function (b) { return deplacerBien(b); },
        expedier: function (b) { return expedierBien(b); },
        service: function (b) { return mettreEnService(b); },
        local: function (b) { aller('#/local/' + p(b, 'idLocal') + '?mode=details'); return Promise.resolve(false); },
        affecterqr: function (b) { return affecterQrBien(b); },
        detacherqr: function (b) { return detacherQrBien(b); },
        photo: function (b) { return ajouterPhoto(b); },
        declasser: function (b) { return declasserBien(b); },
        ceder: async function (b) {
            if (!(await cederBien(b))) { return false; }
            // Depuis la fiche, le bien cédé est sorti du patrimoine : on revient à la liste des déclassés.
            if (/^#\/bien\//.test(location.hash)) { location.replace('#/declasses'); return false; }
            return true;
        },
        fiche: function (b) { aller('#/bien/' + p(b, 'id')); return Promise.resolve(false); }
    };
    async function executerActionBien(id, b) {
        var f = ACTIONS_BIEN[id];
        if (!f) { return; }
        try {
            await sheetFermee();
            if (await f(b)) { rendre(); }
        } catch (e) { if (!e.session) { toast(e.message, 'danger'); } }
    }

    /* Feuille d'actions d'un bien (bouton ⋮ d'une ligne) : mêmes actions que la fiche, sans l'ouvrir.
       declasse : le bien est dans un local des déclassés (cession proposée au lieu du déclassement). */
    async function menuBien(b, depuisFiche, declasse) {
        var qr = !!p(b, 'qrCode');
        var items = [
            { section: 'Inventaire' },
            { id: 'identifier', icone: 'iconoir-check-circle', libelle: bienVu(b) ? 'Identifier à nouveau' : 'Identifier (marquer vu)' },
            { id: 'nonvu', icone: 'iconoir-eye-closed', libelle: 'Déclarer non vu' },
            { section: 'Mouvements' },
            { id: 'deplacer', icone: 'iconoir-arrow-right', libelle: 'Déplacer vers un autre local' },
            { id: 'expedier', icone: 'iconoir-send', libelle: 'Expédier vers un autre organe' },
            { id: 'service', icone: 'iconoir-log-in', libelle: 'Mettre en service (réceptionner)' }
        ];
        if (p(b, 'idLocal')) { items.push({ id: 'local', icone: 'iconoir-home-alt', libelle: 'Voir le local' }); }
        items.push(
            { section: 'Étiquette et photos' },
            qr ? { id: 'detacherqr', icone: 'iconoir-link-xmark', libelle: 'Détacher le QR code' } : { id: 'affecterqr', icone: 'iconoir-scan-qr-code', libelle: 'Affecter un QR code' },
            { id: 'photo', icone: 'iconoir-camera', libelle: 'Ajouter une photo' });
        if (!depuisFiche) { items.push({ id: 'fiche', icone: 'iconoir-page', libelle: 'Voir la fiche du bien' }); }
        items.push({ section: 'Sortie du patrimoine' }, declasse
            ? { id: 'ceder', icone: 'iconoir-log-out', libelle: 'Céder ce bien (sortie définitive)…', classe: 'danger' }
            : { id: 'declasser', icone: 'iconoir-archive', libelle: 'Déclasser le bien…', classe: 'neutre' });
        var intro = '<div class="m-sous">' + esc([p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ')) + '</div>';
        var choix = await choisir(p(b, 'designation') || ('Bien ' + p(b, 'id')), items, intro);
        if (choix) { await executerActionBien(choix, b); }
    }

    /* Raccourcis de l'accueil : scanner un bien puis enchaîner l'action. */
    async function scannerPuis(titre, f) {
        try {
            var b = await trouverBienParQr(titre, 'Scannez l\'étiquette QR du bien.');
            if (!b) { return; }
            await f(b);
        } catch (e) { if (!e.session) { toast(e.message, 'danger'); } }
    }
    /* Page Déclassés : scanner un bien, le déclasser puis recharger la liste. */
    function declasserPuisRecharger() {
        return scannerPuis('Déclasser un bien', async function (b) { if (await declasserBien(b)) { rendre(); } });
    }

    function nomBien(b) { return p(b, 'designation') || ('Bien ' + p(b, 'id')); }

    /* Déclarer un bien non vu (introuvable) : POST /immo/nonvu/{id} — le serveur range le bien dans le local
       « non vu » de l'organe d'affectation (il apparaît dans la page Non vu) ; mis en file sans réseau. */
    async function declarerNonVu(b) {
        if (!(await confirmer('Bien non vu', 'Confirmez-vous définitivement que le bien est non vu ?<br><strong>' + esc(nomBien(b)) + '</strong><br>Il sera placé dans le local « non vu » de votre organe.', 'Oui, déclarer', 'Non', 'm-btn-rouge'))) { return false; }
        var res = await envoyerOuMettreEnAttente('/immo/nonvu/' + p(b, 'id'), {}, 'Non vu : ' + nomBien(b));
        return res.ok && !res.attente;
    }

    /* Déplacer le bien dans un autre local (scanner le QR du local) : POST /immo/changelocal/. */
    async function deplacerBien(b) {
        var codeLocal = await scanner('Local de destination', 'Scannez le QR code du local où placer « ' + nomBien(b) + ' ».');
        if (!codeLocal) { return false; }
        if (!GUID.test(codeLocal)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return false; }
        return action('/immo/changelocal/', { IdImmo: p(b, 'id'), QrCodeLocal: codeLocal });
    }

    /* Liste à plat des organes (la réponse peut être imbriquée : on descend dans « organes »). */
    function aplatirOrganes(liste) {
        var res = [];
        (function parcourir(l, niveau) {
            (l || []).forEach(function (o) { res.push({ organe: o, niveau: niveau }); parcourir(p(o, 'organes'), niveau + 1); });
        })(liste, 0);
        return res;
    }
    /* Choix d'un organe dans une feuille avec recherche ; résout avec l'organe ou null. */
    async function choisirOrgane(titre, plat) {
        var corps = await ouvrirFeuille(titre,
            '<div class="mb-2">' + recherche('coFiltre', 'Rechercher un organe (nom, sigle, code)') + '</div>' +
            '<div class="m-choix m-choix-defilant" id="coListe"></div><div id="coVide" hidden>' + vide('iconoir-search', 'Aucun organe trouvé', 'Essayez un autre nom, sigle ou code.') + '</div>');
        return new Promise(function (resolve) {
            var fini = false;
            var fin = function (v) { if (fini) { return; } fini = true; resolve(v); };
            var liste = $('#coListe', corps);
            plat.forEach(function (x) {
                var o = x.organe, oid = p(o, 'id');
                var bt = el('<button type="button" style="padding-left:' + (0.9 + Math.min(x.niveau, 4) * 0.9) + 'rem"><i class="iconoir-city"></i><span>' + esc(p(o, 'nom') || oid) +
                    '<br><small class="m-sous">' + esc(oid) + (p(o, 'sigle') ? ' · ' + esc(p(o, 'sigle')) : '') + '</small></span></button>');
                bt.dataset.recherche = minuscule((p(o, 'nom') || '') + ' ' + (p(o, 'sigle') || '') + ' ' + oid);
                bt.onclick = function () { fin(o); fermerSheet(); };
                liste.appendChild(bt);
            });
            brancherFiltre($('#coFiltre', corps), liste, 'button', $('#coVide', corps));
            surFermetureSheet(function () { fin(null); });
        });
    }

    /* Expédier le bien vers un autre organe (il part dans le local de transit du destinataire) : POST /immo/expedier/ { IdImmo, CodeOrgane }. */
    async function expedierBien(b) {
        var plat = [];
        try {
            var r = await api('/organe/entite/');
            plat = aplatirOrganes(Array.isArray(r.content) ? r.content : []);
        } catch (e) { if (e.session) { return false; } plat = []; }
        if (!plat.length) {
            // Liste des entités vide ou indisponible : organigramme déjà chargé par l'application.
            try { await chargerArbre(); } catch (e2) { toast(e2.message, 'danger'); return false; }
            plat = etat.plat || [];
        }
        if (!plat.length) { toast('Aucun organe destinataire disponible.', 'warning'); return false; }
        var destination = await choisirOrgane('Organe destinataire', plat);
        if (!destination) { return false; }
        var ok = await confirmer('Expédier le bien', 'Expédier <strong>' + esc(nomBien(b)) + '</strong> vers <strong>' + esc(p(destination, 'nom') || p(destination, 'id')) + '</strong> ?<br>' +
            'Il sera placé dans le local de transit de cet organe en attendant sa mise en service.', 'Expédier', 'Annuler');
        if (!ok) { return false; }
        return action('/immo/expedier/', { IdImmo: p(b, 'id'), CodeOrgane: p(destination, 'id') });
    }

    /* Mise en service (réception) : scanner le QR du local d'affectation, POST /immo/misenservice/ { IdImmo, QrCodeLocal }. */
    async function mettreEnService(b) {
        var codeLocal = await scanner('Mise en service', 'Scannez le QR code du local où « ' + nomBien(b) + ' » est mis en service.');
        if (!codeLocal) { return false; }
        if (!GUID.test(codeLocal)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return false; }
        return action('/immo/misenservice/', { IdImmo: p(b, 'id'), QrCodeLocal: codeLocal });
    }

    /* Affecter une étiquette QR au bien : POST /immo/qrcode/ { Id, QrCode }. */
    async function affecterQrBien(b) {
        var code = await scanner('Affecter un QR code au bien', 'Scannez l\'étiquette QR collée sur « ' + nomBien(b) + ' ».');
        if (!code) { return false; }
        if (!GUID.test(code)) { toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return false; }
        return action('/immo/qrcode/', { Id: p(b, 'id'), QrCode: code });
    }

    /* Détacher le QR code : le réaffecter ailleurs (GET /immo/resetqrcode/{id}) ou le jeter (GET /immo/liveqrcode/{id}),
       puis proposer de scanner une nouvelle étiquette. */
    async function detacherQrBien(b) {
        var choix = await choisir('Détacher le QR code', [
            { id: 'reset', icone: 'iconoir-refresh-double', libelle: 'Le réaffecter à un autre bien', sous: 'L\'étiquette reste utilisable' },
            { id: 'live', icone: 'iconoir-trash', libelle: 'Le jeter (abîmé ou perdu)', sous: 'L\'étiquette ne servira plus', classe: 'danger' }
        ], 'Que devient l\'étiquette actuelle de <b>' + esc(nomBien(b)) + '</b> ?');
        if (!choix) { return false; }
        if (!(await action((choix === 'reset' ? '/immo/resetqrcode/' : '/immo/liveqrcode/') + encodeURIComponent(p(b, 'id'))))) { return false; }
        if (await confirmer('QR code détaché', 'Coller et scanner une nouvelle étiquette maintenant ?', 'Scanner', 'Plus tard')) {
            var copie = Object.assign({}, b); copie.qrCode = null; copie.QrCode = null;
            await affecterQrBien(copie);
        }
        return true;
    }

    /* Déclasser le bien : POST /immo/declasser/{id} (formulaire vide). Le serveur range le bien dans le local
       « Déclassé » de l'entité de l'utilisateur ; le bien reste actif et pourra ensuite être cédé (page Déclassés). */
    async function declasserBien(b) {
        if (!(await confirmer('Déclasser le bien', 'Déclasser «&nbsp;<strong>' + esc(nomBien(b)) + '</strong>&nbsp;»&nbsp;? Il sera rangé dans le local des déclassés de votre entité ; il pourra ensuite être cédé.', 'Déclasser', 'Annuler'))) { return false; }
        return action('/immo/declasser/' + encodeURIComponent(p(b, 'id')), {});
    }

    /* Confirmation forte d'une cession : confirmation puis saisie du mot CEDER ; résout avec vrai si confirmé. */
    async function confirmerCession(titre, texte, libelleBouton) {
        if (!(await confirmer(titre, texte + '<br>Cette opération est définitive.', 'Continuer', 'Annuler', 'm-btn-rouge'))) { return false; }
        var saisie = await demanderTexte({
            titre: 'Confirmer la cession', texte: 'Pour confirmer, tapez <b>CEDER</b>.',
            placeholder: 'CEDER', maxlength: 10, majuscules: true, bouton: libelleBouton, icone: 'iconoir-log-out', classe: 'm-btn-rouge'
        });
        if (saisie === null) { return false; }
        // « CÉDER » (avec accent) est aussi accepté.
        if (saisie.trim().toUpperCase().replace('É', 'E') !== 'CEDER') { toast('Cession annulée : confirmation incorrecte.', 'warning'); return false; }
        return true;
    }

    /* Céder le bien (sortie définitive du patrimoine) : POST /immo/cession/{id}.
       Le serveur exige que le bien ait un QR code. */
    async function cederBien(b) {
        if (!p(b, 'qrCode')) {
            var affecter = await confirmer('Cession impossible', 'Seuls les biens étiquetés peuvent être cédés : affectez d\'abord un QR code à <strong>' + esc(nomBien(b)) + '</strong>.', 'Affecter un QR code', 'Fermer');
            return affecter ? affecterQrBien(b) : false;
        }
        var codes = [p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ');
        if (!(await confirmerCession('Céder le bien', '<strong>' + esc(nomBien(b)) + '</strong>' + (codes ? ' (' + esc(codes) + ')' : '') + ' va sortir définitivement du patrimoine.', 'Céder définitivement'))) { return false; }
        return action('/immo/cession/' + encodeURIComponent(p(b, 'id')), {});
    }

    /* Céder tous les biens du local des déclassés de l'entité : POST /immo/local/cession/ (formulaire vide). */
    async function cederTousLesBiens(nb) {
        var texte = nb > 1
            ? 'Les <strong>' + nombre(nb) + ' biens</strong> du local des déclassés de votre entité vont sortir définitivement du patrimoine.'
            : 'Le bien du local des déclassés de votre entité va sortir définitivement du patrimoine.';
        if (!(await confirmerCession(nb > 1 ? 'Céder tous les biens' : 'Céder le bien', texte, nb > 1 ? 'Céder les ' + nombre(nb) + ' biens' : 'Céder le bien'))) { return false; }
        return action('/immo/local/cession/', {});
    }

    /* Feuille ⋮ d'une ligne de la page Déclassés : cession ou fiche, rien d'autre. */
    async function menuDeclasse(b) {
        var intro = '<div class="m-sous">' + esc([p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ')) + '</div>';
        var choix = await choisir(nomBien(b), [
            { id: 'ceder', icone: 'iconoir-log-out', libelle: 'Céder ce bien (sortie définitive)', classe: 'danger' },
            { id: 'fiche', icone: 'iconoir-page', libelle: 'Voir la fiche du bien' }
        ], intro);
        if (choix === 'fiche') { aller('#/bien/' + p(b, 'id') + '?depuis=declasses'); }
        else if (choix === 'ceder') { await executerActionBien('ceder', b); }
    }

    /* Réduit la photo (côté le plus long : 1600 px, JPEG) avant l'envoi ; renvoie le fichier d'origine en cas d'échec. */
    async function preparerPhoto(fichier) {
        try {
            var image = null;
            if (window.createImageBitmap) {
                try { image = await createImageBitmap(fichier, { imageOrientation: 'from-image' }); } catch (e) { image = null; }
            }
            if (!image) {
                image = await new Promise(function (res, rej) {
                    var url = URL.createObjectURL(fichier), img = new Image();
                    img.onload = function () { URL.revokeObjectURL(url); res(img); };
                    img.onerror = function () { URL.revokeObjectURL(url); rej(new Error('image illisible')); };
                    img.src = url;
                });
            }
            var l = image.width, h = image.height, max = 1600;
            if (!l || !h) { return fichier; }
            var echelle = Math.min(1, max / Math.max(l, h));
            if (echelle === 1 && fichier.type === 'image/jpeg' && fichier.size < 1500000) { return fichier; }
            var c = document.createElement('canvas');
            c.width = Math.round(l * echelle); c.height = Math.round(h * echelle);
            c.getContext('2d').drawImage(image, 0, 0, c.width, c.height);
            var blob = await new Promise(function (res) { c.toBlob(res, 'image/jpeg', 0.85); });
            return blob || fichier;
        } catch (e) { return fichier; }
    }

    /* Ajouter une photo (constat) : POST multipart /immo/add/photo { IdImmo, Constat, file }. */
    async function ajouterPhoto(b) {
        var corps = await ouvrirFeuille('Ajouter une photo',
            '<div class="m-feuille-intro">' + esc(nomBien(b)) + '</div>' +
            '<form id="frmPhoto" autocomplete="off" novalidate>' +
            '<label class="m-photo-zone" id="phZone"><input type="file" id="phFichier" accept="image/*" capture="environment">' +
            '<span class="vide"><i class="iconoir-camera"></i><b>Prendre la photo</b><small>ou choisir une image de l\'appareil</small></span><img alt="" hidden></label>' +
            '<div class="mt-3"><label class="m-libelle" for="phConstat">Constat (facultatif)</label>' +
            '<input type="text" class="form-control" id="phConstat" maxlength="120" placeholder="Ex. vue de face, plaque signalétique"></div>' +
            '<button type="submit" class="m-btn m-btn-primaire m-btn-bloc mt-3" id="phEnvoyer" disabled><i class="iconoir-upload"></i>Envoyer la photo</button></form>');
        return new Promise(function (resolve) {
            var fini = false, fichier = null, apercu = null;
            var fin = function (v) { if (fini) { return; } fini = true; if (apercu) { URL.revokeObjectURL(apercu); } resolve(v); };
            var img = $('#phZone img', corps);
            $('#phFichier', corps).onchange = function () {
                fichier = this.files && this.files[0] ? this.files[0] : null;
                if (apercu) { URL.revokeObjectURL(apercu); apercu = null; }
                if (fichier) { apercu = URL.createObjectURL(fichier); img.src = apercu; }
                img.hidden = !fichier;
                $('#phZone', corps).classList.toggle('rempli', !!fichier);
                $('#phEnvoyer', corps).disabled = !fichier;
            };
            $('#frmPhoto', corps).onsubmit = async function (e) {
                e.preventDefault();
                if (!fichier) { toast('Prenez d\'abord une photo.', 'warning'); return; }
                var bouton = $('#phEnvoyer', corps);
                bouton.disabled = true;
                bouton.innerHTML = '<i class="iconoir-refresh"></i>Envoi en cours…';
                // Le serveur découpe les constats sur la virgule (un par photo) : aucune virgule dans le texte.
                var constat = $('#phConstat', corps).value.replace(/\s*,\s*/g, ' ; ').trim();
                var ok = false;
                try {
                    var photo = await preparerPhoto(fichier);
                    var fd = new FormData();
                    fd.append('IdImmo', String(p(b, 'id')));
                    fd.append('Constat', constat);
                    fd.append('file', photo, 'photo.jpg');
                    var r = await api('/immo/add/photo', { multipart: fd });
                    ok = reussi(r);
                    toast(r.message || (ok ? 'Photo ajoutée.' : 'La photo n\'a pas été enregistrée.'), ok ? 'success' : 'danger');
                } catch (err) {
                    if (!err.session) { toast(err.reseau ? 'Photo non envoyée : ' + err.message : err.message, 'danger'); }
                }
                if (ok) { fin(true); fermerSheet(); return; }
                bouton.disabled = false;
                bouton.innerHTML = '<i class="iconoir-upload"></i>Réessayer l\'envoi';
            };
            surFermetureSheet(function () { fin(false); });
        });
    }

    /* ------------------------------------------------------------------ pages : biens non vus / en transit / déclassés (local particulier
       de l'organe d'affectation ; pour les déclassés, local « Déclassé » de l'entité, même format de réponse que le transit) */
    var PAGES_SPECIALES = {
        nonvu: { titre: 'Biens non vus', court: 'Non vu', url: '/immo/nonvu/', icone: 'iconoir-eye-closed', couleur: 'ico-violet' },
        transit: { titre: 'Biens en transit', court: 'Transit', url: '/immo/transit/', icone: 'iconoir-delivery-truck', couleur: 'ico-orange' },
        declasses: { titre: 'Biens déclassés', court: 'Déclassés', url: '/immo/declasser/', icone: 'iconoir-archive', couleur: 'ico-gris' }
    };
    /* Sélecteur Non vu / Transit / Déclassés en haut des trois pages (les déclassés n'ont pas d'onglet dans la barre du bas). */
    function segSpeciales(type) {
        return '<div class="m-seg">' + ['nonvu', 'transit', 'declasses'].map(function (t) {
            return '<button type="button" data-speciale="' + t + '"' + (t === type ? ' class="actif"' : '') + '><i class="' + PAGES_SPECIALES[t].icone + '"></i>' + PAGES_SPECIALES[t].court + '</button>';
        }).join('') + '</div>';
    }
    function brancherSegSpeciales() {
        $$('[data-speciale]').forEach(function (b) { b.onclick = function () { location.replace('#/' + b.dataset.speciale); }; });
    }
    async function pageSpeciale(type) {
        var conf = PAGES_SPECIALES[type];
        entete(conf.titre, type === 'declasses', type);
        afficherChargement();
        var r = null, erreur = null;
        try { r = await api(conf.url); } catch (e) { if (!etat.user) { return; } erreur = e.message; }
        if (erreur || !reussi(r)) {
            $('#page').innerHTML = segSpeciales(type) + vide(conf.icone, conf.titre, erreur || (r && r.message) || 'Aucune donnée disponible.', 'attention', { id: 'btnReessayer', libelle: 'Réessayer' });
            $('#btnReessayer').onclick = rendre;
            brancherSegSpeciales();
            return;
        }
        var biens = Array.isArray(r.content) ? r.content : [];
        var declasses = type === 'declasses';
        var idLocal = biens.length ? p(biens[0], 'idLocal') : null;
        var local = idLocal ? await chargerLocal(idLocal).catch(function () { return null; }) : null;
        await chargerArbre().catch(function () { return null; });
        var organe = local ? organeParId(p(local, 'codeOrgane')) : null;
        var tete = segSpeciales(type) + '<div class="m-card"><div class="m-entete-carte"><span class="ico-gros ' + conf.couleur + '"><i class="' + conf.icone + '"></i></span><div class="min-w-0"><h6>' + esc(local ? p(local, 'designation') : conf.titre) + '</h6>' +
            '<div class="m-sous">' + esc(organe ? p(organe, 'nom') : (declasses ? 'Local des déclassés de votre entité' : 'Organe d\'affectation')) + (local && p(local, 'code') ? ' · ' + esc(p(local, 'code')) : '') + '</div></div></div></div>';
        if (!biens.length) {
            $('#page').innerHTML = tete + vide('iconoir-check-circle', declasses ? 'Aucun bien déclassé' : 'Aucun bien',
                type === 'nonvu' ? 'Aucun bien n\'est classé « non vu » pour votre organe.'
                    : declasses ? 'Le local des déclassés de votre entité est vide. Un bien déclassé y est rangé en attendant sa cession.'
                    : 'Aucun bien n\'est en transit pour votre organe.', 'succes');
            brancherSegSpeciales();
            if (declasses) { setActions([{ icone: 'iconoir-archive', libelle: 'Déclasser un bien', action: declasserPuisRecharger, principal: true }]); }
            return;
        }
        var zone = $('#page');
        if (declasses) {
            // Biens en attente de cession : pas de progression « vus » ; menu ⋮ réduit (céder, voir la fiche).
            var barreCession = '<div class="m-cession"><span>' + nombre(biens.length) + ' bien(s) en attente de cession</span>' +
                '<button type="button" class="m-btn m-btn-danger-discret" id="btnCederTous"><i class="iconoir-log-out"></i>Céder tous les biens</button></div>';
            rendreListeBiens(zone, biens, tete + barreCession, { menu: menuDeclasse, avancement: false, lienFiche: '?depuis=declasses' });
            brancherSegSpeciales();
            $('#btnCederTous').onclick = async function () {
                try {
                    if (await cederTousLesBiens(biens.length)) { rendre(); }
                } catch (e) { if (!e.session) { toast(e.message, 'danger'); } }
            };
            setActions([
                { icone: 'iconoir-archive', libelle: 'Déclasser un bien', court: 'Déclasser', action: declasserPuisRecharger },
                { icone: 'iconoir-qr-code', libelle: 'Scanner un bien', court: 'Scanner', action: scanBien }
            ].concat(local ? [{ icone: 'iconoir-home-alt', libelle: 'Voir le local', court: 'Local', action: function () { aller('#/local/' + p(local, 'id') + '?mode=details'); } }] : []));
            return;
        }
        rendreListeBiens(zone, biens, tete);
        brancherSegSpeciales();
        if (local) {
            setActions([
                { icone: 'iconoir-scan-qr-code', libelle: 'Inventorier', action: function () { lancerRapideLocal(local); }, principal: true },
                { icone: 'iconoir-home-alt', libelle: 'Voir le local', court: 'Local', action: function () { aller('#/local/' + p(local, 'id') + '?mode=details'); } },
                { icone: 'iconoir-qr-code', libelle: 'Scanner un bien', court: 'Scanner', action: scanBien }
            ]);
        }
    }

    /* ==================================================================
       INVENTAIRE RAPIDE
       1) scanner le QR du local ; 2) caméra en continu : chaque QR de bien est traité aussitôt
       (identifié « Bon » s'il est dans le local, « Déplacer ici » s'il est ailleurs, signalé s'il est inconnu) ;
       3) compteur vus / total et liste des biens traités ; « Terminer » propose de déclarer les non vus,
          puis de clôturer l'inventaire du local, et revient au local.
       ================================================================== */
    var rapide = null;

    async function demarrerRapide() {
        var code = await scanner('Inventaire rapide', 'Étape 1 : scannez le QR code du local à inventorier.');
        if (!code) { return; }
        if (!GUID.test(code)) { signal('err'); toast('Ce code n\'est pas une étiquette Locate.', 'warning'); return; }
        try {
            var l = await localParQr(code);
            if (!l) {
                var b = await bienParQr(code).catch(function () { return null; });
                signal('err');
                toast(b ? 'C\'est l\'étiquette d\'un bien : commencez par le QR code du local.' : 'Aucun local ne porte ce QR code.', 'warning');
                return;
            }
            aller('#/rapide/' + p(l, 'id'));
        } catch (e) { toast(e.message, 'danger'); }
    }

    function quitterRapide() {
        if (!rapide) { return; }
        rapide.actif = false;
        rapide = null;
        arreterCamera();
        document.removeEventListener('visibilitychange', surVisibilite);
        couleurTheme('#ffffff');
    }
    function surVisibilite() {
        if (!rapide) { return; }
        if (document.hidden) { arreterCamera(); rapide.enPause = true; }
        else if (rapide.enPause) { rapide.enPause = false; lancerCameraRapide(rapide); }
    }

    function indexerBien(r, b) {
        var id = String(p(b, 'id'));
        r.biens[id] = b;
        if (p(b, 'qrCode')) { r.parQr[minuscule(p(b, 'qrCode'))] = id; }
    }
    function majCompteur(r) {
        var ids = Object.keys(r.biens), vus = 0;
        ids.forEach(function (id) { if (r.vusSession[id] || bienVu(r.biens[id])) { vus++; } });
        var pct = pourcent(vus, ids.length);
        $('#rpVus').textContent = nombre(vus);
        $('#rpTotal').textContent = nombre(ids.length);
        $('#rpBarre').style.width = pct + '%';
        $('#rpPct').textContent = pct + ' %';
    }

    async function pageRapide(id) {
        var nav = navigation;
        document.body.classList.add('mode-rapide');
        document.body.classList.remove('page-accueil');
        document.title = 'Inventaire rapide - Locate Terrain';
        couleurTheme('#0b1f3a');
        $('#page').innerHTML = '<div class="m-rapide"><div class="m-rapide-entete" style="min-height:64px"></div><div class="m-rapide-corps">' + squelette(3) + '</div></div>';
        var local = await chargerLocal(id);
        if (!local) { throw new Error('Local introuvable.'); }
        var rb = await api('/immo/local/' + id);
        var liste = Array.isArray(rb.content) ? rb.content : [];
        if (nav !== navigation) { return; } // l'utilisateur a quitté la page entre-temps

        var r = rapide = { id: String(p(local, 'id')), local: local, biens: {}, parQr: {}, recents: {}, vusSession: {}, session: [], file: Promise.resolve(), actif: true, enPause: false };
        liste.forEach(function (b) { indexerBien(r, b); });
        document.addEventListener('visibilitychange', surVisibilite);

        $('#page').innerHTML =
            '<div class="m-rapide"><div class="m-rapide-grille">' +
            '<div class="m-rapide-haut">' +
            '<div class="m-rapide-entete">' +
            '<button type="button" class="m-rond-btn" id="rpFermer" aria-label="Terminer"><i class="iconoir-xmark"></i></button>' +
            '<div class="infos"><div class="etiquette">Inventaire rapide · <span id="rpPct">0 %</span></div><div class="local">' + esc(p(local, 'designation')) + '</div></div>' +
            '<div class="m-rapide-compteur"><b id="rpVus">0</b><span> / <span id="rpTotal">0</span></span><small>biens vus</small></div>' +
            '</div>' +
            '<div class="m-rapide-progress"><div id="rpBarre" style="width:0"></div></div>' +
            '<div class="m-rapide-camera" id="rpCamera">' +
            '<video id="rpVideo" playsinline muted autoplay></video>' +
            '<div class="m-cadre"><i></i><i></i><i></i><i></i><b class="m-laser"></b></div>' +
            '<div class="outils"><button type="button" class="m-rond-btn" id="rpLampe" aria-label="Lampe" hidden><i class="iconoir-flash"></i></button></div>' +
            '<div id="rpRetour"></div>' +
            '</div></div>' +
            '<div class="m-rapide-corps">' +
            '<form class="m-saisie clair" id="rpSaisie" autocomplete="off"><i class="iconoir-input-field"></i><input type="text" id="rpCode" placeholder="Saisir ou lire un code (douchette)" autocapitalize="none" spellcheck="false" enterkeyhint="go"><button type="submit" aria-label="Valider"><i class="iconoir-arrow-right"></i></button></form>' +
            '<div class="m-section"><h2>Traités pendant la session</h2><span id="rpNb">0</span></div>' +
            '<div class="m-liste m-session" id="rpSession"></div>' +
            '<div id="rpVide">' + vide('iconoir-scan-qr-code', 'Scannez les biens du local', 'Chaque bien de ce local est marqué vu en « Bon état ». Touchez « Mauvais » ou « Observation » pour corriger.') + '</div>' +
            '</div></div>' +
            '<div class="m-barre-fin"><button type="button" class="m-btn m-btn-primaire" id="rpTerminer"><i class="iconoir-check-circle"></i>Terminer</button></div>' +
            '</div>';
        majCompteur(r);
        $('#rpFermer').onclick = function () { terminerRapide(r); };
        $('#rpTerminer').onclick = function () { terminerRapide(r); };
        $('#rpLampe').onclick = function () { basculerLampe(this); };
        $('#rpSaisie').onsubmit = function (e) {
            e.preventDefault();
            var code = extraireCode($('#rpCode').value);
            $('#rpCode').value = '';
            if (code && r.actif) { r.recents[code] = Date.now(); mettreEnFile(r, code); }
        };
        retourScan(r, 'gris', 'iconoir-scan-qr-code', 'Étape 2 : scannez les biens', 'La caméra reste ouverte : visez les étiquettes une à une.');
        // Précharge les observations par défaut (Bon / Mauvais) pour une identification immédiate.
        observations('B').catch(function () { return null; });
        observations('M').catch(function () { return null; });
        lancerCameraRapide(r);
    }

    async function lancerCameraRapide(r) {
        var zone = $('#rpCamera');
        if (!zone || r !== rapide) { return; }
        var ancien = $('.m-camera-msg', zone); if (ancien) { ancien.remove(); }
        zone.classList.remove('sans-camera');
        var statut = await demarrerCamera($('#rpVideo'), function (code) { recevoirCode(r, code); return false; });
        if (r !== rapide) { return; }
        if (statut === 'ok') { $('#rpLampe').hidden = !lampeDisponible(); return; }
        var texte = messageCamera(statut);
        if (texte) { zone.classList.add('sans-camera'); zone.appendChild(el('<div class="m-camera-msg"><i class="iconoir-camera"></i>' + esc(texte) + '</div>')); }
    }

    /* Code lu par la caméra : on ignore le même QR relu dans les 3 secondes, puis on le traite dans l'ordre d'arrivée. */
    function recevoirCode(r, code) {
        if (r !== rapide || !r.actif) { return; }
        // Lecture suspendue pendant qu'une feuille ou une confirmation est ouverte.
        if ($('#sheet').classList.contains('show') || $('#confirmModal').classList.contains('show')) { return; }
        var maintenant = Date.now(), precedent = r.recents[code];
        r.recents[code] = maintenant;
        if (precedent && maintenant - precedent < 3000) { return; }
        mettreEnFile(r, code);
    }
    function mettreEnFile(r, code) {
        r.file = r.file.then(function () { return traiterCode(r, code); }).catch(function () { /* erreur déjà signalée */ });
    }

    /* Bandeau de retour affiché sur la caméra. actions : [{libelle, icone, action}] */
    function retourScan(r, couleur, icone, titre, texte, actions) {
        var zone = $('#rpRetour');
        if (!zone || r !== rapide) { return; }
        var b = el('<div class="m-retour-scan ' + couleur + '" role="status"><span class="ico"><i class="' + icone + '"></i></span><div class="txt"><b>' + esc(titre) + '</b>' + (texte ? '<span>' + esc(texte) + '</span>' : '') + '</div><div class="acts"></div></div>');
        (actions || []).forEach(function (a) {
            var bt = el('<button type="button"' + (a.icone && !a.libelle ? ' aria-label="' + esc(a.aria || '') + '"' : '') + '>' + (a.icone ? '<i class="' + a.icone + '"></i>' : '') + (a.libelle ? esc(a.libelle) : '') + '</button>');
            bt.onclick = a.action;
            $('.acts', b).appendChild(bt);
        });
        zone.innerHTML = '';
        zone.appendChild(b);
        if (couleur === 'vert') {
            var cam2 = $('#rpCamera');
            cam2.classList.remove('m-flash-vert'); void cam2.offsetWidth; cam2.classList.add('m-flash-vert');
        }
    }

    /* Entrée de la liste de session (une par bien ou par code inconnu). */
    function entreeSession(r, cle, bien, code) {
        var e = r.session.filter(function (x) { return x.cle === cle; })[0];
        if (!e) { e = { cle: cle, code: code || cle, bien: bien, statut: null, etat: null, obs: null, message: '', heure: heureCourte(), el: null }; r.session.unshift(e); }
        else {
            r.session.splice(r.session.indexOf(e), 1); r.session.unshift(e);
            e.heure = heureCourte(); if (bien) { e.bien = bien; } if (code) { e.code = code; }
        }
        e.enTete = true; // à (re)placer en haut de la liste au prochain rendu
        return e;
    }
    function rendreEntree(r, e) {
        if (r !== rapide) { return; }
        var b = e.bien, liste = $('#rpSession');
        if (!liste) { return; }
        var ok = e.statut === 'vu' || e.statut === 'deja' || e.statut === 'deplace';
        var qrClasse = ok ? 'vu' : e.statut === 'ailleurs' ? 'oui' : 'non';
        var titre = b ? (p(b, 'designation') || ('Bien ' + p(b, 'id'))) : (e.statut === 'erreur' ? 'Erreur de lecture' : 'QR code inconnu');
        var detail = b ? [p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ') : e.cle;
        var badges = '';
        if (e.statut === 'vu') { badges += '<span class="m-badge m-badge-inv"><i class="iconoir-check"></i>vu</span>'; }
        if (e.statut === 'deja') { badges += '<span class="m-badge m-badge-ident"><i class="iconoir-eye"></i>déjà vu' + (fmtDate(p(b, 'dateVu')) ? ' le ' + esc(fmtDate(p(b, 'dateVu'))) : '') + '</span>'; }
        if (e.statut === 'deplace') { badges += '<span class="m-badge m-badge-ident"><i class="iconoir-download"></i>déplacé ici</span>'; }
        if (e.statut === 'ailleurs') { badges += '<span class="m-badge m-badge-att"><i class="iconoir-warning-triangle"></i>dans « ' + esc(e.ancien || 'un autre local') + ' »</span>'; }
        if (ok && e.etat) { badges += '<span class="m-badge m-badge-etat-' + e.etat + '">' + (e.etat === 'M' ? 'Mauvais état' : 'Bon état') + '</span>'; }
        if (ok && e.obs) { badges += '<span class="m-badge m-badge-obs">' + esc(e.obs) + '</span>'; }
        if (ok && e.attente) { badges += '<span class="m-badge m-badge-att"><i class="iconoir-cloud-upload"></i>en attente d\'envoi</span>'; }
        if (e.message && !ok) { badges += '<span class="m-badge m-badge-etat-M">' + esc(e.message) + '</span>'; }
        var boutons = '';
        if (ok) {
            boutons = '<button type="button" data-a="B" class="' + (e.etat === 'B' ? 'b-on' : '') + '"><i class="iconoir-check-circle"></i>Bon</button>' +
                '<button type="button" data-a="M" class="' + (e.etat === 'M' ? 'm-on' : '') + '"><i class="iconoir-warning-triangle"></i>Mauvais</button>' +
                '<button type="button" data-a="obs"><i class="iconoir-message-text"></i>Observation</button>';
        } else if (e.statut === 'ailleurs') {
            boutons = '<button type="button" data-a="deplacer" class="principal"><i class="iconoir-download"></i>Déplacer ici</button>';
        } else if (e.statut === 'erreur') {
            boutons = '<button type="button" data-a="reessayer"><i class="iconoir-refresh"></i>Réessayer</button>';
        }
        var n = el('<div class="m-item statut-' + (e.statut || 'attente') + (ok ? ' m-vu' : '') + (e.enCours ? ' en-cours' : '') + '">' +
            '<div class="m-qr petit ' + qrClasse + '"><i class="' + (ok ? 'iconoir-check' : e.statut === 'ailleurs' ? 'iconoir-arrow-right' : 'iconoir-qr-code') + '"></i></div>' +
            '<div class="corps"><div class="titre">' + esc(titre) + '</div><div class="detail">' + esc(detail) + '</div>' +
            '<div class="badges">' + badges + '</div>' + (boutons && !e.enCours ? '<div class="etat-rapide">' + boutons + '</div>' : '') + '</div>' +
            '<div class="heure">' + esc(e.heure) + '</div></div>');
        $$('button[data-a]', n).forEach(function (bt) {
            bt.onclick = function () {
                var a = bt.dataset.a;
                if (a === 'B' || a === 'M') { if (e.etat !== a || e.statut === 'deja') { identifierRapide(r, e, a, null); } }
                else if (a === 'obs') { sheetObservationRapide(r, e); }
                else if (a === 'deplacer') { deplacerIciRapide(r, e); }
                else if (a === 'reessayer') { r.recents[e.code] = Date.now(); mettreEnFile(r, e.code); }
            };
        });
        // Une entrée (re)scannée remonte en tête ; une simple mise à jour (état, observation) reste à sa place.
        if (e.el && e.el.parentNode && !e.enTete) { e.el.parentNode.replaceChild(n, e.el); }
        else {
            if (e.el && e.el.parentNode) { e.el.parentNode.removeChild(e.el); }
            liste.insertBefore(n, liste.firstChild);
        }
        e.el = n; e.enTete = false;
        $('#rpNb').textContent = r.session.length;
        $('#rpVide').hidden = r.session.length > 0;
    }

    async function traiterCode(r, code) {
        if (r !== rapide) { return; }
        if (!GUID.test(code)) {
            var e0 = entreeSession(r, code, null, code); e0.statut = 'inconnu'; e0.message = 'pas une étiquette Locate'; rendreEntree(r, e0);
            retourScan(r, 'rouge', 'iconoir-xmark-circle', 'Code non reconnu', 'Ce code n\'est pas une étiquette Locate.');
            signal('err');
            return;
        }
        if (code === minuscule(p(r.local, 'qrCode'))) {
            retourScan(r, 'gris', 'iconoir-home-alt', 'QR code de ce local', 'Scannez maintenant les étiquettes des biens.');
            return;
        }
        var id = r.parQr[code];
        if (id) { return traiterBienDuLocal(r, r.biens[id], code); }

        retourScan(r, 'gris', 'iconoir-search', 'Recherche du code…', code);
        var bien = null, local = null, errBien = null, errLocal = null;
        try { bien = await bienParQr(code); } catch (err) { errBien = err; }
        if (r !== rapide) { return; }
        if (bien) {
            if (String(p(bien, 'idLocal')) === r.id) { indexerBien(r, bien); majCompteur(r); return traiterBienDuLocal(r, bien, code); }
            var ancien = p(p(bien, 'local'), 'designation') || (await nomLocal(p(bien, 'idLocal'))) || 'un autre local';
            if (r !== rapide) { return; }
            var e = entreeSession(r, String(p(bien, 'id')), bien, code);
            e.statut = 'ailleurs'; e.ancien = ancien; e.message = '';
            rendreEntree(r, e);
            retourScan(r, 'orange', 'iconoir-warning-triangle', p(bien, 'designation') || 'Bien d\'un autre local', 'Enregistré dans « ' + ancien + ' »',
                [{ libelle: 'Déplacer ici', icone: 'iconoir-download', action: function () { deplacerIciRapide(r, e); } }]);
            signal('att');
            return;
        }
        try { local = await localParQr(code); } catch (err) { errLocal = err; }
        if (r !== rapide) { return; }
        if (local) {
            retourScan(r, 'bleu', 'iconoir-home-alt', 'QR code d\'un autre local', p(local, 'designation') || '',
                [{ libelle: 'Inventorier', icone: 'iconoir-arrow-right', action: function () { aller('#/rapide/' + p(local, 'id')); } }]);
            signal('att');
            return;
        }
        var e2 = entreeSession(r, code, null, code);
        if (errBien && errLocal) {
            e2.statut = 'erreur'; e2.message = errBien.message;
            retourScan(r, 'rouge', 'iconoir-wifi-off', 'Lecture non traitée', errBien.message);
        } else {
            e2.statut = 'inconnu'; e2.message = 'aucun bien ni local';
            retourScan(r, 'rouge', 'iconoir-xmark-circle', 'QR code inconnu', 'Aucun bien ni local ne porte cette étiquette.');
        }
        rendreEntree(r, e2);
        signal('err');
    }

    /* Bien du local : identifié « Bon » s'il n'a pas encore été vu cette année ; sinon on ne réécrit pas l'état déjà saisi. */
    async function traiterBienDuLocal(r, bien, code) {
        var e = entreeSession(r, String(p(bien, 'id')), bien, code);
        if (bienVu(bien) && !r.vusSession[String(p(bien, 'id'))]) {
            e.statut = 'deja'; e.etat = p(bien, 'lastEtat') === 'M' ? 'M' : (p(bien, 'lastEtat') === 'B' ? 'B' : null); e.obs = p(bien, 'lastObservation') || null;
            rendreEntree(r, e);
            retourScan(r, 'bleu', 'iconoir-eye', p(bien, 'designation') || 'Bien', 'Déjà vu' + (fmtDate(p(bien, 'dateVu')) ? ' le ' + fmtDate(p(bien, 'dateVu')) : '') + ' · ' + libelleEtat(bien), actionsEtat(r, e));
            signal('ok');
            return;
        }
        if (r.vusSession[String(p(bien, 'id'))] && e.statut) {
            // Déjà identifié pendant cette session : simple rappel, pas de nouvelle requête.
            rendreEntree(r, e);
            retourScan(r, 'vert', 'iconoir-check-circle', p(bien, 'designation') || 'Bien', 'Déjà identifié · ' + (e.etat === 'M' ? 'Mauvais état' : 'Bon état'), actionsEtat(r, e));
            signal('ok');
            return;
        }
        await identifierRapide(r, e, 'B', null);
    }
    function actionsEtat(r, e) {
        return [
            e.etat === 'M'
                ? { libelle: 'Bon', icone: 'iconoir-check-circle', action: function () { identifierRapide(r, e, 'B', null); } }
                : { libelle: 'Mauvais', icone: 'iconoir-warning-triangle', action: function () { identifierRapide(r, e, 'M', null); } },
            { icone: 'iconoir-message-text', aria: 'Observation', action: function () { sheetObservationRapide(r, e); } }
        ];
    }

    /* Même requête que l'identification classique : POST /inventaire/identifier/ { IdImmo, LastEtat, IdLastObservation }. */
    async function identifierRapide(r, e, etatB, obs) {
        if (r !== rapide) { return false; }
        var bien = e.bien;
        e.enCours = true; rendreEntree(r, e);
        try {
            if (!obs) { obs = await observationParDefaut(etatB); }
            if (!obs) { throw new Error('Aucune observation « ' + (etatB === 'M' ? 'Mauvais état' : 'Bon état') + ' » n\'est paramétrée.'); }
            var champs = { IdImmo: p(bien, 'id'), LastEtat: etatB, IdLastObservation: obs.id };
            var res = null, enAttente = horsLigne();
            if (!enAttente) {
                try { res = await api('/inventaire/identifier/', { form: champs }); }
                catch (errReseau) { if (errReseau.reseau) { enAttente = true; } else { throw errReseau; } }
            }
            // Sans réseau : l'identification part dans la file hors ligne et le bien compte comme vu dans la session.
            if (enAttente) { ajouterFile('/inventaire/identifier/', champs, 'Identification : ' + (p(bien, 'designation') || ('bien ' + p(bien, 'id')))); }
            if (r !== rapide) { return false; }
            e.enCours = false;
            if (!enAttente && !reussi(res)) { throw new Error(res.message || 'L\'identification a échoué.'); }
            var id = String(p(bien, 'id'));
            r.vusSession[id] = true;
            marquerVuLocalement(bien); bien.lastEtat = etatB; bien.lastObservation = obs.libelle; bien.idLastObservation = obs.id;
            bien.lastEtatString = etatB === 'M' ? 'Mauvais état' : 'Bon état';
            e.statut = e.statut === 'deplace' ? 'deplace' : 'vu'; e.etat = etatB; e.obs = obs.libelle; e.message = ''; e.attente = enAttente;
            rendreEntree(r, e);
            majCompteur(r);
            if (enAttente) {
                retourScan(r, 'orange', 'iconoir-cloud-upload', 'Hors ligne', 'Enregistré, sera envoyé au retour du réseau', actionsEtat(r, e));
            } else {
                retourScan(r, etatB === 'M' ? 'orange' : 'vert', etatB === 'M' ? 'iconoir-warning-triangle' : 'iconoir-check-circle', p(bien, 'designation') || 'Bien identifié',
                    (e.statut === 'deplace' ? 'Déplacé ici · ' : 'Vu · ') + (etatB === 'M' ? 'Mauvais état' : 'Bon état') + (obs.libelle ? ' · ' + obs.libelle : ''), actionsEtat(r, e));
            }
            signal(etatB === 'M' || enAttente ? 'att' : 'ok');
            return true;
        } catch (err) {
            if (r !== rapide) { return false; }
            e.enCours = false;
            if (!e.statut || e.statut === 'erreur') { e.statut = 'erreur'; e.message = err.message; }
            rendreEntree(r, e);
            retourScan(r, 'rouge', 'iconoir-xmark-circle', p(bien, 'designation') || 'Identification impossible', err.message);
            signal('err');
            return false;
        }
    }

    /* Déplacement vers le local en cours : même requête que « Déplacer un bien ici » (POST /immo/changelocal/), puis identification « Bon ». */
    async function deplacerIciRapide(r, e) {
        if (r !== rapide) { return; }
        if (!p(r.local, 'qrCode')) { toast('Ce local n\'a pas de QR code : affectez-lui d\'abord une étiquette pour y déplacer des biens.', 'warning'); return; }
        var bien = e.bien;
        e.enCours = true; rendreEntree(r, e);
        try {
            var res = await api('/immo/changelocal/', { form: { IdImmo: p(bien, 'id'), QrCodeLocal: p(r.local, 'qrCode') } });
            if (r !== rapide) { return; }
            e.enCours = false;
            if (!reussi(res)) { throw new Error(res.message || 'Le déplacement a échoué.'); }
            bien.idLocal = p(r.local, 'id');
            indexerBien(r, bien);
            e.statut = 'deplace';
            majCompteur(r);
            await identifierRapide(r, e, 'B', null);
        } catch (err) {
            if (r !== rapide) { return; }
            e.enCours = false;
            rendreEntree(r, e);
            retourScan(r, 'rouge', 'iconoir-xmark-circle', p(bien, 'designation') || 'Déplacement impossible', err.message);
            signal('err');
        }
    }

    /* Choix d'une observation (et de l'état) pour un bien de la session. */
    function sheetObservationRapide(r, e) {
        var etatCourant = e.etat === 'M' ? 'M' : 'B';
        var corps = ouvrirSheet('Observation',
            '<div class="m-sous mb-3">' + esc(p(e.bien, 'designation') || '') + '</div>' +
            '<span class="m-libelle">État constaté</span><div class="m-etat mb-3">' +
            '<label><input type="radio" name="rpEtat" value="B"' + (etatCourant === 'B' ? ' checked' : '') + '><span class="B"><i class="iconoir-check-circle"></i>Bon état</span></label>' +
            '<label><input type="radio" name="rpEtat" value="M"' + (etatCourant === 'M' ? ' checked' : '') + '><span class="M"><i class="iconoir-warning-triangle"></i>Mauvais état</span></label></div>' +
            '<span class="m-libelle">Observation</span><div class="m-choix" id="rpObs" style="max-height:45vh;overflow:auto"></div>');
        var zone = $('#rpObs', corps);
        var charger = async function (etatB) {
            zone.innerHTML = squelette(2);
            try {
                var liste = await observations(etatB);
                zone.innerHTML = liste.length ? '' : vide('iconoir-message-text', 'Aucune observation', 'Aucune observation n\'est paramétrée pour cet état.');
                liste.forEach(function (o) {
                    var b = el('<button type="button"' + (e.obs === o.libelle && e.etat === etatB ? ' class="choisi"' : '') + '><i class="iconoir-message-text"></i><span>' + esc(o.libelle) + '</span></button>');
                    b.onclick = function () { fermerSheet(); identifierRapide(r, e, etatB, o); };
                    zone.appendChild(b);
                });
            } catch (err) { zone.innerHTML = vide('iconoir-warning-triangle', 'Observations indisponibles', err.message, 'attention'); }
        };
        $$('input[name=rpEtat]', corps).forEach(function (x) { x.onchange = function () { charger(x.value); }; });
        charger(etatCourant);
    }

    /* Fin de l'inventaire rapide : s'il reste des biens non vus dans le local, proposer de les déclarer non vus en une fois ;
       ensuite (ou directement si tout a été vu), proposer de clôturer l'inventaire du local, puis revenir au local. */
    async function terminerRapide(r) {
        if (r !== rapide || r.fin) { return; }
        var retour = function () { aller('#/local/' + r.id + '?mode=details'); };
        var restants = Object.keys(r.biens).map(function (id) { return r.biens[id]; })
            .filter(function (b) { return !(r.vusSession[String(p(b, 'id'))] || bienVu(b) || bienInventorie(b)); });
        if (restants.length) {
            var n = restants.length;
            var libDeclarer = n === 1 ? 'Déclarer le bien non vu' : 'Déclarer les ' + nombre(n) + ' biens non vus';
            var choix = await choisir(n === 1 ? '1 bien de ce local n\'a pas été vu' : nombre(n) + ' biens de ce local n\'ont pas été vus', [
                { id: 'terminer', icone: 'iconoir-log-out', libelle: 'Terminer sans rien déclarer', sous: 'Les biens restent « à voir »' },
                { id: 'declarer', icone: 'iconoir-eye-closed', libelle: libDeclarer, sous: 'Introuvables dans ce local', classe: 'danger' },
                { id: 'liste', icone: 'iconoir-list', libelle: 'Voir la liste des biens restants' },
                { id: 'continuer', icone: 'iconoir-scan-qr-code', libelle: 'Continuer l\'inventaire' }
            ]);
            if (r !== rapide) { return; }
            if (choix === 'liste') { choix = await listeRestants(restants, libDeclarer); if (r !== rapide) { return; } }
            if (choix === 'declarer') { if (!(await declarerRestants(r, restants))) { return; } }
            else if (choix !== 'terminer') { return; } // continuer l'inventaire
        }
        await proposerClotureRapide(r);
        if (r === rapide) { retour(); }
    }

    /* Clôture proposée en fin d'inventaire rapide quand des biens du local sont identifiés mais pas encore inventoriés. */
    async function proposerClotureRapide(r) {
        if (r !== rapide) { return; }
        var aCloturer = Object.keys(r.biens).filter(function (id) {
            var b = r.biens[id];
            return (r.vusSession[id] || bienVu(b)) && !bienInventorie(b) && !!p(b, 'qrCode');
        }).length;
        if (!aCloturer) { return; }
        r.fin = true; r.actif = false; // plus de lecture de codes pendant la clôture
        var bouton = $('#rpTerminer');
        if (bouton) { bouton.disabled = true; }
        await cloturerLocal(r.local);
    }

    /* Liste des biens restants (non vus) ; résout avec 'declarer' ou null (continuer l'inventaire). */
    async function listeRestants(restants, libDeclarer) {
        var corps = await ouvrirFeuille('Biens restants (' + nombre(restants.length) + ')',
            '<div class="m-liste m-restants">' + restants.map(function (b) {
                return '<div class="m-item"><div class="m-qr petit ' + (p(b, 'qrCode') ? 'oui bleu' : 'non') + '"><i class="iconoir-qr-code"></i></div><div class="corps"><div class="titre">' + esc(nomBien(b)) + '</div>' +
                    '<div class="detail">' + esc([p(b, 'codeADM'), p(b, 'codeImmo')].filter(Boolean).join(' · ')) + '</div></div></div>';
            }).join('') + '</div>' +
            '<button type="button" class="m-btn m-btn-rouge m-btn-bloc mt-3" id="lrDeclarer"><i class="iconoir-eye-closed"></i>' + esc(libDeclarer) + '</button>' +
            '<button type="button" class="m-btn m-btn-clair m-btn-bloc mt-2" id="lrContinuer"><i class="iconoir-scan-qr-code"></i>Continuer l\'inventaire</button>');
        return new Promise(function (resolve) {
            var fini = false;
            var fin = function (v) { if (fini) { return; } fini = true; resolve(v); };
            $('#lrDeclarer', corps).onclick = function () { fin('declarer'); fermerSheet(); };
            $('#lrContinuer', corps).onclick = function () { fin(null); fermerSheet(); };
            surFermetureSheet(function () { fin(null); });
        });
    }

    /* Déclare non vus tous les biens restants (observation commune facultative), un par un avec progression ;
       sans réseau, chaque déclaration part dans la file hors ligne. Résout avec vrai si les déclarations ont été faites
       (l'appelant propose alors la clôture du local et revient au local). */
    async function declarerRestants(r, restants) {
        var n = restants.length;
        if (r !== rapide) { return false; }
        var question = (n === 1 ? 'Confirmez-vous définitivement que ce bien est non vu ?' : 'Confirmez-vous définitivement que ces ' + nombre(n) + ' biens sont non vus ?') +
            '<br>Ils seront placés dans le local « non vu » de votre organe.';
        if (!(await confirmer('Confirmation', question, 'Oui, déclarer', 'Non', 'm-btn-rouge')) || r !== rapide) { return false; }
        r.fin = true; r.actif = false; // plus de lecture de codes pendant les déclarations
        var bouton = $('#rpTerminer');
        if (bouton) { bouton.disabled = true; }
        var ok = 0, attente = 0, refus = 0, premierRefus = null;
        for (var i = 0; i < n; i++) {
            var b = restants[i];
            if (bouton && bouton.isConnected) { bouton.innerHTML = '<i class="iconoir-refresh"></i>Déclaration des non vus… ' + (i + 1) + ' / ' + n; }
            retourScan(r, 'gris', 'iconoir-eye-closed', 'Déclaration des non vus… ' + (i + 1) + ' / ' + n, nomBien(b));
            var res = await envoyerOuMettreEnAttente('/immo/nonvu/' + p(b, 'id'), {}, 'Non vu : ' + nomBien(b), true);
            if (res.attente) { attente++; } else if (res.ok) { ok++; } else { refus++; if (!premierRefus) { premierRefus = nomBien(b) + ' : ' + res.message; } }
            if (!etat.user) { return false; } // session perdue : l'écran de connexion a pris la main
        }
        toast(nombre(ok) + ' bien(s) déclaré(s) non vu(s)' + (attente ? ', ' + nombre(attente) + ' en attente d\'envoi (hors ligne)' : '') +
            (refus ? ', ' + nombre(refus) + ' refusé(s) par le serveur' : '') + '.', refus || attente ? 'warning' : 'success');
        if (premierRefus) { toast(premierRefus, 'danger'); }
        if (bouton && bouton.isConnected) { bouton.innerHTML = '<i class="iconoir-check-circle"></i>Terminer'; }
        return r === rapide;
    }

    /* ------------------------------------------------------------------ démarrage */
    if (!location.hash) { location.hash = '#/'; /* déclenche hashchange, donc rendre() */ } else { rendre(); }
})();
