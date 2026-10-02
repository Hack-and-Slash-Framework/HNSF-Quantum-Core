using Photon.Client;
using Photon.Realtime;
using UnityEngine;

namespace HnSF.sessionhandling.handlers
{
    public partial class SessionHandlerQuickMatchPhotonRealtime
    {
        public static partial class Messages
        {
            public static bool Send_StartGame(SessionHandlerQuickMatchPhotonRealtime sessionHandler)
            {
                var quantumClient = sessionHandler.quantumClient;

                if (quantumClient == null || !quantumClient.InRoom || !quantumClient.LocalPlayer.IsMasterClient ||
                    !quantumClient.CurrentRoom.IsOpen)
                    return false;
                
                return quantumClient.OpRaiseEvent((byte)110, null, 
                    new RaiseEventArgs { Receivers = ReceiverGroup.All },
                    SendOptions.SendReliable);
            }
            
            public static async void Received_StartGame(SessionHandlerQuickMatchPhotonRealtime sessionHandler, EventData eventData)
            {
                var quantumClient = sessionHandler.quantumClient;
                quantumClient.CurrentRoom.CustomProperties.TryGetValue(SessionHandlerQuickMatchPhotonRealtime.MAP_PROP_KEY, out object mapAssetRef);
                if (mapAssetRef == null)
                {
                    Debug.LogError("Failed to read the map asset ref during start");
                    quantumClient?.Disconnect();
                    return;
                }

                if (sessionHandler.selectedMapDefinition.IsValid == false)
                {
                    var mapLoadedAssetHandle = await HnSFManagersContainer.instance.contentManager.LoadAssetFromModAsync(new ModAssetSoftReference(mapAssetRef as string));
                    if (mapLoadedAssetHandle == null)
                    {
                        Debug.LogError($"Failed to load map asset [{mapAssetRef as string}].");
                        quantumClient?.Disconnect();
                        return;
                    }

                    sessionHandler.selectedMapDefinition = mapLoadedAssetHandle;

                    await sessionHandler.selectedMapDefinition.GetAsset<IMapDefinition>().LoadAssets();
                }
                
                if (quantumClient.LocalPlayer.IsMasterClient)
                {
                    // Save the started state in room properties for late joiners
                    var ht = new PhotonHashtable { { "STARTED", true } };
                    quantumClient.CurrentRoom.SetCustomProperties(ht);

                    if (quantumClient.CurrentRoom.CustomProperties.TryGetValue("HIDE-ROOM", out var hideRoom) && (bool)hideRoom)
                    {
                        quantumClient.CurrentRoom.IsVisible = false;
                    }
                }

                _ = sessionHandler.TransitionToQuantumGameSession();
            }
        }
    }
}