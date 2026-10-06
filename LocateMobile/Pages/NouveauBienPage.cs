using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>
/// Nouveau bien dans un local, comme « Nouveau bien » du site : article, état constaté, observation,
/// code immo facultatif et quantité, puis POST /immo/add/ (route du formulaire web). Les articles du local sont proposés d'abord ;
/// la recherche porte sur tout le catalogue (chargé une fois par session).
/// </summary>
public class NouveauBienPage : PageBase
{
    private static List<Article>? catalogue;

    private readonly Local local;
    private readonly TaskCompletionSource<(long idArticle, int nombre)?> tcs = new();
    private readonly Entry recherche = new() { Placeholder = "Rechercher un article (désignation, marque, code)", FontSize = 16 };
    private readonly CollectionView articles = new() { SelectionMode = SelectionMode.None, HeightRequest = 300 };
    private readonly Label articleChoisi = new() { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
    private readonly Label aideArticle = new() { FontSize = 12, TextColor = Couleurs.Muet, Text = "Touchez un article dans la liste." };
    private readonly Entry codeImmo = new() { Placeholder = "Facultatif", FontSize = 16 };
    private readonly Label nombre = new() { Text = "1", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, WidthRequest = 48 };
    private readonly Stepper quantite = new() { Minimum = 1, Maximum = 50, Increment = 1, Value = 1, VerticalOptions = LayoutOptions.Center };
    private readonly Picker observations = new() { Title = "Choisissez une observation", FontSize = 16 };
    private List<Article> duLocal = new();
    private Article? article;
    private string etat = "B";
    private bool repondu;

    private NouveauBienPage(Local local) : base("Nouveau bien")
    {
        this.local = local;
        Actualiser = async () =>
        {
            duLocal = (await Api.Get<List<Article>>($"article/local/{local.Id}")).Content ?? new List<Article>();
            catalogue = (await Api.Get<List<Article>>("article/")).Content?.OrderBy(a => a.Designation).ToList();
            Filtrer();
            await ChargerObservations();
        };
        articles.ItemTemplate = new DataTemplate(ModeleArticle);
        recherche.TextChanged += (_, _) => Filtrer();
        quantite.ValueChanged += (_, e) => nombre.Text = ((int)e.NewValue).ToString();
        observations.TextColor = Couleurs.Texte;
        observations.TitleColor = Couleurs.Muet;

        Border Champ(View v) => new() { BackgroundColor = Couleurs.Carte, StrokeThickness = 1, Stroke = Couleurs.Bordure, Padding = new Thickness(12, 2), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = v };
        Label Section(string t) => new() { Text = t, FontSize = 11.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, CharacterSpacing = 1.2, Margin = new Thickness(4, 8, 0, 2) };

        var bon = ChoixEtat(Icones.CocheRonde, "Bon état", Couleurs.VertFonce);
        var mauvais = ChoixEtat(Icones.Attention, "Mauvais état", Couleurs.Rouge);
        void Styler()
        {
            bon.Stroke = etat == "B" ? Couleurs.VertFonce : Couleurs.Bordure; bon.BackgroundColor = etat == "B" ? Couleurs.VertClair : Couleurs.Carte;
            mauvais.Stroke = etat == "M" ? Couleurs.Rouge : Couleurs.Bordure; mauvais.BackgroundColor = etat == "M" ? Couleurs.RougeClair : Couleurs.Carte;
        }
        Toucher(bon, async () => { etat = "B"; Styler(); await ChargerObservations(); });
        Toucher(mauvais, async () => { etat = "M"; Styler(); await ChargerObservations(); });
        Styler();
        var etats = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        etats.Add(bon, 0, 0);
        etats.Add(mauvais, 1, 0);

        var ligneQuantite = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };
        ligneQuantite.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { Titre("Nombre de biens", 15), Sous("Autant de biens identiques que d'objets à enregistrer") } }, 0, 0);
        ligneQuantite.Add(nombre, 1, 0);
        ligneQuantite.Add(quantite, 2, 0);

        var creer = BoutonPrincipal("Créer le bien");
        creer.ImageSource = Icones.Image(Icones.Plus, Colors.White, 18);
        creer.HeightRequest = 54;
        creer.Clicked += async (_, _) => await Creer();
        var annuler = new Button { Text = "Annuler", BackgroundColor = Couleurs.Carte, TextColor = Couleurs.Texte, CornerRadius = 12, HeightRequest = 48, BorderColor = Couleurs.Bordure, BorderWidth = 1 };
        annuler.Clicked += async (_, _) => await Fermer(null);

        Afficher(new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Carte(new VerticalStackLayout { Children = { Titre("Local"), Sous((local.Designation ?? "") + (string.IsNullOrEmpty(local.Code) ? "" : " · " + local.Code)) } }, marge: new Thickness(0, 0, 0, 4)),
                Section("ARTICLE *"),
                Champ(recherche),
                Carte(new VerticalStackLayout { Spacing = 2, Children = { articleChoisi, aideArticle } }, marge: new Thickness(0, 4, 0, 0)),
                articles,
                Section("ÉTAT CONSTATÉ"), etats,
                Section("OBSERVATION"), Champ(observations),
                Section("CODE IMMO"), Champ(codeImmo),
                Carte(ligneQuantite, marge: new Thickness(0, 10, 0, 0)),
                new BoxView { HeightRequest = 8, Color = Colors.Transparent },
                creer, annuler
            }
        });
        AfficherChoix();
    }

    private static Border ChoixEtat(string glyphe, string titre, Color couleur) => new()
    {
        StrokeThickness = 2, Padding = new Thickness(12), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
        Content = new HorizontalStackLayout { Spacing = 8, HorizontalOptions = LayoutOptions.Center, Children = { Icones.Ico(glyphe, 22, couleur), new Label { Text = titre, FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, VerticalOptions = LayoutOptions.Center } } }
    };

    private static void Toucher(View vue, Func<Task> action)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await action();
        vue.GestureRecognizers.Add(tap);
    }

    private View ModeleArticle()
    {
        var photo = new Image { WidthRequest = 46, HeightRequest = 46, Aspect = Aspect.AspectFill };
        photo.SetBinding(Image.SourceProperty, nameof(Article.UrlPhoto));
        var nom = new Label { FontSize = 14.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, LineBreakMode = LineBreakMode.TailTruncation };
        nom.SetBinding(Label.TextProperty, nameof(Article.Designation));
        var sous = new Label { FontSize = 12, TextColor = Couleurs.Muet };
        sous.SetBinding(Label.TextProperty, nameof(Article.SousTitre));
        var grille = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        grille.Add(new Border { StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, BackgroundColor = Couleurs.GrisClair, Content = photo, WidthRequest = 46, HeightRequest = 46 }, 0, 0);
        grille.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { nom, sous } }, 1, 0);
        var carte = new Border { BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(8), Margin = new Thickness(0, 0, 0, 6), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = grille };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, _) => { if (((View)s!).BindingContext is Article a) { article = a; AfficherChoix(); } };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }

    private void AfficherChoix()
    {
        articleChoisi.Text = article == null ? "Aucun article choisi" : article.Designation;
        articleChoisi.TextColor = article == null ? Couleurs.Muet : Couleurs.Texte;
        aideArticle.Text = article == null ? "Touchez un article dans la liste." : article.SousTitre + " · n° " + article.Id;
    }

    private void Filtrer()
    {
        string q = Normaliser(recherche.Text);
        if (q.Length < 2)
        {
            articles.ItemsSource = duLocal;
            return;
        }
        var mots = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        articles.ItemsSource = (catalogue ?? duLocal)
            .Where(a => { string t = Normaliser($"{a.Designation} {a.Marque} {a.Modele} {a.Code} {a.Id}"); return mots.All(m => t.Contains(m)); })
            .Take(60).ToList();
    }

    private static string Normaliser(string? s)
    {
        string d = (s ?? "").ToLowerInvariant().Normalize(System.Text.NormalizationForm.FormD);
        return new string(d.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark).ToArray()).Trim();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (observations.ItemsSource == null) { await ChargerObservations(); }
        try
        {
            if (duLocal.Count == 0)
            {
                duLocal = (await Api.Get<List<Article>>($"article/local/{local.Id}")).Content ?? new List<Article>();
                Filtrer();
            }
            catalogue ??= (await Api.Get<List<Article>>("article/")).Content?.OrderBy(a => a.Designation).ToList();
            Filtrer();
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task ChargerObservations()
    {
        try
        {
            int? choisie = (observations.SelectedItem as Observation)?.Id;
            var liste = (await Api.Get<List<Observation>>($"observation/etat/{etat}")).Content ?? new List<Observation>();
            observations.ItemsSource = liste;
            int index = choisie == null ? -1 : liste.FindIndex(o => o.Id == choisie);
            observations.SelectedIndex = index >= 0 ? index : (liste.Count == 1 ? 0 : -1);
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task Creer()
    {
        if (article == null) { Informer("Choisissez l'article du bien.", false); return; }
        if (observations.SelectedItem is not Observation obs) { Informer("Choisissez une observation.", false); return; }
        int n = (int)quantite.Value;
        if (!await Confirmer("Nouveau bien", $"Créer {n} « {article.Designation} » dans « {local.Designation} » ?", "Créer", "Annuler")) { return; }
        bool ok = await Executer("immo/add/", new Dictionary<string, string>
        {
            ["IdArticle"] = article.Id.ToString(), ["IdLocal"] = local.Id.ToString(), ["CodeImmo"] = codeImmo.Text?.Trim() ?? "",
            ["LastEtat"] = etat, ["IdLastObservation"] = obs.Id.ToString(), ["Nbre"] = n.ToString()
        });
        if (ok) { await Fermer((article.Id, n)); }
    }

    private async Task Fermer((long, int)? resultat)
    {
        if (repondu) { return; }
        repondu = true;
        tcs.TrySetResult(resultat);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed() { _ = Fermer(null); return true; }

    /// <summary>Ouvre le formulaire ; renvoie l'article et le nombre de biens créés, ou null.</summary>
    public static async Task<(long idArticle, int nombre)?> Ouvrir(INavigation navigation, Local local)
    {
        var page = new NouveauBienPage(local);
        await navigation.PushModalAsync(new NavigationPage(page));
        return await page.tcs.Task;
    }
}
