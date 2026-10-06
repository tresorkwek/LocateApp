using System.Text.Json.Serialization;
using LocateMobile.Pages;

namespace LocateMobile.Models;

/* Modèles minimaux des réponses JSON du serveur Locate (noms de propriétés insensibles à la casse à la désérialisation).
   Les dates non renseignées arrivent sous la forme 0001-01-01 : Dates.Ok() les filtre. */

public static class Dates
{
    public static bool Ok(DateTime? d) => d.HasValue && d.Value.Year > 1900;
    public static string? Courte(DateTime? d) => Ok(d) ? d!.Value.ToString("dd MMM yyyy") : null;
    public static string? Longue(DateTime? d) => Ok(d) ? d!.Value.ToString("dd MMM yyyy HH:mm") : null;
}

public class Identite
{
    public string? UserName { get; set; }
    public string? Nom { get; set; }
    public string? Postnom { get; set; }
    public string? Prenom { get; set; }
    public string? IdInstitution { get; set; }
    public string? CodeOrgane { get; set; }
    public string? Photo { get; set; }
    public Profil? Profil { get; set; }
    public bool IsExternalUser { get; set; }
    public bool FirstConnexion { get; set; }

    [JsonIgnore] public string NomComplet => string.Join(" ", new[] { Nom, Postnom, Prenom }.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!.Trim()));
}

public class Profil
{
    public int IdProfil { get; set; }
    public string? Nom { get; set; }
}

public class Inventaire
{
    public int Annee { get; set; }
    public bool Actif { get; set; }
}

/// <summary>Avancement de l'inventaire d'un local ou d'un organe, déduit des compteurs total / identifiés / inventoriés.</summary>
public enum Etape { Vide, AFaire, ACloturer, Incomplet, Cloture }

public static class Avancement
{
    /// <summary>
    /// Rouge : rien n'est fait. Bleu : des biens ont été identifiés mais le local n'est pas encore clôturé.
    /// Orange : clôturé, mais des biens ne sont ni vus ni déclarés non vus. Vert : tous les biens sont inventoriés.
    /// </summary>
    public static Etape De(long total, long identifies, long inventories) =>
        total == 0 && inventories == 0 ? Etape.Vide
        : identifies > inventories ? Etape.ACloturer
        : inventories >= total ? Etape.Cloture
        : inventories > 0 || identifies > 0 ? Etape.Incomplet
        : Etape.AFaire;

    public static string Libelle(Etape e) => e switch { Etape.Vide => "Vide", Etape.AFaire => "À faire", Etape.ACloturer => "À clôturer", Etape.Incomplet => "Incomplet", _ => "Clôturé" };
    public static Color Couleur(Etape e) => e switch { Etape.Vide => Couleurs.Gris, Etape.AFaire => Couleurs.Rouge, Etape.ACloturer => Couleurs.Bleu, Etape.Incomplet => Couleurs.Orange, _ => Couleurs.VertFonce };
    public static Color Fond(Etape e) => e switch { Etape.Vide => Couleurs.GrisClair, Etape.AFaire => Couleurs.RougeClair, Etape.ACloturer => Couleurs.BleuClair, Etape.Incomplet => Couleurs.OrangeClair, _ => Couleurs.VertClair };
    public static string Glyphe(Etape e) => e switch { Etape.Vide => Icones.Cube, Etape.AFaire => Icones.Historique, Etape.ACloturer => Icones.Oeil, Etape.Incomplet => Icones.Attention, _ => Icones.CocheRonde };
}

public class Organe
{
    public string Id { get; set; } = "";
    public string? Nom { get; set; }
    public string? Sigle { get; set; }
    public string? IdOrganeParent { get; set; }
    public string? IdStructure { get; set; }
    public long NbreBien { get; set; }
    public long NbreBienIdentifie { get; set; }
    public long NbreBienInventorie { get; set; }
    public List<Organe>? Organes { get; set; }

    [JsonIgnore] public int Niveau { get; set; }
    [JsonIgnore] public string SousTitre => string.IsNullOrWhiteSpace(Sigle) ? Id : $"{Id} · {Sigle}";
    [JsonIgnore] public Thickness Retrait => new(Niveau * 14, 0, 0, 0);
    [JsonIgnore] public Etape Etape => Avancement.De(NbreBien, NbreBienIdentifie, NbreBienInventorie);
    [JsonIgnore] public string EtapeLibelle => Avancement.Libelle(Etape);
    [JsonIgnore] public Color EtapeCouleur => Avancement.Couleur(Etape);
    [JsonIgnore] public Color EtapeFond => Avancement.Fond(Etape);
    [JsonIgnore] public string EtapeGlyphe => Avancement.Glyphe(Etape);
    /// <summary>Compteur : total / identifiés / inventoriés.</summary>
    [JsonIgnore] public string Compteur => $"{NbreBien:N0} / {NbreBienIdentifie:N0} / {NbreBienInventorie:N0}";
}

public class Local
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public string? Designation { get; set; }
    public string? CodeOrgane { get; set; }
    public Guid? QrCode { get; set; }
    public bool IsSpace { get; set; }
    public bool IsActive { get; set; } = true;
    public int? IdTypeLocal { get; set; }
    public long QuantiteImmo { get; set; }
    public long QuantiteImmoIdentifier { get; set; }
    public long QuantiteImmoInventorier { get; set; }

    [JsonIgnore] public bool AQrCode => QrCode.HasValue && QrCode.Value != Guid.Empty;
    [JsonIgnore] public string SousTitre => (Code ?? "") + (AQrCode ? "" : (string.IsNullOrEmpty(Code) ? "" : " · ") + "sans QR code") + (IsSpace ? " · espace" : "");
    /// <summary>Image embarquée : QR code vert si le local a son étiquette, QR code gris barré sinon.</summary>
    [JsonIgnore] public string ImageQr => AQrCode ? "qr_present.png" : "qr_absent.png";
    [JsonIgnore] public string QrLibelle => AQrCode ? "QR code affecté" : "Sans QR code";
    [JsonIgnore] public Color CouleurQr => AQrCode ? Couleurs.VertFonce : Couleurs.Orange;
    [JsonIgnore] public Color FondQr => AQrCode ? Couleurs.VertClair : Couleurs.OrangeClair;
    [JsonIgnore] public Etape Etape => Avancement.De(QuantiteImmo, QuantiteImmoIdentifier, QuantiteImmoInventorier);
    [JsonIgnore] public string EtapeLibelle => Avancement.Libelle(Etape);
    [JsonIgnore] public Color EtapeCouleur => Avancement.Couleur(Etape);
    [JsonIgnore] public Color EtapeFond => Avancement.Fond(Etape);
    [JsonIgnore] public string EtapeGlyphe => Avancement.Glyphe(Etape);
    /// <summary>Compteur : total / identifiés / inventoriés.</summary>
    [JsonIgnore] public string Compteur => $"{QuantiteImmo:N0} / {QuantiteImmoIdentifier:N0} / {QuantiteImmoInventorier:N0}";
}

public class Article
{
    public long Id { get; set; }
    public string? Code { get; set; }
    public string? Designation { get; set; }
    public string? Marque { get; set; }
    public string? Modele { get; set; }
    public long NbreImmo { get; set; }
    public string? Photo { get; set; }
    public bool IsPhotographed { get; set; }

    [JsonIgnore] public string SousTitre { get { var s = string.Join(" ", new[] { Marque, Modele }.Where(x => !string.IsNullOrWhiteSpace(x))); return string.IsNullOrWhiteSpace(s) ? (Code ?? "") : s; } }
    [JsonIgnore] public string UrlPhoto => App.Api.Url("Content/images/articles/" + (Photo ?? ""));
}

public class ImmoPhoto
{
    public Guid Id { get; set; }
    public long IdImmo { get; set; }
    public string? Constat { get; set; }

    [JsonIgnore] public string Url => App.Api.Url("Content/images/equipements/" + Id + ".jpg");
}

public class Immo
{
    public long Id { get; set; }
    public string? CodeADM { get; set; }
    public string? CodeImmo { get; set; }
    public long? IdArticle { get; set; }
    public long? IdLocal { get; set; }
    public Guid? QrCode { get; set; }
    public string? LastEtat { get; set; }
    public string? LastEtatString { get; set; }
    public int IdLastObservation { get; set; }
    public string? LastObservation { get; set; }
    public int LastAnneeComptable { get; set; }
    public string? Responsable { get; set; }
    public string? Inventorieur { get; set; }
    public DateTime? DateInventaire { get; set; }
    public DateTime? DateMisEnService { get; set; }
    public string? UserVu { get; set; }
    public DateTime? DateVu { get; set; }
    public string? Observation { get; set; }
    public string? Designation { get; set; }
    public string? DesignationImmoPrincipal { get; set; }
    public long? IdImmoParent { get; set; }
    public bool Inventorier { get; set; }
    public string? Photo { get; set; }
    public List<ImmoPhoto>? ImmoPhotos { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DateDeclassement { get; set; }
    public string? UserDeclassement { get; set; }
    public Local? Local { get; set; }          // présent dans la réponse /immo/qrcode/{guid}
    public Article? Article { get; set; }      // idem

    /// <summary>Année d'inventaire en cours, fournie par la session, pour savoir si le bien a déjà été vu.</summary>
    public static int AnneeEnCours { get; set; } = DateTime.Now.Year;

    [JsonIgnore] public bool AQrCode => QrCode.HasValue && QrCode.Value != Guid.Empty;
    [JsonIgnore] public string ImageQr => AQrCode ? "qr_present.png" : "qr_absent.png";
    [JsonIgnore] public string QrLibelle => AQrCode ? "QR code affecté" : "Sans QR code";
    [JsonIgnore] public Color FondQr => AQrCode ? Couleurs.VertClair : Couleurs.GrisClair;
    /// <summary>
    /// Identifié dans l'inventaire en cours : le serveur remet UserVu à vide au lancement de chaque inventaire
    /// (l'année d'inventaire est comptable : un bien de l'inventaire 2023 peut être vu en 2024).
    /// </summary>
    [JsonIgnore] public bool EstVu => !string.IsNullOrWhiteSpace(UserVu);
    /// <summary>Inventorié dans l'inventaire en cours : on n'inventorie que ce qu'on a vu (identifié), Inventorieur renseigné et année en cours.</summary>
    [JsonIgnore] public bool EstInventorie => EstVu && !string.IsNullOrWhiteSpace(Inventorieur) && LastAnneeComptable == AnneeEnCours;
    /// <summary>Ordre d'affichage : à faire, puis identifiés (à clôturer), puis inventoriés.</summary>
    [JsonIgnore] public int Rang => EstInventorie ? 2 : EstVu ? 1 : 0;
    [JsonIgnore] public string Titre => string.IsNullOrWhiteSpace(Designation) ? "Bien " + Id : Designation!;
    [JsonIgnore] public string Codes => string.Join(" · ", new[] { CodeADM, CodeImmo }.Where(x => !string.IsNullOrWhiteSpace(x)));
    [JsonIgnore] public string EtatLibelle => !string.IsNullOrWhiteSpace(LastEtatString) ? LastEtatString! : LastEtat == "B" ? "Bon état" : LastEtat == "M" ? "Mauvais état" : "État inconnu";
    [JsonIgnore] public Color EtatCouleur => LastEtat == "B" ? Couleurs.VertFonce : LastEtat == "M" ? Couleurs.Rouge : Couleurs.Gris;
    [JsonIgnore] public Color EtatFond => LastEtat == "B" ? Couleurs.VertClair : LastEtat == "M" ? Couleurs.RougeClair : Couleurs.GrisClair;
    // Coloration : bleu dès que le bien est identifié, vert quand l'inventaire du local est clôturé.
    [JsonIgnore] public Color FondLigne => EstInventorie ? Couleurs.VertClair : EstVu ? Couleurs.BleuClair : Couleurs.Carte;
    [JsonIgnore] public string VuLibelle => EstInventorie ? "inventorié" + (Dates.Ok(DateInventaire) ? " le " + Dates.Courte(DateInventaire) : "") : EstVu ? "vu" + (Dates.Ok(DateVu) ? " le " + Dates.Courte(DateVu) : "") + " · à clôturer" : "";
    [JsonIgnore] public bool VuVisible => EstVu || EstInventorie;
    [JsonIgnore] public Color VuCouleur => EstInventorie ? Couleurs.VertFonce : Couleurs.Bleu;
    [JsonIgnore] public string EtatIcone => EstInventorie ? Icones.CocheRonde : EstVu ? Icones.Oeil : Icones.OeilBarre;
    [JsonIgnore] public Color EtatIconeCouleur => EstInventorie ? Couleurs.VertFonce : EstVu ? Couleurs.Bleu : Couleurs.Bordure;
    [JsonIgnore] public string UrlPhoto => App.Api.Url("Content/images/equipements/" + (string.IsNullOrWhiteSpace(Photo) ? "defaultImmo.jpg" : Photo));
}

public class Observation
{
    public int Id { get; set; }
    [JsonPropertyName("observation")] public string? Libelle { get; set; }
    public string? Etat { get; set; }
    public override string ToString() => Libelle ?? "";
}
