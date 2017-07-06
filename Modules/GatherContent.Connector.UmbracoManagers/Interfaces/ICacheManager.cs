namespace GatherContent.Connector.Managers.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICacheManager
    {
        bool IsSet(string key);

        T Get<T>(string key);

        void Set(string key, object data, int cacheTime);
    }
}