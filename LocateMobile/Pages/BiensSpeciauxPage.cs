using LocateMobile.Models;
using LocateMobile.Services;

namespace LocateMobile.Pages;

/// <summary>
/// Onglets « Non vu », « Transit » et « Déclassé » : biens d'un local particulier de l'organe d'affectation de l'agent.
/// Le serveur choisit lui-même l'organe (celui de l'utilisateur connecté) : les routes existantes ne prennent aucun paramètre.
/// </summary>
public class BiensSpeciauxPage : PageBase
{
    private readonly string? chemin;
    private readonly string description;
    private readonly CollectionView liste = new() { SelectionMode = SelectionMode.None, Margin = new Thickness(12, 0) };
    private readonly Label resume = new() { FontSize = 12, TextColor = Couleurs.Muet, Margin = new Thickness(14, 4, 14, 6) };
    private readonly VerticalStackLayout enTete = new() { Margin = new Thickness(12, 12, 12, 0) };

    /// <param name="titre">Titre de la page et de l'onglet.</param>
    /// <param name="chemin">Route du serveur qui renvoie la liste des biens (null : aucune route disponible, un message l'explique).</param>
    /// <param name="description">Phrase affichée sous le titre.</param>
    public BiensSpeciauxPage(string titre, string? chemin, string description) : base(titre)
    {
        Actualiser = () => Charger();
        this.chemin = chemin;
        this.description = description;
        NavigationPage.SetHasBackButton(this, false);
        ToolbarItems.Add(new ToolbarItem("Rafraîchir", null, async () => await Charger()));
        Func<Models.Immo, Task>? menu = chemin == "immo/declasser/"
            ? async b => { if (await MenuBienDeclasse(b)) { await Charger(); } }
            : async b => { if (await MenuBien(b)) { await Charger(); } };
        liste.ItemTemplate = new DataTemplate(() => Lignes.Bien(b => Navigation.PushAsync(new BienPage(b.Id)), menu));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Charger();
    }

    private async Task Charger()
    {
        if (chemin == null)
        {
            Afficher(new VerticalStackLayout
            {
                Children =
                {
                    Carte(new VerticalStackLayout { Children = { Titre(Title), Sous(description) } }),
                    Carte(new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "Liste indisponible", FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Orange },
                            new Label { Text = "Pour déclasser un bien, ouvrez sa fiche ou utilisez la tuile « Déclasser un bien » de l'accueil. Locate ne fournit pas de liste des biens déclassés : cet onglet l'affichera dès qu'elle existera.", FontSize = 13, TextColor = Couleurs.Muet }
                        }
                    }, Couleurs.OrangeClair)
                }
            });
            return;
        }

        AfficherAttente();
        try
        {
            var r = await Api.Get<List<Immo>>(chemin);
            var biens = (r.Content ?? new List<Immo>()).OrderBy(b => b.EstVu ? 1 : 0).ThenBy(b => b.Titre).ToList();
            string? nomLocal = biens.Count > 0 ? await S.NomLocal(biens[0].IdLocal) : null;
            // En cas de succès, le message du serveur n'est qu'un libellé technique (« Bien : liste des déclassés ») : on ne l'affiche pas.
            Construire(biens, nomLocal, r.Ok ? null : r.Message);
        }
        catch (SessionExpireeException e) { await Erreur(e); }
        catch (ApiException e)
        {
            // Le serveur répond « Success = 0 » avec une explication quand l'organe n'a pas ce type de local : ce n'est pas une panne.
            Construire(new List<Immo>(), null, e.Message);
        }
        catch (Exception e) { AfficherErreur(e.Message, Charger); }
    }

    private void Construire(List<Immo> biens, string? nomLocal, string? message)
    {
        int vus = biens.Count(b => b.EstVu);
        enTete.Children.Clear();
        var badges = new HorizontalStackLayout
        {
            Margin = new Thickness(0, 6, 0, 0),
            Children =
            {
                Badge(biens.Count.ToString("N0"), Couleurs.GrisClair, Couleurs.Gris, Icones.Biens),
                Badge(vus.ToString("N0"), Couleurs.BleuClair, Couleurs.Bleu, Icones.Oeil),
                Badge(biens.Count(b => b.AQrCode).ToString("N0"), Couleurs.VertClair, Couleurs.VertFonce, Icones.Qr)
            }
        };
        var organe = S.OrganeParId(S.Utilisateur?.CodeOrgane) ?? S.OrganeParId(S.Utilisateur?.IdInstitution);
        string sous = description + (organe != null ? "\n" + organe.Nom : "") + (nomLocal != null ? "\nLocal : " + nomLocal : "");
        var carte = new VerticalStackLayout { Children = { Titre(Title), Sous(sous), badges } };
        if (chemin == "immo/declasser/" && biens.Count > 0)
        {
            // Cession de tout le local des déclassés (POST /immo/local/cession/).
            var cederTout = new Button
            {
                Text = $"Céder les {biens.Count} biens", ImageSource = Icones.Image(Icones.CroixRonde, Couleurs.Rouge, 18),
                BackgroundColor = Couleurs.RougeClair, TextColor = Couleurs.Rouge, FontAttributes = FontAttributes.Bold,
                CornerRadius = 12, HeightRequest = 46, Margin = new Thickness(0, 10, 0, 0)
            };
            cederTout.Clicked += async (_, _) => { if (await CederTousLesDeclasses(biens.Count)) { await Charger(); } };
            carte.Children.Add(cederTout);
        }
        enTete.Children.Add(Carte(carte, marge: new Thickness(0)));

        resume.Text = biens.Count == 0
            ? (message ?? "")
            : $"{biens.Count} bien(s), {vus} déjà vu(s) en {S.Annee}. Touchez un bien pour sa fiche.";
        resume.IsVisible = !string.IsNullOrWhiteSpace(resume.Text);
        liste.EmptyView = string.IsNullOrWhiteSpace(message) ? EtatVidePropre() : null;
        liste.ItemsSource = biens;

        var grille = new Grid { RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Star) } };
        grille.Add(enTete, 0, 0);
        grille.Add(resume, 0, 1);
        grille.Add(ListeActualisable(liste), 0, 2);
        AfficherBrut(grille);
    }

    /// <summary>État vide propre à chaque onglet, quand le serveur a répondu sans erreur mais sans bien.</summary>
    private View EtatVidePropre() => chemin switch
    {
        "immo/declasser/" => Ui.EtatVide(Icones.Archive, "Aucun bien déclassé", "Le local des déclassés de votre entité est vide. Pour déclasser un bien, ouvrez sa fiche ou utilisez la tuile « Déclasser un bien » de l'accueil."),
        "immo/transit/" => Ui.EtatVide(Icones.Livraison, "Aucun bien en transit", "Aucun bien expédié vers votre organe n'attend d'être mis en service."),
        "immo/nonvu/" => Ui.EtatVide(Icones.OeilBarre, "Aucun bien non vu", "Aucun bien n'a été déclaré introuvable dans votre organe."),
        _ => Ui.EtatVide(Icones.Biens, "Aucun bien", "")
    };
}
