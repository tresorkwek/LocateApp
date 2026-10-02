using LocateMobile.Models;

namespace LocateMobile.Services;

/// <summary>État partagé : utilisateur connecté, année d'inventaire, organigramme de l'agent et cache des locaux.</summary>
public sealed class Session
{
    private readonly ApiClient api;

    public Identite? Utilisateur { get; private set; }
    public int Annee { get; private set; } = DateTime.Now.Year;
    /// <summary>Vrai si le serveur a un inventaire ouvert (non clôturé) ; sinon Annee est l'année du calendrier.</summary>
    public bool InventaireOuvert { get; private set; }
    public List<Organe> Arbre { get; private set; } = new();
    public List<Organe> Plat { get; private set; } = new();
    public Dictionary<long, string> NomsLocaux { get; } = new();

    public Session(ApiClient api) { this.api = api; }

    public bool Connecte => Utilisateur != null;

    public void Vider()
    {
        Utilisateur = null;
        Arbre = new List<Organe>();
        Plat = new List<Organe>();
        NomsLocaux.Clear();
    }

    /// <summary>Charge l'identité (/profile/) et l'année d'inventaire en cours (/inventaire/encours/).</summary>
    public async Task<Identite> ChargerProfil()
    {
        var r = await api.Get<Identite>("profile/");
        Utilisateur = r.Content ?? throw new SessionExpireeException();
        try
        {
            var inv = await api.Get<Inventaire>("inventaire/encours/");
            InventaireOuvert = inv.Content?.Annee > 0;
            Annee = InventaireOuvert ? inv.Content!.Annee : DateTime.Now.Year;
        }
        catch (ApiException) { InventaireOuvert = false; Annee = DateTime.Now.Year; }
        Immo.AnneeEnCours = Annee;
        return Utilisateur;
    }

    /// <summary>Organigramme des organes affectés à l'agent (structure IdInstitution), avec les compteurs de biens.</summary>
    public async Task<List<Organe>> ChargerArbre(bool forcer = false)
    {
        if (Arbre.Count > 0 && !forcer) { return Arbre; }
        string inst = Utilisateur?.IdInstitution ?? "";
        var r = await api.Get<List<Organe>>(inst.Length > 0 ? $"local/organe/list/{Uri.EscapeDataString(inst)}/" : "local/organe/list/");
        Arbre = r.Content ?? new List<Organe>();
        Plat = new List<Organe>();
        void Aplatir(IEnumerable<Organe>? liste, int niveau)
        {
            foreach (var o in liste ?? Enumerable.Empty<Organe>())
            {
                o.Niveau = niveau;
                Plat.Add(o);
                Aplatir(o.Organes, niveau + 1);
            }
        }
        Aplatir(Arbre, 0);
        return Arbre;
    }

    public Organe? OrganeParId(string? id) => id == null ? null : Plat.FirstOrDefault(o => string.Equals(o.Id, id, StringComparison.OrdinalIgnoreCase));

    public (long total, long identifies, long inventories) Totaux()
        => (Plat.Sum(o => o.NbreBien), Plat.Sum(o => o.NbreBienIdentifie), Plat.Sum(o => o.NbreBienInventorie));

    public async Task<Local?> ChargerLocal(long id)
    {
        var local = await api.Premier<Local>($"local/id/{id}");
        if (local != null) { NomsLocaux[local.Id] = local.Designation ?? ""; }
        return local;
    }

    public async Task<string?> NomLocal(long? id)
    {
        if (id == null) { return null; }
        if (NomsLocaux.TryGetValue(id.Value, out var nom)) { return nom; }
        try { return (await ChargerLocal(id.Value))?.Designation; } catch (ApiException) { return null; }
    }
}
