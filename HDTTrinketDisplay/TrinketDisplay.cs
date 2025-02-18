using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using HSCard = Hearthstone_Deck_Tracker.Hearthstone.Card;

using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using ControlzEx.Standard;
using System.Reflection;

namespace HDTTrinketDisplay
{
    public class TrinketDisplay
    {
        public CardImage CardImage;
        public static MoveCardManager MoveManager;
        public Boolean enCours;

        public TrinketDisplay()
        {
        }

        public void InitializeView(String cardId)
        {
            if (CardImage == null)
            {
                CardImage = new CardImage();

                Core.OverlayCanvas.Children.Add(CardImage);
                Canvas.SetTop(CardImage, Settings.Default.TrinketCardTop);
                Canvas.SetLeft(CardImage, Settings.Default.TrinketCardLeft);
                CardImage.Visibility = System.Windows.Visibility.Visible;

                MoveManager = new MoveCardManager(CardImage, SettingsView.IsUnlocked);
                Settings.Default.PropertyChanged += SettingsChanged;
                SettingsChanged(null, null);
            }

            HSCard card = Database.GetCardFromId(cardId);
            card.BaconCard = true;  // Ensure we are getting the Battlegrounds version
            CardImage.SetCardIdFromCard(card);
        }

        private void SettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            CardImage.RenderTransform = new ScaleTransform(Settings.Default.TrinketCardScale / 100, Settings.Default.TrinketCardScale / 100);
            Canvas.SetTop(CardImage, Settings.Default.TrinketCardTop);
            Canvas.SetLeft(CardImage, Settings.Default.TrinketCardLeft);
        }

        public void HandleGameStart()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;

            enCours = true;

        }

        public void ClearCard()
        {
            enCours = false;
            if (CardImage != null)
            {
                CardImage.SetCardIdFromCard(null);
                Core.OverlayCanvas.Children.Remove(CardImage);
                CardImage = null;
            }

            if (MoveManager != null)
            {
                Log.Info("Destroying the MoveManager...");
                MoveManager.Dispose();
                MoveManager = null;
            }

            Settings.Default.PropertyChanged -= SettingsChanged;
        }

        internal async void HandleStartTurnAsync()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;
            Player player = Core.Game.Player;
            IEnumerable<Entity> trinkets = player.Trinkets;
            IEnumerable<Entity> questRewards = player.QuestRewards;
            IEnumerable<Entity> quests = player.Quests;
            int? AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity);

            // Combine trinket, quest reward, and quest card IDs into a single list
            List<string> allCardIds = trinkets.Select(t => t.CardId)
                                              .Concat(questRewards.Select(q => q.CardId))
                                              .Concat(quests.Select(q => q.CardId))
                                              .ToList();

            if (AnomalyDbfId.HasValue)
            {
                Log.Info("Anomaly DbfId found: " + AnomalyDbfId.Value);
                HSCard card = Database.GetCardFromDbfId(AnomalyDbfId.Value, false);
                Log.Info("Anomaly DbfId found: " + card.Id);
                allCardIds.Add(card.Id);
            }

            int index = 0;

            while (enCours)
            {
                trinkets = player.Trinkets;
                questRewards = player.QuestRewards;
                quests = player.Quests;
                AnomalyDbfId = BattlegroundsUtils.GetBattlegroundsAnomalyDbfId(Core.Game.GameEntity);


                allCardIds = trinkets.Select(t => t.CardId) 
                                     .Concat(questRewards.Select(q => q.CardId))
                                     .Concat(quests.Select(q => q.CardId))
                                     .ToList();

                if (AnomalyDbfId.HasValue)
                {
                    Log.Info("Anomaly DbfId found: " + AnomalyDbfId.Value);
                    HSCard card = Database.GetCardFromDbfId(AnomalyDbfId.Value, false);
                    Log.Info("Anomaly DbfId found: " + card.Id);
                    allCardIds.Add(card.Id);
                }

                if (!allCardIds.Any())
                {
                    await Task.Delay(10000);
                }
                else
                {
                    InitializeView(allCardIds[index % allCardIds.Count]);
                    await Task.Delay(20000);
                    index++;
                }
            }
        }
    }
}
