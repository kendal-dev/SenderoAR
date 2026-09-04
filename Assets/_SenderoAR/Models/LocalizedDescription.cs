using System;
using UnityEngine;

namespace SenderoAR.Models
{
    [Serializable]
    public class LocalizedDescription
    {
        [TextArea(3, 8)] public string Es;
        [TextArea(3, 8)] public string En;
        [TextArea(3, 8)] public string Pt;
    }
}