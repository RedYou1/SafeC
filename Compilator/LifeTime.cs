namespace RedRust
{
    internal class LifeTime
    {
        public readonly LifeTime? from;

        public LifeTime(LifeTime? from = null)
        {
            this.from = from;
        }

        public bool Ok(LifeTime lifeTime)
            => this == lifeTime || (from is not null && from.Ok(lifeTime));

        public int Length()
            => (from?.Length() ?? 0) + 1;
    }
}
