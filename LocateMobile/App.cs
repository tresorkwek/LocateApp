using LocateMobile.Pages;
using LocateMobile.Services;

namespace LocateMobile;

public class App : Application
{
    public static ApiClient Api { get; private set; } = null!;
    public static Session Session { get; private set; } = null!;

    public App(ApiClient api, Session session)
    {
        Api = api;
        Session = session;
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Sans adresse de serveur enregistrée, on commence par les paramètres ; sinon par la connexion.
        Page premiere = string.IsNullOrWhiteSpace(Api.BaseUrl) ? new ParametresPage(premierLancement: true) : new LoginPage();
        var nav = new NavigationPage(premiere)
        {
            BarBackgroundColor = Couleurs.Carte,
            BarTextColor = Couleurs.Texte
        };
        return new Window(nav) { Title = "Locate Terrain" };
    }

    /// <summary>Après connexion : l'écran principal à onglets (Accueil, Non vu, Transit, Déclassé) remplace la page de connexion.</summary>
    public static void OuvrirPrincipal()
    {
        var fenetre = Current?.Windows.FirstOrDefault();
        if (fenetre != null) { fenetre.Page = new PrincipalPage(); }
    }

    /// <summary>Retourne à l'écran de connexion (session expirée ou déconnexion).</summary>
    public static Task RetourConnexion(string? message = null)
    {
        Session.Vider();
        var fenetre = Current?.Windows.FirstOrDefault();
        if (fenetre != null)
        {
            fenetre.Page = new NavigationPage(new LoginPage(message)) { BarBackgroundColor = Couleurs.Carte, BarTextColor = Couleurs.Texte };
        }
        return Task.CompletedTask;
    }
}

/// <summary>Palette alignée sur le thème clair du site (Approx).</summary>
public static class Couleurs
{
    public static readonly Color Accent = Color.FromArgb("#3167f3");
    public static readonly Color Fond = Color.FromArgb("#f3f4f7");
    public static readonly Color Carte = Colors.White;
    public static readonly Color Texte = Color.FromArgb("#1f2937");
    public static readonly Color Muet = Color.FromArgb("#7c8ea7");
    public static readonly Color Bordure = Color.FromArgb("#e5e7eb");
    public static readonly Color Vert = Color.FromArgb("#22c55e");
    public static readonly Color VertClair = Color.FromArgb("#dcfce7");
    public static readonly Color VertFonce = Color.FromArgb("#15803d");
    public static readonly Color Bleu = Color.FromArgb("#1d4ed8");
    public static readonly Color BleuClair = Color.FromArgb("#dbeafe");
    public static readonly Color Orange = Color.FromArgb("#b45309");
    public static readonly Color OrangeClair = Color.FromArgb("#fef3c7");
    public static readonly Color Rouge = Color.FromArgb("#b91c1c");
    public static readonly Color RougeClair = Color.FromArgb("#fee2e2");
    public static readonly Color Gris = Color.FromArgb("#374151");
    public static readonly Color GrisClair = Color.FromArgb("#e5e7eb");
}
