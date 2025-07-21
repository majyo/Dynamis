namespace Dynamis.Behaviours.Runtimes.Blackboards
{
    public interface IBlackboard
    {
        public void Debug();
        BlackboardKey GetOrRegisterKey(string keyName);
        void SetValue<T>(string keyName, T value);
        void SetValue<T>(BlackboardKey key, T value);
        bool TryGetValue<T>(string keyName, out T value);
        bool TryGetValue<T>(BlackboardKey key, out T value);
        bool ContainsKey(BlackboardKey key);
    }
}