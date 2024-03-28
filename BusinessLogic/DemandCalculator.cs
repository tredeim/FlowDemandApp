using Domain;

namespace BusinessLogic
{
    public class DemandCalculator
    {
        public static async Task<int> CalculateDemandAsync(Product product, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(10); //Can be replaced by a real asynchronous operation if necessary

            if (product.Prediction <= product.Stock)
            {
                return 0;
            }
            else if (product.Prediction <= 2 * product.Stock)
            {
                return product.Prediction - product.Stock;
            }
            else
            {
                return (int)(1.5 * (product.Prediction - product.Stock));
            }
        }
    }
}
