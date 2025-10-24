using UnityEngine;
using System;
using BinaryBetrayal.GlobalEvents;
using BinaryBetrayal.InputManagement;

namespace BinaryBetrayal.DataManagement
{
    public static class GameSettings
    {
        public static readonly string SETTINGS_KEY = "PlayerSettings";
        private static PlayerSettings playerSettings;

        static GameSettings()
        {
            LoadSettings();
        }

        /// <summary>
        /// Loads player settings from PlayerPrefs
        /// </summary>
        public static void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SETTINGS_KEY);
                    playerSettings = JsonUtility.FromJson<PlayerSettings>(json);
                    // SBGDebug.LogInfo("Settings loaded successfully", "GameMaster");
                }
                catch (System.Exception e)
                {
                    SBGDebug.LogError($"Failed to load settings: {e.Message}", "GameMaster");
                    playerSettings = new PlayerSettings();
                }
            }
            else
            {
                playerSettings = new PlayerSettings();
                // SBGDebug.LogInfo("Using default settings", "GameMaster");
            }
        }

        /// <summary>
        /// Gets the current player settings
        /// </summary>
        public static PlayerSettings GetSettings()
        {
            return playerSettings;
        }

        /// <summary>
        /// Saves and applies player settings
        /// </summary>
        public static void SaveAndApplySettings()
        {
            SaveSettings();
            ApplySettings();
        }

        /// <summary>
        /// Saves player settings to PlayerPrefs
        /// </summary>
        private static void SaveSettings()
        {
            try
            {
                string json = JsonUtility.ToJson(playerSettings);
                PlayerPrefs.SetString(SETTINGS_KEY, json);
                PlayerPrefs.Save();
                // SBGDebug.LogInfo("Settings saved successfully", "GameMaster");
            }
            catch (Exception e)
            {
                SBGDebug.LogError($"Failed to save settings: {e.Message}", "GameMaster");
            }
        }

        /// <summary>
        /// Applies settings changes to relevant systems
        /// </summary>
        private static void ApplySettings()
        {
            InputSystem.UpdateInputSettings();

            ConfigEvents.RaiseSettingsChanged();
        }

        /// <summary>
        /// Resets player settings to default values
        /// </summary>
        public static void ResetSettingsToDefault()
        {
            playerSettings = new PlayerSettings();
            SaveSettings();
            ApplySettings();
        }
    }

    
}