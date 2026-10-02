using LocateMobile.Models;
using LocateMobile.Services;

namespace LocateMobile.Pages;

/// <summary>
/// Actions sur un bien, partagées par la fiche, les listes et l'inventaire rapide (fonctions reprises d'eTracking :
/// déclarer non vu, expédier, mettre en service, déclasser, détacher le QR code, ajouter une photo).
/// Toutes passent par des routes existantes du serveur. Chaque méthode renvoie vrai si le bien a changé (à recharger).
/// </summary>
public abstract partial class PageBase
{
    /// <summary>Affecter une étiquette QR à un local (POST /local/qrcode/).</summary>
    protected async Task<bool> AffecterQrLocal(Local l)
    {
        var g = await ScannerGuid("Affecter un QR code au local", $"Scannez l'étiquette QR à coller sur « {l.Designation} ».");
        if (g == null) { return false; }
        return await Executer("local/qrcode/", new Dictionary<string, string> { ["Id"] = l.Id.ToString(), ["QrCode"] = g.ToString()! });
    }

    /// <summary>
    /// QR code d'un local, comme eTracking : sans étiquette, en affecter une ; sinon la détacher (pour la réaffecter ailleurs,
    /// GET /local/resetqrcode/{id}) ou la jeter (abîmée, perdue : GET /local/liveqrcode/{id}), puis proposer d'en scanner une nouvelle.
    /// </summary>
    protected async Task<bool> MenuQrLocal(Local l)
    {
        if (!l.AQrCode) { return await AffecterQrLocal(l); }
        string choix = await DisplayActionSheet($"QR code de « {l.Designation} »", "Annuler", null, "Détacher pour l'affecter ailleurs", "Jeter le QR code (abîmé ou perdu)");
        string? url = choix switch
        {
            "Détacher pour l'affecter ailleurs" => $"local/resetqrcode/{l.Id}",
            "Jeter le QR code (abîmé ou perdu)" => $"local/liveqrcode/{l.Id}",
            _ => null
        };
        if (url == null) { return false; }
        if (!await Confirmer("Retirer le QR code", $"Retirer le QR code du local « {l.Designation} » ?", "Retirer", "Annuler")) { return false; }
        if (!await Executer(url)) { return false; }
        if (await Confirmer("QR code retiré", "Scanner le nouveau QR code maintenant ?", "Scanner", "Plus tard")) { await AffecterQrLocal(l); }
        return true;
    }

    /// <summary>Appui long (Android) sur une vue : MAUI n'a pas de geste « appui long » intégré.</summary>
    protected static void AppuiLong(View vue, Func<Task> action)
    {
        vue.HandlerChanged += (_, _) =>
        {
#if ANDROID
            if (vue.Handler?.PlatformView is Android.Views.View v)
            {
                v.LongClickable = true;
                v.LongClick += async (_, e) => { e.Handled = true; await MainThread.InvokeOnMainThreadAsync(action); };
            }
#endif
        };
    }

    /// <summary>Niveau de chaque organe dans l'organigramme (pour l'indentation), d'après IdOrganeParent.</summary>
    protected static List<Organe> AvecNiveaux(List<Organe> organes)
    {
        var parId = organes.GroupBy(o => o.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var o in organes)
        {
            int niveau = 0;
            string? parent = o.IdOrganeParent;
            var vus = new HashSet<string>();
            while (parent != null && parId.TryGetValue(parent, out var p) && vus.Add(parent)) { niveau++; parent = p.IdOrganeParent; }
            o.Niveau = niveau;
        }
        return organes;
    }

    private const string ActIdentifier = "Identifier (marquer vu)";
    private const string ActNonVu = "Déclarer non vu";
    private const string ActDeplacer = "Déplacer vers un autre local";
    private const string ActExpedier = "Expédier vers un autre organe";
    private const string ActMiseEnService = "Mettre en service (réceptionner)";
    private const string ActPhoto = "Ajouter une photo";
    private const string ActAffecterQr = "Affecter un QR code";
    private const string ActDetacherQr = "Détacher le QR code";
    private const string ActFiche = "Voir la fiche du bien";
    private const string ActDeclasser = "Déclasser le bien…";

    /// <summary>Menu complet d'un bien (bouton ⋮ d'une ligne). <paramref name="depuisFiche"/> masque « Voir la fiche ».</summary>
    protected async Task<bool> MenuBien(Immo b, bool depuisFiche = false)
    {
        var options = new List<string> { ActIdentifier, ActNonVu, ActDeplacer, ActExpedier, ActMiseEnService, ActPhoto, b.AQrCode ? ActDetacherQr : ActAffecterQr };
        if (!depuisFiche) { options.Add(ActFiche); }
        string choix = await DisplayActionSheet(b.Titre, "Fermer", ActDeclasser, options.ToArray());
        return choix switch
        {
            ActIdentifier => await IdentifierPage.Ouvrir(Navigation, b),
            ActNonVu => await DeclarerNonVu(b),
            ActDeplacer => await DeplacerBien(b),
            ActExpedier => await ExpedierBien(b),
            ActMiseEnService => await MettreEnService(b),
            ActPhoto => await AjouterPhoto(b),
            ActAffecterQr => await AffecterQrBien(b),
            ActDetacherQr => await DetacherQrBien(b),
            ActDeclasser => await DeclasserBien(b),
            ActFiche => await OuvrirFiche(b),
            _ => false
        };
    }

    private async Task<bool> OuvrirFiche(Immo b) { await Navigation.PushAsync(new BienPage(b.Id)); return false; }

    /// <summary>Exécute une action ; sans réseau, la met dans la file hors ligne (pour les opérations d'inventaire).</summary>
    protected async Task<bool> ExecuterOuMettreEnAttente(string chemin, Dictionary<string, string> champs, string libelle)
    {
        try
        {
            var r = await Api.Action(chemin, champs);
            Informer(string.IsNullOrWhiteSpace(r.Message) ? (r.Ok ? "Opération effectuée." : "L'opération a échoué.") : r.Message, r.Ok);
            return r.Ok;
        }
        catch (ReseauException)
        {
            FileHorsLigne.Ajouter(chemin, champs, libelle);
            Informer($"Hors ligne : « {libelle} » enregistré sur la tablette, envoyé au retour du réseau.");
            return true;
        }
        catch (Exception e) { await Erreur(e); return false; }
    }

    /// <summary>
    /// Déclarer un bien non vu (introuvable), comme eTracking : POST /immo/nonvu/{id}. Le serveur range le bien dans le local
    /// « non vu » de l'organe d'affectation ; il apparaît alors dans l'onglet Non vu.
    /// </summary>
    protected async Task<bool> DeclarerNonVu(Immo b, bool demanderConfirmation = true)
    {
        if (demanderConfirmation && !await Confirmer("Bien non vu", $"« {b.Titre} » ({b.Codes}) est introuvable ?\nConfirmez-vous définitivement que le bien est non vu ? Il sera placé dans le local « non vu » de votre organe.", "Oui, déclarer", "Non")) { return false; }
        return await ExecuterOuMettreEnAttente($"immo/nonvu/{b.Id}", new Dictionary<string, string>(), $"Non vu : {b.Titre}");
    }

    /// <summary>Déplacer le bien dans un autre local en scannant le QR du local : POST /immo/changelocal/.</summary>
    protected async Task<bool> DeplacerBien(Immo b)
    {
        var g = await ScannerGuid("Local de destination", $"Scannez le QR code du local où placer « {b.Titre} ».");
        if (g == null) { return false; }
        return await Executer("immo/changelocal/", new Dictionary<string, string> { ["IdImmo"] = b.Id.ToString(), ["QrCodeLocal"] = g.ToString()! });
    }

    /// <summary>Expédier le bien vers un autre organe (il part dans le local de transit) : POST /immo/expedier/.</summary>
    protected async Task<bool> ExpedierBien(Immo b)
    {
        List<Organe> organes;
        try
        {
            organes = (await Api.Get<List<Organe>>("organe/entite/")).Content ?? new List<Organe>();
        }
        catch (Exception e) when (e is not SessionExpireeException) { organes = new List<Organe>(); }
        if (organes.Count == 0) { await S.ChargerArbre(); organes = S.Plat; }
        var page = new ChoixOrganePage(AvecNiveaux(organes)) { Title = "Organe destinataire" };
        await Navigation.PushModalAsync(new NavigationPage(page));
        var destination = await page.Resultat;
        if (destination == null) { return false; }
        if (!await Confirmer("Expédier le bien", $"Expédier « {b.Titre} » vers « {destination.Nom} » ?\nIl sera placé dans le local de transit en attendant sa mise en service.", "Expédier", "Annuler")) { return false; }
        return await Executer("immo/expedier/", new Dictionary<string, string> { ["IdImmo"] = b.Id.ToString(), ["CodeOrgane"] = destination.Id });
    }

    /// <summary>Mise en service (réception) d'un bien : scanner le local d'affectation, POST /immo/misenservice/.</summary>
    protected async Task<bool> MettreEnService(Immo b)
    {
        var g = await ScannerGuid("Mise en service", $"Scannez le QR code du local où « {b.Titre} » est mis en service.");
        if (g == null) { return false; }
        return await Executer("immo/misenservice/", new Dictionary<string, string> { ["IdImmo"] = b.Id.ToString(), ["QrCodeLocal"] = g.ToString()! });
    }

    /// <summary>Affecter une étiquette QR au bien : POST /immo/qrcode/.</summary>
    protected async Task<bool> AffecterQrBien(Immo b)
    {
        var g = await ScannerGuid("Affecter un QR code au bien", "Scannez l'étiquette QR collée sur le bien.");
        if (g == null) { return false; }
        return await Executer("immo/qrcode/", new Dictionary<string, string> { ["Id"] = b.Id.ToString(), ["QrCode"] = g.ToString()! });
    }

    /// <summary>Détacher le QR code du bien, pour le réaffecter ailleurs ou parce qu'il est abîmé.</summary>
    protected async Task<bool> DetacherQrBien(Immo b)
    {
        string choix = await DisplayActionSheet("Que devient l'étiquette actuelle ?", "Annuler", null, "La réaffecter à un autre bien", "La jeter (abîmée ou perdue)");
        string? chemin = choix switch
        {
            "La réaffecter à un autre bien" => $"immo/resetqrcode/{b.Id}",
            "La jeter (abîmée ou perdue)" => $"immo/liveqrcode/{b.Id}",
            _ => null
        };
        if (chemin == null) { return false; }
        if (!await Executer(chemin)) { return false; }
        if (await Confirmer("QR code détaché", "Coller et scanner une nouvelle étiquette maintenant ?", "Scanner", "Plus tard")) { await AffecterQrBien(b); }
        return true;
    }

    /// <summary>Prendre une photo du bien (constat) et l'envoyer : POST multipart /immo/add/photo.</summary>
    protected async Task<bool> AjouterPhoto(Immo b)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) { Informer("La prise de photo n'est pas disponible sur cet appareil.", false); return false; }
            var statut = await Permissions.RequestAsync<Permissions.Camera>();
            if (statut != PermissionStatus.Granted) { Informer("Autorisez l'appareil photo pour ajouter une photo.", false); return false; }
            var fichier = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = b.Titre });
            if (fichier == null) { return false; }
            string? constat = await DisplayPromptAsync("Constat", "Que montre cette photo ? (facultatif)", "Envoyer", "Annuler", "Ex. vue de face, plaque signalétique", 120);
            if (constat == null) { return false; }
            byte[] octets;
            using (var flux = await fichier.OpenReadAsync())
            using (var memoire = new MemoryStream())
            {
                await flux.CopyToAsync(memoire);
                octets = memoire.ToArray();
            }
            // Le serveur découpe les constats sur la virgule (un par photo) : on n'en garde aucune dans le texte.
            var r = await Api.ActionAvecFichier("immo/add/photo", new Dictionary<string, string> { ["IdImmo"] = b.Id.ToString(), ["Constat"] = constat.Replace(",", " ;").Trim() }, octets, "photo.jpg");
            Informer(string.IsNullOrWhiteSpace(r.Message) ? (r.Ok ? "Photo ajoutée." : "La photo n'a pas été enregistrée.") : r.Message, r.Ok);
            return r.Ok;
        }
        catch (Exception e) { await Erreur(e); return false; }
    }

    /// <summary>
    /// Déclasser le bien, comme eTracking : POST /immo/declasser/{id}. Le serveur range le bien dans le local « Déclassé »
    /// de l'entité de l'utilisateur ; il reste actif et pourra ensuite être cédé.
    /// </summary>
    protected async Task<bool> DeclasserBien(Immo b)
    {
        if (!await Confirmer("Déclasser le bien", $"Déclasser « {b.Titre} » ({b.Codes}) ?\nIl sera rangé dans le local des déclassés de votre entité ; il pourra ensuite être cédé.", "Déclasser", "Annuler")) { return false; }
        return await Executer($"immo/declasser/{b.Id}", new Dictionary<string, string>());
    }

    /// <summary>
    /// Clôturer l'inventaire d'un local, comme eTracking : POST /inventaire/details/local/ (IdLocal). Le serveur enregistre
    /// comme inventoriés les biens identifiés et étiquetés du local ; les biens non identifiés ne sont pas concernés.
    /// </summary>
    protected async Task<bool> CloturerLocal(Local l)
    {
        long aCloturer = Math.Max(0, l.QuantiteImmoIdentifier - l.QuantiteImmoInventorier);
        string detail = aCloturer > 0
            ? $"{aCloturer:N0} bien(s) identifié(s) seront enregistrés comme inventoriés pour l'inventaire {S.Annee}."
            : $"Aucun bien identifié n'attend la clôture ({l.Compteur} : total / identifiés / inventoriés).";
        long restants = Math.Max(0, l.QuantiteImmo - l.QuantiteImmoIdentifier);
        if (restants > 0) { detail += $"\n{restants:N0} bien(s) non identifié(s) ne sont pas concernés : déclarez-les non vus s'ils sont introuvables."; }
        if (!await Confirmer("Clôturer l'inventaire du local", $"« {l.Designation} »\n{detail}", "Clôturer", "Annuler")) { return false; }
        return await ExecuterOuMettreEnAttente("inventaire/details/local/", new Dictionary<string, string> { ["IdLocal"] = l.Id.ToString() }, $"Clôture : {l.Designation}");
    }

    /// <summary>Confirmation forte des sorties définitives : le mot à saisir doit être tapé exactement.</summary>
    private async Task<bool> ConfirmerSaisie(string titre, string message, string mot)
    {
        if (!await Confirmer(titre, message, "Continuer", "Annuler")) { return false; }
        string? saisie = await DisplayPromptAsync("Confirmation", $"Pour confirmer, tapez {mot}", titre, "Annuler", mot, mot.Length + 4, Keyboard.Text);
        if (string.Equals(saisie?.Trim(), mot, StringComparison.OrdinalIgnoreCase)) { return true; }
        if (saisie != null) { Informer("Opération annulée : confirmation incorrecte.", false); }
        return false;
    }

    /// <summary>Céder un bien déclassé (sortie définitive du patrimoine) : POST /immo/cession/{id} — le serveur exige un QR code.</summary>
    protected async Task<bool> CederBien(Immo b)
    {
        if (!b.AQrCode) { await DisplayAlert("Cession", "Le serveur n'accepte de céder que les biens étiquetés : affectez d'abord un QR code à ce bien.", "OK"); return false; }
        if (!await ConfirmerSaisie("Céder le bien", $"« {b.Titre} » ({b.Codes}) va sortir définitivement du patrimoine.", "CEDER")) { return false; }
        return await Executer($"immo/cession/{b.Id}", new Dictionary<string, string>());
    }

    /// <summary>Céder tous les biens du local des déclassés de l'entité : POST /immo/local/cession/.</summary>
    protected async Task<bool> CederTousLesDeclasses(int nombre)
    {
        if (!await ConfirmerSaisie("Céder tous les biens déclassés", $"Les {nombre} bien(s) du local des déclassés de votre entité vont sortir définitivement du patrimoine.", "CEDER")) { return false; }
        return await Executer("immo/local/cession/", new Dictionary<string, string>());
    }

    /// <summary>Menu d'un bien de la liste des déclassés : céder ou voir la fiche.</summary>
    protected async Task<bool> MenuBienDeclasse(Immo b)
    {
        string choix = await DisplayActionSheet(b.Titre, "Fermer", "Céder ce bien (sortie définitive)", "Voir la fiche du bien");
        return choix switch
        {
            "Céder ce bien (sortie définitive)" => await CederBien(b),
            "Voir la fiche du bien" => await OuvrirFiche(b),
            _ => false
        };
    }

    /// <summary>Scanner un bien puis enchaîner une action (raccourcis de l'accueil : expédier, réceptionner, déclasser).</summary>
    protected async Task ScannerPuis(string titre, Func<Immo, Task<bool>> action)
    {
        var g = await ScannerGuid(titre, "Scannez l'étiquette QR du bien.");
        if (g == null) { return; }
        try
        {
            var bien = await Api.Premier<Immo>($"immo/qrcode/{g}");
            if (bien == null) { Informer("Aucun bien ne porte ce QR code.", false); return; }
            await action(bien);
        }
        catch (Exception e) { await Erreur(e); }
    }
}
