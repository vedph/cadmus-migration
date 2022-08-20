using Cadmus.Cli.Core;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Export;
using Cadmus.Export.Preview;
using Cadmus.Migration.Cli.Services;
using Fusi.Cli;
using Fusi.Cli.Commands;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Migration.Cli.Commands
{
    /// <summary>
    /// Render items via item composers.
    /// </summary>
    /// <seealso cref="ICommand" />
    internal sealed class RenderItemsCommand : ICommand
    {
        private readonly RenderItemsCommandOptions _options;

        private RenderItemsCommand(RenderItemsCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication app,
            ICliAppContext context)
        {
            app.Description = "Render a set of items.";
            app.HelpOption("-?|-h|--help");

            CommandArgument dbArgument = app.Argument("[DatabaseName]",
               "The name of the source Cadmus database.");
            CommandArgument cfgPathArgument = app.Argument("[ConfigPath]",
               "The path to the rendering configuration file.");
            CommandArgument outDirArgument = app.Argument("[OutputDir]",
               "The output directory.");
            // when you use plugins, your plugin must import all the required
            // components, and export a tagged implementation of
            // ICadmusPreviewFactoryProvider. Its tag is specified here.
            CommandOption pluginTagOption = app.Option("--plugin|-p",
                "The tag of the factory provider plugin",
                CommandOptionType.SingleValue);
            CommandOption composerKeyOption = app.Option("--composer|-c",
                "The key of the item composer to use (default='default').",
                CommandOptionType.SingleValue);

            app.OnExecute(() =>
            {
                context.Command = new RenderItemsCommand(
                    new RenderItemsCommandOptions(context)
                    {
                        DatabaseName = dbArgument.Value,
                        ConfigPath = cfgPathArgument.Value,
                        OutputDir = outDirArgument.Value,
                        FactoryProviderTag = pluginTagOption.Value(),
                        ComposerKey = composerKeyOption.Value(),
                    });
                return 0;
            });
        }

        public Task Run()
        {
            ColorConsole.WriteWrappedHeader("Render Items",
                headerColor: ConsoleColor.Green);
            Console.WriteLine($"Database: {_options.DatabaseName}");
            Console.WriteLine($"Config path: {_options.ConfigPath}");
            Console.WriteLine($"Output dir: {_options.OutputDir}");
            Console.WriteLine($"Factory provider tag: {_options.FactoryProviderTag ?? "---"}");
            Console.WriteLine($"Composer: {_options.ComposerKey ?? "---"}\n");

            // load config
            ColorConsole.WriteInfo("Loading config...");
            string config = CommandHelper.LoadFileContent(_options.ConfigPath!);

            // get factory from its provider
            ColorConsole.WriteInfo("Building preview factory...");
            ICadmusPreviewFactoryProvider? provider = _options.Context
                .GetContextService(_options.DatabaseName).GetFactoryProvider();
            if (provider == null)
            {
                ColorConsole.WriteError("Preview factory provider not found");
                return Task.CompletedTask;
            }
            CadmusPreviewFactory factory = provider.GetFactory(config);

            ColorConsole.WriteInfo("Building repository factory...");
            ICliCadmusRepositoryProvider repositoryProvider =
                _options.RepositoryProviderTag != null
                ? PluginFactoryProvider.GetFromTag<ICliCadmusRepositoryProvider>
                    (_options.RepositoryProviderTag)
                : new StandardCliCadmusRepositoryProvider(_options.Configuration!);
            if (repositoryProvider == null)
            {
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    _options.RepositoryProviderTag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
            }

            // create composer
            ColorConsole.WriteInfo("Creating item composer...");
            IItemComposer? composer = factory.GetComposerByKey(
                _options.ComposerKey ?? "default");
            if (composer == null)
            {
                ColorConsole.WriteError(
                    $"Could not find composer with key {_options.ComposerKey}.");
                return Task.CompletedTask;
            }

            // create ID collector
            ColorConsole.WriteInfo("Creating item collector...");
            IItemIdCollector? collector = factory.GetItemIdCollector();
            if (collector == null)
            {
                ColorConsole.WriteError("No item ID collector defined in configuration.");
                return Task.CompletedTask;
            }

            // process items
            ColorConsole.WriteInfo("Processing items...");
            if (!Directory.Exists(_options.OutputDir))
                Directory.CreateDirectory(_options.OutputDir);

            ICadmusRepository repository = repositoryProvider.CreateRepository(
                _options.DatabaseName);

            foreach (string id in collector.GetIds())
            {
                ColorConsole.WriteInfo(" - " + id);
                IItem item = repository.GetItem(id, true);
                composer.Compose(item);
            }

            return Task.CompletedTask;
        }
    }

    internal class RenderItemsCommandOptions :
        CommandOptions<CadmusMigCliAppContext>
    {
        /// <summary>
        /// Gets or sets the tag of the component found in some plugin and
        /// implementing <see cref="ICadmusPreviewFactoryProvider"/>.
        /// </summary>
        public string? FactoryProviderTag { get; set; }
        /// <summary>
        /// Gets or sets the tag of the component found in some plugin and
        /// implementing <see cref="ICliCadmusRepositoryProvider"/>.
        /// </summary>
        public string? RepositoryProviderTag { get; set; }
        public string DatabaseName { get; set; }
        public string ConfigPath { get; set; }
        public string ComposerKey { get; set; }
        public string OutputDir { get; set; }

        public RenderItemsCommandOptions(ICliAppContext options)
            : base((CadmusMigCliAppContext)options)
        {
            DatabaseName = "cadmus";
            ConfigPath = "render.config";
            ComposerKey = "default";
            OutputDir = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
        }
    }
}