using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Contenu d'un local : par article ou en détails ; biens identifiés en bleu, inventoriés en vert ; identifier, clôturer, déplacer.</summary>
public class BiensPage : PageBase
{
    private readonly long idLocal;
    private readonly long? idArticle;
    private bool parArticle;
    private Local? local;

    private readonly CollectionView liste = new() { SelectionMode = SelectionMode.None, Margin = new Thickness(12, 0) };
    private readonly Label resume = new() { FontSize = 12, TextColor = Couleurs.Muet, Margin = new Thickness(14, 4, 14, 4) };
    private readonly ProgressBar progression = new() { ProgressColor = Couleurs.Vert, Margin = new Thickness(14, 0, 14, 6) };
    private readonly Button segArticles = new() { Text = "Par article", CornerRadius = 9, HeightRequest = 40 };
    private readonly Button segDetails = new() { Text = "En détails", CornerRadius = 9, HeightRequest = 40 };
    private readonly VerticalStackLayout enTete = new() { Margin = new Thickness(12, 12, 12, 0) };

    public BiensPage(long idLocal, long? idArticle = null) : base("Local")
    {
        Actualiser = async () => { await ConstruireEnTete(); await ChargerContenu(); };
        this.idLocal = idLocal;
        this.idArticle = idArticle;
        parArticle = idArticle == null;

        segArticles.Clicked += async (_, _) => { parArticle = true; await ChargerContenu(); };
        segDetails.Clicked += async (_, _) => { parArticle = false; await ChargerContenu(); };
        var segments = new Grid { ColumnSpacing = 4, Padding = 3, BackgroundColor = Couleurs.GrisClair, Margin = new Thickness(12, 8, 12, 4), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        segments.Add(segArticles, 0, 0);
        segments.Add(segDetails, 1, 0);
        var borde = new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = segments, BackgroundColor = Couleurs.GrisClair, Margin = new Thickness(12, 8, 12, 4), Padding = 0 };
        segments.Margin = 0;

        var grille = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grille.Add(enTete, 0, 0);
        grille.Add(borde, 0, 1);
        grille.Add(resume, 0, 2);
        grille.Add(progression, 0, 3);
        grille.Add(ListeActualisable(liste), 0, 4);
        AfficherBrut(grille, Flottant("Actions", async () => await MenuActions(), Icones.Eclair));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await ConstruireEnTete();
            if (local == null) { return; }
            await ChargerContenu();
        }
        catch (Exception e) { await Erreur(e); }
    }

    /// <summary>En-tête du local, recalculé à chaque chargement : sa couleur suit l'avancement (rouge, bleu à clôturer, vert clôturé).</summary>
    private async Task ConstruireEnTete()
    {
        local = await S.ChargerLocal(idLocal);
        if (local == null) { AfficherErreur("Local introuvable.", () => Task.CompletedTask); return; }
        Title = local.Designation;
        await S.ChargerArbre();
        var organe = S.OrganeParId(local.CodeOrgane);
        enTete.Children.Clear();
        var ligne = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        // Appui long sur le QR code du local : l'affecter, le détacher ou le jeter.
        var qrLocal = new Border { WidthRequest = 46, HeightRequest = 46, StrokeThickness = 0, BackgroundColor = local.FondQr, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, Padding = new Thickness(4), Content = new Image { Source = local.ImageQr, Aspect = Aspect.AspectFit, WidthRequest = 34, HeightRequest = 34, InputTransparent = true } };
        SemanticProperties.SetHint(qrLocal, "Appui long : affecter, détacher ou jeter le QR code du local");
        AppuiLong(qrLocal, QrDuLocal);
        ligne.Add(qrLocal, 0, 0);
        var pastille = PastilleEtape();
        pastille.BindingContext = local;
        ligne.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { Titre(local.Designation ?? ""), Sous((local.Code ?? "") + (organe != null ? " · " + organe.Nom : "") + (local.AQrCode ? "" : " · sans QR code")), pastille } }, 1, 0);
        var contenu = new VerticalStackLayout { Spacing = 10, Children = { ligne } };
        if (local.Etape == Etape.ACloturer)
        {
            // Des biens ont été identifiés depuis la dernière clôture : bouton bien visible pour clôturer.
            var cloturer = new Button
            {
                Text = $"Clôturer l'inventaire ({Math.Max(0, local.QuantiteImmoIdentifier - local.QuantiteImmoInventorier):N0})",
                ImageSource = Icones.Image(Icones.CocheRonde, Colors.White, 18), BackgroundColor = Couleurs.VertFonce, TextColor = Colors.White,
                FontAttributes = FontAttributes.Bold, CornerRadius = 12, HeightRequest = 46
            };
            cloturer.Clicked += async (_, _) => await Cloturer();
            contenu.Children.Add(cloturer);
        }
        var carte = Carte(contenu, local.EtapeFond, marge: new Thickness(0));
        carte.Stroke = local.EtapeCouleur;
        carte.StrokeThickness = 1.5;
        enTete.Children.Add(carte);
    }

    private async Task Cloturer()
    {
        if (local == null) { return; }
        if (await CloturerLocal(local)) { await S.ChargerArbre(true); await ConstruireEnTete(); await ChargerContenu(); }
    }

    private void StyleSegments()
    {
        segArticles.BackgroundColor = parArticle ? Couleurs.Carte : Colors.Transparent;
        segArticles.TextColor = parArticle ? Couleurs.Texte : Couleurs.Muet;
        segDetails.BackgroundColor = parArticle ? Colors.Transparent : Couleurs.Carte;
        segDetails.TextColor = parArticle ? Couleurs.Muet : Couleurs.Texte;
    }

    private async Task ChargerContenu()
    {
        StyleSegments();
        try
        {
            if (parArticle)
            {
                var r = await Api.Get<List<Article>>($"article/local/{idLocal}");
                var articles = r.Content ?? new List<Article>();
                liste.ItemTemplate = new DataTemplate(ModeleArticle);
                liste.ItemsSource = articles;
                progression.IsVisible = false;
                resume.Text = articles.Count == 0 ? "Aucun bien dans ce local." : $"{articles.Count} article(s). Touchez un article pour voir ses biens.";
            }
            else
            {
                var r = await Api.Get<List<Immo>>(idArticle == null ? $"immo/local/{idLocal}" : $"immo/local/{idLocal}/article/{idArticle}");
                var biens = (r.Content ?? new List<Immo>()).OrderBy(b => b.Rang).ThenBy(b => b.Titre).ToList();
                int inventories = biens.Count(b => b.EstInventorie);
                int aCloturer = biens.Count(b => b.EstVu && !b.EstInventorie);
                liste.ItemTemplate = new DataTemplate(() => Lignes.Bien(b => Navigation.PushAsync(new BienPage(b.Id)), async b => { if (await MenuBien(b)) { await ChargerContenu(); } }));
                liste.ItemsSource = biens;
                progression.IsVisible = biens.Count > 0;
                progression.Progress = biens.Count == 0 ? 0 : (inventories + aCloturer) / (double)biens.Count;
                resume.Text = biens.Count == 0 ? "Aucun bien." : $"{biens.Count} bien(s) · {aCloturer} identifié(s) à clôturer · {inventories} inventorié(s)" + (idArticle != null ? " · filtré sur un article" : "");
            }
        }
        catch (Exception e) { await Erreur(e); }
    }

    private View ModeleArticle()
    {
        var grille = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        var photo = new Image { WidthRequest = 56, HeightRequest = 56, Aspect = Aspect.AspectFill };
        photo.SetBinding(Image.SourceProperty, nameof(Article.UrlPhoto));
        grille.Add(new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, BackgroundColor = Couleurs.GrisClair, Content = photo, WidthRequest = 56, HeightRequest = 56 }, 0, 0);
        var nom = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
        nom.SetBinding(Label.TextProperty, nameof(Article.Designation));
        var sous = new Label { FontSize = 12, TextColor = Couleurs.Muet };
        sous.SetBinding(Label.TextProperty, nameof(Article.SousTitre));
        grille.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { nom, sous } }, 1, 0);
        grille.Add(BadgeLie(nameof(Article.NbreImmo), Couleurs.Accent, Colors.White), 2, 0);
        var carte = new Border { BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = grille };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, _) => { if (((View)s!).BindingContext is Article a) { await Navigation.PushAsync(new BiensPage(idLocal, a.Id)); } };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }

    private async Task MenuActions()
    {
        string qr = local?.AQrCode == true ? "QR code du local (détacher, jeter)" : "Affecter un QR code au local";
        string choix = await DisplayActionSheet(local?.Designation ?? "Actions", "Fermer", null, "Identifier un bien", "Nouveau bien", "Clôturer l'inventaire du local", "Scanner un bien", "Déplacer un bien ici", qr);
        switch (choix)
        {
            case "Identifier un bien": await Identifier(); break;
            case "Nouveau bien": await NouveauBien(); break;
            case "QR code du local (détacher, jeter)":
            case "Affecter un QR code au local": await QrDuLocal(); break;
            case "Clôturer l'inventaire du local": await Cloturer(); break;
            case "Scanner un bien": await ScannerBien(); break;
            case "Déplacer un bien ici": await DeplacerIci(); break;
        }
    }

    private async Task QrDuLocal()
    {
        if (local == null) { return; }
        if (await MenuQrLocal(local)) { await ConstruireEnTete(); await ChargerContenu(); }
    }

    /// <summary>Nouveau bien dans ce local (POST /immo/add/), puis proposer de coller son étiquette QR.</summary>
    private async Task NouveauBien()
    {
        if (local == null) { return; }
        var cree = await NouveauBienPage.Ouvrir(Navigation, local);
        if (cree == null) { return; }
        await S.ChargerArbre(true);
        await ConstruireEnTete();
        parArticle = false;
        await ChargerContenu();
        try
        {
            // Les biens créés sont les plus récents de l'article, sans étiquette.
            var nouveaux = ((await Api.Get<List<Immo>>($"immo/local/{local.Id}")).Content ?? new List<Immo>())
                .Where(b => b.IdArticle == cree.Value.idArticle && !b.AQrCode).OrderByDescending(b => b.Id).Take(cree.Value.nombre).OrderBy(b => b.Id).ToList();
            foreach (var b in nouveaux)
            {
                if (!await Confirmer("Étiquette du bien", $"Coller et scanner l'étiquette QR de « {b.Titre} » ({b.Id}) maintenant ?", "Scanner", nouveaux.Count > 1 ? "Arrêter" : "Plus tard")) { break; }
                await AffecterQrBien(b);
            }
            if (nouveaux.Count > 0) { await ChargerContenu(); }
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task<Immo?> TrouverBien(string titre, string aide)
    {
        var g = await ScannerGuid(titre, aide);
        if (g == null) { return null; }
        var bien = await Api.Premier<Immo>($"immo/qrcode/{g}");
        if (bien == null) { Informer("Aucun bien ne porte ce QR code.", false); }
        return bien;
    }

    /// <summary>Identifier = marquer vu avec état et observation ; si le bien est ailleurs, proposer de le déplacer d'abord.</summary>
    private async Task Identifier()
    {
        try
        {
            var bien = await TrouverBien("Identifier un bien", "Scannez l'étiquette du bien à identifier.");
            if (bien == null) { return; }
            if (bien.IdLocal != idLocal)
            {
                string ancien = bien.Local?.Designation ?? await S.NomLocal(bien.IdLocal) ?? "un autre local";
                if (await Confirmer("Bien hors de ce local", $"« {bien.Titre} » est enregistré dans « {ancien} ».\nVoulez-vous le déplacer ?", "Déplacer", "Non"))
                {
                    var gLocal = await ScannerGuid("Local de destination", "Scannez le QR code du local dans lequel se trouve le bien.");
                    if (gLocal != null) { await Executer("immo/changelocal/", new Dictionary<string, string> { ["IdImmo"] = bien.Id.ToString(), ["QrCodeLocal"] = gLocal.ToString()! }); }
                }
            }
            if (await IdentifierPage.Ouvrir(Navigation, bien)) { Informer($"« {bien.Titre} » identifié."); parArticle = false; await ConstruireEnTete(); await ChargerContenu(); }
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task ScannerBien()
    {
        try
        {
            var bien = await TrouverBien("Scanner un bien", "Scannez l'étiquette QR du bien.");
            if (bien != null) { await Navigation.PushAsync(new BienPage(bien.Id)); }
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task DeplacerIci()
    {
        if (local == null || !local.AQrCode) { Informer("Ce local n'a pas de QR code : affectez-lui d'abord une étiquette.", false); return; }
        try
        {
            var bien = await TrouverBien("Déplacer un bien ici", $"Scannez l'étiquette du bien à placer dans « {local.Designation} ».");
            if (bien == null) { return; }
            if (bien.IdLocal == idLocal) { Informer("Ce bien est déjà dans ce local."); return; }
            string ancien = bien.Local?.Designation ?? await S.NomLocal(bien.IdLocal) ?? "son local actuel";
            if (!await Confirmer("Déplacer le bien", $"Déplacer « {bien.Titre} » de « {ancien} » vers « {local.Designation} » ?")) { return; }
            if (await Executer("immo/changelocal/", new Dictionary<string, string> { ["IdImmo"] = bien.Id.ToString(), ["QrCodeLocal"] = local.QrCode.ToString()! })) { parArticle = false; await ChargerContenu(); }
        }
        catch (Exception e) { await Erreur(e); }
    }
}
