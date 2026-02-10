namespace Quantum
{
    public static unsafe partial class FlagsExtensions 
    {
        public static ActorInputButtonType None()
        {
            return 0;
        }
        
        public static ActorInputButtonType All()
        {
            return (ActorInputButtonType)~0;
        }
        
        public static void SetNone(this ActorInputButtonType self)
        {
            self = 0;
        }

        public static void SetAll(this ActorInputButtonType self)
        {
            self = (ActorInputButtonType)~0;
        }
        
        public static ActorInputButtonType None(this ActorInputButtonType self)
        {
            return 0;
        }
        
        public static ActorInputButtonType All(this ActorInputButtonType self)
        {
            return (ActorInputButtonType)~0;
        }
    }
}