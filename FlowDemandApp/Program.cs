using DataAccess;
using Domain;
using Microsoft.Extensions.Configuration;


class Program
{
    static async Task Main()
    {
        Console.WriteLine("Press 'c' to cancel the operation...");

        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        var parallelismOptions = configuration.GetSection("ParallelismOptions").Get<ParallelismOptions>();
        var degreeOfParallelism = parallelismOptions.DegreeOfParallelism;

        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var productRepository = new ProductRepository();
        var productService = new ProductService(productRepository, degreeOfParallelism);

        var cancelTask = Task.Run(() =>
        {

            while (Console.ReadKey(true).Key != ConsoleKey.C)
            {
                // Wait for the 'c' key to be pressed
            }
            cancellationTokenSource.Cancel();
            Console.WriteLine("Cancellation requested.");
        });

        try
        {
            await productService.ProcessProductsAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }
}
