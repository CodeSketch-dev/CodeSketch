namespace CodeSketch.Modules.StatSystem
{
    public sealed class StatModifierInstance
    {
        public readonly float Flat;
        public readonly float Percent;

        float _remainingTime; // seconds (game time)

        public StatModifierInstance(StatModifier modifier, float duration)
        {
            Flat = modifier.Flat;
            Percent = modifier.Percent;
            _remainingTime = duration;
        }

        /// <summary>
        /// Tick theo game time.
        /// return true nếu đã hết hạn
        /// </summary>
        public bool Tick(float deltaTime)
        {
            if (_remainingTime <= 0f)
                return false;

            _remainingTime -= deltaTime;
            return _remainingTime <= 0f;
        }

        public float RemainingTime => _remainingTime;
    }
}