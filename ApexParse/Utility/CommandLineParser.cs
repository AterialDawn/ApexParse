using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Aterial.Utility
{
    public class CommandLineParser
    {
        public delegate void OptionEnabled(string Arg);

        private List<CommandLineOption> RegisteredOptions = new List<CommandLineOption>();

        public Tuple<string, string>[] ActiveOptions = null; // This is null until ParseString or ParseCommandLine is called.

        public bool IgnoreCase = false;
        public bool ExitOnUsagePrinted = true;

        public void RegisterUsageCallbacks()
        {
            RegisteredOptions.Add(new CommandLineOption("usage", "Writes usage to console", false, PrintUsageToConsoleCallback));
            RegisteredOptions.Add(new CommandLineOption("help", "Writes usage to console", false, PrintUsageToConsoleCallback));
            RegisteredOptions.Add(new CommandLineOption("h", "Writes usage to console", false, PrintUsageToConsoleCallback));
        }

        /// <summary>
        /// Adds a parseable option
        /// </summary>
        /// <param name="OptionName">Name of the option</param>
        /// <param name="OptionDescription">Description of the option</param>
        /// <param name="RequiresArgument">True if an argument is required</param>
        /// <param name="Callback">The callback to call if this option is parsed. May be null.</param>
        public void AddOption(string OptionName, string OptionDescription, bool RequiresArgument, OptionEnabled Callback = null)
        {
            if (IsOptionRegistered(OptionName)) return; // Don't re-register an already existing option.
            RegisteredOptions.Add(new CommandLineOption(OptionName, OptionDescription, RequiresArgument, Callback));
        }

        /// <summary>
        /// Parse a string array for command line args. Will throw exceptions if parsing fails.
        /// </summary>
        /// <param name="ArrayToParse"></param>
        public void Parse(string[] ArrayToParse)
        {
            List<Tuple<string, string>> ActiveOptionsList = new List<Tuple<string, string>>();
            // Happy fun logic time
            for (int OptionIndex = 0; OptionIndex < ArrayToParse.Length; OptionIndex++)
            {
                string CurrentOptionString = ArrayToParse[OptionIndex];
                if (CurrentOptionString.Length <= 1) continue;
                if (CurrentOptionString.StartsWith("-")) // All valid options must start with a - or + character. Ignore current option if it doesn't contain this.
                {
                    CurrentOptionString = CurrentOptionString.Substring(1); // Chop off the initial -
                }
                else //+command line arguments are stored but are not processed
                {
                    if (CurrentOptionString.StartsWith("+"))
                    {
                        ActiveOptionsList.Add(ParsePlusOption(ref OptionIndex, ref ArrayToParse));
                    }
                    continue;
                }

                CommandLineOption Option = GetOptionByName(CurrentOptionString);
                if (Option == null) continue; // Option isn't registered or invalid casing. Ignore.
                if (Option.ArgumentRequired) // Unconditionally grab the next argument and skip it for the purposes of parsing.
                {
                    string OptionArg = ArrayToParse[++OptionIndex];
                    Option.Callback?.Invoke(OptionArg);
                    ActiveOptionsList.Add(new Tuple<string, string>(Option.OptionName, OptionArg));
                }
                else
                {
                    if (ArrayToParse.Length > OptionIndex + 1)
                    {
                        string NextOptionString = ArrayToParse[OptionIndex + 1];
                        if (!(NextOptionString.StartsWith("+") || NextOptionString.StartsWith("-")))
                        {
                            //Next option is available and is not an option
                            ActiveOptionsList.Add(new Tuple<string, string>(Option.OptionName, NextOptionString));
                            Option.Callback?.Invoke(NextOptionString);

                            OptionIndex++; //Skip next option since we consumed it
                            continue;
                        }
                    }
                    ActiveOptionsList.Add(new Tuple<string, string>(Option.OptionName, ""));
                    Option.Callback?.Invoke(""); // Call the option's callback with an empty string since no args were found

                }
            }
            ActiveOptions = ActiveOptionsList.ToArray();
        }

        Tuple<string, string> ParsePlusOption(ref int optionIndex, ref string[] ArrayToParse)
        {
            //Plus options will lookahead 1 argument, and if it does not start with a + or a -, it is asssumed to be an argument to the option.
            string CurrentOption = ArrayToParse[optionIndex];
            string Argument = "";

            if (optionIndex + 1 < ArrayToParse.Length)
            {
                string NextItem = ArrayToParse[optionIndex + 1];
                if (NextItem.StartsWith("-") || NextItem.StartsWith("+"))
                {
                    //Next item is another valid item, it is not an argument therefore we ignore it
                }
                else
                {
                    Argument = NextItem;
                    optionIndex++;
                }
            }

            return new Tuple<string, string>(CurrentOption, Argument);
        }

        public void ParseCommandLine() //Essentially pointless.
        {
            string[] CommandLineArgs = Environment.GetCommandLineArgs();
            if (CommandLineArgs.Length == 1)
            {
                ActiveOptions = new Tuple<string, string>[0];
                return; // No command line args passed to the program. Don't parse.
            }
            string[] TrueCommandLineArgs = CommandLineArgs.Skip(1).ToArray();
            Parse(TrueCommandLineArgs);
        }

        public void PrintUsageToConsole() // Essentially pointless.
        {
            Console.WriteLine(GetUsageString());
            if (ExitOnUsagePrinted) Environment.Exit(0);
        }

        public string GetUsageString()
        {
            if (RegisteredOptions.Count == 0) return "No registered options.";
            StringBuilder OutputBuilder = new StringBuilder();
            OutputBuilder.AppendLine("Usage : \n");
            foreach (CommandLineOption CurrentOption in RegisteredOptions)
            {
                if (CurrentOption.ArgumentRequired)
                {
                    OutputBuilder.AppendFormat("-{0} [Argument] : {1}\n", CurrentOption.OptionName, CurrentOption.OptionDescription);
                }
                else
                {
                    OutputBuilder.AppendFormat("-{0} : {1}\n", CurrentOption.OptionName, CurrentOption.OptionDescription);
                }
            }
            return OutputBuilder.ToString();
        }

        public bool IsOptionRegistered(string OptionName)
        {
            foreach (CommandLineOption CurrentOption in RegisteredOptions)
            {
                if (IgnoreCase)
                {
                    if (CurrentOption.OptionName.ToLowerInvariant() == OptionName.ToLowerInvariant()) return true;
                }
                else
                {
                    if (CurrentOption.OptionName == OptionName) return true;
                }
            }
            return false;
        }

        public bool RemoveOption(string OptionName)
        {
            CommandLineOption OptionToRemove = null;
            foreach (CommandLineOption CurrentOption in RegisteredOptions)
            {
                if (IgnoreCase)
                {
                    if (CurrentOption.OptionName.ToLowerInvariant() == OptionName.ToLowerInvariant())
                    {
                        OptionToRemove = CurrentOption;
                        break;
                    }
                }
                else
                {
                    if (CurrentOption.OptionName == OptionName)
                    {
                        OptionToRemove = CurrentOption;
                        break;
                    }
                }
            }
            if (OptionToRemove != null)
            {
                RegisteredOptions.Remove(OptionToRemove);
                return true;
            }
            else
            {
                return false;
            }
        }

        private CommandLineOption GetOptionByName(string OptionName)
        {
            foreach (CommandLineOption CurrentOption in RegisteredOptions)
            {
                if (IgnoreCase)
                {
                    if (CurrentOption.OptionName.ToLowerInvariant() == OptionName.ToLowerInvariant()) return CurrentOption;
                }
                else
                {
                    if (CurrentOption.OptionName == OptionName) return CurrentOption;
                }
            }
            return null;
        }

        private void PrintUsageToConsoleCallback(string Unused)
        {
            PrintUsageToConsole();
        }

        private class CommandLineOption
        {
            public string OptionName { get; private set; }
            public string OptionDescription { get; private set; }
            public bool ArgumentRequired { get; private set; }
            public OptionEnabled Callback { get; private set; }

            public CommandLineOption(string Name, string Description, bool ArgRequired, OptionEnabled Callback)
            {
                this.OptionName = Name;
                this.OptionDescription = Description;
                this.ArgumentRequired = ArgRequired;
                this.Callback = Callback;
            }
        }
    }
}