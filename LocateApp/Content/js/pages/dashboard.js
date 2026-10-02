/* ============================================================
   Locate - graphiques du tableau de bord (Chart.js v3, style Approx)
   Ne s'exécute que si la page contient le tableau de bord.
   ============================================================ */
$(function () {

    if (!document.getElementById('radar-chart') || typeof Chart === 'undefined') { return; }

    var colors = {
        green: '#22c55e', yellow: '#fac146', blue: '#3167f3', cyan: '#00a6cb', red: '#ef4d56', purple: '#7c4dff',
        tick: '#7c8ea7', grid: 'rgba(132, 145, 183, 0.15)'
    };
    var basePalette = [colors.blue, colors.green, colors.yellow, colors.cyan, colors.red, colors.purple, '#f97316', '#14b8a6', '#a3e635', '#e879f9', '#38bdf8', '#fb7185'];

    Chart.defaults.color = colors.tick;
    // Pas d'animation d'entrée (anneau qui tourne, barres qui montent) : elle donnait l'impression d'une attente.
    Chart.defaults.animation = false;
    var donneesPage = window.LocateDashboard || {};
    Chart.defaults.font.family = getComputedStyle(document.body).fontFamily;

    function getJson(url, done) {
        $.ajax({ type: 'get', url: url, dataType: 'json' })
            .done(function (result) { done(result && result.success && result.content ? result.content : []); })
            .fail(function () { done([]); });
    }

    function palette(n) {
        var out = [];
        for (var i = 0; i < n; i++) { out.push(basePalette[i % basePalette.length]); }
        return out;
    }

    function hexToRgba(hex, alpha) {
        var m = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return m ? 'rgba(' + parseInt(m[1], 16) + ',' + parseInt(m[2], 16) + ',' + parseInt(m[3], 16) + ',' + alpha + ')' : hex;
    }

    var legendOpts = { labels: { color: colors.tick, usePointStyle: true, boxWidth: 8 } };
    var scalesOpts = {
        y: { beginAtZero: true, ticks: { color: colors.tick }, grid: { color: colors.grid, borderDash: [3], borderColor: colors.grid } },
        x: { ticks: { color: colors.tick }, grid: { display: false } }
    };

    // ---------- Chargement robuste d'un graphique : attente visible, message + « Réessayer » si le serveur n'a pas répondu ----------
    var chargeurs = {};
    function charger(url, canvasId, dessiner, donnees) {
        var el = document.getElementById(canvasId);
        if (!el) { return; }
        chargeurs[canvasId] = function () { charger(url, canvasId, dessiner); };
        // Données déjà fournies par la page : dessin immédiat, sans requête ni « Chargement... ».
        if (Array.isArray(donnees) && donnees.length) {
            var etatPage = el.parentNode.querySelector('.chart-etat');
            if (etatPage) { etatPage.style.display = 'none'; }
            el.style.display = '';
            dessiner(el, donnees);
            return;
        }
        var conteneur = el.parentNode;
        var compact = conteneur.clientHeight > 0 && conteneur.clientHeight < 100;
        if (Chart.getChart) { var ancien = Chart.getChart(el); if (ancien) { ancien.destroy(); } }
        var etat = conteneur.querySelector('.chart-etat');
        if (!etat) {
            etat = document.createElement('div');
            etat.className = 'chart-etat d-flex flex-column align-items-center justify-content-center text-center text-muted h-100' + (compact ? ' fs-12' : ' small');
            conteneur.appendChild(etat);
        }
        el.style.display = 'none';
        etat.style.display = '';
        etat.innerHTML = '<span><span class="spinner-border spinner-border-sm text-primary me-1" role="status"></span>Chargement...</span>';
        getJson(url, function (rows) {
            if (!rows.length) {
                etat.innerHTML = compact
                    ? '<span>Indisponible <a href="javascript:void(0)" class="chart-retry">réessayer</a></span>'
                    : '<i class="iconoir-warning-triangle fs-3 mb-1"></i><span>Données indisponibles : le serveur n\'a pas répondu à temps.</span><a href="javascript:void(0)" class="btn btn-sm btn-light mt-2 chart-retry"><i class="iconoir-refresh me-1"></i>Réessayer</a>';
                etat.querySelector('.chart-retry').addEventListener('click', chargeurs[canvasId]);
                return;
            }
            etat.style.display = 'none';
            el.style.display = '';
            dessiner(el, rows);
        });
    }

    // L'icône « Rafraîchir » d'une carte recharge les graphiques qu'elle contient.
    $(document).on('click', '.card-refresh', function () {
        $(this).closest('.card').find('canvas').each(function () {
            if (chargeurs[this.id]) { chargeurs[this.id](); }
        });
    });

    // ---------- Quantité de biens : bon / mauvais par observation ----------
    charger('/observation/stat/', 'observation-chart', function (el, rows) {
        new Chart(el.getContext('2d'), {
            type: 'bar',
            data: {
                labels: rows.map(function (r) { return r.observation; }),
                datasets: [
                    { label: 'Bon état', data: rows.map(function (r) { return r.nbreBon; }), backgroundColor: colors.green, borderRadius: 6, borderSkipped: false, maxBarThickness: 28 },
                    { label: 'Mauvais état', data: rows.map(function (r) { return r.nbreMauvais; }), backgroundColor: colors.yellow, borderRadius: 6, borderSkipped: false, maxBarThickness: 28 }
                ]
            },
            options: { maintainAspectRatio: false, plugins: { legend: legendOpts }, scales: scalesOpts }
        });
    }, donneesPage.observations);

    // ---------- Analyse qualitative par famille (radar) ----------
    charger('/famille/stat/radar/', 'radar-chart', function (el, rows) {
        new Chart(el.getContext('2d'), {
            type: 'radar',
            data: {
                labels: rows.map(function (r) { return r.label; }),
                datasets: [
                    { label: '% Bon état', data: rows.map(function (r) { return r.pourcentageBon; }), borderColor: colors.blue, backgroundColor: hexToRgba(colors.blue, 0.2), pointBackgroundColor: colors.blue, borderWidth: 2, pointRadius: 3 },
                    { label: '% Mauvais état', data: rows.map(function (r) { return r.pourcentageMauvais; }), borderColor: colors.yellow, backgroundColor: hexToRgba(colors.yellow, 0.2), pointBackgroundColor: colors.yellow, borderWidth: 2, pointRadius: 3 }
                ]
            },
            options: {
                maintainAspectRatio: false,
                plugins: { legend: legendOpts },
                scales: {
                    r: {
                        beginAtZero: true,
                        suggestedMax: 100,
                        angleLines: { color: colors.grid },
                        grid: { color: colors.grid },
                        pointLabels: { color: colors.tick, font: { size: 10 } },
                        ticks: { display: false }
                    }
                }
            }
        });
    }, donneesPage.famillesRadar);

    // ---------- Analyse quantitative par famille (anneau) ----------
    charger('/famille/stat/pie/', 'chart4', function (el, rows) {
        new Chart(el.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: rows.map(function (r) { return r.label; }),
                datasets: [{ data: rows.map(function (r) { return r.value; }), backgroundColor: palette(rows.length), borderWidth: 0, hoverOffset: 6 }]
            },
            options: {
                maintainAspectRatio: false,
                cutout: '65%',
                plugins: { legend: { position: 'bottom', labels: { color: colors.tick, usePointStyle: true, boxWidth: 8, padding: 12 } } }
            }
        });
    }, donneesPage.famillesPie);

    // ---------- Mini graphiques des cartes de statistiques ----------
    var sparkOptions = {
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { displayColors: false } },
        scales: { x: { display: false }, y: { display: false, beginAtZero: true } },
        elements: { point: { radius: 0 }, line: { tension: 0.4 } }
    };

    charger('/famille/details/', 'sparkline-line', function (el, rows) {
        new Chart(el.getContext('2d'), {
            type: 'line',
            data: {
                labels: rows.map(function (r) { return r.nom || r.label || ''; }),
                datasets: [{ label: 'Biens', data: rows.map(function (r) { return r.nbre; }), borderColor: colors.green, backgroundColor: hexToRgba(colors.green, 0.15), fill: true, borderWidth: 2 }]
            },
            options: sparkOptions
        });
    }, donneesPage.famillesBiens);

    charger('/famille/identifie/', 'sparkline-bar', function (el, rows) {
        new Chart(el.getContext('2d'), {
            type: 'bar',
            data: {
                labels: rows.map(function (r) { return r.nom || r.label || ''; }),
                datasets: [{ label: 'Identifiés', data: rows.map(function (r) { return r.nbre; }), backgroundColor: colors.cyan, borderRadius: 3, borderSkipped: false }]
            },
            options: sparkOptions
        });
    }, donneesPage.famillesIdentifies);
});
