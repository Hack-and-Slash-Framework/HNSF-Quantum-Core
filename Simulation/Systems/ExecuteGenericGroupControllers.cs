using System.Collections.Generic;
using HnSF.core.GroupControl;
using Quantum;
using Quantum.Collections;

namespace HnSF.Systems
{
    public unsafe class ExecuteGenericGroupControllers : SystemMainThread
    {
        List<EntityRef> keysToRemove = new();
        List<AssetRef> assetRefKeysToRemove = new();
        public override void Update(Frame frame)
        {
            var genericControlManager = frame.GetOrAddSingleton<GenericGroupControlManager>();
            var entityToScriptsMap = frame.ResolveDictionary(genericControlManager.controlInfoEntityMap);
            var assetRefToScriptsMap = frame.ResolveDictionary(genericControlManager.controlInfoEntityAssetRefMap);
            
            ExecuteScriptsInMap(frame, entityToScriptsMap);
            ExecuteScriptsInMap(frame, assetRefToScriptsMap);
            
            keysToRemove.Clear();
            assetRefKeysToRemove.Clear();
        }

        private void ExecuteScriptsInMap(Frame frame, QDictionary<AssetRef, ScriptsControllingEntity> entityMap)
        {
            foreach (var map in entityMap)
            {
                var scriptEntityList = frame.ResolveList(map.Value.scriptEntityList);
                
                for (int i = scriptEntityList.Count - 1; i >= 0; i--)
                {
                    if (!frame.Unsafe.TryGetPointer<GenericGroupControl>(scriptEntityList[i], out var ggc))
                    {
                        scriptEntityList.RemoveAt(i);
                        continue;
                    }
                    
                    var groupControlContext = new BattleScriptContext();
                    groupControlContext.SetScriptEntityAndBlackboard(frame, scriptEntityList[i], null);
                    
                    var result = ggc->data.Tick(frame, scriptEntityList[i], ref groupControlContext);
                    switch (result)
                    {
                        case BattleScriptResult.Running:
                            break;
                        case BattleScriptResult.Succeeded:
                        case BattleScriptResult.Failed:
                        case BattleScriptResult.Canceled:
                            ggc->data.currentAction = -1;
                            RemoveScriptEntityFromList(frame, ggc, ref scriptEntityList, i);
                            break;
                    }

                    if (scriptEntityList.Count == 0)
                    {
                        map.Value.Cleanup(frame);
                        assetRefKeysToRemove.Add(map.Key);
                    }
                }
                
                foreach (var infoEntityKey in assetRefKeysToRemove)
                {
                    entityMap.Remove(infoEntityKey);
                }
            }
        }

        private void ExecuteScriptsInMap(Frame frame, QDictionary<EntityRef, ScriptsControllingEntity> entityMap)
        {
            foreach (var map in entityMap)
            {
                var scriptEntityList = frame.ResolveList(map.Value.scriptEntityList);
                
                for (int i = scriptEntityList.Count - 1; i >= 0; i--)
                {
                    if (!frame.Unsafe.TryGetPointer<GenericGroupControl>(scriptEntityList[i], out var ggc))
                    {
                        scriptEntityList.RemoveAt(i);
                        continue;
                    }
                    
                    var groupControlContext = new BattleScriptContext();
                    groupControlContext.SetScriptEntityAndBlackboard(frame, scriptEntityList[i], null);
                    
                    var result = ggc->data.Tick(frame, scriptEntityList[i], ref groupControlContext);
                    switch (result)
                    {
                        case BattleScriptResult.Running:
                            break;
                        case BattleScriptResult.Succeeded:
                        case BattleScriptResult.Failed:
                        case BattleScriptResult.Canceled:
                            ggc->data.currentAction = -1;
                            RemoveScriptEntityFromList(frame, ggc, ref scriptEntityList, i);
                            break;
                    }

                    if (scriptEntityList.Count == 0)
                    {
                        map.Value.Cleanup(frame);
                        keysToRemove.Add(map.Key);
                    }
                }
                
                foreach (var infoEntityKey in keysToRemove)
                {
                    entityMap.Remove(infoEntityKey);
                }
            }
        }

        private static void RemoveScriptEntityFromList(Frame frame, GenericGroupControl* ggc, ref QList<EntityRef> scriptEntityList, int i)
        {
            if(ggc->autoDestroy) frame.Destroy(scriptEntityList[i]);
            scriptEntityList.RemoveAt(i);
        }
    }
}
