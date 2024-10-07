using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zork
{
    public struct CommandContext
    {
        public string CommandString { get; set; }
        public Command Command { get; }
        public CommandContext(string commandString, Command command)
        {
            CommandString = commandString;
            Command = command;
        }
    }
}
