namespace Dynamis.Behaviours.Runtimes.Blackboards
{
    public class BBValue<T>
    {
        private readonly IBlackboard _blackboard;
        private readonly BlackboardKey _key;

        public T Value
        {
            get => _blackboard.TryGetValue(_key, out T value) ? value : default;
            set => _blackboard.SetValue(_key, value);
        }

        public BBValue(IBlackboard blackboard)
        {
            _blackboard = blackboard;
            _key = new BlackboardKey(GetType().Name);
        }
    }
}