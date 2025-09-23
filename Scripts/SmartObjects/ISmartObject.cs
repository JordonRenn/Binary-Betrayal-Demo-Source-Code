using UnityEngine;

namespace SBG.SmartObjects
{
    /// <summary>
    /// Interface for Smart Objects to implement common functionality.
    /// </summary>}
    public interface ISmartObject
    {
        void Refresh();
        void Clear();
        void Flatten();
    }
}