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
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kit_invalid_parameter"));
                throw new WrongUsageOfCommandException(caller, this);
            }

            int kitX = Kits.Instance.Configuration.Instance.Kits.FindIndex(k => k.Name.ToLower() == command[0].ToLower());
            if(kitX!=-1&&(command.Length < 4 || command[3] != "true"))
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitadd_exists"));
                return;
            }
            Kit FullKit = new Kit { Name = command[0] };
            if(int.TryParse(command[1], out int Cd))FullKit.Cooldown = Cd;
            if(int.TryParse(command[2], out int Cost)) FullKit.Money = -Cost;

            Items[] InvItems = player.Inventory.items;
            for(int i = 0; i < InvItems.Length; i++)
            {
                for(int y=0;y< InvItems[i].items.Count; y++)
                {
                    ItemJar Ijar = InvItems[i].items[y];
                    KitItem kitItem = new KitItem(Ijar.item.id, Ijar.item.amount, Ijar.item.durability, Ijar.item.metadata);
                    FullKit.Items.Add(kitItem);
                }
            }
            if (kitX != -1)
            {
                Kits.Instance.Configuration.Instance.Kits[kitX] = FullKit;
            }
            else Kits.Instance.Configuration.Instance.Kits.Add(FullKit);

            UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitadd_success", FullKit.Name));
            Kits.Instance.Configuration.Save();
        }
    }
}
