﻿using System.Collections.Generic;
using CommandDotNet.Models;

namespace CommandDotNet.MicrosoftCommandLineUtils
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        bool ShowInHelpText { get; }
        string ExtendedHelpText { get; }
        IEnumerable<CommandOption> GetOptions();
        string GetFullCommandName();
        HashSet<CommandOption> Options { get; }
        HashSet<CommandArgument> Arguments { get; }
        List<ICommand> Commands { get; }
        CommandOption OptionHelp { get; }
        List<ArgumentInfo> CustomArguments { get; set; }
        List<CommandInfo> CustomCommands { get; set; }
    }
}