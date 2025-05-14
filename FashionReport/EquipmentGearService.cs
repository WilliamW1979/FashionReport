using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Logging;
using Dalamud.Game.Inventory;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Lumina.Excel.Sheets;

namespace FashionReport
{
    public unsafe class EquippedGearService : IDisposable
    {
        public InventoryItem Weapon { get; private set; }
        public InventoryItem Head { get; private set; }
        public InventoryItem Body { get; private set; }
        public InventoryItem Gloves { get; private set; }
        public InventoryItem Legs { get; private set; }
        public InventoryItem Boots { get; private set; }
        public InventoryItem Earrings { get; private set; }
        public InventoryItem Necklace { get; private set; }
        public InventoryItem Bracelet { get; private set; }
        public InventoryItem RightRing { get; private set; }
        public InventoryItem LeftRing { get; private set; }


        public EquippedGearService()
        {
            SERVICES.GameInventory.InventoryChanged += OnInventoryChanged;
            SERVICES.Log.Debug("EquippedGearService subscribed to InventoryChanged.");
            UpdateEquippedGear();
        }

        private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
        {
            foreach (InventoryEventArgs change in events)
                if (change.ToString().Contains("EquippedItems"))
                {
                    UpdateEquippedGear();
                    return;
                }
        }

        private InventoryItem GetEquippedItem(uint slot)
        {
            if (slot > 13) return new InventoryItem { ItemId = 0 };
            InventoryItem* equipmentInventoryItem = InventoryManager.Instance()->GetInventorySlot(InventoryType.EquippedItems, (int)slot);
            if (equipmentInventoryItem != null)
            {
                InventoryItem inventoryItem = *equipmentInventoryItem;
                if (inventoryItem.ItemId > 0)
                    return inventoryItem;
            }
            return new InventoryItem { ItemId = 0 };
        }

        private void UpdateEquippedGear()
        {
            Weapon = GetEquippedItem(0);
            Head = GetEquippedItem(2);
            Body = GetEquippedItem(3);
            Gloves = GetEquippedItem(4);
            Legs = GetEquippedItem(6);
            Boots = GetEquippedItem(7);
            Earrings = GetEquippedItem(8);
            Necklace = GetEquippedItem(9);
            Bracelet = GetEquippedItem(10);
            RightRing = GetEquippedItem(11);
            LeftRing = GetEquippedItem(12);
        }

        public void Dispose() => SERVICES.GameInventory.InventoryChanged -= OnInventoryChanged;
    }
}
