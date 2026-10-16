using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using SenderoAR.Core.Contracts;
using SenderoAR.Models;

namespace SenderoAR.Core.Repositories
{
    /// <summary>
    /// IMonumentRepository sobre el MonumentCatalogSO que ya vive en el proyecto.
    /// </summary>
    /// <remarks>
    /// <para>SNAPSHOTS CONSTRUIDOS UNA VEZ, EN EL CONSTRUCTOR: el catalogo es un
    /// ScriptableObject que no cambia en runtime, y MonumentSnapshot es inmutable.
    /// Construirlos al arrancar convierte un dato invalido (DisplayName vacio, ID
    /// duplicado) en una excepcion en el Composition Root, con el ID del monumento
    /// en el mensaje, en vez de un null 50 frames despues en un ViewModel.</para>
    ///
    /// <para>THREADING: todo es sincronico; los Task vuelven completados y se
    /// pueden esperar en el main thread sin cambiar de contexto.</para>
    ///
    /// <para>ORDEN: GetAllAsync respeta el orden del catalogo (mon_01 a mon_05).
    /// El contrato no lo garantiza; esta implementacion si, sin costo.</para>
    /// </remarks>
    public sealed class CatalogMonumentRepository : IMonumentRepository
    {
        private static readonly LanguageCode[] AllLanguages =
            { LanguageCode.EsBO, LanguageCode.EnUS, LanguageCode.PtBR };

        private readonly IReadOnlyList<MonumentSnapshot> _all;
        private readonly Dictionary<string, MonumentSnapshot> _byId;

        public bool IsReady => true;

        public CatalogMonumentRepository(MonumentCatalogSO catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));

            var snapshots = new List<MonumentSnapshot>(catalog.Count);
            _byId = new Dictionary<string, MonumentSnapshot>(catalog.Count, StringComparer.Ordinal);

            for (int i = 0; i < catalog.Count; i++)
            {
                var data = catalog.All[i];
                // Un slot vacio en el Inspector no es un monumento: se ignora, igual
                // que en MonumentCatalogSO.TryGetById.
                if (data == null) continue;

                var snapshot = ToSnapshot(data);
                if (_byId.ContainsKey(snapshot.Identifier))
                {
                    throw new InvalidOperationException(
                        $"[CatalogMonumentRepository] MonumentId duplicado en el catalogo: '{snapshot.Identifier}'.");
                }

                _byId.Add(snapshot.Identifier, snapshot);
                snapshots.Add(snapshot);
            }

            _all = new ReadOnlyCollection<MonumentSnapshot>(snapshots);
        }

        public Task<MonumentSnapshot> GetByIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<MonumentSnapshot>(cancellationToken);

            // Contrato: null si el identificador no existe. Un id vacio no existe.
            if (string.IsNullOrEmpty(identifier) || !_byId.TryGetValue(identifier, out var snapshot))
                return Task.FromResult<MonumentSnapshot>(null);

            return Task.FromResult(snapshot);
        }

        public Task<IReadOnlyList<MonumentSnapshot>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled<IReadOnlyList<MonumentSnapshot>>(cancellationToken);

            return Task.FromResult(_all);
        }

        private static MonumentSnapshot ToSnapshot(MonumentDataSO data)
        {
            try
            {
                return new MonumentSnapshot(
                    data.MonumentId,
                    data.DisplayName,
                    data.FoundationYear,
                    data.AnchorImageName,
                    ToDescriptions(data.Description));
            }
            catch (ArgumentException e)
            {
                // El snapshot dice QUE campo falta; aca se agrega DE QUE monumento.
                throw new InvalidOperationException(
                    $"[CatalogMonumentRepository] Datos invalidos en '{data.name}': {e.Message}", e);
            }
        }

        private static IReadOnlyDictionary<LanguageCode, string> ToDescriptions(LocalizedDescription description)
        {
            // MonumentSnapshot promete lookup para los 3 idiomas: un idioma sin
            // traducir (hoy EN y PT) devuelve "" en vez de faltar la clave.
            var result = new Dictionary<LanguageCode, string>(AllLanguages.Length);
            foreach (var language in AllLanguages)
                result[language] = TextFor(description, language) ?? string.Empty;
            return result;
        }

        private static string TextFor(LocalizedDescription description, LanguageCode language)
        {
            if (description == null) return null;
            switch (language)
            {
                case LanguageCode.EsBO: return description.Es;
                case LanguageCode.EnUS: return description.En;
                case LanguageCode.PtBR: return description.Pt;
                default: return null;
            }
        }
    }
}
