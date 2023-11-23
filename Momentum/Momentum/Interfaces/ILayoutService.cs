namespace Momentum.Interfaces
{
    public interface ILayoutService
    {
        Task<Dictionary<string, string>> GetSettingAsync();
    }
}
