using System.Collections.Generic;

namespace Quantum
{
    public static unsafe partial class TagContainerHelper
    {
        
        
        public static bool HasTag(Frame frame, EntityRef entityRef, AssetRef tag)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                return tagContainer->HasTag(frame, tag);
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                return tagCountContainer->HasTag(frame, tag);
            }
            return false;
        }

        public static bool HasAny(Frame frame, EntityRef entityRef, List<AssetRef> tags)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                return tagContainer->HasAny(frame, tags);
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                return tagCountContainer->HasAny(frame, tags);
            }
            return false;
        }

        public static int GetTagCount(Frame frame, EntityRef entityRef, AssetRef tag)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                return tagContainer->GetTagCount(frame, tag);
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                return tagCountContainer->GetTagCount(frame, tag);
            }
            return 0;
        }
    }
}
