using LocateMobile.Models;

namespace LocateMobile.Pages;

/// <summary>Accueil : progression de l'inventaire du périmètre de l'agent, inventaire rapide et accès rapides.</summary>
public class DashboardPage : PageBase
{
    public DashboardPage() : base("Locate Terrain")
    {
        Actualiser = async () => { await S.ChargerProfil(); await Charger(true); await Synchroniser(false); };
        NavigationPage.SetHasBackButton(this, false);
        // Logo Locate (celui de l'écran de connexion) à la place du titre texte.
        var logo = new Image { Source = "locate_logo.png", HeightRequest = 34, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Start, VerticalOptions = LayoutOptions.Center };
        SemanticProperties.SetDescription(logo, "Locate Terrain");
        NavigationPage.SetTitleView(this, logo);
        ToolbarItems.Add(new ToolbarItem { Text = "Rafraîchir", IconImageSource = Icones.Image(Icones.Actualiser, Couleurs.Texte, 20), Command = new Command(async () => await Charger(true)) });
        ToolbarItems.Add(new ToolbarItem { Text = "Compte", IconImageSource = Icones.Image(Icones.Utilisateur, Couleurs.Texte, 22), Command = new Command(Compte) });
    }

    private bool abonne;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!abonne)
        {
            abonne = true;
            Connectivity.Current.ConnectivityChanged += async (_, e) =>
            {
                if (e.NetworkAccess == NetworkAccess.Internet) { await MainThread.InvokeOnMainThreadAsync(() => Synchroniser(false)); }
            };
        }
        await Charger(false);
        await Synchroniser(false);
    }

    private async Task Charger(bool forcer)
    {
        AfficherAttente();
        try
        {
            await S.ChargerArbre(forcer);
            Construire();
        }
        catch (Exception e)
        {
            if (e is Services.SessionExpireeException) { await Erreur(e); return; }
            AfficherErreur(e.Message, () => Charger(true));
        }
    }

    private void Construire()
    {
        var u = S.Utilisateur!;
        var (total, identifies, inventories) = S.Totaux();
        double progression = total == 0 ? 0 : (double)inventories / total;
        string prenom = string.IsNullOrWhiteSpace(u.Prenom) ? (u.Nom ?? u.UserName ?? "") : u.Prenom!;
        string salut = DateTime.Now.Hour < 18 ? "Bonjour" : "Bonsoir";

        // En-tête en dégradé : salutation, périmètre, anneau de progression de l'inventaire.
        var anneau = new AnneauProgression(92) { Progression = progression };
        var enTeteGrille = new Grid { ColumnSpacing = 14, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        enTeteGrille.Add(Photo(u), 0, 0);
        enTeteGrille.Add(new VerticalStackLayout
        {
            Spacing = 4, VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = $"{salut}, {Majuscule(prenom)}", FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                new HorizontalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        Icones.Ico(Icones.Organigramme, 14, Colors.White),
                        new Label { Text = "Organe d'inventaire : " + NomOrganeInventaire(u), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, LineBreakMode = LineBreakMode.TailTruncation, VerticalOptions = LayoutOptions.Center }
                    }
                },
                new Label { Text = $"{S.Plat.Count} organe(s) dans votre périmètre", FontSize = 12.5, TextColor = Color.FromRgba(255, 255, 255, 200) },
                new FlexLayout
                {
                    Wrap = Microsoft.Maui.Layouts.FlexWrap.Wrap, Margin = new Thickness(0, 6, 0, 0),
                    Children = { PastilleAnnee(), PastilleEnTete(null, $"{inventories:N0} sur {total:N0} biens inventoriés", Color.FromRgba(255, 255, 255, 38)) }
                }
            }
        }, 1, 0);
        enTeteGrille.Add(anneau, 2, 0);
        var enTete = Ui.EnTete(enTeteGrille);

        var kpis = new Grid { ColumnSpacing = 8, Margin = new Thickness(0, 0, 0, 12), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        kpis.Add(Ui.Kpi(Icones.Biens, Couleurs.Gris, Couleurs.GrisClair, total.ToString("N0"), "Biens"), 0, 0);
        kpis.Add(Ui.Kpi(Icones.Oeil, Couleurs.Bleu, Couleurs.BleuClair, identifies.ToString("N0"), "Vus"), 1, 0);
        kpis.Add(Ui.Kpi(Icones.Coche, Couleurs.VertFonce, Couleurs.VertClair, inventories.ToString("N0"), "Inventoriés"), 2, 0);

        // Action principale : l'inventaire rapide d'un local (scan du local puis des biens en continu).
        var rapideGrille = new Grid { ColumnSpacing = 14, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        rapideGrille.Add(Ui.Pastille(Icones.Eclair, Color.FromRgba(255, 255, 255, 45), Colors.White, 50, 26), 0, 0);
        rapideGrille.Add(new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = "Inventaire rapide", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Colors.White },
                new Label { Text = "Scannez un local, puis ses biens à la chaîne", FontSize = 12.5, TextColor = Color.FromRgba(255, 255, 255, 220) }
            }
        }, 1, 0);
        rapideGrille.Add(Icones.Ico(Icones.Fleche, 18, Colors.White), 2, 0);
        var rapide = new Border
        {
            BackgroundColor = Couleurs.Accent, StrokeThickness = 0, Padding = new Thickness(16, 14), Margin = new Thickness(0, 0, 0, 16),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
            Shadow = new Shadow { Brush = Couleurs.Accent, Opacity = 0.35f, Radius = 16, Offset = new Point(0, 6) },
            Content = rapideGrille
        };
        var tapRapide = new TapGestureRecognizer();
        tapRapide.Tapped += async (_, _) => { await rapide.ScaleTo(0.98, 60); await rapide.ScaleTo(1, 60); await InventaireRapidePage.Demarrer(Navigation, this); };
        rapide.GestureRecognizers.Add(tapRapide);

        // Accès rapides en grille.
        var tuiles = new Grid { ColumnSpacing = 10, RowSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        void Tuile(int ligne, int col, string g, Color c, Color f, string t, string st, Action clic)
        {
            while (tuiles.RowDefinitions.Count <= ligne) { tuiles.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); }
            tuiles.Add(Ui.Tuile(g, c, f, t, st, clic), col, ligne);
        }
        Tuile(0, 0, Icones.Organigramme, Couleurs.Accent, Couleurs.BleuClair, "Organes", "Locaux et biens par organe", async () => await Navigation.PushAsync(new OrganesPage()));
        Tuile(0, 1, Icones.Porte, Couleurs.VertFonce, Couleurs.VertClair, "Scanner un local", "Ouvre le contenu du local", async () => await ScannerLocal());
        Tuile(1, 0, Icones.Qr, Couleurs.Orange, Couleurs.OrangeClair, "Scanner un bien", "Ouvre la fiche du bien", async () => await ScannerBien());
        Tuile(1, 1, Icones.OeilBarre, Couleurs.Rouge, Couleurs.RougeClair, "Non vus", "Biens déclarés non vus", () => PrincipalPage.Aller(1));
        Tuile(2, 0, Icones.Livraison, Color.FromArgb("#7c3aed"), Color.FromArgb("#ede9fe"), "Transit", "Biens en transit", () => PrincipalPage.Aller(2));
        Tuile(2, 1, Icones.Archive, Couleurs.Gris, Couleurs.GrisClair, "Déclassés", "Local des déclassés, en attente de cession", () => PrincipalPage.Aller(3));

        var mouvements = new Grid { ColumnSpacing = 10, RowSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) }, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) } };
        mouvements.Add(Ui.Tuile(Icones.Livraison, Color.FromArgb("#0891b2"), Color.FromArgb("#cffafe"), "Expédier un bien", "Vers un autre organe (transit)", async () => await ScannerPuis("Expédier un bien", ExpedierBien)), 0, 0);
        mouvements.Add(Ui.Tuile(Icones.Porte, Couleurs.VertFonce, Couleurs.VertClair, "Mettre en service", "Réceptionner un bien dans un local", async () => await ScannerPuis("Mettre en service", MettreEnService)), 1, 0);
        mouvements.Add(Ui.Tuile(Icones.OeilBarre, Couleurs.Rouge, Couleurs.RougeClair, "Déclarer non vu", "Un bien introuvable", async () => await ScannerPuis("Déclarer non vu", b => DeclarerNonVu(b))), 0, 1);
        mouvements.Add(Ui.Tuile(Icones.Archive, Couleurs.Gris, Couleurs.GrisClair, "Déclasser un bien", "Le ranger dans le local des déclassés", async () => await ScannerPuis("Déclasser un bien", DeclasserBien)), 1, 1);

        Label Section(string texte) => new() { Text = texte, FontSize = 11.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, CharacterSpacing = 1.2, Margin = new Thickness(4, 6, 0, 8) };
        var pile = new VerticalStackLayout { Children = { enTete } };
        if (Services.FileHorsLigne.Nombre > 0) { pile.Children.Add(CarteAttente()); }
        pile.Children.Add(kpis);
        pile.Children.Add(rapide);
        pile.Children.Add(Section("CONSULTER"));
        pile.Children.Add(tuiles);
        pile.Children.Add(Section("MOUVEMENTS"));
        pile.Children.Add(mouvements);
        Afficher(pile);
    }

    /// <summary>Organe d'inventaire affecté (IdInstitution) : racine du périmètre de l'agent ; sans affectation, toute l'institution.</summary>
    private string NomOrganeInventaire(Models.Identite u)
    {
        if (string.IsNullOrWhiteSpace(u.IdInstitution)) { return "toute l'institution"; }
        return S.OrganeParId(u.IdInstitution)?.Nom?.Trim() ?? u.IdInstitution!;
    }

    /// <summary>Année de l'inventaire en cours, bien visible ; orange si aucun inventaire n'est ouvert.</summary>
    private View PastilleAnnee() => S.InventaireOuvert
        ? PastilleEnTete(Icones.Calendrier, $"Inventaire {S.Annee} en cours", Color.FromRgba(255, 255, 255, 64))
        : PastilleEnTete(Icones.Attention, "Aucun inventaire en cours", Color.FromArgb("#f59e0b"));

    private static View PastilleEnTete(string? glyphe, string texte, Color fond)
    {
        var ligne = new HorizontalStackLayout { Spacing = 6 };
        if (glyphe != null) { ligne.Children.Add(Icones.Ico(glyphe, 13, Colors.White)); }
        ligne.Children.Add(new Label { Text = texte, FontSize = 12.5, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, VerticalOptions = LayoutOptions.Center });
        return new Border
        {
            BackgroundColor = fond, StrokeThickness = 0, Padding = new Thickness(10, 4), Margin = new Thickness(0, 0, 6, 6),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(10) }, Content = ligne
        };
    }

    /// <summary>
    /// Photo de l'agent connecté (même fichier que le site : /Content/images/photos/{Photo}), en médaillon.
    /// Les initiales sont dessinées dessous : elles restent visibles si la photo ne se charge pas.
    /// </summary>
    private View Photo(Models.Identite u)
    {
        string initiales = string.Concat(new[] { u.Prenom, u.Nom }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => char.ToUpperInvariant(s!.Trim()[0])));
        var fond = new Grid { WidthRequest = 72, HeightRequest = 72 };
        fond.Add(new Label { Text = initiales.Length > 0 ? initiales : "?", FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Colors.White, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center });
        if (!string.IsNullOrWhiteSpace(u.Photo))
        {
            fond.Add(new Image
            {
                Aspect = Aspect.AspectFill, WidthRequest = 72, HeightRequest = 72,
                Source = new UriImageSource { Uri = new Uri(Api.Url("Content/images/photos/" + Uri.EscapeDataString(u.Photo!))), CacheValidity = TimeSpan.FromDays(7) }
            });
        }
        return new Border
        {
            WidthRequest = 76, HeightRequest = 76, Padding = 0, VerticalOptions = LayoutOptions.Center,
            BackgroundColor = Color.FromRgba(255, 255, 255, 50), Stroke = Colors.White, StrokeThickness = 2,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(), Content = fond
        };
    }

    /// <summary>Opérations faites hors ligne et pas encore envoyées, avec un bouton pour les envoyer tout de suite.</summary>
    private View CarteAttente()
    {
        int n = Services.FileHorsLigne.Nombre;
        var g = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        g.Add(Ui.Pastille(Icones.Actualiser, Couleurs.Carte, Couleurs.Orange, 42, 20), 0, 0);
        g.Add(new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = $"{n} opération(s) en attente d'envoi", FontSize = 14.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Orange },
                new Label { Text = "Faites hors ligne ; envoyées automatiquement au retour du réseau.", FontSize = 12, TextColor = Couleurs.Gris }
            }
        }, 1, 0);
        var envoyer = new Button { Text = "Envoyer", BackgroundColor = Couleurs.Orange, TextColor = Colors.White, CornerRadius = 10, HeightRequest = 40, Padding = new Thickness(14, 0), VerticalOptions = LayoutOptions.Center };
        envoyer.Clicked += async (_, _) => await Synchroniser(true);
        g.Add(envoyer, 2, 0);
        return Carte(g, Couleurs.OrangeClair);
    }

    /// <summary>Envoie la file hors ligne ; <paramref name="manuel"/> affiche le bilan même si rien n'a changé.</summary>
    private async Task Synchroniser(bool manuel)
    {
        if (Services.FileHorsLigne.Nombre == 0) { return; }
        var (envoyees, refusees, restantes) = await Services.FileHorsLigne.Synchroniser(Api);
        if (envoyees + refusees > 0 || manuel)
        {
            Informer(restantes > 0 && envoyees == 0
                ? "Toujours hors ligne : les opérations seront envoyées plus tard."
                : $"{envoyees} opération(s) envoyée(s)" + (refusees > 0 ? $", {refusees} refusée(s)" : "") + (restantes > 0 ? $", {restantes} en attente" : "") + ".",
                refusees == 0 && (envoyees > 0 || restantes == 0));
        }
        if (S.Utilisateur != null && envoyees + refusees > 0) { await S.ChargerArbre(true); Construire(); }
    }

    private static string Majuscule(string texte)
        => string.IsNullOrEmpty(texte) ? texte : char.ToUpper(texte[0]) + texte.Substring(1).ToLowerInvariant();

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

    private async Task ScannerBien()
    {
        var g = await ScannerGuid("Scanner un bien", "Scannez l'étiquette QR du bien.");
        if (g == null) { return; }
        try
        {
            var bien = await Api.Premier<Immo>($"immo/qrcode/{g}");
            if (bien == null) { Informer("Aucun bien ne porte ce QR code.", false); return; }
            await Navigation.PushAsync(new BienPage(bien.Id));
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async void Compte()
    {
        var u = S.Utilisateur;
        string choix = await DisplayActionSheet(u == null ? "Compte" : $"{u.NomComplet} ({u.UserName})", "Fermer", "Se déconnecter", "Recharger les données", "Adresse du serveur");
        switch (choix)
        {
            case "Recharger les données": await Charger(true); break;
            case "Adresse du serveur": await Navigation.PushAsync(new ParametresPage()); break;
            case "Se déconnecter":
                try { await Api.Get<string>("auth/logout"); } catch { /* la session locale est vidée de toute façon */ }
                Api.Deconnexion();
                await App.RetourConnexion();
                break;
        }
    }
}
