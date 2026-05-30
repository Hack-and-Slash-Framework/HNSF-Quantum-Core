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
        
        public static bool HasAny(Frame frame, EntityRef entityRef, IEnumerable<AssetRef> tags)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                foreach (var tag in tags)
                {
                    if (tagContainer->HasTag(frame, tag)) return true;
                }
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                foreach (var tag in tags)
                {
                    if (tagCountContainer->HasTag(frame, tag)) return true;
                }
            }
            return false;
        }
        
        public static bool HasAny(Frame frame, EntityRef entityRef, IEnumerable<AssetRef<Tag>> tags)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                foreach (var tag in tags)
                {
                    if (tagContainer->HasTag(frame, tag)) return true;
                }
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                foreach (var tag in tags)
                {
                    if (tagCountContainer->HasTag(frame, tag)) return true;
                }
            }
            return false;
        }
        
        public static bool HasAll(Frame frame, EntityRef entityRef, IEnumerable<AssetRef> tags)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                foreach (var t in tags)
                {
                    if (!tagContainer->HasTag(frame, t))
                        return false;
                }
                return true;
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                foreach (var t in tags)
                {
                    if (!tagCountContainer->HasTag(frame, t))
                        return false;
                }
                return true;
            }
            return false;
        }
        
        public static bool HasAll(Frame frame, EntityRef entityRef, IEnumerable<AssetRef<Tag>> tags)
        {
            if (frame.Unsafe.TryGetPointer<GameplayTagContainer>(entityRef, out var tagContainer))
            {
                foreach (var t in tags)
                {
                    if (!tagContainer->HasTag(frame, t))
                        return false;
                }
                return true;
            }else if (frame.Unsafe.TryGetPointer<GameplayTagCountContainer>(entityRef, out var tagCountContainer))
            {
                foreach (var t in tags)
                {
                    if (!tagCountContainer->HasTag(frame, t))
                        return false;
                }
                return true;
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
