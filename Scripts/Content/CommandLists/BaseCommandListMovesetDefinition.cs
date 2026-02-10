using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HnSF
{
    [CreateAssetMenu(menuName = "HnSF/Content/Command List Moveset")]
    public class BaseCommandListMovesetDefinition : IContentDefinition
    {
        public string label;
        public BaseCommandListEntryGrouping[] EntryGroups;
    }
}