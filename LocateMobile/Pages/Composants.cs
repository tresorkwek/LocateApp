using Microsoft.Maui.Controls.Shapes;

namespace LocateMobile.Pages;

/// <summary>Anneau de progression dessiné (pourcentage au centre), utilisé sur l'accueil et l'inventaire rapide.</summary>
public class AnneauProgression : GraphicsView
{
    private readonly Dessin dessin = new();

    public AnneauProgression(double taille = 96, Color? couleurPiste = null, Color? couleurValeur = null, Color? couleurTexte = null)
    {
        WidthRequest = taille;
        HeightRequest = taille;
        dessin.Piste = couleurPiste ?? Color.FromRgba(255, 255, 255, 60);
        dessin.Valeur = couleurValeur ?? Colors.White;
        dessin.Texte = couleurTexte ?? Colors.White;
        Drawable = dessin;
        InputTransparent = true;
    }

    /// <summary>Progression entre 0 et 1.</summary>
    public double Progression
    {
        get => dessin.Progression;
        set { dessin.Progression = Math.Clamp(value, 0, 1); Invalidate(); }
    }

    private sealed class Dessin : IDrawable
    {
        public double Progression;
        public Color Piste = Colors.LightGray;
        public Color Valeur = Colors.Blue;
        public Color Texte = Colors.Black;

        public void Draw(ICanvas canvas, RectF rect)
        {
            float epaisseur = Math.Max(6, rect.Width * 0.09f);
            var zone = new RectF(rect.X + epaisseur / 2, rect.Y + epaisseur / 2, rect.Width - epaisseur, rect.Height - epaisseur);
            canvas.StrokeSize = epaisseur;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeColor = Piste;
            canvas.DrawEllipse(zone);
            if (Progression > 0.001)
            {
                canvas.StrokeColor = Valeur;
                float fin = 90 - (float)(360 * Progression);
                if (Progression >= 0.999) { canvas.DrawEllipse(zone); }
                else { canvas.DrawArc(zone, 90, fin, true, false); }
            }
            canvas.FontColor = Texte;
            canvas.FontSize = rect.Width * 0.24f;
            canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
            canvas.DrawString($"{Math.Round(Progression * 100)}%", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
}

/// <summary>Briques visuelles de l'application : en-tête en dégradé, tuiles, états vides, pastilles d'icône.</summary>
public static class Ui
{
    /// <summary>Dégradé de marque (bleu nuit vers sarcelle), identique au bandeau des fiches du site.</summary>
    public static LinearGradientBrush Degrade() => new()
    {
        StartPoint = new Point(0, 0), EndPoint = new Point(1, 1),
        GradientStops =
        {
            new GradientStop(Color.FromArgb("#0b1f3a"), 0f),
            new GradientStop(Color.FromArgb("#0f3d5c"), 0.55f),
            new GradientStop(Color.FromArgb("#0f766e"), 1f)
        }
    };

    /// <summary>Carte d'en-tête en dégradé avec contenu libre (texte blanc).</summary>
    public static Border EnTete(View contenu, Thickness? marge = null) => new()
    {
        Background = Degrade(), StrokeThickness = 0, Padding = new Thickness(18, 16),
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(18) }, Margin = marge ?? new Thickness(0, 0, 0, 12),
        Shadow = new Shadow { Brush = Color.FromArgb("#0b1f3a"), Opacity = 0.25f, Radius = 18, Offset = new Point(0, 8) },
        Content = contenu
    };

    /// <summary>Pastille carrée arrondie contenant une icône.</summary>
    public static Border Pastille(string glyphe, Color fond, Color couleur, double taille = 44, double tailleIcone = 22) => new()
    {
        BackgroundColor = fond, StrokeThickness = 0, WidthRequest = taille, HeightRequest = taille,
        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(taille * 0.28) },
        VerticalOptions = LayoutOptions.Center, Content = Icones.Ico(glyphe, tailleIcone, couleur)
    };

    /// <summary>Tuile d'accès rapide (grille de l'accueil).</summary>
    public static Border Tuile(string glyphe, Color couleur, Color fond, string titre, string sousTitre, Action clic)
    {
        var contenu = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                Pastille(glyphe, fond, couleur, 42, 21),
                new VerticalStackLayout
                {
                    Spacing = 1,
                    Children =
                    {
                        new Label { Text = titre, FontSize = 14.5, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, LineBreakMode = LineBreakMode.TailTruncation },
                        new Label { Text = sousTitre, FontSize = 12, TextColor = Couleurs.Muet, MaxLines = 2, LineBreakMode = LineBreakMode.TailTruncation }
                    }
                }
            }
        };
        var carte = new Border
        {
            BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(14),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(16) },
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.05f, Radius = 12, Offset = new Point(0, 4) },
            Content = contenu
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => { await carte.ScaleTo(0.97, 60); await carte.ScaleTo(1, 60); clic(); };
        carte.GestureRecognizers.Add(tap);
        return carte;
    }

    /// <summary>Indicateur chiffré compact.</summary>
    public static Border Kpi(string glyphe, Color couleur, Color fond, string valeur, string libelle)
    {
        var g = new Grid { ColumnSpacing = 10, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        g.Add(Pastille(glyphe, fond, couleur, 38, 19), 0, 0);
        g.Add(new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { Text = valeur, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte },
                new Label { Text = libelle, FontSize = 11.5, TextColor = Couleurs.Muet, LineBreakMode = LineBreakMode.TailTruncation }
            }
        }, 1, 0);
        return new Border
        {
            BackgroundColor = Couleurs.Carte, StrokeThickness = 0, Padding = new Thickness(12, 10),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) },
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.05f, Radius = 10, Offset = new Point(0, 3) },
            Content = g
        };
    }

    /// <summary>État vide illustré (icône dans un cercle, titre, explication).</summary>
    public static View EtatVide(string glyphe, string titre, string texte, Color? couleur = null) => new VerticalStackLayout
    {
        Spacing = 8, Padding = new Thickness(24, 40), HorizontalOptions = LayoutOptions.Center,
        Children =
        {
            new Border
            {
                WidthRequest = 72, HeightRequest = 72, StrokeThickness = 0, HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = Couleurs.BleuClair, StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(36) },
                Content = Icones.Ico(glyphe, 32, couleur ?? Couleurs.Accent)
            },
            new Label { Text = titre, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Couleurs.Texte, HorizontalTextAlignment = TextAlignment.Center },
            new Label { Text = texte, FontSize = 13, TextColor = Couleurs.Muet, HorizontalTextAlignment = TextAlignment.Center, MaximumWidthRequest = 420 }
        }
    };

    /// <summary>Champ de recherche arrondi avec loupe.</summary>
    public static Border Recherche(Entry saisie)
    {
        saisie.FontSize = 15;
        saisie.TextColor = Couleurs.Texte;
        saisie.PlaceholderColor = Couleurs.Muet;
        saisie.BackgroundColor = Colors.Transparent;
        saisie.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
        var g = new Grid { ColumnSpacing = 6, ColumnDefinitions = { new ColumnDefinition(GridLength.Auto), new ColumnDefinition(GridLength.Star) } };
        g.Add(Icones.Ico(Icones.Recherche, 18, Couleurs.Muet), 0, 0);
        g.Add(saisie, 1, 0);
        return new Border
        {
            BackgroundColor = Couleurs.Carte, StrokeThickness = 1, Stroke = Couleurs.Bordure, Padding = new Thickness(12, 0), HeightRequest = 48,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(14) }, Content = g
        };
    }
}
