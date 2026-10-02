using System.Collections.ObjectModel;
using LocateMobile.Models;
using LocateMobile.Services;
using Microsoft.Maui.Controls.Shapes;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace LocateMobile.Pages;

/// <summary>
/// Inventaire rapide d'un local : la caméra reste ouverte et chaque étiquette de bien lue est traitée aussitôt.
/// - bien de ce local : identifié (marqué vu) en « Bon état » avec l'observation par défaut (POST /inventaire/identifier/),
///   repassable en « Mauvais état » d'un toucher ;
/// - bien d'un autre local : proposition « Déplacer ici » (POST /immo/changelocal/) puis identification ;
/// - étiquette inconnue : signalée.
/// Uniquement des routes existantes du serveur.
/// </summary>
public class InventaireRapidePage : PageBase
{
    private readonly Local local;
    private readonly Dictionary<Guid, Immo> parQr = new();
    private readonly List<Immo> biens = new();
    private readonly ObservableCollection<ResultatScan> resultats = new();
    private readonly Dictionary<Guid, DateTime> dernieresLectures = new();
    private readonly SemaphoreSlim verrou = new(1, 1);
    private List<Observation> observationsBon = new();
    private List<Observation> observationsMauvais = new();

    private readonly CameraBarcodeReaderView camera;
    private readonly Label compteur = new() { FontSize = 13, TextColor = Colors.White, FontAttributes = FontAttributes.Bold };
    private readonly ProgressBar barre = new() { ProgressColor = Color.FromArgb("#4ade80"), BackgroundColor = Color.FromRgba(255, 255, 255, 50), HeightRequest = 6 };
    private readonly Border retour = new() { StrokeThickness = 0, Padding = new Thickness(14, 10), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Margin = new Thickness(12, 10, 12, 4), IsVisible = false };
    private readonly Label retourIcone = new() { FontFamily = Icones.Police, FontSize = 22, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center };
    private readonly Label retourTitre = new() { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Colors.White };
    private readonly Label retourTexte = new() { FontSize = 12.5, TextColor = Color.FromRgba(255, 255, 255, 220) };
    private readonly Button torche;
    private CancellationTokenSource? retourCts;

    private InventaireRapidePage(Local local) : base("Inventaire rapide")
    {
        Actualiser = () => Charger();
        this.local = local;
        LargeurMax = 900;

        camera = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions { Formats = BarcodeFormat.QrCode, AutoRotate = true, Multiple = false, TryHarder = false },
            CameraLocation = CameraLocation.Rear,
            IsDetecting = false
        };
        camera.BarcodesDetected += (_, e) =>
        {
            var valeur = e.Results?.FirstOrDefault()?.Value;
            if (!string.IsNullOrWhiteSpace(valeur)) { MainThread.BeginInvokeOnMainThread(async () => await Traiter(valeur)); }
        };

        torche = new Button
        {
            ImageSource = Icones.Image(Icones.Lampe, Colors.White, 20), BackgroundColor = Color.FromRgba(0, 0, 0, 120), CornerRadius = 22,
            WidthRequest = 44, HeightRequest = 44, Padding = 0, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Start, Margin = new Thickness(12)
        };
        torche.Clicked += (_, _) => { camera.IsTorchOn = !camera.IsTorchOn; torche.BackgroundColor = camera.IsTorchOn ? Color.FromArgb("#f59e0b") : Color.FromRgba(0, 0, 0, 120); };

        var viseur = new Border
        {
            Stroke = Colors.White, StrokeThickness = 3, WidthRequest = 210, HeightRequest = 210, InputTransparent = true,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22) }, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center,
            StrokeDashArray = new DoubleCollection { 6, 4 }
        };

        // En-tête posé sur la caméra : local, organe, progression.
        var entete = new VerticalStackLayout
        {
            Spacing = 6, Padding = new Thickness(14, 12), VerticalOptions = LayoutOptions.End, InputTransparent = true,
            Background = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1), GradientStops = { new GradientStop(Colors.Transparent, 0), new GradientStop(Color.FromRgba(0, 0, 0, 190), 1) } },
            Children =
            {
                new Label { Text = local.Designation ?? "Local", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, LineBreakMode = LineBreakMode.TailTruncation },
                new Label { Text = (local.Code ?? "") + (S.OrganeParId(local.CodeOrgane) is Organe o ? " · " + o.Nom : ""), FontSize = 12, TextColor = Color.FromRgba(255, 255, 255, 200), LineBreakMode = LineBreakMode.TailTruncation },
                barre, compteur
            }
        };

        var zoneCamera = new Grid { BackgroundColor = Colors.Black };
        zoneCamera.Add(camera);
        zoneCamera.Add(viseur);
        zoneCamera.Add(entete);
        zoneCamera.Add(torche);

        var ligneRetour = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        ligneRetour.Add(retourIcone, 0, 0);
        ligneRetour.Add(new VerticalStackLayout { Children = { retourTitre, retourTexte } }, 1, 0);
        retour.Content = ligneRetour;

        var liste = new CollectionView
        {
            ItemsSource = resultats, SelectionMode = SelectionMode.None, Margin = new Thickness(12, 4, 12, 0),
            ItemTemplate = new DataTemplate(ModeleResultat),
            EmptyView = Ui.EtatVide(Icones.Qr, "Visez l'étiquette d'un bien", "Chaque bien lu est identifié automatiquement en bon état. Touchez un résultat pour le corriger.")
        };

        var saisir = new Button { Text = "Saisir un code", ImageSource = Icones.Image(Icones.Clavier, Couleurs.Texte, 18), BackgroundColor = Couleurs.Carte, TextColor = Couleurs.Texte, CornerRadius = 12, HeightRequest = 48, BorderColor = Couleurs.Bordure, BorderWidth = 1 };
        saisir.Clicked += async (_, _) =>
        {
            string? code = await DisplayPromptAsync("Saisir un code", "Code de l'étiquette (ou lecture avec une douchette)", "Valider", "Annuler");
            if (!string.IsNullOrWhiteSpace(code)) { await Traiter(code.Trim()); }
        };
        var terminer = new Button { Text = "Terminer", ImageSource = Icones.Image(Icones.Coche, Colors.White, 18), BackgroundColor = Couleurs.Accent, TextColor = Colors.White, CornerRadius = 12, HeightRequest = 48, FontAttributes = FontAttributes.Bold };
        terminer.Clicked += async (_, _) => await Terminer();
        var pied = new Grid { ColumnSpacing = 10, Padding = new Thickness(12, 8, 12, 14), BackgroundColor = Couleurs.Fond, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        pied.Add(saisir, 0, 0);
        pied.Add(terminer, 1, 0);

        var page = new Grid
        {
            RowDefinitions = { new RowDefinition(new GridLength(0.42, GridUnitType.Star)), new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(0.58, GridUnitType.Star)), new RowDefinition(GridLength.Auto) }
        };
        page.Add(zoneCamera, 0, 0);
        page.Add(retour, 0, 1);
        page.Add(ListeActualisable(liste), 0, 2);
        page.Add(pied, 0, 3);
        AfficherBrut(page);
    }

    /// <summary>
    /// Fin de l'inventaire du local : s'il reste des biens non vus, proposer de les déclarer non vus en une fois,
    /// puis proposer de clôturer l'inventaire du local (les biens identifiés passent en « inventorié »).
    /// </summary>
    private async Task Terminer()
    {
        var restants = biens.Where(b => !b.EstVu && !b.EstInventorie).ToList();
        if (restants.Count == 0) { await ProposerCloture(); await Navigation.PopAsync(); return; }
        camera.IsDetecting = false;
        string choix = await DisplayActionSheet($"{restants.Count} bien(s) de ce local n'ont pas été vus", "Continuer l'inventaire", null,
            "Terminer sans rien déclarer", $"Déclarer les {restants.Count} biens non vus", "Voir la liste des biens restants");
        switch (choix)
        {
            case "Terminer sans rien déclarer":
                await ProposerCloture();
                await Navigation.PopAsync();
                return;
            case "Voir la liste des biens restants":
                await DisplayAlert("Biens restants", string.Join("\n", restants.Take(40).Select(b => "• " + b.Titre + (string.IsNullOrWhiteSpace(b.Codes) ? "" : " (" + b.Codes + ")"))) + (restants.Count > 40 ? $"\n… et {restants.Count - 40} autre(s)" : ""), "OK");
                camera.IsDetecting = true;
                return;
        }
        if (!choix.StartsWith("Déclarer", StringComparison.Ordinal)) { camera.IsDetecting = true; return; }

        if (!await Confirmer("Confirmation", $"Confirmez-vous définitivement que ces {restants.Count} biens sont non vus ?\nIls seront placés dans le local « non vu » de votre organe.", "Oui, déclarer", "Non"))
        {
            camera.IsDetecting = true;
            return;
        }
        int ok = 0, attente = 0, refus = 0;
        foreach (var b in restants)
        {
            var champs = new Dictionary<string, string>();
            try
            {
                var r = await Api.Action($"immo/nonvu/{b.Id}", champs);
                if (r.Ok) { ok++; } else { refus++; }
            }
            catch (Services.ReseauException) { Services.FileHorsLigne.Ajouter($"immo/nonvu/{b.Id}", champs, $"Non vu : {b.Titre}"); attente++; }
            catch (Services.SessionExpireeException e) { await Erreur(e); return; }
            catch (Services.ApiException) { refus++; }
            Signaler(null, Icones.OeilBarre, "Déclaration des non vus…", $"{ok + attente + refus} / {restants.Count}");
        }
        await DisplayAlert("Inventaire du local terminé",
            $"{ok} bien(s) déclaré(s) non vu(s)." + (attente > 0 ? $"\n{attente} en attente d'envoi (hors ligne)." : "") + (refus > 0 ? $"\n{refus} refusé(s) par le serveur." : ""), "OK");
        await ProposerCloture();
        await Navigation.PopAsync();
    }

    /// <summary>S'il y a des biens identifiés non encore inventoriés, proposer la clôture du local (POST /inventaire/details/local/).</summary>
    private async Task ProposerCloture()
    {
        int aCloturer = biens.Count(b => b.EstVu && !b.EstInventorie);
        if (aCloturer == 0) { return; }
        camera.IsDetecting = false;
        Local aJour;
        try { aJour = await S.ChargerLocal(local.Id) ?? local; } catch (Exception) { aJour = local; }
        if (aJour.QuantiteImmoIdentifier <= aJour.QuantiteImmoInventorier) { aJour.QuantiteImmoIdentifier = aJour.QuantiteImmoInventorier + aCloturer; }
        await CloturerLocal(aJour);
    }

    /// <summary>Scanne l'étiquette d'un local puis ouvre l'inventaire rapide de ce local.</summary>
    public static async Task Demarrer(INavigation navigation, PageBase appelant)
    {
        string? code = await ScanPage.Scanner(navigation, "Inventaire rapide", "Scannez d'abord le QR code du local à inventorier.");
        var g = ExtraireGuid(code);
        if (code == null) { return; }
        if (g == null) { await appelant.DisplayAlert("Inventaire rapide", "Ce code n'est pas une étiquette Locate.", "OK"); return; }
        try
        {
            var local = await App.Api.Premier<Local>($"local/qrcode/{g}");
            if (local == null) { await appelant.DisplayAlert("Inventaire rapide", "Aucun local ne porte ce QR code. Affectez-le d'abord depuis la liste des locaux.", "OK"); return; }
            await Ouvrir(navigation, local);
        }
        catch (SessionExpireeException) { await App.RetourConnexion("Votre session a expiré, reconnectez-vous."); }
        catch (Exception e) { await appelant.DisplayAlert("Inventaire rapide", e.Message, "OK"); }
    }

    /// <summary>Ouvre l'inventaire rapide d'un local déjà connu.</summary>
    public static async Task Ouvrir(INavigation navigation, Local local)
    {
        var statut = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (statut != PermissionStatus.Granted) { await Permissions.RequestAsync<Permissions.Camera>(); }
        App.Session.NomsLocaux[local.Id] = local.Designation ?? "";
        await navigation.PushAsync(new InventaireRapidePage(local));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        DeviceDisplay.Current.KeepScreenOn = true;
        if (biens.Count == 0) { await Charger(); }
        camera.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        camera.IsDetecting = false;
        camera.IsTorchOn = false;
        DeviceDisplay.Current.KeepScreenOn = false;
    }

    private async Task Charger()
    {
        try
        {
            var r = await Api.Get<List<Immo>>($"immo/local/{local.Id}");
            biens.Clear();
            parQr.Clear();
            foreach (var b in r.Content ?? new List<Immo>())
            {
                biens.Add(b);
                if (b.AQrCode) { parQr[b.QrCode!.Value] = b; }
            }
            MajCompteur();
            observationsBon = (await Api.Get<List<Observation>>("observation/etat/B")).Content ?? new List<Observation>();
            observationsMauvais = (await Api.Get<List<Observation>>("observation/etat/M")).Content ?? new List<Observation>();
        }
        catch (Exception e) { await Erreur(e); }
    }

    private void MajCompteur()
    {
        int vus = biens.Count(b => b.EstVu);
        compteur.Text = biens.Count == 0 ? "Aucun bien enregistré dans ce local" : $"{vus} / {biens.Count} biens vus en {S.Annee}";
        barre.Progress = biens.Count == 0 ? 0 : (double)vus / biens.Count;
    }

    private static Observation? ParDefaut(List<Observation> liste, string motCle)
        => liste.FirstOrDefault(o => (o.Libelle ?? "").Contains(motCle, StringComparison.OrdinalIgnoreCase)) ?? liste.FirstOrDefault();

    /// <summary>Traite une lecture : anti-rebond de 3 secondes par étiquette, un seul traitement à la fois.</summary>
    private async Task Traiter(string valeur)
    {
        var g = ExtraireGuid(valeur);
        if (g == null) { Signaler(false, Icones.CroixRonde, "Code non reconnu", "Ce n'est pas une étiquette Locate."); return; }
        if (dernieresLectures.TryGetValue(g.Value, out var quand) && DateTime.Now - quand < TimeSpan.FromSeconds(3)) { return; }
        dernieresLectures[g.Value] = DateTime.Now;
        if (!await verrou.WaitAsync(0)) { return; }
        try
        {
            if (local.QrCode == g) { Signaler(null, Icones.Porte, "Étiquette du local", "Scannez maintenant les étiquettes des biens."); return; }

            if (parQr.TryGetValue(g.Value, out var bien))
            {
                if (bien.EstVu && resultats.Any(x => x.Bien.Id == bien.Id)) { Signaler(null, Icones.Oeil, "Déjà identifié", bien.Titre); return; }
                await Identifier(bien, "B", null);
                return;
            }

            var ailleurs = await Api.Premier<Immo>($"immo/qrcode/{g}");
            if (ailleurs == null) { Signaler(false, Icones.CroixRonde, "Étiquette inconnue", "Aucun bien ne porte ce QR code."); Ajouter(new ResultatScan(new Immo { Designation = "Étiquette inconnue" }, EtatScan.Inconnu, g.ToString()!)); return; }
            string origine = ailleurs.Local?.Designation ?? await S.NomLocal(ailleurs.IdLocal) ?? "un autre local";
            Signaler(null, Icones.Echange, "Bien d'un autre local", $"{ailleurs.Titre} est enregistré dans « {origine} ». Touchez-le pour le déplacer ici.");
            Ajouter(new ResultatScan(ailleurs, EtatScan.Ailleurs, "Enregistré dans « " + origine + " »"));
        }
        catch (Exception e)
        {
            if (e is SessionExpireeException) { await Erreur(e); return; }
            Signaler(false, Icones.Attention, "Erreur", e.Message);
        }
        finally { verrou.Release(); }
    }

    /// <summary>Marque le bien comme vu avec l'état et l'observation donnés (observation par défaut si null).</summary>
    private async Task<bool> Identifier(Immo bien, string etat, Observation? observation)
    {
        observation ??= etat == "M" ? ParDefaut(observationsMauvais, "mauvais") : ParDefaut(observationsBon, "bon");
        if (observation == null) { Signaler(false, Icones.Attention, "Aucune observation", "Aucune observation n'est paramétrée pour cet état sur le serveur."); return false; }
        var champs = new Dictionary<string, string>
        {
            ["IdImmo"] = bien.Id.ToString(), ["LastEtat"] = etat, ["IdLastObservation"] = observation.Id.ToString()
        };
        bool horsLigne = false;
        try
        {
            var r = await Api.Action("inventaire/identifier/", champs);
            if (!r.Ok) { Signaler(false, Icones.Attention, "Identification refusée", string.IsNullOrWhiteSpace(r.Message) ? bien.Titre : r.Message); return false; }
        }
        catch (Services.ReseauException)
        {
            // Pas de réseau : l'identification est gardée sur la tablette et envoyée au retour du réseau (comme le mode hors ligne d'eTracking).
            Services.FileHorsLigne.Ajouter("inventaire/identifier/", champs, $"Identifier : {bien.Titre}");
            horsLigne = true;
        }

        bien.DateVu = DateTime.Now;
        bien.LastEtat = etat;
        bien.LastEtatString = null;
        bien.IdLastObservation = observation.Id;
        bien.LastObservation = observation.Libelle;
        bien.UserVu = S.Utilisateur?.UserName;
        MajCompteur();
        Signaler(true, Icones.CocheRonde, (etat == "M" ? "Identifié · mauvais état" : "Identifié · bon état") + (horsLigne ? " (hors ligne)" : ""), horsLigne ? bien.Titre + " — sera envoyé au retour du réseau" : bien.Titre);
        Ajouter(new ResultatScan(bien, etat == "M" ? EtatScan.Mauvais : EtatScan.Bon, observation.Libelle ?? ""));
        return true;
    }

    /// <summary>Ajoute (ou remplace) la ligne du bien en tête de la liste de session.</summary>
    private void Ajouter(ResultatScan r)
    {
        int i = r.Bien.Id == 0 ? -1 : resultats.ToList().FindIndex(x => x.Bien.Id == r.Bien.Id);
        if (i >= 0) { resultats.RemoveAt(i); }
        resultats.Insert(0, r);
    }

    /// <summary>Bandeau de retour immédiat (vert = réussi, rouge = problème, bleu = information) avec vibration.</summary>
    private void Signaler(bool? ok, string glyphe, string titre, string texte)
    {
        retourCts?.Cancel();
        retourCts = new CancellationTokenSource();
        var jeton = retourCts.Token;
        retour.BackgroundColor = ok == true ? Color.FromArgb("#15803d") : ok == false ? Color.FromArgb("#b91c1c") : Color.FromArgb("#1d4ed8");
        retourIcone.Text = glyphe;
        retourTitre.Text = titre;
        retourTexte.Text = texte;
        retour.IsVisible = true;
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(ok == false ? 250 : 70)); } catch { /* pas de vibreur */ }
        _ = Task.Run(async () =>
        {
            try { await Task.Delay(4500, jeton); MainThread.BeginInvokeOnMainThread(() => retour.IsVisible = false); } catch (TaskCanceledException) { }
        });
    }

    private View ModeleResultat()
    {
        var grille = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var icone = new Label { FontFamily = Icones.Police, FontSize = 22, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
        icone.SetBinding(Label.TextProperty, nameof(ResultatScan.Glyphe));
        icone.SetBinding(Label.TextColorProperty, nameof(ResultatScan.Couleur));
        var pastille = new Border { WidthRequest = 44, HeightRequest = 44, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = icone, VerticalOptions = LayoutOptions.Center };
        pastille.SetBinding(VisualElement.BackgroundColorProperty, nameof(ResultatScan.Fond));
        grille.Add(pastille, 0, 0);

        var titre = new Label { FontSize = 14.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, LineBreakMode = LineBreakMode.TailTruncation };
        titre.SetBinding(Label.TextProperty, "Bien.Titre");
        var codes = new Label { FontSize = 12, TextColor = Couleurs.Muet, LineBreakMode = LineBreakMode.TailTruncation };
        codes.SetBinding(Label.TextProperty, "Bien.Codes");
        var detail = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold };
        detail.SetBinding(Label.TextProperty, nameof(ResultatScan.Libelle));
        detail.SetBinding(Label.TextColorProperty, nameof(ResultatScan.Couleur));
        grille.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { titre, codes, detail } }, 1, 0);

        var action = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Accent, VerticalOptions = LayoutOptions.Center };
        action.SetBinding(Label.TextProperty, nameof(ResultatScan.Action));
        grille.Add(action, 2, 0);

        var carte = new Border
        {
            BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }, Content = grille,
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.05f, Radius = 8, Offset = new Point(0, 3) }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, _) => { if (((View)s!).BindingContext is ResultatScan r) { await Corriger(r); } };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }

    /// <summary>Toucher un résultat : changer l'état, choisir une autre observation, déplacer ici ou ouvrir la fiche.</summary>
    private async Task Corriger(ResultatScan r)
    {
        if (r.Etat == EtatScan.Inconnu) { return; }
        camera.IsDetecting = false;
        try
        {
            if (r.Etat == EtatScan.Ailleurs)
            {
                if (local.QrCode == null) { Informer("Ce local n'a pas de QR code : impossible d'y déplacer un bien.", false); return; }
                string choixA = await DisplayActionSheet(r.Bien.Titre, "Fermer", null, "Déplacer ici et identifier", "Voir la fiche du bien");
                if (choixA == "Voir la fiche du bien") { await Navigation.PushAsync(new BienPage(r.Bien.Id)); return; }
                if (choixA != "Déplacer ici et identifier") { return; }
                var dep = await Api.Action("immo/changelocal/", new Dictionary<string, string> { ["IdImmo"] = r.Bien.Id.ToString(), ["QrCodeLocal"] = local.QrCode.Value.ToString() });
                if (!dep.Ok) { Signaler(false, Icones.Attention, "Déplacement refusé", dep.Message); return; }
                r.Bien.IdLocal = local.Id;
                biens.Add(r.Bien);
                if (r.Bien.AQrCode) { parQr[r.Bien.QrCode!.Value] = r.Bien; }
                await Identifier(r.Bien, "B", null);
                return;
            }

            string choix = await DisplayActionSheet(r.Bien.Titre, "Fermer", null, "Bon état", "Mauvais état", "Voir la fiche du bien");
            switch (choix)
            {
                case "Bon état":
                case "Mauvais état":
                    var liste = choix == "Bon état" ? observationsBon : observationsMauvais;
                    Observation? obs = null;
                    if (liste.Count > 1)
                    {
                        string libelle = await DisplayActionSheet("Observation", "Annuler", null, liste.Select(o => o.Libelle ?? "").ToArray());
                        obs = liste.FirstOrDefault(o => o.Libelle == libelle);
                        if (obs == null) { return; }
                    }
                    await Identifier(r.Bien, choix == "Bon état" ? "B" : "M", obs);
                    break;
                case "Voir la fiche du bien":
                    await Navigation.PushAsync(new BienPage(r.Bien.Id));
                    break;
            }
        }
        catch (Exception e) { await Erreur(e); }
        finally { camera.IsDetecting = true; }
    }

    public enum EtatScan { Bon, Mauvais, Ailleurs, Inconnu }

    /// <summary>Une ligne de la session d'inventaire rapide.</summary>
    public sealed class ResultatScan
    {
        public ResultatScan(Immo bien, EtatScan etat, string detail) { Bien = bien; Etat = etat; Detail = detail; }
        public Immo Bien { get; }
        public EtatScan Etat { get; }
        public string Detail { get; }

        public string Glyphe => Etat switch { EtatScan.Bon => Icones.CocheRonde, EtatScan.Mauvais => Icones.Attention, EtatScan.Ailleurs => Icones.Echange, _ => Icones.CroixRonde };
        public Color Couleur => Etat switch { EtatScan.Bon => Couleurs.VertFonce, EtatScan.Mauvais => Couleurs.Orange, EtatScan.Ailleurs => Couleurs.Bleu, _ => Couleurs.Rouge };
        public Color Fond => Etat switch { EtatScan.Bon => Couleurs.VertClair, EtatScan.Mauvais => Couleurs.OrangeClair, EtatScan.Ailleurs => Couleurs.BleuClair, _ => Couleurs.RougeClair };
        public string Libelle => Etat switch
        {
            EtatScan.Bon => "Bon état" + (string.IsNullOrWhiteSpace(Detail) ? "" : " · " + Detail),
            EtatScan.Mauvais => "Mauvais état" + (string.IsNullOrWhiteSpace(Detail) ? "" : " · " + Detail),
            EtatScan.Ailleurs => Detail,
            _ => "QR " + Detail
        };
        public string Action => Etat switch { EtatScan.Ailleurs => "Déplacer", EtatScan.Inconnu => "", _ => "Modifier" };
    }
}
