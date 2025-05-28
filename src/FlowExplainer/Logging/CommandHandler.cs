using System.Globalization;
using System.Reflection;

namespace FlowExplainer.Logging
{
    public class CommandHandler
    {
        public class Command
        {
            public CommandAttribute Attribute { get; set; }
            public MethodInfo MethodInfo { get; set; }
        }
        private Dictionary<string, Command> CommandsByName { get; set; } = new Dictionary<string, Command>();
        public FlowExplainer FlowExplainer;
        public List<KeyValuePair<string, Command>> GetCommands()
        {
            return CommandsByName.ToList();
        }

        public void InitilizeCommands()
        {
            AddCommandsInReflectionSaver();
        }

        private void AddCommandsInReflectionSaver()
        {
            foreach (var method in Assembly.GetExecutingAssembly().GetExportedTypes().SelectMany(t => t.GetRuntimeMethods()))
            {
                if (method.GetCustomAttributes(typeof(CommandAttribute), true).Any())
                {
                    Add(new Command()
                    {
                        MethodInfo = method,
                        Attribute = method.GetCustomAttributes(typeof(CommandAttribute), true)[0] as CommandAttribute,
                    });
                }
            }
        }


        public Command FindCommand(string name)
        {
            CommandsByName.TryGetValue(name.ToUpperInvariant(), out Command command);
            return command;
        }

        public void Add(Command command)
        {
            CommandsByName.Add(command.MethodInfo.Name.ToUpperInvariant(), command);
        }

        public void Execute(string input)
        {
            Logger.LogMessage($"Command: {input}");
            if (string.IsNullOrWhiteSpace(input))
                return;

            var splitted = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = splitted[0];
            var command = FindCommand(commandName);

            if (command == null)
            {
                Logger.LogMessage($"No command with name {commandName}");
                return;
            }

            var args = new object[] { input };
            object target = null;
            Type declaringType = command.MethodInfo.DeclaringType;
            if (declaringType.IsSubclassOf(typeof(GlobalService)))
            {
                target = FlowExplainer.GetGlobalService(declaringType);

                if (target == null)
                {
                    Logger.LogMessage($"No instance of {declaringType.Name} in NeuroTrace.");
                    return;
                }
            }


            if (declaringType.IsSubclassOf(typeof(VisualisationService)))
            {
                //temp should be the current highlighted visualization.
                var visualisation = FlowExplainer.GetGlobalService<VisualisationManagerService>()!.Visualisations[0] ?? throw new Exception("No visualizations.");
                target = visualisation!.GetVisualisationService(declaringType);

                if (target == null)
                {
                    Logger.LogMessage($"No instance of {declaringType.Name} in NeuroTrace.");
                    return;
                }
            }

            if (!command.Attribute.TakesRawInput)
            {
                args = new object[splitted.Length - 1];
                ParameterInfo[] parameters = command.MethodInfo.GetParameters();
                if (parameters.Length != args.Length)
                {
                    Logger.LogWarn("Wrong number of arguments");
                    return;
                }
                for (int i = 0; i < parameters.Length; i++)
                {
                    Type parameterType = parameters[i].ParameterType;
                    if (parameterType == typeof(string))
                    {
                        args[i] = splitted[i + 1];
                    }
                    else
                    {
                        try
                        {
                            //parse with Parse(string s, IFormatProvider? provider) method.
                            args[i] = parameterType.GetMethods().Where(m => m.Name == "Parse").ToList()[2].Invoke(null, new object[] { splitted[i + 1], CultureInfo.InvariantCulture });
                        }
                        catch (Exception)
                        {

                            Logger.LogWarn($"Can't parse argument {splitted[i]}");
                            return;
                        }
                    }
                }
            }
            command.MethodInfo.Invoke(target, args);
        }
    }
}