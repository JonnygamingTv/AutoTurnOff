using System;
using System.Collections.Generic;
using Rocket.API;

namespace Pustalorc.Plugins.AutoTurnOff.Configuration
{
    [Serializable]
    public sealed class AutoTurnOffConfiguration : IRocketPluginConfiguration
    {
        [System.Xml.Serialization.XmlArrayItem(ElementName = "Id")]
        public HashSet<ushort> ItemsToDisable;
        public List<InteractableItem> Interactables;

        public void LoadDefaults()
        {
            ItemsToDisable = new HashSet<ushort>
            {
                458, 1230, 362, 459, 1050, 1466, 1250, 1261, 359, 360, 361, 1049, 1222
            };
            Interactables = new List<InteractableItem> { new InteractableItem() { Name = "Generator", KeepEnabled = true }, new InteractableItem() { Name = "Oxygenator", KeepEnabled = true }, new InteractableItem() { Name = "Safezone", KeepEnabled = true }, new InteractableItem() { Name = "Fire", KeepEnabled = false }, new InteractableItem() { Name = "Oven", KeepEnabled = false }, new InteractableItem() { Name = "Spot", KeepEnabled = false } };
        }
    }
}