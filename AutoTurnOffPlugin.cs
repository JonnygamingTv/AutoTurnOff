using System;
using System.Collections.Generic;
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
        public Dictionary<Type, System.Action<Interactable>> InteractAct = new Dictionary<Type, System.Action<Interactable>>();
        protected override void Load()
        {
            U.Events.OnPlayerDisconnected += Disconnected;
            Logger.Log("Auto Turn Off has been loaded!");
            InteractAct.Clear();
            foreach(Configuration.InteractableItem gg in Configuration.Instance.Interactables)
            {
                if (gg.KeepEnabled) continue;
                Type type = typeof(Interactable).Assembly
                    .GetType("SDG.Unturned.Interactable" + gg.Name);

                if (type == null)
                {
                    Logger.Log("Could not find type: "+gg.Name);
                    continue;
                }

                InteractAct[type] = CreateAction(type);
            }
        }

        protected override void Unload()
        {
            U.Events.OnPlayerDisconnected -= Disconnected;
            InteractAct.Clear();
            Logger.Log("Auto Turn Off has been unloaded!");
        }

        private async void Disconnected([NotNull] UnturnedPlayer player)
        {
#if DEBUG
            Logger.Log("onDisconnect event triggered.");
#endif
            if (BarricadeManager.regions == null)
            {
#if DEBUG
                Logger.Log("No regions found.");
#endif
                return;
            }
            await Rocket.Core.Utils.TaskDispatcher.OffThread(() => {
                ulong playerId = player.CSteamID.m_SteamID;
#if DEBUG
                Logger.Log(playerId.ToString() + " logged off.");
#endif
                var buildables = BarricadeManager.regions.Cast<BarricadeRegion>().Concat(BarricadeManager.vehicleRegions)
                    .SelectMany(k => k.drops)
                    .Select(k =>
                        !Configuration.Instance.ItemsToDisable.Contains(k.asset.id) ||
                        k.GetServersideData().owner != playerId
                            ? null
                            : k.interactable).Where(k => k != null).ToList();

#if DEBUG
                Logger.Log("Found "+buildables.Count.ToString()+" buildables to loop through.");
#endif

                foreach (var interactable in buildables)
                {
                    var type = interactable.GetType();

                    if (InteractAct.TryGetValue(type, out var action))
                    {
#if DEBUG
                        Logger.Log("Executing action for interactable.");
#endif
                        action(interactable);
                    }
#if DEBUG
                    else
                    {
                        Logger.Log("Did not find action for interactable: "+type.FullName);
                    }
#endif
                }
                /*
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
                */
#if DEBUG
                Logger.Log("Turned off " + buildables.Count + " barricades.");
#endif
            });
        }

        private static Action<Interactable> CreateAction(Type type)
        {
#if DEBUG
            Logger.Log("Populating dict with: "+type.FullName);
#endif
            if (type == typeof(InteractableSafezone))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetSafezonePowered((InteractableSafezone)i, false));

            if (type == typeof(InteractableOxygenator))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetOxygenatorPowered((InteractableOxygenator)i, false));

            if (type == typeof(InteractableSpot))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetSpotPowered((InteractableSpot)i, false));

            if (type == typeof(InteractableGenerator))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetGeneratorPowered((InteractableGenerator)i, false));

            if (type == typeof(InteractableSpot))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetSpotPowered((InteractableSpot)i, false));

            if (type == typeof(InteractableFire))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetFireLit((InteractableFire)i, false));

            if (type == typeof(InteractableOven))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetOvenLit((InteractableOven)i, false));

            if (type == typeof(InteractableStereo))
                return i => Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(
                    () => BarricadeManager.ServerSetStereoTrack((InteractableStereo)i, Guid.Empty));

            return null;
        }
    }
}