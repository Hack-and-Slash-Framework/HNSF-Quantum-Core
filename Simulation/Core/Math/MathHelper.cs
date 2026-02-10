namespace Quantum
{
    public static unsafe class MathHelper
    {
        public static bool IsPowerOfTwo(int i) {
            return i > 0 && (i & (i-1)) == 0;
        }
    }
}
