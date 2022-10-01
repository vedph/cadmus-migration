using Cadmus.Export;
using Cadmus.Export.ML;
using Cadmus.Export.Preview;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using SimpleInjector;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml;

namespace CadmusPreviewBuilder.Pages
{
    public partial class Builder
    {
        private int _configHash;
        private CadmusPreviewer? _previewer;

        private BuilderModel Model { get; }
        private EditContext Context { get; }

        public Builder()
        {
            Model = new BuilderModel();
            Context = new EditContext(Model);
            Model.Json = LoadResourceText("SampleFragment.json");
            Model.Config = LoadResourceText("SampleConfig.json");
            Model.Css = LoadResourceText("SampleStyles.css");
        }

        private static Stream GetResourceStream(string name)
        {
            return Assembly.GetExecutingAssembly()!
                .GetManifestResourceStream($"CadmusPreviewBuilder.Assets.{name}")!;
        }

        private static string LoadResourceText(string name)
        {
            using StreamReader reader = new(GetResourceStream(name),
                Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private CadmusPreviewFactory GetFactory()
        {
            Container container = new();
            CadmusPreviewFactory.ConfigureServices(container,
                new[] { typeof(TeiStandoffTextBlockRenderer).Assembly });

            ConfigurationBuilder cb = new();
            IConfigurationRoot config = cb
                .AddInMemoryJson(Model.Config ?? "{}")
                .Build();
            return new CadmusPreviewFactory(container, config)
            {
                ConnectionString = "mongodb://localhost:27017/cadmus-test"
            };
        }

        private void BuildXml()
        {
            if (Model.Json.Length == 0 || Model.IsRunning) return;

            try
            {
                Model.IsRunning = true;
                Model.Error = null;

                XmlDocument? doc = JsonConvert.DeserializeXmlNode(Model.Json);
                if (doc is null)
                {
                    Model.Xml = "";
                    return;
                }
                Model.Xml = doc.OuterXml;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Model.Error = ex.Message;
            }
            finally
            {
                Model.IsRunning = false;
            }
        }

        private async Task PreviewAsync()
        {
            if (Model.Json.Length == 0 || Model.Config.Length == 0 ||
                Model.IsRunning) return;

            try
            {
                Model.IsRunning = true;
                // https://stackoverflow.com/questions/56604886/blazor-display-wait-or-spinner-on-api-call
                await Task.Delay(1);
                Model.Error = null;

                // build previewer
                int configHash = Model.Config.GetHashCode();
                if (_previewer == null || _configHash != configHash)
                {
                    CadmusPreviewFactory factory = GetFactory();
                    // not using a repository here as we're serverless
                    _previewer = new CadmusPreviewer(factory, null);
                    _configHash = configHash;
                }

                await Task.Run(() =>
                {
                    string result;
                    if (Model.IsFragment)
                        result = _previewer.RenderFragmentJson(Model.Json, 0);
                    else
                        result = _previewer.RenderPartJson(Model.Json);

                    // TODO combine with CSS
                    Model.Html = result;

                    Model.Result = (MarkupString)result;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                Model.Error = ex.Message;
            }
            finally
            {
                Model.IsRunning = false;
            }
        }
    }
}
