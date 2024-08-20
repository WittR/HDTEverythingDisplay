using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Windows.Controls;

namespace HDTTrinketDisplay
{
    public class TrinketDisplayPlugin : IPlugin
    {
        public string Name => "HDTTrinketDisplay";

        public string Description => "Displays your current Battlegrounds trinkets on your overlay";

        public string ButtonText => "SETTINGS";

        // Thank you Tignus for HDTAnomalyDisplay ! I used a lot of your code.
        // Thank you Mouchoir for HDTBudyDisplay ! I used everything of your code :3
        public string Author => "Mouchoir & Tignus, adapted by Monsieur__R";

        public Version Version => new Version(1, 0, 0);

        public MenuItem MenuItem => CreateMenu();

        private MenuItem CreateMenu()
        {
            MenuItem settingsMenuItem = new MenuItem { Header = "Trinkets Display Settings" };

            settingsMenuItem.Click += (sender, args) =>
            {
                SettingsView.Flyout.IsOpen = true;
            };

            return settingsMenuItem;
        }
        public TrinketDisplay trinketDisplay;

        public void OnButtonPress() => SettingsView.Flyout.IsOpen = true;

        public void OnLoad()
        {
            trinketDisplay = new TrinketDisplay();
            GameEvents.OnGameStart.Add(trinketDisplay.HandleGameStart);
            GameEvents.OnGameStart.Add(trinketDisplay.HandleStartTurnAsync);
            GameEvents.OnGameEnd.Add(trinketDisplay.ClearCard);

            // Processing GameStart logic in case plugin was loaded/unloaded after starting a game without restarting HDT
            trinketDisplay.HandleGameStart();
            trinketDisplay.HandleStartTurnAsync();
        }

        public void OnUnload()
        {
            Settings.Default.Save();
            trinketDisplay.ClearCard();
            trinketDisplay = null;
        }

        public void OnUpdate()
        {
        }
    }
}
