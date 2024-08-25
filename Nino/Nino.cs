using dotenv.net;
using Localizer;
using System.Text;
using static Localizer.Localizer;

namespace Nino
{
    public class Nino
    {
        public static async Task Main()
        {
            var env = DotEnv.Read();
            Console.OutputEncoding = Encoding.UTF8;
            LoadLocalizations(new Uri(Directory.GetCurrentDirectory()));

            await AzureHelper.Setup(env["AZURE_COSMOS_ENDPOINT"], env["AZURE_CLIENT_SECRET"], env["AZURE_COSMOS_DB_NAME"]);

            Console.WriteLine(T("test.example.singular2", "en-US"));
        }
    }
}
