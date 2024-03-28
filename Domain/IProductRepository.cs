using System.Threading.Channels;

namespace Domain
{
    public interface IProductRepository
    {
        Task ReadFileAsync(ChannelWriter<Product> writer, CancellationToken cancellationToken);
        Task WriteDemandsAsync(ChannelReader<DemandResult> reader, CancellationToken cancellationToken);
    }
}
