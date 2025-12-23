using System;
using System.Linq;
using JetBrains.Annotations;
using Pustalorc.Plugins.AutoTurnOff.Configuration;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;

namespace Pustalorc.Plugins.AutoTurnOff
{
    public sealed class AutoTurnOffPlugin : RocketPlugin<AutoTurnOffConfiguration>
    {
        protected override void Load()
        {
            if (BarricadeManager.regions != null) U.Events.OnPlayerDisconnected += Disconnected;
            Logger.Log("Auto Turn Off has been loaded!");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerDisconnected -= Disconnected;
            Logger.Log("Auto Turn Off has been unloaded!");
        }

        private void Disconnected([NotNull] UnturnedPlayer player)
        {
            Rocket.Core.Utils.TaskDispatcher.RunAsync(() => {
                ulong playerId = player.CSteamID.m_SteamID;

                var buildables = BarricadeManager.regions.Cast<BarricadeRegion>().Concat(BarricadeManager.vehicleRegions)
                    .SelectMany(k => k.drops)
                    .Select(k =>
                        !Configuration.Instance.ItemsToDisable.Contains(k.asset.id) ||
                        k.GetServersideData().owner != playerId
                            ? null
                            : k.interactable).Where(k => k != null).ToList();

                foreach (var interactable in buildables)
                    switch (interactable)
                    {
                        case InteractableSafezone saf:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetSafezonePowered(saf, false));
                            break;
                        case InteractableOxygenator oxy:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetOxygenatorPowered(oxy, false));
                            break;
                        case InteractableSpot spot:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetSpotPowered(spot, false));
                            break;
                        case InteractableGenerator gen:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetGeneratorPowered(gen, false));
                            break;
                        case InteractableFire fire:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetFireLit(fire, false));
                            break;
                        case InteractableOven oven:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetOvenLit(oven, false));
                            break;
                        case InteractableStereo stereo:
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>BarricadeManager.ServerSetStereoTrack(stereo, Guid.Empty));
                            break;
                    }
#if DEBUG
                Logger.Log("Turned off " + buildables.Count + " barricades.");
#endif
            });
        }    
    }
}