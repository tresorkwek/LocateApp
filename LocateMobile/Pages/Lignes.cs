using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Modèles de lignes partagés entre les listes de biens (contenu d'un local, non vus, transit…).</summary>
public static class Lignes
{
    /// <summary>
    /// Ligne d'un bien : à gauche la pastille QR (QR code vert si le bien a une étiquette, QR code gris barré sinon),
    /// puis désignation, codes, état et date de vue ; à droite l'icône d'avancement et un chevron. La ligne passe en bleu
    /// quand le bien est identifié, puis en vert quand l'inventaire du local est clôturé.
    /// </summary>
    public static View Bien(Func<Immo, Task> ouvrir, Func<Immo, Task>? menu = null)
    {
        var grille = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Auto) } };

        var qrImage = new Image { WidthRequest = 34, HeightRequest = 34, Aspect = Aspect.AspectFit, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        qrImage.SetBinding(Image.SourceProperty, nameof(Immo.ImageQr));
        qrImage.SetBinding(SemanticProperties.DescriptionProperty, nameof(Immo.QrLibelle));
        var qr = new Border { WidthRequest = 46, HeightRequest = 46, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }, Padding = new Thickness(4), Content = qrImage, VerticalOptions = LayoutOptions.Center };
        qr.SetBinding(VisualElement.BackgroundColorProperty, nameof(Immo.FondQr));
        grille.Add(qr, 0, 0);

        var titre = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte };
        titre.SetBinding(Label.TextProperty, nameof(Immo.Titre));
        var codes = new Label { FontSize = 12, TextColor = Couleurs.Muet };
        codes.SetBinding(Label.TextProperty, nameof(Immo.Codes));
        var etat = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold };
        etat.SetBinding(Label.TextProperty, nameof(Immo.EtatLibelle));
        etat.SetBinding(Label.TextColorProperty, nameof(Immo.EtatCouleur));
        var vu = new Label { FontSize = 12, FontAttributes = FontAttributes.Bold };
        vu.SetBinding(Label.TextProperty, nameof(Immo.VuLibelle));
        vu.SetBinding(Label.TextColorProperty, nameof(Immo.VuCouleur));
        vu.SetBinding(VisualElement.IsVisibleProperty, nameof(Immo.VuVisible));
        grille.Add(new VerticalStackLayout { Children = { titre, codes, new HorizontalStackLayout { Spacing = 8, Children = { etat, vu } } } }, 1, 0);

        var etatIcone = new Label { FontSize = 22, FontFamily = Icones.Police, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, WidthRequest = 36, HeightRequest = 36 };
        etatIcone.SetBinding(Label.TextProperty, nameof(Immo.EtatIcone));
        etatIcone.SetBinding(Label.TextColorProperty, nameof(Immo.EtatIconeCouleur));
        grille.Add(etatIcone, 2, 0);
        if (menu == null)
        {
            grille.Add(Icones.Ico(Icones.Chevron, 13, Couleurs.Muet), 3, 0);
        }
        else
        {
            // Bouton ⋮ : toutes les actions du bien sans ouvrir sa fiche (identifier, non vu, déplacer, expédier…).
            var bouton = new Button { Text = Icones.Menu, FontFamily = Icones.Police, FontSize = 20, TextColor = Couleurs.Muet, BackgroundColor = Colors.Transparent, WidthRequest = 44, HeightRequest = 44, Padding = 0, VerticalOptions = LayoutOptions.Center };
            bouton.Clicked += async (s, _) => { if (((View)s!).BindingContext is Immo b) { await menu(b); } };
            grille.Add(bouton, 3, 0);
        }

        var carte = new Border { StrokeThickness = 0, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 8), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) }, Content = grille };
        carte.SetBinding(VisualElement.BackgroundColorProperty, nameof(Immo.FondLigne));
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (s, _) => { if (((View)s!).BindingContext is Immo b) { await ouvrir(b); } };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }
}
