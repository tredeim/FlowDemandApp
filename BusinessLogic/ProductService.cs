using BusinessLogic;
using Domain;
using System.Threading.Channels;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly int _degreeOfParallelism;

    public ProductService(IProductRepository productRepository, int degreeOfParallelism)
    {
        _productRepository = productRepository;
        _degreeOfParallelism = degreeOfParallelism;
    }

    public async Task ProcessProductsAsync(CancellationToken cancellationToken)
    {
        var readChannel = Channel.CreateUnbounded<Product>();
        var writeChannel = Channel.CreateUnbounded<DemandResult>();

        var readingTask = _productRepository.ReadFileAsync(readChannel.Writer, cancellationToken);
        var processingTask = ProcessProductsAsync(readChannel.Reader, writeChannel.Writer, cancellationToken);
        var writingTask = _productRepository.WriteDemandsAsync(writeChannel.Reader, cancellationToken);

        await Task.WhenAll(readingTask, processingTask, writingTask);
    }

    private async Task ProcessProductsAsync(ChannelReader<Product> readChannel, ChannelWriter<DemandResult> writeChannel, CancellationToken cancellationToken)
    {
        int demandsCalculated = 0;
        var semaphore = new SemaphoreSlim(_degreeOfParallelism);

        await foreach (var product in readChannel.ReadAllAsync(cancellationToken))
        {
            await semaphore.WaitAsync(cancellationToken);

            _ = ProcessProductAsync(product, writeChannel, semaphore, cancellationToken);
            demandsCalculated++;
            Console.WriteLine($"Calculated: {demandsCalculated}.");
        }

        writeChannel.Complete();
    }

    private async Task ProcessProductAsync(Product product, ChannelWriter<DemandResult> writeChannel, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        try
        {
            var demand = await DemandCalculator.CalculateDemandAsync(product, cancellationToken);
            await writeChannel.WriteAsync(new DemandResult { ProductId = product.Id, Demand = demand }, cancellationToken);
        }
        finally
        {
            semaphore.Release();
        }
    }
}
