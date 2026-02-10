namespace Quantum
{
    public partial class FrameContextUser
    {
        public Quantum.LayerMask hitboxLayerMask;
        public Quantum.LayerMask hurtboxLayerMask;
        public Quantum.LayerMask collisionboxLayerMask;
        public Quantum.LayerMask throwboxLayerMask;
        
        public void GetMasks(Frame f)
        {
            hitboxLayerMask = f.Layers.DefaultLayerMatrix[f.Layers.GetLayerIndex("Hitbox")];
            hurtboxLayerMask = f.Layers.DefaultLayerMatrix[f.Layers.GetLayerIndex("Hurtbox")];
            collisionboxLayerMask = f.Layers.DefaultLayerMatrix[f.Layers.GetLayerIndex("Collisionbox")];
            throwboxLayerMask = f.Layers.DefaultLayerMatrix[f.Layers.GetLayerIndex("Throwbox")];
        }
    }
}
