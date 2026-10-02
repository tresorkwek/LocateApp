using Microsoft.Maui.Controls.Shapes;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace LocateMobile.Pages;

/// <summary>Lecture d'un QR code avec la caméra arrière (ZXing) : viseur, lampe, et saisie manuelle de secours (douchette ou clavier).</summary>
public class ScanPage : ContentPage
{
    private readonly TaskCompletionSource<string?> resultat = new();
    private readonly CameraBarcodeReaderView camera;
    private bool termine;

    private ScanPage(string titre, string aide)
    {
        Title = titre;
        BackgroundColor = Colors.Black;
        NavigationPage.SetHasNavigationBar(this, false);

        camera = new CameraBarcodeReaderView
        {
            Options = new BarcodeReaderOptions { Formats = BarcodeFormat.QrCode, AutoRotate = true, Multiple = false, TryHarder = true },
            CameraLocation = CameraLocation.Rear,
            IsDetecting = true
        };
        camera.BarcodesDetected += (_, e) =>
        {
            var valeur = e.Results?.FirstOrDefault()?.Value;
            if (!string.IsNullOrWhiteSpace(valeur)) { MainThread.BeginInvokeOnMainThread(() => Terminer(valeur)); }
        };

        // Viseur : quatre coins blancs et une ligne de balayage animée.
        const double cote = 250;
        var viseur = new Grid { WidthRequest = cote, HeightRequest = cote, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, InputTransparent = true };
        foreach (var (h, v) in new[] { (LayoutOptions.Start, LayoutOptions.Start), (LayoutOptions.End, LayoutOptions.Start), (LayoutOptions.Start, LayoutOptions.End), (LayoutOptions.End, LayoutOptions.End) })
        {
            viseur.Add(new BoxView { Color = Colors.White, WidthRequest = 42, HeightRequest = 5, CornerRadius = 3, HorizontalOptions = h, VerticalOptions = v });
            viseur.Add(new BoxView { Color = Colors.White, WidthRequest = 5, HeightRequest = 42, CornerRadius = 3, HorizontalOptions = h, VerticalOptions = v });
        }
        var balayage = new BoxView { Color = Color.FromArgb("#4ade80"), HeightRequest = 2, Margin = new Thickness(16, 0), VerticalOptions = LayoutOptions.Start, Opacity = 0.9 };
        viseur.Add(balayage);
        var animation = new Animation(v => balayage.TranslationY = v, 12, cote - 14);
        animation.Commit(balayage, "balayage", length: 1800, easing: Easing.SinInOut, repeat: () => !termine);

        // Barre du haut : fermer, titre, lampe.
        var fermer = BoutonRond(Icones.Croix);
        fermer.Clicked += (_, _) => Terminer(null);
        var lampe = BoutonRond(Icones.Lampe);
        lampe.Clicked += (_, _) =>
        {
            camera.IsTorchOn = !camera.IsTorchOn;
            lampe.BackgroundColor = camera.IsTorchOn ? Color.FromArgb("#f59e0b") : Color.FromRgba(0, 0, 0, 130);
        };
        var haut = new Grid { Padding = new Thickness(14, 18, 14, 0), ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) }, VerticalOptions = LayoutOptions.Start };
        haut.Add(fermer, 0, 0);
        haut.Add(new Label { Text = titre, TextColor = Colors.White, FontSize = 17, FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, 1, 0);
        haut.Add(lampe, 2, 0);

        // Feuille du bas : consigne et saisie manuelle.
        var saisie = new Entry { Placeholder = "Saisir ou lire le code avec une douchette", TextColor = Colors.White, PlaceholderColor = Color.FromArgb("#9ca3af"), ReturnType = ReturnType.Done, BackgroundColor = Colors.Transparent, FontSize = 15 };
        saisie.Completed += (_, _) => { if (!string.IsNullOrWhiteSpace(saisie.Text)) { Terminer(saisie.Text.Trim()); } };
        var valider = new Button { ImageSource = Icones.Image(Icones.Fleche, Colors.White, 18), BackgroundColor = Couleurs.Accent, CornerRadius = 12, WidthRequest = 48, HeightRequest = 48, Padding = 0 };
        valider.Clicked += (_, _) => { if (!string.IsNullOrWhiteSpace(saisie.Text)) { Terminer(saisie.Text.Trim()); } };
        var champ = new Grid { ColumnSpacing = 8, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) } };
        champ.Add(Icones.Ico(Icones.Clavier, 18, Color.FromArgb("#9ca3af")), 0, 0);
        champ.Add(saisie, 1, 0);
        champ.Add(valider, 2, 0);
        var pied = new Border
        {
            BackgroundColor = Color.FromArgb("#111827"), StrokeThickness = 0, Padding = new Thickness(16, 16, 16, 26), VerticalOptions = LayoutOptions.End,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(22, 22, 0, 0) },
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label { Text = aide, TextColor = Color.FromArgb("#e5e7eb"), FontSize = 14, HorizontalTextAlignment = TextAlignment.Center },
                    new Border { BackgroundColor = Color.FromArgb("#1f2937"), StrokeThickness = 0, Padding = new Thickness(12, 0, 0, 0), StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }, Content = champ }
                }
            }
        };

        var grille = new Grid();
        grille.Add(camera);
        grille.Add(viseur);
        grille.Add(haut);
        grille.Add(pied);
        Content = grille;
    }

    private static Button BoutonRond(string glyphe) => new()
    {
        ImageSource = Icones.Image(glyphe, Colors.White, 20), BackgroundColor = Color.FromRgba(0, 0, 0, 130), CornerRadius = 22,
        WidthRequest = 44, HeightRequest = 44, Padding = 0
    };

    private void Terminer(string? valeur)
    {
        if (termine) { return; }
        termine = true;
        camera.IsDetecting = false;
        camera.IsTorchOn = false;
        try { if (valeur != null) { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(60)); } } catch { /* pas de vibreur */ }
        resultat.TrySetResult(valeur);
        MainThread.BeginInvokeOnMainThread(async () => { try { await Navigation.PopModalAsync(); } catch { /* déjà fermée */ } });
    }

    protected override bool OnBackButtonPressed()
    {
        Terminer(null);
        return true;
    }

    /// <summary>Ouvre le scanner en modal et renvoie le texte lu (ou null si annulé).</summary>
    public static async Task<string?> Scanner(INavigation navigation, string titre, string aide)
    {
        var statut = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (statut != PermissionStatus.Granted) { statut = await Permissions.RequestAsync<Permissions.Camera>(); }
        var page = new ScanPage(titre, statut == PermissionStatus.Granted ? aide : "Caméra refusée : saisissez le code ci-dessous.");
        await navigation.PushModalAsync(new NavigationPage(page) { BarBackgroundColor = Colors.Black, BarTextColor = Colors.White });
        return await page.resultat.Task;
    }
}
