using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Kits
{
    public class CommandKitAdd : IRocketCommand
    {
        public string Help
        {
            get { return "Creates a kit"; }
        }

        public string Name
        {
            get { return "kitadd"; }
        }

        public string Syntax
        {
            get { return "<kitName> [kitCooldown] [Cost] [Override?true]"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
        public System.Collections.Generic.List<string> Aliases => new System.Collections.Generic.List<string> { "kiadd","kitcreate","addkit" };

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "kits.kitadd" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length < 1)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            int kitX = Kits.Instance.Configuration.Instance.Kits.FindIndex(k => k.Name.ToLower() == command[0].ToLower());
            if(kitX!=-1&&(command.Length < 4 || command[3] != "true"))
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitadd_exists", command[0]));
                return;
            }
            Kit FullKit = new Kit { Name = command[0] };
            if (player.IsInVehicle)
            {
                FullKit.Vehicle = player.CurrentVehicle.id;
                FullKit.VehicleGUID = player.CurrentVehicle.asset?.GUID;
            }
            if(command.Length>1 && int.TryParse(command[1], out int Cd))FullKit.Cooldown = Cd;
            if(command.Length>2 && int.TryParse(command[2], out int Cost)) FullKit.Money = -Cost;
            FullKit.Items = new List<KitItem>();
            PlayerClothing clothes = player.Player.clothing;
            if (player.Player.clothing.hatAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.hatAsset.id, 1)); }
            if (player.Player.clothing.glassesAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.glassesAsset.id, 1)); }
            if (player.Player.clothing.maskAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.maskAsset.id, 1)); }
            if (player.Player.clothing.backpackAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.backpackAsset.id, 1)); }
            if (player.Player.clothing.vestAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.vestAsset.id, 1)); }
            if (player.Player.clothing.shirtAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.shirtAsset.id,1)); }
            if (player.Player.clothing.pantsAsset != null) { FullKit.Items.Add(new KitItem(player.Player.clothing.pantsAsset.id, 1)); }

            Items[] InvItems = player.Inventory.items;
#if DEBUG
Logger.Log(InvItems.Length.ToString());
#endif
            for (int i = 0; i < InvItems.Length; i++)
            {
#if DEBUG
Logger.Log(InvItems[i].items.Count.ToString());
#endif
                if (InvItems[i] ==null || InvItems[i].items == null) continue;
                for (int y=0;y< InvItems[i].items.Count; y++)
                {
                    if (InvItems[i].items[y] == null) continue;
                    try
                    {
                        ItemJar Ijar = InvItems[i].items[y];
                        KitItem kitItem = new KitItem(Ijar.item.id, Ijar.item.amount, Ijar.item.durability, Ijar.item.metadata);
                        FullKit.Items.Add(kitItem);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException(e,"Problem adding kit to kitItems list:");
                    }
                }
            }
#if DEBUG
Logger.Log("Kit will now be added..");
#endif
            if (kitX != -1)
            {
                Kits.Instance.Configuration.Instance.Kits[kitX] = FullKit;
            }
            else Kits.Instance.Configuration.Instance.Kits.Add(FullKit);
#if DEBUG
Logger.Log("Kit added to list!");
#endif
            UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitadd_success", FullKit.Name));
            Kits.Instance.Configuration.Save();
        }
    }
}
