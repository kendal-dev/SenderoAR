using System;
using System.Collections.Generic;
using UnityEngine;

namespace SenderoAR.Core.Infrastructure
{
    /// <summary>
    /// Service Locator del proyecto: el Composition Root (AppBootstrapper)
    /// registra los servicios y los puntos de entrada (MonoBehaviours) los resuelven.
    /// </summary>
    /// <remarks>
    /// <para>CICLO DE VIDA: Register (solo antes de Initialize) → Initialize →
    /// Resolve / TryResolve. Cerrar el registro al inicializar obliga a que todas
    /// las decisiones de composicion (incluida la decoracion del modo demo) se
    /// tomen en un solo lugar, antes de que la app arranque.</para>
    ///
    /// <para>CLAVE = EL TIPO PEDIDO EN Register&lt;T&gt;: registrar como
    /// IMonumentRepository y pedir CatalogMonumentRepository no lo encuentra. Se
    /// registra y se resuelve siempre por el contrato.</para>
    ///
    /// <para>DEPENDENCIAS PROFUNDAS: las clases que no son puntos de entrada
    /// reciben sus dependencias por constructor; no llaman a Resolve
    /// (docs/04_MVVM_Patterns.md §1, principio 6).</para>
    ///
    /// <para>THREADING: solo main thread de Unity.</para>
    /// </remarks>
    public static class AppContext
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static bool IsReady { get; private set; }

        public static void Initialize()
        {
            if (IsReady)
            {
                Debug.LogWarning("[AppContext] Initialize() called twice. Ignored.");
                return;
            }
            IsReady = true;
            Debug.Log($"[AppContext] Initialized with {Services.Count} service(s).");
        }

        /// <summary>
        /// Registra un servicio bajo el contrato T. Solo antes de Initialize().
        /// </summary>
        /// <exception cref="ArgumentNullException">El servicio es null.</exception>
        /// <exception cref="InvalidOperationException">Ya se inicializo, o T ya tiene un servicio.</exception>
        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (IsReady)
            {
                throw new InvalidOperationException(
                    $"[AppContext] Register<{typeof(T).Name}> rejected: the registry closes at Initialize(). " +
                    "Register every service in the Composition Root before initializing.");
            }

            if (Services.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException(
                    $"[AppContext] A service for {typeof(T).Name} is already registered.");
            }

            Services.Add(typeof(T), service);
        }

        /// <summary>
        /// Devuelve el servicio registrado para T.
        /// </summary>
        /// <exception cref="InvalidOperationException">Todavia no se inicializo, o T no tiene servicio.</exception>
        public static T Resolve<T>() where T : class
        {
            EnsureReady($"Resolve<{typeof(T).Name}>");

            if (Services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[AppContext] No service registered for {typeof(T).Name}.");
        }

        /// <summary>
        /// Como Resolve, pero sin lanzar: false si todavia no se inicializo o T no tiene servicio.
        /// </summary>
        public static bool TryResolve<T>(out T service) where T : class
        {
            service = null;
            if (!IsReady || !Services.TryGetValue(typeof(T), out var found))
                return false;

            service = (T)found;
            return true;
        }

        /// <summary>
        /// Vacia el registro y vuelve al estado previo a Initialize(). Para tests.
        /// No llama a Dispose de los servicios: el registro no es su dueño.
        /// </summary>
        public static void Reset()
        {
            Services.Clear();
            IsReady = false;
            Debug.Log("[AppContext] Reset.");
        }

        internal static void EnsureReady(string callerName)
        {
            if (!IsReady)
            {
                throw new InvalidOperationException(
                    $"[AppContext] Access from '{callerName}' rejected: " +
                    "Initialize() has not been called yet.");
            }
        }
    }
}
