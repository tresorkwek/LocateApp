using LocateMobile.Models;

namespace LocateMobile.Pages;

/// <summary>Identification d'un bien : état constaté (bon / mauvais) et observation, puis POST /inventaire/identifier/.</summary>
public class IdentifierPage : PageBase
{
    private readonly Immo bien;
    private readonly TaskCompletionSource<bool> tcs = new();
    private readonly Picker observations = new() { Title = "Choisissez une observation", FontSize = 16 };
    private readonly RadioButton bon = new() { Content = "Bon état", Value = "B", FontSize = 16 };
    private readonly RadioButton mauvais = new() { Content = "Mauvais état", Value = "M", FontSize = 16 };
    private List<Observation> liste = new();
    private bool repondu;

    private IdentifierPage(Immo bien) : base("Identifier")
    {
        this.bien = bien;
        Actualiser = () => ChargerObservations(mauvais.IsChecked ? "M" : "B");
        if (bien.LastEtat == "M") { mauvais.IsChecked = true; } else { bon.IsChecked = true; }
        bon.CheckedChanged += async (_, e) => { if (e.Value) { await ChargerObservations("B"); } };
        mauvais.CheckedChanged += async (_, e) => { if (e.Value) { await ChargerObservations("M"); } };

        var valider = BoutonPrincipal("Marquer comme vu", Couleurs.VertFonce);
        valider.ImageSource = Icones.Image(Icones.CocheRonde, Colors.White, 20);
        valider.HeightRequest = 54;
        valider.Clicked += async (_, _) => await Valider();
        var annuler = new Button { Text = "Annuler", BackgroundColor = Couleurs.Carte, TextColor = Couleurs.Texte, CornerRadius = 12, HeightRequest = 48, BorderColor = Couleurs.Bordure, BorderWidth = 1 };
        annuler.Clicked += async (_, _) => await Fermer(false);

        // Deux grands choix d'état, faciles à toucher : la sélection est rendue par la couleur de la carte.
        bon.IsVisible = false;
        mauvais.IsVisible = false;
        var choixBon = ChoixEtat(Icones.CocheRonde, "Bon état", "Fonctionne, rien à signaler", Couleurs.VertFonce, Couleurs.VertClair);
        var choixMauvais = ChoixEtat(Icones.Attention, "Mauvais état", "Abîmé, en panne, incomplet", Couleurs.Rouge, Couleurs.RougeClair);
        void Rafraichir()
        {
            StylerChoix(choixBon, bon.IsChecked, Couleurs.VertFonce, Couleurs.VertClair);
            StylerChoix(choixMauvais, mauvais.IsChecked, Couleurs.Rouge, Couleurs.RougeClair);
        }
        AjouterTap(choixBon, () => { bon.IsChecked = true; Rafraichir(); });
        AjouterTap(choixMauvais, () => { mauvais.IsChecked = true; Rafraichir(); });
        Rafraichir();

        var etats = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) } };
        etats.Add(choixBon, 0, 0);
        etats.Add(choixMauvais, 1, 0);
        var groupe = new HorizontalStackLayout { Children = { bon, mauvais } };
        RadioButtonGroup.SetGroupName(groupe, "etat");

        var bienCarte = new Grid { ColumnSpacing = 12, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        bienCarte.Add(Ui.Pastille(bien.AQrCode ? Icones.Qr : Icones.Biens, Couleurs.BleuClair, Couleurs.Accent, 48, 24), 0, 0);
        bienCarte.Add(new VerticalStackLayout { VerticalOptions = LayoutOptions.Center, Children = { Titre(bien.Titre), Sous(bien.Codes) } }, 1, 0);

        observations.TextColor = Couleurs.Texte;
        observations.TitleColor = Couleurs.Muet;

        Afficher(new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                Carte(bienCarte, marge: new Thickness(0)),
                new Label { Text = "ÉTAT CONSTATÉ", FontSize = 11.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, CharacterSpacing = 1.2, Margin = new Thickness(4, 6, 0, 0) },
                etats, groupe,
                new Label { Text = "OBSERVATION", FontSize = 11.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Muet, CharacterSpacing = 1.2, Margin = new Thickness(4, 6, 0, 0) },
                new Border { BackgroundColor = Couleurs.Carte, StrokeThickness = 1, Stroke = Couleurs.Bordure, Padding = new Thickness(12, 2), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(14) }, Content = observations },
                new BoxView { HeightRequest = 6, Color = Colors.Transparent },
                valider, annuler
            }
        });
    }

    private static Border ChoixEtat(string glyphe, string titre, string texte, Color couleur, Color fond) => new()
    {
        StrokeThickness = 2, Padding = new Thickness(14), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(16) },
        Content = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                Icones.Ico(glyphe, 30, couleur),
                new Label { Text = titre, FontSize = 15.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, HorizontalTextAlignment = TextAlignment.Center },
                new Label { Text = texte, FontSize = 12, TextColor = Couleurs.Muet, HorizontalTextAlignment = TextAlignment.Center }
            }
        }
    };

    private static void StylerChoix(Border carte, bool actif, Color couleur, Color fond)
    {
        carte.Stroke = actif ? couleur : Couleurs.Bordure;
        carte.BackgroundColor = actif ? fond : Couleurs.Carte;
    }

    private static void AjouterTap(View vue, Action action)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => action();
        vue.GestureRecognizers.Add(tap);
    }

    private async Task ChargerObservations(string etat)
    {
        int? choisie = (observations.SelectedItem as Observation)?.Id;
        observations.ItemsSource = null;
        try
        {
            var r = await Api.Get<List<Observation>>($"observation/etat/{etat}");
            liste = r.Content ?? new List<Observation>();
            observations.ItemsSource = liste;
            int index = liste.FindIndex(o => o.Id == (choisie ?? bien.IdLastObservation));
            observations.SelectedIndex = index >= 0 ? index : (liste.Count == 1 ? 0 : -1);
        }
        catch (Exception e) { await Erreur(e); }
    }

    private async Task Valider()
    {
        if (observations.SelectedItem is not Observation obs) { Informer("Choisissez une observation.", false); return; }
        string etat = mauvais.IsChecked ? "M" : "B";
        bool ok = await ExecuterOuMettreEnAttente("inventaire/identifier/", new Dictionary<string, string>
        {
            ["IdImmo"] = bien.Id.ToString(), ["LastEtat"] = etat, ["IdLastObservation"] = obs.Id.ToString()
        }, $"Identifier : {bien.Titre}");
        if (ok) { await Fermer(true); }
    }

    private async Task Fermer(bool resultat)
    {
        if (repondu) { return; }
        repondu = true;
        tcs.TrySetResult(resultat);
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed() { _ = Fermer(false); return true; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (liste.Count == 0) { await ChargerObservations(mauvais.IsChecked ? "M" : "B"); }
    }

    /// <summary>Ouvre la feuille d'identification et renvoie vrai si le bien a été marqué vu.</summary>
    public static async Task<bool> Ouvrir(INavigation navigation, Immo bien)
    {
        var page = new IdentifierPage(bien);
        await navigation.PushModalAsync(new NavigationPage(page));
        return await page.tcs.Task;
    }
}
