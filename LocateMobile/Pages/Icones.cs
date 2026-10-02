namespace LocateMobile.Pages;

/// <summary>
/// Icônes vectorielles (police Line Awesome embarquée, Resources/Fonts/LineAwesome.ttf) : nettes à toutes les tailles,
/// colorables, identiques sur tous les appareils (contrairement aux caractères Unicode ou aux emoji).
/// </summary>
public static class Icones
{
    public const string Police = "LineAwesome";

    public const string Accueil = "";
    public const string Qr = "";
    public const string Organigramme = "";
    public const string Batiment = "";
    public const string Cube = "";
    public const string Biens = "";
    public const string Oeil = "";
    public const string OeilBarre = "";
    public const string Echange = "";
    public const string Archive = "";
    public const string Recherche = "";
    public const string Camera = "";
    public const string Coche = "";
    public const string CocheRonde = "";
    public const string Croix = "";
    public const string CroixRonde = "";
    public const string Attention = "";
    public const string Alerte = "";
    public const string Eclair = "";
    public const string Chevron = "";
    public const string ChevronGauche = "";
    public const string Actualiser = "";
    public const string Utilisateur = "";
    public const string Reglages = "";
    public const string Deconnexion = "";
    public const string Lieu = "";
    public const string CodeBarres = "";
    public const string Fleche = "";
    public const string Plus = "";
    public const string Liste = "";
    public const string Grille = "";
    public const string Clavier = "";
    public const string Lampe = "";
    public const string Camion = "";
    public const string Calques = "";
    public const string Porte = "";
    public const string Info = "";
    public const string Serveur = "";
    public const string Historique = "";
    public const string Calendrier = "";
    public const string Tableau = "";
    public const string Dossier = "";
    public const string Annuler = "";
    public const string Carton = "";
    public const string Livraison = "";
    public const string Drapeau = "";
    public const string Crayon = "";
    public const string Corbeille = "";
    public const string Lien = "";
    public const string Delier = "";
    public const string Presse = "";
    public const string Menu = "";

    /// <summary>Icône sous forme de Label (dans une mise en page).</summary>
    public static Label Ico(string glyphe, double taille, Color couleur) => new()
    {
        Text = glyphe, FontFamily = Police, FontSize = taille, TextColor = couleur,
        HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center,
        HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
    };

    /// <summary>Icône sous forme d'image (onglets, boutons, barre d'outils).</summary>
    public static FontImageSource Image(string glyphe, Color couleur, double taille = 24) => new()
    {
        Glyph = glyphe, FontFamily = Police, Color = couleur, Size = taille
    };
}
