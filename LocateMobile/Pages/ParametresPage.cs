using LocateMobile.Services;

namespace LocateMobile.Pages;

/// <summary>Adresse du serveur Locate, saisie en trois parties (protocole, adresse IP ou nom, port), mémorisée sur l'appareil.</summary>
public class ParametresPage : PageBase
{
    private readonly bool premierLancement;
    private readonly Picker protocole = new() { Title = "Protocole", FontSize = 16, ItemsSource = new List<string> { "http", "https" } };
    private readonly Entry adresse = new() { Placeholder = "172.20.10.2 ou locate.monentreprise.cd", Keyboard = Keyboard.Url, FontSize = 16 };
    private readonly Entry port = new() { Placeholder = "80", Keyboard = Keyboard.Numeric, FontSize = 16, MaxLength = 5 };
    private readonly Label apercu = new() { FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Accent };
    private readonly Label etat = new() { TextColor = Couleurs.Muet, FontSize = 13 };

    public ParametresPage(bool premierLancement = false) : base("Serveur")
    {
        this.premierLancement = premierLancement;
        LargeurMax = 560;

        // Décompose l'adresse mémorisée en protocole / hôte / port.
        if (Uri.TryCreate(Api.BaseUrl, UriKind.Absolute, out var actuelle))
        {
            protocole.SelectedItem = actuelle.Scheme == "https" ? "https" : "http";
            adresse.Text = actuelle.Host;
            port.Text = actuelle.IsDefaultPort ? "" : actuelle.Port.ToString();
        }
        else { protocole.SelectedItem = "http"; }

        protocole.SelectedIndexChanged += (_, _) => Apercu();
        adresse.TextChanged += (_, _) => Apercu();
        port.TextChanged += (_, _) => Apercu();
        Apercu();

        var tester = new Button { Text = "Tester la connexion", BackgroundColor = Couleurs.GrisClair, TextColor = Couleurs.Texte, CornerRadius = 12, HeightRequest = 46 };
        var enregistrer = BoutonPrincipal("Enregistrer");

        tester.Clicked += async (_, _) =>
        {
            string? url = UrlComposee();
            if (url == null) { etat.Text = "Indiquez au moins l'adresse du serveur."; return; }
            etat.Text = "Test en cours...";
            tester.IsEnabled = false;
            var client = new ApiClient(url);
            try
            {
                await client.Get<Models.Inventaire>("inventaire/encours/");
                etat.Text = "✓ Serveur joignable.";
            }
            catch (SessionExpireeException) { etat.Text = "✓ Serveur joignable (connexion requise)."; }
            catch (Exception e) { etat.Text = "✗ " + e.Message; }
            finally { tester.IsEnabled = true; }
        };
        enregistrer.Clicked += async (_, _) =>
        {
            string? url = UrlComposee();
            if (url == null) { Informer("Indiquez au moins l'adresse du serveur.", false); return; }
            Api.Configurer(url);
            if (this.premierLancement)
            {
                Navigation.InsertPageBefore(new LoginPage(), this);
                await Navigation.PopAsync();
            }
            else { await Navigation.PopAsync(); }
        };

        var grille = new Grid { ColumnSpacing = 10, RowSpacing = 4, ColumnDefinitions = { new ColumnDefinition(new GridLength(1.2, GridUnitType.Star)), new ColumnDefinition(new GridLength(3, GridUnitType.Star)), new ColumnDefinition(new GridLength(1.2, GridUnitType.Star)) }, RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) } };
        grille.Add(Sous("Protocole"), 0, 0);
        grille.Add(Sous("Adresse IP ou nom du serveur"), 1, 0);
        grille.Add(Sous("Port"), 2, 0);
        grille.Add(protocole, 0, 1);
        grille.Add(adresse, 1, 1);
        grille.Add(port, 2, 1);

        Afficher(new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Carte(new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        Titre("Serveur Locate"),
                        Sous("Renseignez le serveur avant de vous connecter. Ces informations sont mémorisées sur l'appareil."),
                        grille,
                        new HorizontalStackLayout { Spacing = 6, Margin = new Thickness(0, 6, 0, 0), Children = { Sous("Adresse utilisée :"), apercu } },
                        etat, tester
                    }
                }),
                enregistrer,
                Sous("Locate Terrain · RandareCx")
            }
        });
    }

    private string? UrlComposee()
    {
        string hote = (adresse.Text ?? "").Trim().TrimEnd('/');
        if (hote.Length == 0) { return null; }
        // L'utilisateur peut coller une adresse complète : on la respecte.
        if (hote.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || hote.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) { return hote; }
        string schema = protocole.SelectedItem as string ?? "http";
        string p = (port.Text ?? "").Trim();
        return schema + "://" + hote + (p.Length > 0 ? ":" + p : "");
    }

    private void Apercu() => apercu.Text = UrlComposee() ?? "—";
}
