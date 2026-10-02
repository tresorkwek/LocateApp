using LocateMobile.Services;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Socle des pages : accès au serveur et à la session, messages, confirmations, petits composants visuels.</summary>
public abstract partial class PageBase : ContentPage
{
    protected ApiClient Api => App.Api;
    protected Session S => App.Session;

    private readonly Grid racine = new();
    private readonly Border bandeau;
    private readonly Label bandeauTexte = new() { TextColor = Colors.White, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center };
    private readonly ActivityIndicator attente = new() { Color = Couleurs.Accent, IsRunning = true, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, WidthRequest = 42, HeightRequest = 42 };
    private CancellationTokenSource? bandeauCts;

    protected PageBase(string titre)
    {
        Title = titre;
        BackgroundColor = Couleurs.Fond;
        bandeau = new Border
        {
            BackgroundColor = Couleurs.Gris, StrokeThickness = 0, Padding = new Thickness(14, 10),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
            Margin = new Thickness(16, 0, 16, 24), VerticalOptions = LayoutOptions.End, HorizontalOptions = LayoutOptions.Center,
            IsVisible = false, Content = bandeauTexte, ZIndex = 50
        };
        Content = racine;
    }

    /* ------------------------------------------------------------------ contenu */

    /// <summary>Affiche un contenu défilant (pages de détail).</summary>
    /// <summary>Largeur maximale du contenu (dp) : au-delà, il est centré (tablettes, mode paysage).</summary>
    protected double LargeurMax { get; set; } = 720;

    private View? contenuAdapte;

    /// <summary>
    /// Rechargement de l'écran quand on glisse vers le bas (RefreshView). Null : pas d'actualisation (formulaires, scanner).
    /// À définir dans le constructeur, avant le premier Afficher / AfficherBrut.
    /// </summary>
    protected Func<Task>? Actualiser { get; set; }

    private bool enActualisation;

    private View AvecActualisation(View vue)
    {
        if (Actualiser == null) { return vue; }
        var rv = new RefreshView { Content = vue, RefreshColor = Couleurs.Accent };
        rv.Command = new Command(async () =>
        {
            enActualisation = true;
            try { await Actualiser(); }
            catch (Exception e) { await Erreur(e); }
            finally { enActualisation = false; rv.IsRefreshing = false; }
        });
        return rv;
    }

    private RefreshView? zoneListe;

    /// <summary>
    /// Liste (CollectionView) actualisable en glissant vers le bas. Le RefreshView doit entourer directement la liste :
    /// autour d'une grille (en-tête + liste), Android ne sait pas si la liste est en haut et le geste n'arrive jamais.
    /// Un seul RefreshView par page, réutilisé quand la page reconstruit sa grille.
    /// </summary>
    protected View ListeActualisable(View liste)
    {
        if (Actualiser == null) { return liste; }
        zoneListe ??= (RefreshView)AvecActualisation(liste);
        return zoneListe;
    }

    private void AdapterLargeur()
    {
        if (contenuAdapte == null || Width <= 0) { return; }
        contenuAdapte.WidthRequest = Math.Min(LargeurMax, Width - 24);
        contenuAdapte.HorizontalOptions = LayoutOptions.Center;
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        AdapterLargeur();
    }

    protected void Afficher(View contenu)
    {
        contenuAdapte = contenu;
        AdapterLargeur();
        racine.Children.Clear();
        racine.Add(AvecActualisation(new ScrollView { Content = new Grid { Children = { contenu } }, Padding = new Thickness(12, 12, 12, 96) }));
        racine.Add(bandeau);
    }

    /// <summary>Affiche un contenu qui gère lui-même son défilement (CollectionView) avec, au besoin, un bouton flottant.</summary>
    protected void AfficherBrut(View contenu, View? flottant = null)
    {
        contenuAdapte = contenu;
        AdapterLargeur();
        racine.Children.Clear();
        racine.Add(contenu); // l'actualisation est portée par la liste elle-même (ListeActualisable)
        if (flottant != null) { racine.Add(flottant); }
        racine.Add(bandeau);
    }

    protected void AfficherAttente()
    {
        if (enActualisation) { return; } // le cercle du glissement vers le bas suffit
        racine.Children.Clear();
        racine.Add(attente);
        racine.Add(bandeau);
    }

    protected void AfficherErreur(string message, Func<Task> reessayer)
    {
        var bouton = new Button { Text = "Réessayer", BackgroundColor = Couleurs.Accent, TextColor = Colors.White, CornerRadius = 10, HorizontalOptions = LayoutOptions.Center };
        bouton.Clicked += async (_, _) => await reessayer();
        Afficher(new VerticalStackLayout
        {
            Spacing = 12, Padding = new Thickness(16, 48), HorizontalOptions = LayoutOptions.Center,
            Children = { new Label { Text = "⚠", FontSize = 40, HorizontalTextAlignment = TextAlignment.Center, TextColor = Couleurs.Orange }, new Label { Text = message, TextColor = Couleurs.Muet, HorizontalTextAlignment = TextAlignment.Center }, bouton }
        });
    }

    /* ------------------------------------------------------------------ messages */

    protected void Informer(string message, bool ok = true)
    {
        bandeauCts?.Cancel();
        bandeauCts = new CancellationTokenSource();
        var jeton = bandeauCts.Token;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bandeau.BackgroundColor = ok ? Couleurs.VertFonce : Couleurs.Rouge;
            bandeauTexte.Text = message;
            bandeau.IsVisible = true;
            try { await Task.Delay(ok ? 3000 : 5000, jeton); bandeau.IsVisible = false; } catch (TaskCanceledException) { }
        });
    }

    protected Task<bool> Confirmer(string titre, string texte, string oui = "Oui", string non = "Non") => DisplayAlert(titre, texte, oui, non);

    /// <summary>Traite une erreur : retour à la connexion si la session est perdue, sinon message.</summary>
    protected async Task Erreur(Exception e)
    {
        if (e is SessionExpireeException) { await App.RetourConnexion("Votre session a expiré, reconnectez-vous."); return; }
        await DisplayAlert("Erreur", e.Message, "OK");
    }

    /// <summary>Exécute une action serveur et affiche son message ; renvoie vrai si elle a réussi.</summary>
    protected async Task<bool> Executer(string chemin, IDictionary<string, string>? formulaire = null)
    {
        try
        {
            var r = await Api.Action(chemin, formulaire);
            Informer(string.IsNullOrWhiteSpace(r.Message) ? (r.Ok ? "Opération effectuée." : "L'opération a échoué.") : r.Message, r.Ok);
            return r.Ok;
        }
        catch (Exception e) { await Erreur(e); return false; }
    }

    /* ------------------------------------------------------------------ composants */

    public static Border Carte(View contenu, Color? fond = null, Thickness? marge = null)
        => new()
        {
            BackgroundColor = fond ?? Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(14, 12),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }, Margin = marge ?? new Thickness(0, 0, 0, 10),
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.06f, Radius = 12, Offset = new Point(0, 4) }, Content = contenu
        };

    public static Border Badge(string texte, Color fond, Color couleur, string? glyphe = null)
    {
        var ligne = new HorizontalStackLayout { Spacing = 4 };
        if (!string.IsNullOrEmpty(glyphe)) { ligne.Children.Add(Icones.Ico(glyphe, 12, couleur)); }
        ligne.Children.Add(new Label { Text = texte, FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = couleur, VerticalOptions = LayoutOptions.Center });
        return new Border
        {
            BackgroundColor = fond, StrokeThickness = 0, Padding = new Thickness(8, 3), Margin = new Thickness(0, 0, 5, 0),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) }, VerticalOptions = LayoutOptions.Center, Content = ligne
        };
    }

    /// <summary>Badge dont le nombre est lié à une propriété du BindingContext (pour les modèles de liste), avec icône optionnelle.</summary>
    public static Border BadgeLie(string propriete, Color fond, Color couleur, string? glyphe = null)
    {
        var label = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = couleur, VerticalOptions = LayoutOptions.Center };
        label.SetBinding(Label.TextProperty, new Binding(propriete, stringFormat: "{0:N0}"));
        var ligne = new HorizontalStackLayout { Spacing = 4 };
        if (!string.IsNullOrEmpty(glyphe)) { ligne.Children.Add(Icones.Ico(glyphe, 12, couleur)); }
        ligne.Children.Add(label);
        return new Border
        {
            BackgroundColor = fond, StrokeThickness = 0, Padding = new Thickness(8, 3), Margin = new Thickness(0, 0, 5, 0),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) }, VerticalOptions = LayoutOptions.Center, Content = ligne
        };
    }

    /// <summary>Pastille d'avancement liée (Local ou Organe) : « À faire » rouge, « À clôturer » bleu, « Clôturé » vert, avec le compteur total / identifiés / inventoriés.</summary>
    public static Border PastilleEtape()
    {
        var icone = new Label { FontFamily = Icones.Police, FontSize = 12, VerticalOptions = LayoutOptions.Center };
        icone.SetBinding(Label.TextProperty, "EtapeGlyphe");
        icone.SetBinding(Label.TextColorProperty, "EtapeCouleur");
        var libelle = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center };
        libelle.SetBinding(Label.TextProperty, "EtapeLibelle");
        libelle.SetBinding(Label.TextColorProperty, "EtapeCouleur");
        var compteur = new Label { FontSize = 12, VerticalOptions = LayoutOptions.Center };
        compteur.SetBinding(Label.TextProperty, "Compteur");
        compteur.SetBinding(Label.TextColorProperty, "EtapeCouleur");
        var bord = new Border
        {
            StrokeThickness = 0, Padding = new Thickness(8, 3), Margin = new Thickness(0, 6, 0, 0), HorizontalOptions = LayoutOptions.Start,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(8) },
            Content = new HorizontalStackLayout { Spacing = 5, Children = { icone, libelle, new Label { Text = "·", FontSize = 12, TextColor = Couleurs.Muet }, compteur } }
        };
        bord.SetBinding(VisualElement.BackgroundColorProperty, "EtapeFond");
        return bord;
    }

    /// <summary>Barre verticale colorée selon l'avancement (bord gauche des cartes de locaux et d'organes).</summary>
    public static BoxView BarreEtape()
    {
        var barre = new BoxView { WidthRequest = 5, CornerRadius = 3, VerticalOptions = LayoutOptions.Fill, Margin = new Thickness(0, 2) };
        barre.SetBinding(BoxView.ColorProperty, "EtapeCouleur");
        return barre;
    }

    public static Label Titre(string texte, double taille = 16) => new() { Text = texte, FontSize = taille, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
    public static Label Sous(string? texte) => new() { Text = texte ?? "", FontSize = 12.5, TextColor = Couleurs.Muet };

    public static Button BoutonPrincipal(string texte, Color? fond = null)
        => new() { Text = texte, BackgroundColor = fond ?? Couleurs.Accent, TextColor = Colors.White, CornerRadius = 12, HeightRequest = 50, FontAttributes = FontAttributes.Bold };

    /// <summary>Ligne d'action (icône dans une pastille colorée, titre, sous-titre, chevron).</summary>
    public static Border Action(string glyphe, Color couleurIcone, string titre, string sousTitre, Action clic)
    {
        var grille = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        grille.Add(Ui.Pastille(glyphe, couleurIcone.WithAlpha(0.14f), couleurIcone, 44, 22), 0, 0);
        grille.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { Titre(titre, 15), Sous(sousTitre) } }, 1, 0);
        grille.Add(Icones.Ico(Icones.Chevron, 14, Couleurs.Muet), 2, 0);
        var carte = Carte(grille);
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => { await carte.ScaleTo(0.98, 60); await carte.ScaleTo(1, 60); clic(); };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }

    /// <summary>Bouton flottant rond en bas à droite.</summary>
    public static Button Flottant(string texte, Action clic, string? glyphe = Icones.Qr)
    {
        var b = new Button
        {
            Text = texte, ImageSource = glyphe == null ? null : Icones.Image(glyphe, Colors.White, 20), ContentLayout = new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 8), FontSize = 15, FontAttributes = FontAttributes.Bold, BackgroundColor = Couleurs.Accent, TextColor = Colors.White,
            CornerRadius = 28, HeightRequest = 56, Padding = new Thickness(18, 0), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(16), Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.3f, Radius = 14, Offset = new Point(0, 6) }, ZIndex = 20
        };
        b.Clicked += (_, _) => clic();
        return b;
    }

    public static Guid? ExtraireGuid(string? texte)
    {
        if (string.IsNullOrWhiteSpace(texte)) { return null; }
        var m = System.Text.RegularExpressions.Regex.Match(texte, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        return m.Success && Guid.TryParse(m.Value, out var g) ? g : null;
    }

    /// <summary>Ouvre le scanner et renvoie le GUID lu, ou null si l'utilisateur annule ou si le code n'est pas une étiquette Locate.</summary>
    protected async Task<Guid?> ScannerGuid(string titre, string aide)
    {
        string? code = await ScanPage.Scanner(Navigation, titre, aide);
        if (code == null) { return null; }
        var g = ExtraireGuid(code);
        if (g == null) { Informer("Ce code n'est pas une étiquette Locate.", false); }
        return g;
    }
}
