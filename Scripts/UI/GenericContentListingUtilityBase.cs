using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HnSF
{
    [System.Serializable]
    public class GenericContentListingUtilityBase
    {
        public UnityEvent<GenericContentListingUtilityBase> onPreContentListChanged = new ();
        public UnityEvent<GenericContentListingUtilityBase> onPostContentListChanged = new ();
        
        public bool Paginated { get; protected set; } = false;
        public int PageIndex { get; protected set; } = 0;
        public int AmountPerPage { get; protected set; } = 10;
        // Not set by default, must call UpdateMaxPageCount.
        public int MaxPages { get; protected set; } = 0;

        public List<ModAssetSoftReference> currentAssetList = new List<ModAssetSoftReference>();
        
        public virtual void Initialize()
        {
            
        }

        public virtual void Initialize(int amountPerPage)
        {
            if (amountPerPage <= 0)
            {
                Debug.LogError("ContentAmountPerPage must be a positive integer.");
                return;
            }
            Paginated = true;
            AmountPerPage = amountPerPage;
            
            UpdatePaginatedList();
        }

        public virtual void Uninitialize()
        {
            currentAssetList = null;
        }

        public virtual void SetPage(int pageIndex)
        {
            PageIndex = pageIndex;
            UpdatePaginatedList();
        }

        public virtual void UpdateMaxPageCount()
        {
            
        }
        
        protected virtual void UpdatePaginatedList()
        {
            
        }
    }
}