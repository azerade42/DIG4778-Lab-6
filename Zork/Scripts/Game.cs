using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;


namespace Zork
{
    public class Game
    {
        [JsonIgnore]
        public static Game Instance { get; private set; }
        public World World { get; private set; }

        [JsonIgnore]
        public Player player { get; private set; }

        [JsonIgnore]
        private bool IsRunning { get; set; }

        [JsonIgnore]
        public CommandManager CommandManager { get; }

        public Game(World world, Player player)
        {
            this.World = world;
            this.player = player;
        }

        public Game()
        {
            CommandManager = new CommandManager();

            if (World == null) World = new World();
        }

        public static void Start(string gameFilename)
        {
            if (!File.Exists(gameFilename))
            {
                throw new FileNotFoundException($"Expected file: {gameFilename}");
            }

            while (Instance == null || Instance.mIsRestarting)
            {
                Instance = Load(gameFilename);
                Instance.LoadCommands();
                Instance.LoadScripts();
                Instance.DisplayWelcomeMessage();
                Instance.Run();
            }
        }

        public void Run()
        {
            mIsRunning = true;
            Room previousRoom = null;
            while (mIsRunning)
            {
                Console.WriteLine(player.Location);
                if (previousRoom != player.Location)
                {
                    CommandManager.PerformCommand(this, "LOOK");
                    previousRoom = player.Location;
                }

                Console.Write("\n> ");
                if (CommandManager.PerformCommand(this, Console.ReadLine().Trim()))
                {
                    player.Moves++;
                }
                else
                {
                    Console.WriteLine("That's not the verb I recognize.");
                }
            }
        }

        public void Restart()
        {
            mIsRunning = false;
            mIsRestarting = true;
            Console.Clear();
        }

        public void Quit() => mIsRunning = false;

        public static Game Load(string filename)
        {
            Game game = JsonConvert.DeserializeObject<Game>(File.ReadAllText(filename));
            game.player = game.World.SpawnPlayer();

            return game;
        }

        private void LoadCommands()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type type in types)
            {
                CommandClassAttribute commandClassAttribute = type.GetCustomAttribute<CommandClassAttribute>();
                if (commandClassAttribute != null)
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        CommandAttribute commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                        if (commandAttribute != null)
                        {
                            Command command = new Command(commandAttribute.CommandName, commandAttribute.Verbs,
                                (Action<Game, CommandContext>)Delegate.CreateDelegate(typeof(Action<Game, CommandContext>), method));
                            CommandManager.AddCommand(command);
                        }
                    }
                }
            }
        }

        private void LoadScripts()
        {
            foreach (string file in Directory.EnumerateFiles(ScriptDirectory, ScriptFileExtension))
            {
                try
                {
                    ScriptOptions scriptOptions = ScriptOptions.Default.AddReferences(Assembly.GetExecutingAssembly());
#if DEBUG
                    scriptOptions = scriptOptions.WithEmitDebugInformation(true)
                        .WithFilePath(new FileInfo(file).FullName)
                        .WithFileEncoding(Encoding.UTF8);
#endif
                    string script = File.ReadAllText(file);
                    CSharpScript.RunAsync(script, scriptOptions).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error compiling script {file} Error: {ex.Message}");
                }
            }
        }

        public bool ConfirmAction(string prompt)
        {
            Console.Write(prompt);
            while (true)
            {
                string response = Console.ReadLine().Trim().ToUpper();
                if (response == "YES" || response == "Y")
                    return true;
                if (response == "NO" || response == "N")
                    return false;
                else
                    Console.Write("Please answer yes or no.> ");
            }
        }

        private void DisplayWelcomeMessage() => Console.WriteLine(WelcomeMessage);

        private static readonly string ScriptDirectory = "Scripts";
        private static readonly string ScriptFileExtension = "*.csx";

        public static readonly Random Random = new Random();

        [JsonProperty]
        private string WelcomeMessage = null;

        private bool mIsRunning;
        private bool mIsRestarting;
    }
}
