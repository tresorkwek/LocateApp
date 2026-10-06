using System.Text.Json;

namespace LocateMobile.Services;

/// <summary>Opération serveur faite sans réseau, gardée sur l'appareil jusqu'à son envoi.</summary>
public sealed class OperationEnAttente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Chemin { get; set; } = "";
    public Dictionary<string, string> Champs { get; set; } = new();
    public string Libelle { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string? DerniereErreur { get; set; }
}

/// <summary>
/// File d'attente hors ligne : identifications et déclarations « non vu »
/// faites sans réseau sont enregistrées dans un fichier de l'application, puis renvoyées dans l'ordre dès que le serveur
/// répond. Une opération refusée par le serveur (et non par le réseau) est retirée de la file avec son message.
/// </summary>
public static class FileHorsLigne
{
    private static readonly string Fichier = Path.Combine(FileSystem.AppDataDirectory, "operations_en_attente.json");
    private static readonly SemaphoreSlim Verrou = new(1, 1);
    private static List<OperationEnAttente>? cache;

    /// <summary>Déclenché quand le nombre d'opérations en attente change.</summary>
    public static event Action? Change;

    public static int Nombre => Charger().Count;
    public static IReadOnlyList<OperationEnAttente> Operations => Charger();

    private static List<OperationEnAttente> Charger()
    {
        if (cache != null) { return cache; }
        try { cache = File.Exists(Fichier) ? JsonSerializer.Deserialize<List<OperationEnAttente>>(File.ReadAllText(Fichier)) ?? new() : new(); }
        catch { cache = new(); }
        return cache;
    }

    private static void Enregistrer()
    {
        try { File.WriteAllText(Fichier, JsonSerializer.Serialize(Charger())); } catch { /* stockage indisponible : la file reste en mémoire */ }
        MainThread.BeginInvokeOnMainThread(() => Change?.Invoke());
    }

    public static void Ajouter(string chemin, IDictionary<string, string> champs, string libelle)
    {
        Charger().Add(new OperationEnAttente { Chemin = chemin, Champs = new Dictionary<string, string>(champs), Libelle = libelle });
        Enregistrer();
    }

    /// <summary>Envoie les opérations en attente, dans l'ordre. Renvoie (envoyées, refusées, restantes).</summary>
    public static async Task<(int envoyees, int refusees, int restantes)> Synchroniser(ApiClient api)
    {
        if (Charger().Count == 0) { return (0, 0, 0); }
        if (!await Verrou.WaitAsync(0)) { return (0, 0, Charger().Count); }
        int envoyees = 0, refusees = 0;
        try
        {
            foreach (var op in Charger().ToList())
            {
                try
                {
                    var r = await api.Action(op.Chemin, op.Champs);
                    if (r.Ok) { envoyees++; } else { refusees++; Refusees.Add($"{op.Libelle} : {r.Message}"); }
                    Charger().Remove(op);
                    Enregistrer();
                }
                catch (ReseauException) { break; }                // toujours hors ligne : on réessaiera plus tard
                catch (SessionExpireeException) { break; }        // il faut se reconnecter d'abord
                catch (ApiException e) { refusees++; Refusees.Add($"{op.Libelle} : {e.Message}"); Charger().Remove(op); Enregistrer(); }
            }
        }
        finally { Verrou.Release(); }
        return (envoyees, refusees, Charger().Count);
    }

    /// <summary>Messages des opérations refusées par le serveur lors des synchronisations (pour information).</summary>
    public static List<string> Refusees { get; } = new();
}
