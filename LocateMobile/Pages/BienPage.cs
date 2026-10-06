using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Fiche d'un bien : photos, état, informations, QR code et toutes les actions (inventaire, mouvements, étiquette, photos, déclassement).</summary>
public class BienPage : PageBase
{
    private readonly long id;

    public BienPage(long id) : base("Bien")
    {
        this.id = id;
        Actualiser = () => Charger();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Charger();
    }

    private async Task Charger()
    {
        AfficherAttente();
        try
        {
            var r = await Api.Get<Immo>($"immo/id/{id}");
            var b = r.Content;
            if (b == null) { AfficherErreur("Bien introuvable.", Charger); return; }
            Local? local = null;
            if (b.IdLocal != null) { try { local = await S.ChargerLocal(b.IdLocal.Value); } catch (Services.ApiException) { local = null; } }
            await S.ChargerArbre();
            var organe = S.OrganeParId(local?.CodeOrgane);
            // En-tête : où l'on est (local et organe) ; le nom du bien est déjà en tête de la fiche.
            string nomLocal = local?.Designation?.Trim() ?? "Bien sans local";
            string nomOrgane = organe?.Nom?.Trim() ?? local?.CodeOrgane ?? "";
            Title = nomLocal;
            NavigationPage.SetTitleView(this, new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center, Spacing = 0,
                Children =
                {
                    new Label { Text = nomLocal, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, LineBreakMode = LineBreakMode.TailTruncation },
                    new Label { Text = nomOrgane, FontSize = 12.5, TextColor = Couleurs.Muet, LineBreakMode = LineBreakMode.TailTruncation, IsVisible = nomOrgane.Length > 0 }
                }
            });
            Construire(b, local, organe);
        }
        catch (Exception e)
        {
            if (e is Services.SessionExpireeException) { await Erreur(e); return; }
            AfficherErreur(e.Message, Charger);
        }
    }

    private void Construire(Immo b, Local? local, Organe? organe)
    {
        var contenu = new VerticalStackLayout { Spacing = 0 };

        contenu.Children.Add(new Border
        {
            StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }, BackgroundColor = Couleurs.GrisClair, HeightRequest = 240, Margin = new Thickness(0, 0, 0, 8),
            Content = new Image { Source = ImageSource.FromUri(new Uri(b.UrlPhoto)), Aspect = Aspect.AspectFit }
        });
        var photos = b.ImmoPhotos ?? new List<ImmoPhoto>();
        if (photos.Count > 1)
        {
            var galerie = new HorizontalStackLayout { Spacing = 6 };
            foreach (var ph in photos)
            {
                galerie.Children.Add(new Border { WidthRequest = 84, HeightRequest = 84, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, Content = new Image { Source = ImageSource.FromUri(new Uri(ph.Url)), Aspect = Aspect.AspectFill } });
            }
            contenu.Children.Add(new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = galerie, Margin = new Thickness(0, 0, 0, 8) });
        }

        var badges = new HorizontalStackLayout { Margin = new Thickness(0, 6, 0, 0), Children = { Badge(b.EtatLibelle, b.EtatFond, b.EtatCouleur) } };
        if (b.EstInventorie) { badges.Children.Add(Badge("inventorié" + (Dates.Ok(b.DateInventaire) ? $" le {Dates.Longue(b.DateInventaire)}" : "") + (string.IsNullOrWhiteSpace(b.Inventorieur) ? "" : " par " + b.Inventorieur), Couleurs.VertClair, Couleurs.VertFonce, Icones.CocheRonde)); }
        else if (b.EstVu) { badges.Children.Add(Badge("vu" + (Dates.Ok(b.DateVu) ? $" le {Dates.Longue(b.DateVu)}" : "") + (string.IsNullOrWhiteSpace(b.UserVu) ? "" : " par " + b.UserVu) + " · à clôturer", Couleurs.BleuClair, Couleurs.Bleu, Icones.Oeil)); }
        else { badges.Children.Add(Badge($"pas encore vu (inventaire {S.Annee})", Couleurs.GrisClair, Couleurs.Gris, Icones.OeilBarre)); }
        badges.Children.Add(b.AQrCode ? Badge("QR code", Couleurs.BleuClair, Couleurs.Bleu, Icones.Qr) : Badge("sans QR code", Couleurs.OrangeClair, Couleurs.Orange, Icones.Attention));
        contenu.Children.Add(Carte(new VerticalStackLayout { Children = { Titre(b.Titre), Sous(b.Codes), new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = badges } } }, b.FondLigne));

        var props = new Grid { ColumnSpacing = 8, RowSpacing = 8, Margin = new Thickness(0, 0, 0, 12), ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        int ligne = 0, colonne = 0;
        void Prop(string cle, string? valeur, bool large = false)
        {
            if (string.IsNullOrWhiteSpace(valeur)) { return; }
            if (large && colonne == 1) { ligne++; colonne = 0; }
            props.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var cellule = new Border
            {
                BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(10, 8), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
                Content = new VerticalStackLayout { Children = { new Label { Text = cle.ToUpperInvariant(), FontSize = 10.5, TextColor = Couleurs.Muet }, new Label { Text = valeur, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte } } }
            };
            props.Add(cellule, colonne, ligne);
            if (large) { Grid.SetColumnSpan(cellule, 2); ligne++; colonne = 0; }
            else if (colonne == 0) { colonne = 1; } else { colonne = 0; ligne++; }
        }
        Prop("Local", local == null ? "Aucun local" : local.Designation + (organe != null ? "\n" + organe.Nom : ""), true);
        Prop("Dernière observation", b.LastObservation);
        Prop("Responsable", b.Responsable);
        Prop("Mise en service", Dates.Courte(b.DateMisEnService));
        Prop("Inventorié", b.EstInventorie ? "Oui" + (Dates.Courte(b.DateInventaire) is string d ? " · " + d : "") + (string.IsNullOrWhiteSpace(b.Inventorieur) ? "" : " · " + b.Inventorieur) : "Non");
        Prop("Année comptable", b.LastAnneeComptable > 0 ? b.LastAnneeComptable.ToString() : null);
        Prop("Bien principal", b.IdImmoParent is > 0 ? b.DesignationImmoPrincipal : null, true);
        Prop("Observation", b.Observation, true);
        contenu.Children.Add(props);

        // QR code de l'étiquette, affiché (vérification visuelle, relecture par un autre appareil).
        if (b.AQrCode)
        {
            var qr = new Grid { ColumnSpacing = 14, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
            qr.Add(new ZXing.Net.Maui.Controls.BarcodeGeneratorView
            {
                Format = ZXing.Net.Maui.BarcodeFormat.QrCode, Value = b.QrCode!.Value.ToString(), WidthRequest = 110, HeightRequest = 110,
                ForegroundColor = Colors.Black, BackgroundColor = Colors.White, Margin = 0
            }, 0, 0);
            qr.Add(new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center, Spacing = 2,
                Children = { Titre("Étiquette QR", 14), Sous(b.QrCode.Value.ToString()), Sous(b.Codes) }
            }, 1, 0);
            contenu.Children.Add(Carte(qr));
        }

        // Actions regroupées par thème ; les actions rares ou définitives viennent en dernier.
        Label Section(string texte) => new() { Text = texte, FontSize = 11.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, CharacterSpacing = 1.2, Margin = new Thickness(4, 10, 0, 8) };
        async Task Faire(Func<Task<bool>> action) { if (await action()) { await Charger(); } }

        contenu.Children.Add(Section("INVENTAIRE"));
        contenu.Children.Add(Action(Icones.CocheRonde, Couleurs.VertFonce, "Identifier ce bien", "Le marquer comme vu avec son état", async () =>
        {
            if (await IdentifierPage.Ouvrir(Navigation, b)) { Informer("Bien identifié."); await Charger(); }
        }));
        contenu.Children.Add(Action(Icones.OeilBarre, Couleurs.Rouge, "Déclarer non vu", "Le bien est introuvable", async () => await Faire(() => DeclarerNonVu(b))));

        contenu.Children.Add(Section("MOUVEMENTS"));
        contenu.Children.Add(Action(Icones.Echange, Color.FromArgb("#7c4dff"), "Déplacer ce bien", "Scanner le QR code du local de destination", async () => await Faire(() => DeplacerBien(b))));
        contenu.Children.Add(Action(Icones.Livraison, Color.FromArgb("#0891b2"), "Expédier vers un autre organe", "Le bien passe en transit", async () => await Faire(() => ExpedierBien(b))));
        contenu.Children.Add(Action(Icones.Porte, Couleurs.VertFonce, "Mettre en service", "Réceptionner le bien dans un local", async () => await Faire(() => MettreEnService(b))));
        if (local != null)
        {
            contenu.Children.Add(Action(Icones.Batiment, Couleurs.Accent, "Voir le local", local.Designation ?? "", async () => await Navigation.PushAsync(new BiensPage(local.Id, null))));
        }

        contenu.Children.Add(Section("ÉTIQUETTE ET PHOTOS"));
        contenu.Children.Add(b.AQrCode
            ? Action(Icones.Delier, Couleurs.Orange, "Détacher le QR code", "Pour le réaffecter ou le remplacer", async () => await Faire(() => DetacherQrBien(b)))
            : Action(Icones.Qr, Color.FromArgb("#f97316"), "Affecter un QR code", "Coller et scanner une étiquette", async () => await Faire(() => AffecterQrBien(b))));
        contenu.Children.Add(Action(Icones.Camera, Color.FromArgb("#db2777"), "Ajouter une photo", "Photographier le bien avec un constat", async () => await Faire(() => AjouterPhoto(b))));

        contenu.Children.Add(Section("SORTIE DU PATRIMOINE"));
        if (local?.IdTypeLocal == 4)
        {
            contenu.Children.Add(Action(Icones.CroixRonde, Couleurs.Rouge, "Céder le bien", "Sortie définitive du patrimoine (QR code exigé)", async () => await Faire(() => CederBien(b))));
        }
        else
        {
            contenu.Children.Add(Action(Icones.Archive, Couleurs.Gris, "Déclasser le bien", "Le ranger dans le local des déclassés", async () => await Faire(() => DeclasserBien(b))));
        }

        Afficher(contenu);
    }
}
