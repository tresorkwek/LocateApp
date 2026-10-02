using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace LocateMobile.Pages;

/// <summary>
/// Écran principal après connexion : quatre onglets en bas de l'écran, chacun avec sa propre pile de navigation.
/// Accueil = tableau de bord ; Non vu / Transit / Déclassé = biens des locaux particuliers de l'organe d'affectation.
/// </summary>
public class PrincipalPage : TabbedPage
{
    public PrincipalPage()
    {
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
        On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
        BarBackgroundColor = Couleurs.Carte;
        SelectedTabColor = Couleurs.Accent;
        UnselectedTabColor = Couleurs.Muet;

        Children.Add(Onglet(new DashboardPage(), "Accueil", Icones.Accueil));
        Children.Add(Onglet(new BiensSpeciauxPage("Non vu", "immo/nonvu/", "Biens déclarés non vus dans votre organe d'affectation"), "Non vu", Icones.OeilBarre));
        Children.Add(Onglet(new BiensSpeciauxPage("Transit", "immo/transit/", "Biens en transit au départ de votre organe d'affectation"), "Transit", Icones.Livraison));
        Children.Add(Onglet(new BiensSpeciauxPage("Déclassé", "immo/declasser/", "Biens rangés dans le local des déclassés de votre entité, en attente de cession"), "Déclassé", Icones.Archive));
    }

    /// <summary>Affiche l'onglet demandé (0 Accueil, 1 Non vu, 2 Transit, 3 Déclassé) depuis n'importe quelle page.</summary>
    public static void Aller(int index)
    {
        if (Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page is PrincipalPage p && index >= 0 && index < p.Children.Count) { p.CurrentPage = p.Children[index]; }
    }

    private static NavigationPage Onglet(Page page, string titre, string glyphe) => new(page)
    {
        Title = titre,
        IconImageSource = Icones.Image(glyphe, Couleurs.Muet, 22),
        BarBackgroundColor = Couleurs.Carte,
        BarTextColor = Couleurs.Texte
    };
}
