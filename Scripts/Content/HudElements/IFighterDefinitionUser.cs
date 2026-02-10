using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    public interface IFighterDefinitionUser
    {
        public IFighterDefinition FighterDef { get; set; }
    }
}