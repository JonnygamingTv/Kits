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
    public class CommandKitRemove : IRocketCommand
    {
        public string Help
        {
            get { return "Removes a kit"; }
        }

        public string Name
        {
            get { return "kitremove"; }
        }

        public string Syntax
        {
            get { return "<kitName>"; }
        }

        public bool RunFromConsole
        {
            get { return false; }
        }
        public System.Collections.Generic.List<string> Aliases => new System.Collections.Generic.List<string> { "kitremove","kitdelete","deletekit" };

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
            if(kitX==-1)
            {
                UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitdel_exists", command[0]));
                return;
            }
            Kits.Instance.Configuration.Instance.Kits.RemoveAt(kitX);
            UnturnedChat.Say(caller, Kits.Instance.Translations.Instance.Translate("command_kitdel_success", command[0]));
            Kits.Instance.Configuration.Save();
        }
    }
}
