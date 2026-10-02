using LocateMobile.Models;
using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Connexion, avec le même fond photo et le même voile sombre que la page de connexion du site web.</summary>
public class LoginPage : PageBase
{
    private readonly Button serveur;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        serveur.Text = "⚙  Serveur : " + (Api.BaseUrl.Length > 0 ? Api.BaseUrl.Replace("http://", "").Replace("https://", "") : "non défini"); // mis à jour au retour des paramètres
    }

    public LoginPage(string? message = null) : base("Connexion")
    {
        NavigationPage.SetHasBackButton(this, false);
        NavigationPage.SetHasNavigationBar(this, false);

        var compte = new Entry { Placeholder = "Compte", FontSize = 17, ReturnType = ReturnType.Next, TextColor = Couleurs.Texte, PlaceholderColor = Couleurs.Muet };
        var motDePasse = new Entry { Placeholder = "Mot de passe", IsPassword = true, FontSize = 17, ReturnType = ReturnType.Go, TextColor = Couleurs.Texte, PlaceholderColor = Couleurs.Muet };
        var erreur = new Label { TextColor = Couleurs.Rouge, FontSize = 13, Text = message ?? "" };
        var bouton = BoutonPrincipal("Se connecter");
        // Bouton de paramétrage du serveur (protocole, adresse IP, port) : à régler avant de se connecter.
        serveur = new Button
        {
            Text = "⚙  Serveur", FontSize = 14, BackgroundColor = Colors.White, TextColor = Couleurs.Texte, BorderColor = Couleurs.Bordure, BorderWidth = 1,
            CornerRadius = 12, HeightRequest = 44, Margin = new Thickness(0, 2, 0, 0)
        };
        serveur.Clicked += async (_, _) => await Navigation.PushAsync(new ParametresPage());

        Func<Task> connecter = async () =>
        {
            erreur.Text = "";
            if (string.IsNullOrWhiteSpace(compte.Text) || string.IsNullOrEmpty(motDePasse.Text)) { erreur.Text = "Saisissez le compte et le mot de passe."; return; }
            bouton.IsEnabled = false;
            try
            {
                var r = await Api.Connexion(compte.Text.Trim(), motDePasse.Text);
                if (!r.Ok)
                {
                    erreur.Text = string.IsNullOrWhiteSpace(r.Message) ? "Connexion refusée." : r.Message;
                    if (r.Content?.IsExternalUser == true && r.Content.FirstConnexion) { erreur.Text = "Première connexion : créez d'abord votre mot de passe sur la version web."; }
                    return;
                }
                await S.ChargerProfil();
                App.OuvrirPrincipal();
            }
            catch (Exception e) { erreur.Text = e.Message; }
            finally { bouton.IsEnabled = true; }
        };
        bouton.Clicked += async (_, _) => await connecter();
        motDePasse.Completed += async (_, _) => await connecter();
        compte.Completed += (_, _) => motDePasse.Focus();

        // Logo et photo de fond embarqués dans l'application : l'écran est complet même sans serveur joignable.
        var logo = new Image { Source = "locate_logo.png", HeightRequest = 64, Aspect = Aspect.AspectFit, Margin = new Thickness(0, 0, 0, 4) };

        // Même composition que la carte de connexion du site : bandeau noir avec le logo et « Bienvenue », puis le formulaire.
        var enTete = new Border
        {
            BackgroundColor = Colors.Black, StrokeThickness = 0, Padding = new Thickness(20, 22, 20, 18),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16, 16, 0, 0) },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    logo,
                    new Label { Text = "Bienvenue sur Locate", FontSize = 18, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, TextColor = Colors.White },
                    new Label { Text = "Connectez-vous pour démarrer l'inventaire sur le terrain.", FontSize = 12.5, TextColor = Color.FromArgb("#cbd5e1"), HorizontalTextAlignment = TextAlignment.Center }
                }
            }
        };
        var formulaire = new VerticalStackLayout { Spacing = 10, Padding = new Thickness(20, 18, 20, 22), Children = { Sous("Compte"), compte, Sous("Mot de passe"), motDePasse, bouton, serveur, erreur } };
        var carte = new Border
        {
            BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = 0,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.45f, Radius = 30, Offset = new Point(0, 12) },
            Content = new VerticalStackLayout { Children = { enTete, formulaire } }
        };

        var colonne = new VerticalStackLayout { Spacing = 6, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Children = { carte } };

        // Fond : photo du site (search_image.jpg) + voile sombre à 45 %, comme body.auth-bg dans locate.css.
        var fond = new Image { Source = "search_image.jpg", Aspect = Aspect.AspectFill, Opacity = 0.55 };
        var voile = new BoxView { Color = Color.FromRgba(10, 14, 26, 115), InputTransparent = true }; // rgba(10,14,26,.45) ; la photo est aussi atténuée (opacité 0,55) pour garantir le voile sur tous les rendus

        // La colonne est posée dans une grille au moins aussi haute que l'écran : centrage vertical et horizontal,
        // et défilement quand le clavier réduit la hauteur disponible.
        var grille = new Grid { Children = { colonne } };
        var racine = new Grid { BackgroundColor = Color.FromArgb("#0f172a") };
        racine.Add(fond);
        racine.Add(voile);
        racine.Add(new ScrollView { Content = grille, Padding = new Thickness(16, 16) });
        Content = racine;

        // Largeur adaptée à l'écran : pleine largeur sur téléphone, carte de 440 dp maximum sur tablette.
        SizeChanged += (_, _) =>
        {
            if (Width <= 0 || Height <= 0) { return; }
            colonne.WidthRequest = Math.Min(440, Width - 32);
            grille.MinimumHeightRequest = Height - 32;
        };
    }
}
