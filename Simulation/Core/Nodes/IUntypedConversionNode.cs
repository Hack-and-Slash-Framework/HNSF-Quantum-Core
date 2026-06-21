#if UNITY_EDITOR
namespace HnSF.Nodes
{
    public interface IUntypedConversionNode
    {
        bool TryGetValue<T>(out T value);
    }
}
#endif