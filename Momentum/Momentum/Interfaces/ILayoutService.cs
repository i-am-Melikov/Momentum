using Momentum.ViewModels.BasketVMs;

namespace Momentum.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingAsync();
        Task<List<BasketVM>> GetBasketsAsync();
    }
}
