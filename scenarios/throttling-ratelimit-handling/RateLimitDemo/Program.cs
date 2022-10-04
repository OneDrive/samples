using Spectre.Console;
using System.Threading.Tasks;

namespace RateLimitDemo
{
    internal class Program
    {

        internal static async Task Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("Ratelimit demo").Centered().Color(Color.Green));
            AnsiConsole.WriteLine("");

            // Instantiate the demo class and run the demo
            var demo = new Demo();

            // Specify the number of parallel threads and whether RateLimit headers are processed
            await demo.InitializeAndRunAsync(5, true);
        }
    }
}