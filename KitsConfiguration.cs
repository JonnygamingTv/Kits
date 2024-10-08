﻿using Rocket.API;
using System.Collections.Generic;
using System.Xml.Serialization;
using System;

namespace fr34kyn01535.Kits
{
    public class KitsConfiguration : IRocketPluginConfiguration
    {
        [XmlArrayItem(ElementName = "Kit")]
        public List<Kit> Kits;
        public int GlobalCooldown;

        public void LoadDefaults()
        {
            GlobalCooldown = 10;
            Kits = new List<Kit>() {
                new Kit() { Cooldown = 10, Name = "Survival", XP = 0,Items = new List<KitItem>() { new KitItem(245, 1), new KitItem(81, 2), new KitItem(16, 1) }},
                new Kit() { Cooldown = 10, Name = "Brute Force", XP = 0,Money = 30, Vehicle = 57,Items = new List<KitItem>() { new KitItem(112, 1), new KitItem(113, 3), new KitItem(254, 3) }},
                new Kit() { Cooldown = 10, Name = "Watcher", XP = 200,Money=-20, Items = new List<KitItem>() { new KitItem(109, 1), new KitItem(111, 3), new KitItem(236, 1) }}
            };
        }
    }

    public class Kit
    {
        public Kit() { }
        public Kit(bool a, string n = "", uint? x = null, decimal? m = null, ushort? v = null, System.Guid? vg = null) { ResetCooldownWhenDie = a;Name = n;XP = x;Money = m;Vehicle = v;VehicleGUID = vg; }

        public bool ResetCooldownWhenDie = false;
        public string Name;
        public uint? XP = null;
        public decimal? Money = null;
        public ushort? Vehicle = null;
        public System.Guid? VehicleGUID = null;

        [XmlArrayItem(ElementName = "Item")]
        public List<KitItem> Items;

        public int? Cooldown = null;
    }

    public class KitItem{

        public KitItem(){ }

        public KitItem(ushort itemId, byte amount, byte durability=100)
        {
            ItemId = itemId;
            Amount = amount;
            Durability=durability;
        }
        public KitItem(ushort itemId, byte amount, byte durability, byte[] meta)
        {
            ItemId = itemId;
            Amount = amount;
            Durability = durability;
            Meta = meta;
        }

        [XmlAttribute("id")]
        public ushort ItemId;

        [XmlAttribute("amount")]
        public byte Amount;

        [XmlAttribute("durability")]
        public byte Durability;

        [XmlAttribute("metadata")]
        public byte[] Meta;
    }
}
