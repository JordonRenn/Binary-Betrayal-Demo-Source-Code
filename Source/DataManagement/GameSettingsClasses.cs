using System;
using SBG;

namespace BinaryBetrayal.DataManagement
{
    /// <summary>
    /// Class to store player-specific settings
    /// Used to define "Profile" settings
    /// </summary>
    [Serializable]
    public class PlayerSettings
    {
        public string profileName = "Player";
        public Language language = Language.English; // Default language is English

        public GraphicsSettings graphicsSettings = new GraphicsSettings();
        public AudioSettings audioSettings = new AudioSettings();
        public InputSettings inputSettings = new InputSettings();

        // TODO: Move the following properties to their own classes for better organization
        public float mouseSensitivityHorizontal = 0.5f; // Mouse sensitivity (0.5 is default/neutral)
        public float mouseSensitivityVertical = 0.5f; // Mouse sensitivity (0.5 is default/neutral)
        public bool invertYAxis = false;
        public const float DEFAULT_HORIZONTAL_MULTIPLIER = 1.0f;
        public const float DEFAULT_VERTICAL_MULTIPLIER = 1.0f;

        public float GetHorizontalSensitivityMultiplier()
        {
            return DEFAULT_HORIZONTAL_MULTIPLIER * (0.5f + mouseSensitivityHorizontal);
        }

        public float GetVerticalSensitivityMultiplier()
        {
            return DEFAULT_VERTICAL_MULTIPLIER * (0.5f + mouseSensitivityVertical);
        }
    }

    [Serializable]
    public class GraphicsSettings
    {
        // Graphics settings
    }

    [Serializable]
    public class AudioSettings
    {
        // Audio settings
    }

    [Serializable]
    public class InputSettings
    {
        // Input settings
    }
}