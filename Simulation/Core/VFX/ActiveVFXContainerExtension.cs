namespace Quantum
{
    public unsafe partial struct ActiveVFXContainer
    {
        public static void PlayVFX(Frame frame, EntityRef entityRef, ActiveVFXPart part)
        {
            ActiveVFXContainer* container;
            if (!frame.Unsafe.TryGetPointer<ActiveVFXContainer>(entityRef, out container))
            {
                frame.Add<ActiveVFXContainer>(entityRef, out container);
                container->vfxParts = frame.AllocateList<ActiveVFXPart>();
            }

            var vfxParts = frame.ResolveList(container->vfxParts);
            vfxParts.Add(part);
        }

        public static void StopVFX(Frame frame, EntityRef entityRef, AssetRef<VisualEffectEntry> vfx)
        {
            ActiveVFXContainer* container;
            if (!frame.Unsafe.TryGetPointer<ActiveVFXContainer>(entityRef, out container)) return;
            var vfxParts = frame.ResolveList(container->vfxParts);

            for (int i = vfxParts.Count - 1; i >= 0; i--)
            {
                if(vfxParts[i].vfx != vfx) continue;
                vfxParts.RemoveAt(i);
                break;
            }
        }
    }
}
