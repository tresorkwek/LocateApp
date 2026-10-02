using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Organigramme de l'agent, indenté, avec les badges total / inventoriés / identifiés.</summary>
public class OrganesPage : PageBase
{
    private readonly CollectionView liste = new() { SelectionMode = SelectionMode.None, Margin = new Thickness(12, 0) };
    private readonly Entry recherche = new() { Placeholder = "Rechercher un organe (nom, sigle, code)" };

    public OrganesPage() : base("Organes")
    {
        Actualiser = async () => { await S.ChargerArbre(true); Filtrer(); };
        liste.ItemTemplate = new DataTemplate(() =>
        {
            var grille = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
            var nom = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
            nom.SetBinding(Label.TextProperty, nameof(Organe.Nom));
            var sous = new Label { FontSize = 12, TextColor = Couleurs.Muet };
            sous.SetBinding(Label.TextProperty, nameof(Organe.SousTitre));
            var badges = new HorizontalStackLayout
            {
                Margin = new Thickness(0, 4, 0, 0),
                Children =
                {
                    BadgeLie(nameof(Organe.NbreBien), Couleurs.GrisClair, Couleurs.Gris, Icones.Biens),
                    BadgeLie(nameof(Organe.NbreBienIdentifie), Couleurs.BleuClair, Couleurs.Bleu, Icones.Oeil),
                    BadgeLie(nameof(Organe.NbreBienInventorie), Couleurs.VertClair, Couleurs.VertFonce, Icones.Coche)
                }
            };
            grille.Add(BarreEtape(), 0, 0);
            grille.Add(new VerticalStackLayout { Children = { nom, sous, badges, PastilleEtape() } }, 1, 0);
            grille.Add(Icones.Ico(Icones.Chevron, 14, Couleurs.Muet), 2, 0);
            var carte = new Border
            {
                BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(6, 10, 12, 10), Margin = new Thickness(0, 0, 0, 8),
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = grille
            };
            carte.SetBinding(View.MarginProperty, new Binding(nameof(Organe.Retrait), converter: new RetraitConverter()));
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (s, _) => { if (((View)s!).BindingContext is Organe o) { await Navigation.PushAsync(new LocauxPage(o)); } };
            carte.GestureRecognizers.Add(tap);
            return carte;
        });
        recherche.TextChanged += (_, _) => Filtrer();

        var grillePage = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grillePage.Add(new ContentView { Padding = new Thickness(12, 12, 12, 4), Content = Ui.Recherche(recherche) }, 0, 0);
        grillePage.Add(Legende(), 0, 1);
        grillePage.Add(ListeActualisable(liste), 0, 2);
        AfficherBrut(grillePage, Flottant("Scanner un local", async () => await ScannerLocal()));
    }

    /// <summary>Légende des badges avec les mêmes icônes que les cartes.</summary>
    private static View Legende() => new HorizontalStackLayout
    {
        Spacing = 12, Margin = new Thickness(16, 2, 16, 8),
        Children =
        {
            new HorizontalStackLayout { Spacing = 4, Children = { Icones.Ico(Icones.Biens, 12, Couleurs.Gris), new Label { Text = "total", FontSize = 12, TextColor = Couleurs.Muet } } },
            new HorizontalStackLayout { Spacing = 4, Children = { Icones.Ico(Icones.Oeil, 12, Couleurs.Bleu), new Label { Text = "vus", FontSize = 12, TextColor = Couleurs.Muet } } },
            new HorizontalStackLayout { Spacing = 4, Children = { Icones.Ico(Icones.Coche, 12, Couleurs.VertFonce), new Label { Text = "inventoriés", FontSize = 12, TextColor = Couleurs.Muet } } },
            new BoxView { WidthRequest = 1, Color = Couleurs.Bordure, Margin = new Thickness(2, 2) },
            PointLegende(Couleurs.Rouge, "à faire"), PointLegende(Couleurs.Bleu, "à clôturer"), PointLegende(Couleurs.Orange, "incomplet"), PointLegende(Couleurs.VertFonce, "clôturé")
        }
    };

    private static View PointLegende(Color couleur, string texte) => new HorizontalStackLayout
    {
        Spacing = 4,
        Children = { new BoxView { WidthRequest = 9, HeightRequest = 9, CornerRadius = 5, Color = couleur, VerticalOptions = LayoutOptions.Center }, new Label { Text = texte, FontSize = 12, TextColor = Couleurs.Muet } }
    };

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try { await S.ChargerArbre(); Filtrer(); } catch (Exception e) { await Erreur(e); }
    }

    private void Filtrer()
    {
        string q = (recherche.Text ?? "").Trim().ToLowerInvariant();
        liste.ItemsSource = q.Length == 0 ? S.Plat : S.Plat.Where(o => (o.Nom + " " + o.Sigle + " " + o.Id).ToLowerInvariant().Contains(q)).ToList();
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

    /// <summary>Convertit le retrait du modèle en marge de carte (retrait à gauche + espacement bas).</summary>
    private sealed class RetraitConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => value is Thickness t ? new Thickness(t.Left, 0, 0, 8) : new Thickness(0, 0, 0, 8);
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
    }
}
