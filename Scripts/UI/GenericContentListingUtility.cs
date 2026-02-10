using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HnSF
{
    [System.Serializable]
    public class GenericContentListingUtility<T> : GenericContentListingUtilityBase where T : IContentDefinition
    {
        public override void Initialize()
        {
            Paginated = false;
            var gameManager = HnSFManagersContainer.instance;
            currentAssetList = gameManager.contentManager.GetAssetList<T>();
        }

        public override void UpdateMaxPageCount()
        {
            var gameManager = HnSFManagersContainer.instance;
            MaxPages = (gameManager.contentManager.GetAssetList<T>().Count / AmountPerPage) + 1;
        }
        
        protected override void UpdatePaginatedList()
        {
            onPreContentListChanged.Invoke(this);
            var gameManager = HnSFManagersContainer.instance;
            var aList = gameManager.contentManager.GetAssetListPaginated<T>(AmountPerPage, PageIndex);
            currentAssetList = aList.Item1;
            PageIndex = aList.Item2;
            onPostContentListChanged.Invoke(this);
        }
    }
}