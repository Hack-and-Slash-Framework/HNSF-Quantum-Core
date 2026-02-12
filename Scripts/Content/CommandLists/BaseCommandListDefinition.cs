using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HnSF
{
    public abstract partial class BaseCommandListDefinition : IContentDefinition
    {
        public abstract BaseCommandListMovesetDefinition[] GetMovesets();
    }
}