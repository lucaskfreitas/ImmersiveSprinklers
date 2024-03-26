using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImmersiveSprinklers;

public class WorldEventsHandler
{
    public static void TerrainFeatureListChanged(object sender, StardewModdingAPI.Events.TerrainFeatureListChangedEventArgs e)
    {
        if (!ModEntry.Config.EnableMod)
            return;

        foreach(KeyValuePair<Vector2, TerrainFeature> removed in e.Removed.Where(x => x.Value is HoeDirt))
        {
            for (int mouseCorner = 0; mouseCorner < 4; mouseCorner++)
            {
                if (removed.Value.modData.TryGetValue(ModEntry.sprinklerKey + mouseCorner, out string sprinklerItemId))
                {
                    try
                    {
                        e.Location.terrainFeatures.Add(removed.Key, removed.Value);
                        break;
                    }
                    catch (Exception ex)
                    {
                        ModEntry.SMonitor.Log(
                            $"A sprinkler is being deleted! You can retrieve it from the Lost and Found. Error message: {ex}",
                            LogLevel.Error);

                        ModEntry.SMonitor.Log($"Sprinkler Tile: {removed.Key}", LogLevel.Debug);

                        bool nozzle = removed.Value.modData.ContainsKey(ModEntry.nozzleKey + mouseCorner);
                        SendSprinklerToLostAndFound(removed.Value, mouseCorner, nozzle);
                    }
                }
            }
        }
    }

    private static void SendSprinklerToLostAndFound(TerrainFeature tf, int mouseCorner, bool nozzle)
    {
        try
        {
            var sprinklerObject = ModEntry.GetSprinklerCached(tf, mouseCorner, nozzle);
            Game1.player.team.returnedDonations.Add(sprinklerObject);
            Game1.player.team.newLostAndFoundItems.Value = true;
        }
        catch (Exception ex)
        {
            ModEntry.SMonitor.Log(
                $"Error occurred when trying to save deleted sprinkler " +
                $"to Lost and Found: {ex}", LogLevel.Error);
        }
    }
}
