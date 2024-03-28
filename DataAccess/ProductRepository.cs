using Domain;
using System.Threading.Channels;

namespace DataAccess
{
    public class ProductRepository : IProductRepository
    {
        private const string _inputFilePath = "productData.csv";
        private const string _outputFilePath = "demandData.csv";

        public async Task ReadFileAsync(ChannelWriter<Product> writer, CancellationToken cancellationToken)
        {
            int linesRead = 0;
            using var reader = new StreamReader(_inputFilePath);
            await reader.ReadLineAsync();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                linesRead++;
                Console.WriteLine($"Read: {linesRead}.");
                var parts = line.Split(", ");

                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out var id) &&
                    int.TryParse(parts[1], out var prediction) &&
                    int.TryParse(parts[2], out var stock))
                {
                    var product = new Product
                    {
                        Id = id,
                        Prediction = prediction,
                        Stock = stock
                    };

                    await writer.WriteAsync(product, cancellationToken);
                }
            }

            writer.Complete();
        }

        public async Task WriteDemandsAsync(ChannelReader<DemandResult> reader, CancellationToken cancellationToken)
        {
            int resultsWritten = 0;

            using var writer = new StreamWriter(_outputFilePath);
            await writer.WriteLineAsync("id, demand");

            await foreach (var result in reader.ReadAllAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await writer.WriteLineAsync($"{result.ProductId}, {result.Demand}");
                resultsWritten++;
                Console.WriteLine($"Written: {resultsWritten}.  ");
            }
        }
    }
}
