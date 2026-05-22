using Photon.Deterministic;

namespace Quantum
{
    public partial struct PlayerReadyMap
    {
        public void ClearMap(Frame frame)
        {
            var m = frame.ResolveDictionary(readyMap);
            m.Clear();
        }
        
        public bool CheckForAllReady(Frame frame)
        {
            var m = frame.ResolveDictionary(readyMap);
            
            for (int player = 0; player < frame.MaxPlayerCount; player++) {
                var isPlayerConnected = (frame.GetPlayerInputFlags(player) & DeterministicInputFlags.PlayerNotPresent) == 0;

                if (!isPlayerConnected)
                {
                    m.TryAdd(player, true);
                    continue;
                }
                
                if (frame.TryGetPlayerCommand<PlayerReadyCommand>(player, out var command)) {
                    m.TryAdd(player, true);
                }
            }

            return m.Count >= frame.MaxPlayerCount;
        }

        public bool CheckForAllFinishedWithUnsyncedCutscene(Frame frame, AssetRef<Tag> cutsceneGroupTag, AssetRef<Tag> cutsceneTag)
        {
            var m = frame.ResolveDictionary(readyMap);
            
            for (int player = 0; player < frame.MaxPlayerCount; player++) {
                var isPlayerConnected = (frame.GetPlayerInputFlags(player) & DeterministicInputFlags.PlayerNotPresent) == 0;

                if (!isPlayerConnected)
                {
                    m.TryAdd(player, true);
                    continue;
                }
                
                if (frame.TryGetPlayerCommand<UnsyncedCutsceneFinishedCommand>(player, out var command)) {
                    if(command.cutsceneSourceTag == cutsceneGroupTag && command.cutsceneTag == cutsceneTag)
                        m.TryAdd(player, true);
                }
            }

            return m.Count >= frame.MaxPlayerCount;
        }
    }
}
