﻿using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace fr34kyn01535.Kits
{
    public class CommandKit : IRocketCommand
    {
        public string Help
        {
            get { return "Gives you a kit"; }
        }

        public string Name
        {
            get { return "kit"; }
        }

        public string Syntax
        {
            get { return "<kit>"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "kits.kit" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            Kit kit = Kits.Instance.Configuration.Instance.Kits.Where(k => k.Name.ToLower() == command[0].ToLower()).FirstOrDefault();
            if (kit == null)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_not_found"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            bool hasPermissions = caller.HasPermission("kit.*") | caller.HasPermission("kit." + kit.Name.ToLower());

            if (!hasPermissions)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_no_permissions"));
                throw new NoPermissionsForCommandException(caller, this);
            }

            KeyValuePair<string, DateTime> globalCooldown = Kits.GlobalCooldown.Where(k => k.Key == caller.ToString()).FirstOrDefault();
            if (!globalCooldown.Equals(default(KeyValuePair<string, DateTime>)))
            {
                double globalCooldownSeconds = (DateTime.Now - globalCooldown.Value).TotalSeconds;
                if (globalCooldownSeconds < Kits.Instance.Configuration.Instance.GlobalCooldown)
                {
                    UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_cooldown_command", (int)(Kits.Instance.Configuration.Instance.GlobalCooldown - globalCooldownSeconds)));
                    return;
                }
            }

            KeyValuePair<string, DateTime> individualCooldown = Kits.IndividualCooldown.Where(k => k.Key == (caller.ToString() + kit.Name)).FirstOrDefault();
            if (!individualCooldown.Equals(default(KeyValuePair<string, DateTime>)))
            {
                double individualCooldownSeconds = (DateTime.Now - individualCooldown.Value).TotalSeconds;
                if (individualCooldownSeconds < kit.Cooldown)
                {
                    UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_cooldown_kit", (int)(kit.Cooldown - individualCooldownSeconds)));
                    return;
                }
            }

            bool cancelBecauseNotEnoughtMoney = false;

            if (kit.Money.HasValue && kit.Money.Value != 0)
            {
                if (Rocket.Core.Plugins.RocketPlugin.IsDependencyLoaded("Uconomy"))
                {
                    Kits.ExecuteDependencyCode("Uconomy", (IRocketPlugin plugin) =>
                     {
                         Uconomy.Uconomy Uconomy = (Uconomy.Uconomy)plugin;
                         if ((Uconomy.Database.GetBalance(player.CSteamID.ToString()) + kit.Money.Value) < 0)
                         {
                             cancelBecauseNotEnoughtMoney = true;
                             UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_no_money", Math.Abs(kit.Money.Value), Uconomy.Configuration.Instance.MoneyName, kit.Name));
                             return;
                         }
                         else
                         {
                             UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_money", kit.Money.Value, Uconomy.Configuration.Instance.MoneyName, kit.Name));
                         }
                         Uconomy.Database.IncreaseBalance(player.CSteamID.ToString(), kit.Money.Value);
                     });
                }
                else { 
                    if((player.Experience + kit.Money.Value) < 0)
                    {
                        cancelBecauseNotEnoughtMoney = true;
                        UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_no_money", Math.Abs(kit.Money.Value), "XP", kit.Name));
                    }
                    else
                    {
                        UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_money", kit.Money.Value, "XP", kit.Name));
                        player.Experience += uint.Parse(kit.Money.Value.ToString());
                    }
                }
            }

            if (cancelBecauseNotEnoughtMoney)
            {
                throw new WrongUsageOfCommandException(caller, this);
            }

            foreach (KitItem item in kit.Items)
            {
                Item _item = item.Meta != null && item.Meta.Length!=0 ? new Item(item.ItemId, item.Amount, item.Durability, item.Meta) : new Item(item.ItemId, item.Amount, item.Durability);
                if (_item == null)
                {
#if DEBUG
Logger.Log(item.ItemId + " failed to convert to item.");
#endif
                    continue;
                }
                try
                {
                    if (!player.Inventory.tryAddItem(_item, true))
                    {
                        ItemManager.dropItem(_item, player.Position, true, true, true);
                        Logger.Log(Kits.Instance.Translations.Instance.Translate("command_kit_failed_giving_item", player.CharacterName, item.ItemId, item.Amount));
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Logger.LogException(ex, "Failed giving item "+item.ItemId+" to player");
#endif
                    try
                    {
                        if (!player.Player.inventory.tryAddItem(_item, true)) ItemManager.dropItem(_item, player.Position, true, true, true);
                    }catch(Exception e)
                    {
#if DEBUG
                        Logger.LogException(e, "Failed secondary attempt giving item " + item.ItemId + " to player");
#endif
                        player.GiveItem(item.ItemId,item.Amount);
                    }
                }

            }

            if (kit.XP.HasValue && kit.XP != 0)
            {
                player.Experience += kit.XP.Value;
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_xp",  kit.XP.Value, kit.Name));
            }
            if (!kit.VehicleGUID.HasValue)
            {
                if (kit.Vehicle.HasValue)
                {
                    try
                    {
                        player.GiveVehicle(kit.Vehicle.Value);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex, "Failed giving vehicle " + kit.Vehicle.Value + " to player");
                    }
                }
            }
            else {
                VehicleManager.SpawnVehicleV3((VehicleAsset)Assets.FindBaseVehicleAssetByGuidOrLegacyId(kit.VehicleGUID.Value, kit.Vehicle.HasValue ? kit.Vehicle.Value : (ushort)0), 0, 0, 0, player.Position, player.Player.transform.rotation, false, false, false, false, 100, 100, 100, player.CSteamID, player.SteamGroupID, false, new byte[0][], byte.MaxValue);
            }

            UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_success", kit.Name));

            if (Kits.GlobalCooldown.ContainsKey(caller.ToString()))
            {
                Kits.GlobalCooldown[caller.ToString()] = DateTime.Now;
            }
            else
            {
                Kits.GlobalCooldown.Add(caller.ToString(), DateTime.Now);
            }

            if (Kits.GlobalCooldown.ContainsKey(caller.ToString()))
            {
                Kits.IndividualCooldown[caller.ToString() + kit.Name] = DateTime.Now;
            }
            else
            {
                Kits.IndividualCooldown.Add(caller.ToString() + kit.Name, DateTime.Now);
            }
        }
    }
}
