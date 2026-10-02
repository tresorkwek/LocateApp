using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Locaux d'un organe : QR code présent ou absent, compteurs, actions (biens, modifier, déplacer, QR code).</summary>
public class LocauxPage : PageBase
{
    private readonly Organe organe;
    private readonly CollectionView liste = new() { SelectionMode = SelectionMode.None, Margin = new Thickness(12, 0) };
    private readonly Label resume = new() { FontSize = 12, TextColor = Couleurs.Muet, Margin = new Thickness(14, 6, 14, 6) };

    private readonly Entry recherche = new() { Placeholder = "Rechercher un local (nom ou code)" };
    private List<Local> tous = new();

    public LocauxPage(Organe organe) : base(organe.Nom ?? "Locaux")
    {
        Actualiser = () => Charger();
        this.organe = organe;
        liste.ItemTemplate = new DataTemplate(ModeleLocal);
        liste.EmptyView = Ui.EtatVide(Icones.Porte, "Aucun local", "Aucun local ne correspond. Créez-en un avec le bouton « + » en haut.");
        recherche.TextChanged += (_, _) => Filtrer();
        ToolbarItems.Add(new ToolbarItem { Text = "Nouveau local", IconImageSource = Icones.Image(Icones.Plus, Couleurs.Texte, 20), Command = new Command(async () => await NouveauLocal()) });

        var enTete = Carte(new VerticalStackLayout
        {
            Children =
            {
                Titre(organe.Nom ?? ""), Sous(organe.SousTitre),
                new HorizontalStackLayout
                {
                    Margin = new Thickness(0, 6, 0, 0),
                    Children =
                    {
                        Badge(organe.NbreBien.ToString("N0"), Couleurs.GrisClair, Couleurs.Gris, Icones.Biens),
                        Badge(organe.NbreBienIdentifie.ToString("N0"), Couleurs.BleuClair, Couleurs.Bleu, Icones.Oeil),
                        Badge(organe.NbreBienInventorie.ToString("N0"), Couleurs.VertClair, Couleurs.VertFonce, Icones.Coche)
                    }
                },
                Badge($"{organe.EtapeLibelle} · {organe.Compteur}", organe.EtapeFond, organe.EtapeCouleur, organe.EtapeGlyphe)
            }
        }, marge: new Thickness(12, 12, 12, 0));

        var grille = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grille.Add(enTete, 0, 0);
        grille.Add(new ContentView { Padding = new Thickness(12, 10, 12, 0), Content = Ui.Recherche(recherche) }, 0, 1);
        grille.Add(resume, 0, 2);
        grille.Add(ListeActualisable(liste), 0, 3);
        // Deux actions flottantes, comme dans eTracking : créer un local, scanner un local.
        var nouveau = Flottant("Nouveau local", async () => await NouveauLocal(), Icones.Plus);
        nouveau.BackgroundColor = Couleurs.Carte;
        nouveau.TextColor = Couleurs.Accent;
        nouveau.ImageSource = Icones.Image(Icones.Plus, Couleurs.Accent, 20);
        nouveau.Margin = new Thickness(16, 0, 16, 10);
        var scanner = Flottant("Scanner un local", async () => await ScannerLocal());
        scanner.Margin = new Thickness(16, 0, 16, 16);
        AfficherBrut(grille, new VerticalStackLayout { HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End, ZIndex = 20, Children = { nouveau, scanner } });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Charger();
    }

    private async Task Charger()
    {
        try
        {
            var r = await Api.Get<List<Local>>($"local/organe/{Uri.EscapeDataString(organe.Id)}");
            var locaux = r.Content ?? new List<Local>();
            foreach (var l in locaux) { S.NomsLocaux[l.Id] = l.Designation ?? ""; }
            tous = locaux;
            Filtrer();
            resume.Text = locaux.Count == 0 ? "Aucun local pour cet organe." : $"{locaux.Count} local(aux). Touchez un local pour ses biens ; touchez son QR pour l'affecter ou le changer ; ⋮ pour les actions.";
        }
        catch (Exception e) { await Erreur(e); }
    }

    private void Filtrer()
    {
        string q = (recherche.Text ?? "").Trim().ToLowerInvariant();
        liste.ItemsSource = q.Length == 0 ? tous : tous.Where(l => ((l.Designation ?? "") + " " + (l.Code ?? "")).ToLowerInvariant().Contains(q)).ToList();
    }

    /// <summary>Créer un local dans cet organe (POST /local/add/), puis proposer de lui coller son étiquette QR.</summary>
    private async Task NouveauLocal()
    {
        var page = new NouveauLocalPage(organe);
        await Navigation.PushModalAsync(new NavigationPage(page));
        var saisie = await page.Resultat;
        if (saisie == null) { return; }
        bool ok = await Executer("local/add/", new Dictionary<string, string>
        {
            ["Code"] = saisie.Value.code, ["Designation"] = saisie.Value.designation, ["CodeOrgane"] = organe.Id,
            ["IsSpace"] = saisie.Value.espace ? "true" : "false", ["IdTypeLocal"] = "1"
        });
        if (!ok) { return; }
        await S.ChargerArbre(true);
        await Charger();
        var cree = tous.FirstOrDefault(l => string.Equals(l.Designation, saisie.Value.designation, StringComparison.OrdinalIgnoreCase) && (string.IsNullOrEmpty(saisie.Value.code) || string.Equals(l.Code, saisie.Value.code, StringComparison.OrdinalIgnoreCase)));
        if (cree != null && !cree.AQrCode && await Confirmer("Local créé", $"Coller et scanner l'étiquette QR de « {cree.Designation} » maintenant ?", "Scanner", "Plus tard"))
        {
            await AffecterQr(cree);
        }
    }

    private View ModeleLocal()
    {
        var grille = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };

        // Pictogramme : image d'un QR code vert si le local a son étiquette, QR code gris barré sinon (toucher = affecter / changer).
        var qrImage = new Image { WidthRequest = 34, HeightRequest = 34, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        qrImage.SetBinding(Image.SourceProperty, nameof(Local.ImageQr));
        qrImage.SetBinding(SemanticProperties.DescriptionProperty, nameof(Local.QrLibelle));
        var qr = new Border { WidthRequest = 46, HeightRequest = 46, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, Padding = new Thickness(4), Content = qrImage, VerticalOptions = LayoutOptions.Center };
        qr.SetBinding(VisualElement.BackgroundColorProperty, nameof(Local.FondQr));
        var tapQr = new TapGestureRecognizer();
        tapQr.Tapped += async (s, _) => { if (((View)s!).BindingContext is Local l) { await ChangerQr(l); } };
        qr.GestureRecognizers.Add(tapQr);

        var nom = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
        nom.SetBinding(Label.TextProperty, nameof(Local.Designation));
        var sous = new Label { FontSize = 12, TextColor = Couleurs.Muet };
        sous.SetBinding(Label.TextProperty, nameof(Local.SousTitre));
        var badges = new HorizontalStackLayout
        {
            Margin = new Thickness(0, 4, 0, 0),
            Children =
            {
                BadgeLie(nameof(Local.QuantiteImmo), Couleurs.GrisClair, Couleurs.Gris, Icones.Biens),
                BadgeLie(nameof(Local.QuantiteImmoIdentifier), Couleurs.BleuClair, Couleurs.Bleu, Icones.Oeil),
                BadgeLie(nameof(Local.QuantiteImmoInventorier), Couleurs.VertClair, Couleurs.VertFonce, Icones.Coche)
            }
        };
        var corps = new VerticalStackLayout { Children = { nom, sous, badges, PastilleEtape() } };
        var tapCorps = new TapGestureRecognizer();
        tapCorps.Tapped += async (s, _) => { if (((View)s!).BindingContext is Local l) { await Ouvrir(l); } };
        corps.GestureRecognizers.Add(tapCorps);

        var menu = new Button { Text = "⋮", FontSize = 22, BackgroundColor = Colors.Transparent, TextColor = Couleurs.Muet, WidthRequest = 40, Padding = 0, VerticalOptions = LayoutOptions.Center };
        menu.Clicked += async (s, _) => { if (((View)s!).BindingContext is Local l) { await Menu(l); } };

        // Bord gauche coloré selon l'avancement : rouge à faire, bleu à clôturer, vert clôturé.
        grille.Add(BarreEtape(), 0, 0);
        grille.Add(qr, 1, 0);
        grille.Add(corps, 2, 0);
        grille.Add(menu, 3, 0);
        return new Border { BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(6, 10, 10, 10), Margin = new Thickness(0, 0, 0, 8), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = grille };
    }

    private async Task Ouvrir(Local l)
    {
        if (l.AQrCode) { await Navigation.PushAsync(new BiensPage(l.Id)); return; }
        if (await Confirmer("Local sans QR code", $"Le local « {l.Designation} » n'a pas de QR code.\nVoulez-vous en scanner un pour le lui affecter ?", "Scanner", "Plus tard"))
        {
            await AffecterQr(l);
        }
    }

    private async Task AffecterQr(Local l)
    {
        if (await AffecterQrLocal(l)) { await Charger(); }
    }

    private async Task ChangerQr(Local l)
    {
        if (await MenuQrLocal(l)) { await Charger(); }
    }

    private async Task Menu(Local l)
    {
        string choix = await DisplayActionSheet(l.Designation, "Fermer", null, "Voir les biens", "Clôturer l'inventaire du local", "Modifier le local (site web)", "Déplacer vers un autre organe", l.AQrCode ? "Changer le QR code" : "Affecter un QR code");
        switch (choix)
        {
            case "Voir les biens": await Navigation.PushAsync(new BiensPage(l.Id)); break;
            case "Clôturer l'inventaire du local": if (await CloturerLocal(l)) { await S.ChargerArbre(true); await Charger(); } break;
            case "Modifier le local (site web)": await Browser.Default.OpenAsync(Api.Url($"local/modify/{l.Id}"), BrowserLaunchMode.SystemPreferred); break;
            case "Déplacer vers un autre organe": await Deplacer(l); break;
            case "Changer le QR code":
            case "Affecter un QR code": await ChangerQr(l); break;
        }
    }

    /// <summary>Déplacer = changer l'organe du local via la route de modification existante (tous les autres champs sont conservés).</summary>
    private async Task Deplacer(Local l)
    {
        await S.ChargerArbre();
        var candidats = S.Plat.Where(o => !string.Equals(o.Id, l.CodeOrgane, StringComparison.OrdinalIgnoreCase)).ToList();
        var page = new ChoixOrganePage(candidats);
        await Navigation.PushModalAsync(new NavigationPage(page));
        var destination = await page.Resultat;
        if (destination == null) { return; }
        if (!await Confirmer("Déplacer le local", $"Déplacer « {l.Designation} » vers « {destination.Nom} » ?")) { return; }
        bool ok = await Executer("local/modify/", new Dictionary<string, string>
        {
            ["Id"] = l.Id.ToString(), ["Code"] = l.Code ?? "", ["Designation"] = l.Designation ?? "", ["CodeOrgane"] = destination.Id,
            ["IsSpace"] = l.IsSpace ? "true" : "false", ["IsActive"] = l.IsActive ? "true" : "false", ["IdTypeLocal"] = l.IdTypeLocal?.ToString() ?? ""
        });
        if (ok) { await S.ChargerArbre(true); await Charger(); }
    }

    private async Task ScannerLocal()
    {
        var g = await ScannerGuid("Scanner un local", "Scannez le QR code collé sur la porte du local.");
        if (g == null) { return; }
        try
        {
            var local = await Api.Premier<Local>($"local/qrcode/{g}");
            if (local == null) { Informer("Aucun local ne porte ce QR code.", false); return; }
            S.NomsLocaux[local.Id] = local.Designation ?? "";
            await Navigation.PushAsync(new BiensPage(local.Id));
        }
        catch (Exception e) { await Erreur(e); }
    }
}

/// <summary>Choix d'un organe de destination dans l'organigramme (recherche + liste indentée).</summary>
public class ChoixOrganePage : ContentPage
{
    private readonly TaskCompletionSource<Organe?> tcs = new();
    public Task<Organe?> Resultat => tcs.Task;

    public ChoixOrganePage(List<Organe> organes)
    {
        Title = "Organe de destination";
        BackgroundColor = Couleurs.Fond;
        var recherche = new SearchBar { Placeholder = "Rechercher l'organe" };
        var liste = new CollectionView { ItemsSource = organes, SelectionMode = SelectionMode.Single, Margin = new Thickness(12, 0) };
        liste.ItemTemplate = new DataTemplate(() =>
        {
            var nom = new Label { FontSize = 15, TextColor = Couleurs.Texte };
            nom.SetBinding(Label.TextProperty, nameof(Organe.Nom));
            var id = new Label { FontSize = 12, TextColor = Couleurs.Muet };
            id.SetBinding(Label.TextProperty, nameof(Organe.Id));
            var b = new Border { BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(12, 10), Margin = new Thickness(0, 0, 0, 6), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, Content = new VerticalStackLayout { Children = { nom, id } } };
            b.SetBinding(View.MarginProperty, new Binding(nameof(Organe.Retrait), converter: new MargeConverter()));
            return b;
        });
        liste.SelectionChanged += async (_, e) => { if (e.CurrentSelection.FirstOrDefault() is Organe o) { tcs.TrySetResult(o); await Navigation.PopModalAsync(); } };
        recherche.TextChanged += (_, _) =>
        {
            string q = (recherche.Text ?? "").Trim().ToLowerInvariant();
            liste.ItemsSource = q.Length == 0 ? organes : organes.Where(o => (o.Nom + " " + o.Id + " " + o.Sigle).ToLowerInvariant().Contains(q)).ToList();
        };
        ToolbarItems.Add(new ToolbarItem("Annuler", null, async () => { tcs.TrySetResult(null); await Navigation.PopModalAsync(); }));
        var grille = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grille.Add(recherche, 0, 0);
        grille.Add(liste, 0, 1);
        Content = grille;
    }

    protected override bool OnBackButtonPressed() { tcs.TrySetResult(null); return base.OnBackButtonPressed(); }

    private sealed class MargeConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value is Thickness t ? new Thickness(t.Left, 0, 0, 6) : new Thickness(0, 0, 0, 6);
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
    }
}

/// <summary>Saisie d'un nouveau local : code, désignation, espace (zone sans porte) ou local réel.</summary>
public class NouveauLocalPage : PageBase
{
    private readonly TaskCompletionSource<(string code, string designation, bool espace)?> tcs = new();
    public Task<(string code, string designation, bool espace)?> Resultat => tcs.Task;
    private bool repondu;

    public NouveauLocalPage(Organe organe) : base("Nouveau local")
    {
        var code = new Entry { Placeholder = "Ex. DAG00031", FontSize = 16 };
        var designation = new Entry { Placeholder = "Ex. Bureau du chef de service", FontSize = 16 };
        var espace = new Switch { OnColor = Couleurs.Accent, VerticalOptions = LayoutOptions.Center };
        Border Champ(View v) => new() { BackgroundColor = Couleurs.Carte, StrokeThickness = 1, Stroke = Couleurs.Bordure, Padding = new Thickness(12, 0), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = v };
        Label Etiquette(string t) => new() { Text = t, FontSize = 12.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, Margin = new Thickness(2, 8, 0, 4) };

        var ligneEspace = new Grid { ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, Margin = new Thickness(0, 10, 0, 0) };
        ligneEspace.Add(new VerticalStackLayout { Children = { Titre("C'est un espace", 15), Sous("Couloir, hall, zone ouverte : pas de porte où coller l'étiquette") } }, 0, 0);
        ligneEspace.Add(espace, 1, 0);

        var creer = BoutonPrincipal("Créer le local");
        creer.ImageSource = Icones.Image(Icones.Plus, Colors.White, 18);
        creer.Clicked += async (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(designation.Text)) { Informer("La désignation est obligatoire.", false); return; }
            await Fermer((code.Text?.Trim() ?? "", designation.Text.Trim(), espace.IsToggled));
        };
        var annuler = new Button { Text = "Annuler", BackgroundColor = Couleurs.Carte, TextColor = Couleurs.Texte, CornerRadius = 12, HeightRequest = 48, BorderColor = Couleurs.Bordure, BorderWidth = 1 };
        annuler.Clicked += async (_, _) => await Fermer(null);

        Afficher(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                Carte(new VerticalStackLayout { Children = { Titre("Organe"), Sous((organe.Nom ?? "") + " · " + organe.Id) } }, marge: new Thickness(0, 0, 0, 6)),
                Etiquette("DÉSIGNATION *"), Champ(designation),
                Etiquette("CODE"), Champ(code),
                ligneEspace,
                new BoxView { HeightRequest = 14, Color = Colors.Transparent },
                creer, annuler
            }
        });
    }

    private async Task Fermer((string, string, bool)? valeur)
    {
        if (repondu) { return; }
        repondu = true;
        tcs.TrySetResult(valeur);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed() { _ = Fermer(null); return true; }
}
