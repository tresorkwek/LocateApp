using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocateMobile.Services;

public class ApiException : Exception { public ApiException(string message) : base(message) { } }
public class SessionExpireeException : ApiException { public SessionExpireeException() : base("Connectez-vous pour continuer.") { } }
/// <summary>Serveur injoignable (pas de réseau, délai dépassé) : l'opération peut être mise en attente hors ligne.</summary>
public class ReseauException : ApiException { public ReseauException(string message) : base(message) { } }

/// <summary>Jeton JWT renvoyé par POST /auth/login aux clients non-web.</summary>
public class Jeton
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}

/// <summary>Réponse normalisée du serveur : { success, message, content } (+ token à la connexion).</summary>
public class Reponse<T>
{
    public int Success { get; set; }
    public string Message { get; set; } = "";
    public T? Content { get; set; }
    public Jeton? Token { get; set; }
    public bool Ok => Success == 1 || Success == 4;
}

/// <summary>
/// Accès au serveur Locate. Les routes sont celles du site web ; l'en-tête X-Requested-With obtient du JSON.
/// Authentification : le jeton JWT reçu à la connexion est envoyé dans « Authorization: Bearer … » à chaque requête.
/// Il expire vite (expires_in, 10 min) : les identifiants sont gardés en mémoire (jamais sur disque) le temps de la session
/// pour renouveler le jeton sans redemander le mot de passe.
/// Particularité du serveur : les statuts 401/403/500 sont réécrits en HTTP 200 { "Success": 0, "Message": ... }
/// (clés en PascalCase), alors que les réponses normales ont des clés en camelCase : on s'en sert pour les distinguer.
/// </summary>
public sealed class ApiClient
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new DateSoupleConverter(), new DateSoupleConverterNonNull() }
    };

    /// <summary>
    /// Le serveur sérialise les dates non renseignées en « 0001-01-01T00:00:00.0000000+01:00 » : appliquer ce décalage à l'an 1
    /// sort de la plage de DateTime et fait échouer System.Text.Json. On lit donc les dates avec tolérance :
    /// vide ou an 1 → null, sinon l'heure telle qu'écrite par le serveur (sans conversion de fuseau).
    /// </summary>
    private sealed class DateSoupleConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => LireDate(ref reader);
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null) { writer.WriteNullValue(); } else { writer.WriteStringValue(value.Value.ToString("o")); }
        }
    }

    private sealed class DateSoupleConverterNonNull : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => LireDate(ref reader) ?? DateTime.MinValue;
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("o"));
    }

    private static DateTime? LireDate(ref Utf8JsonReader reader)
    {
        if (reader.TokenType == JsonTokenType.Null) { return null; }
        if (reader.TokenType != JsonTokenType.String) { reader.Skip(); return null; }
        string? s = reader.GetString();
        if (string.IsNullOrWhiteSpace(s) || s.StartsWith("0001-", StringComparison.Ordinal) || s.StartsWith("1900-01-01", StringComparison.Ordinal)) { return null; }
        if (DateTimeOffset.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var dto))
        {
            return dto.Year <= 1900 ? null : dto.DateTime;
        }
        return DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var d) && d.Year > 1900 ? d : null;
    }

    private readonly CookieContainer cookies = new();
    private HttpClient http = new();
    private string? jeton;
    private DateTime jetonExpire = DateTime.MinValue;
    private (string utilisateur, string motDePasse)? identifiants;
    private readonly SemaphoreSlim verrouReconnexion = new(1, 1);

    public string BaseUrl { get; private set; } = "";
    public bool Connecte => jeton != null;

    public ApiClient()
    {
        Configurer(Preferences.Default.Get("ServeurUrl", ""), memoriser: false);
    }

    /// <summary>Client indépendant vers une adresse donnée (test de connexion), sans modifier le réglage mémorisé.</summary>
    public ApiClient(string url)
    {
        Configurer(url, memoriser: false);
    }

    /// <summary>Définit l'adresse du serveur (ex. https://locate.monentreprise.cd) et, par défaut, la mémorise.</summary>
    public void Configurer(string url, bool memoriser = true)
    {
        url = (url ?? "").Trim();
        if (url.Length > 0 && !url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) { url = "http://" + url; }
        BaseUrl = url.TrimEnd('/');
        if (memoriser) { Preferences.Default.Set("ServeurUrl", BaseUrl); }

        var handler = new HttpClientHandler { CookieContainer = cookies, UseCookies = true, AllowAutoRedirect = false };
        http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(90) };
        if (BaseUrl.Length > 0) { http.BaseAddress = new Uri(BaseUrl + "/"); }
        http.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        http.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
        AppliquerJeton();
    }

    public string Url(string chemin) => BaseUrl + "/" + chemin.TrimStart('/');

    /* ------------------------------------------------------------------ connexion / jeton */

    private void AppliquerJeton()
    {
        http.DefaultRequestHeaders.Authorization = jeton == null ? null : new AuthenticationHeaderValue("Bearer", jeton);
    }

    /// <summary>Connexion : enregistre le jeton reçu et garde les identifiants en mémoire pour le renouveler.</summary>
    public async Task<Reponse<Models.Identite>> Connexion(string utilisateur, string motDePasse)
    {
        jeton = null; AppliquerJeton();
        var r = await EnvoyerUneFois<Models.Identite>(RequeteConnexion(utilisateur, motDePasse));
        if (r.Ok && !string.IsNullOrEmpty(r.Token?.AccessToken))
        {
            identifiants = (utilisateur, motDePasse);
            EnregistrerJeton(r.Token!);
        }
        else if (r.Ok)
        {
            // Pas de jeton : le serveur a ouvert une session par cookie, elle suffit.
            identifiants = (utilisateur, motDePasse);
        }
        return r;
    }

    public void Deconnexion()
    {
        jeton = null; identifiants = null; jetonExpire = DateTime.MinValue;
        AppliquerJeton();
    }

    private HttpRequestMessage RequeteConnexion(string utilisateur, string motDePasse) => new(HttpMethod.Post, "auth/login")
    {
        Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["UserName"] = utilisateur, ["Password"] = motDePasse, ["RememberMe"] = "1" })
    };

    private void EnregistrerJeton(Jeton t)
    {
        jeton = t.AccessToken;
        // marge de 30 s pour renouveler avant l'expiration réelle
        jetonExpire = DateTime.UtcNow.AddSeconds(Math.Max(60, t.ExpiresIn) - 30);
        AppliquerJeton();
    }

    /// <summary>Renouvelle le jeton avec les identifiants gardés en mémoire ; faux si impossible.</summary>
    private async Task<bool> Reconnecter()
    {
        if (identifiants == null) { return false; }
        await verrouReconnexion.WaitAsync();
        try
        {
            if (jeton != null && DateTime.UtcNow < jetonExpire) { return true; } // déjà renouvelé par un autre appel
            var r = await EnvoyerUneFois<Models.Identite>(RequeteConnexion(identifiants.Value.utilisateur, identifiants.Value.motDePasse));
            if (r.Ok && !string.IsNullOrEmpty(r.Token?.AccessToken)) { EnregistrerJeton(r.Token!); return true; }
            return r.Ok;
        }
        catch (ApiException) { return false; }
        finally { verrouReconnexion.Release(); }
    }

    /* ------------------------------------------------------------------ requêtes */

    public Task<Reponse<T>> Get<T>(string chemin) => Envoyer<T>(() => new HttpRequestMessage(HttpMethod.Get, chemin.TrimStart('/')));

    public Task<Reponse<T>> Post<T>(string chemin, IDictionary<string, string> formulaire)
        => Envoyer<T>(() => new HttpRequestMessage(HttpMethod.Post, chemin.TrimStart('/')) { Content = new FormUrlEncodedContent(formulaire) });

    /// <summary>Action serveur (réponse { success, message }) : renvoie la réponse, l'appelant affiche le message.</summary>
    public Task<Reponse<string>> Action(string chemin, IDictionary<string, string>? formulaire = null)
        => formulaire == null ? Get<string>(chemin) : Post<string>(chemin, formulaire);

    /// <summary>Envoi multipart (formulaire + un fichier), par exemple une photo de bien vers /immo/add/photo.</summary>
    public Task<Reponse<string>> ActionAvecFichier(string chemin, IDictionary<string, string> champs, byte[] fichier, string nomFichier, string typeMime = "image/jpeg")
        => Envoyer<string>(() =>
        {
            var contenu = new MultipartFormDataContent();
            foreach (var c in champs) { contenu.Add(new StringContent(c.Value), c.Key); }
            var f = new ByteArrayContent(fichier);
            f.Headers.ContentType = new MediaTypeHeaderValue(typeMime);
            contenu.Add(f, "file", nomFichier);
            return new HttpRequestMessage(HttpMethod.Post, chemin.TrimStart('/')) { Content = contenu };
        });

    /// <summary>Premier élément d'une réponse liste, ou l'objet lui-même.</summary>
    public async Task<T?> Premier<T>(string chemin) where T : class
    {
        var r = await Get<List<T>>(chemin);
        return r.Content?.FirstOrDefault();
    }

    /// <summary>Envoi avec renouvellement du jeton : avant l'appel s'il est expiré, et une fois après un refus d'authentification.</summary>
    private async Task<Reponse<T>> Envoyer<T>(Func<HttpRequestMessage> creer)
    {
        if (jeton != null && DateTime.UtcNow >= jetonExpire) { await Reconnecter(); }
        try { return await EnvoyerUneFois<T>(creer()); }
        catch (SessionExpireeException) when (identifiants != null)
        {
            if (!await Reconnecter()) { throw; }
            return await EnvoyerUneFois<T>(creer());
        }
    }

    private async Task<Reponse<T>> EnvoyerUneFois<T>(HttpRequestMessage req)
    {
        if (BaseUrl.Length == 0) { throw new ApiException("Adresse du serveur non définie."); }
        HttpResponseMessage resp;
        try { resp = await http.SendAsync(req); }
        catch (TaskCanceledException) { throw new ReseauException("Le serveur met trop de temps à répondre."); }
        catch (Exception e) { throw new ReseauException("Serveur injoignable : " + e.Message); }

        if (resp.StatusCode == HttpStatusCode.Unauthorized) { throw new SessionExpireeException(); }
        if ((int)resp.StatusCode is 301 or 302 or 303) { throw new SessionExpireeException(); }
        string texte = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) { throw new ApiException($"Erreur {(int)resp.StatusCode} du serveur."); }
        if (string.IsNullOrWhiteSpace(texte) || texte.TrimStart().StartsWith("<")) { throw new ApiException("Réponse inattendue du serveur (page HTML au lieu de données)."); }

        using var doc = JsonDocument.Parse(texte);
        var racine = doc.RootElement;
        if (racine.ValueKind == JsonValueKind.Object)
        {
            if (racine.TryGetProperty("Success", out var majuscule) && !racine.TryGetProperty("success", out _))
            {
                string? msg = racine.TryGetProperty("Message", out var m) ? m.GetString() : null;
                if (Nombre(majuscule) == 0)
                {
                    if (msg != null && msg.Contains("authentification", StringComparison.OrdinalIgnoreCase)) { throw new SessionExpireeException(); }
                    throw new ApiException(msg ?? "Demande refusée par le serveur.");
                }
            }
            if (racine.TryGetProperty("success", out var s))
            {
                var r = new Reponse<T> { Success = Nombre(s), Message = racine.TryGetProperty("message", out var mm) ? mm.GetString() ?? "" : "" };
                if (racine.TryGetProperty("content", out var contenu)) { r.Content = Lire<T>(contenu); }
                if (racine.TryGetProperty("token", out var tok) && tok.ValueKind == JsonValueKind.Object) { r.Token = tok.Deserialize<Jeton>(Options); }
                return r;
            }
        }
        return new Reponse<T> { Success = 1, Content = Lire<T>(racine) };
    }

    private static int Nombre(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.Number => (int)e.GetDouble(),
        JsonValueKind.String => int.TryParse(e.GetString(), out var n) ? n : 0,
        JsonValueKind.True => 1,
        _ => 0
    };

    private static T? Lire<T>(JsonElement e)
    {
        if (e.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) { return default; }
        if (typeof(T) == typeof(string)) { return (T?)(object?)(e.ValueKind == JsonValueKind.String ? e.GetString() : e.GetRawText()); }
        if (e.ValueKind == JsonValueKind.String) { return default; } // ex. Content = "" pour une action
        try { return e.Deserialize<T>(Options); }
        catch (JsonException ex) { throw new ApiException("Données illisibles : " + ex.Message); }
    }
}
