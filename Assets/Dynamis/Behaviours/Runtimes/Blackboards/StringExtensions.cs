namespace Dynamis.Behaviours.Runtimes.Blackboards
{
    public static class StringExtensions
    {
        public static int ComputeFNV1AHash(this string str)
        {
            unchecked
            {
                const int offsetBasis = unchecked((int)2166136261);
                const int prime = 16777619;

                var hash = offsetBasis;

                foreach (var t in str)
                {
                    hash ^= t;
                    hash *= prime;
                }

                return hash;
            }
        }
    }
}