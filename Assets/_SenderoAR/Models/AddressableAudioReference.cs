using System;

namespace SenderoAR.Models
{
    /// <summary>
    /// Pareja idioma + clave Addressable para clip de narración.
    /// Wire definitivo a <c>AssetReferenceT&lt;AudioClip&gt;</c> diferido al Sprint 7
    /// (instalación del paquete <c>com.unity.addressables</c>).
    /// </summary>
    [Serializable]
    public class AddressableAudioReference
    {
        public LanguageCode Language;
        public string AddressableKey;
    }
}