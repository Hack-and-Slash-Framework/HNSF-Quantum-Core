using UnityEngine;

namespace Quantum
{
  using System;
  using System.Linq;
  using UnityEditor;
#if QUANTUM_UNITY
  using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;
#endif

    partial class Statics 
    {
        static partial void InitStaticDelegatesUser()
        {
            var tList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t =>  t.IsClass && typeof(IModQuantumData).IsAssignableFrom(t));
          
          foreach (var t in tList) {
            var bClass = (IModQuantumData)Activator.CreateInstance(t);
            bClass.InitStaticDelegates();
          }
        }

        static partial void RegisterSimulationTypesUser(TypeRegistry typeRegistry) {
          Debug.Log("Registering Types User");
          
          var tList = AppDomain.CurrentDomain.GetAssemblies()
              .Where(a => !a.IsDynamic)
              .SelectMany(a => a.GetTypes())
              .Where(t =>  t.IsClass && typeof(IModQuantumData).IsAssignableFrom(t));
          
          foreach (var t in tList) {
              var bClass = (IModQuantumData)Activator.CreateInstance(t);
              bClass.RegisterSimulationTypesGen(typeRegistry);
          }
        }

        [Preserve]
        static void EnsureNotStrippedUser()
        {
          
        }

        static int GetModComponentCounts() {
            int cnt = 0;
            
            var tList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t =>  t.IsClass && typeof(IModQuantumData).IsAssignableFrom(t));
          
            foreach (var t in tList) {
                var bClass = (IModQuantumData)Activator.CreateInstance(t);
                cnt += bClass.GetComponentTypeIdCount();
            }
            
            return cnt;
        }

        static void InitModComponentTypeIdGen(ref ComponentTypeId.Builder builder) {
            var tList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t =>  t.IsClass && typeof(IModQuantumData).IsAssignableFrom(t));
          
            foreach (var t in tList) {
                var bClass = (IModQuantumData)Activator.CreateInstance(t);
                bClass.InitComponentTypeIdGen(ref builder);
            }
        }
        
        static unsafe void CustomInitComponentTypeIdGen(){
          int modComponentCount = GetModComponentCounts();
          //Debug.Log($"Custom Initalizer. Mod Component Count {modComponentCount}");
          InitComponentTypeId();
          /*
          var componentTypeId = ComponentTypeId.Reset(ComponentTypeId.BuiltInComponentCount + 53 + modComponentCount)
          .AddBuiltInComponents()
          .Add<AIBlackboardComponent>(AIBlackboardComponent.Serialize, AIBlackboardComponent.OnAdded,
            AIBlackboardComponent.OnRemoved, ComponentFlags.None)
          .Add<Quantum.AICharacterActor>(Quantum.AICharacterActor.Serialize, null, null, ComponentFlags.None)
          .Add<BTAgent>(BTAgent.Serialize, BTAgent.OnAdded, BTAgent.OnRemoved, ComponentFlags.None)
          .Add<BotSDKGlobals>(BotSDKGlobals.Serialize, BotSDKGlobals.OnAdded, BotSDKGlobals.OnRemoved,
            ComponentFlags.Singleton)
          .Add<Quantum.BoxCombatant>(Quantum.BoxCombatant.Serialize, null, Quantum.BoxCombatant.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.CharaAnimator>(Quantum.CharaAnimator.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.ActorInputBufferr>(Quantum.ActorInputBufferr.Serialize, null, Quantum.ActorInputBufferr.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.CharaLink>(Quantum.CharaLink.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.CharaPhysics>(Quantum.CharaPhysics.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.CharaStateMachines>(Quantum.CharaStateMachines.Serialize, null,
            Quantum.CharaStateMachines.OnRemoved, ComponentFlags.None)
          .Add<Quantum.Collisionbox>(Quantum.Collisionbox.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.CombatBox>(Quantum.CombatBox.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.CombatHitDetectionInfo>(Quantum.CombatHitDetectionInfo.Serialize, null,
            Quantum.CombatHitDetectionInfo.OnRemoved, ComponentFlags.Singleton)
          .Add<Quantum.CombatTargeter>(Quantum.CombatTargeter.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.CombatTeam>(Quantum.CombatTeam.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.DummyConfiguration>(Quantum.DummyConfiguration.Serialize, null,
            Quantum.DummyConfiguration.OnRemoved, ComponentFlags.None)
          .Add<Quantum.EntityCenter>(Quantum.EntityCenter.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.EntityForce>(Quantum.EntityForce.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.EnvQueryCached>(Quantum.EnvQueryCached.Serialize, null, Quantum.EnvQueryCached.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.Fighter>(Quantum.Fighter.Serialize, null, Quantum.Fighter.OnRemoved, ComponentFlags.None)
          .Add<Quantum.FighterArticle>(Quantum.FighterArticle.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.FighterArticles>(Quantum.FighterArticles.Serialize, null, Quantum.FighterArticles.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.GenericGamemodeGlobals>(Quantum.GenericGamemodeGlobals.Serialize, null,
            Quantum.GenericGamemodeGlobals.OnRemoved, ComponentFlags.Singleton)
          .Add<Quantum.GenericStateAgent>(Quantum.GenericStateAgent.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.GotWallInfo>(Quantum.GotWallInfo.Serialize, null, null, ComponentFlags.None)
          .Add<HFSMAgent>(HFSMAgent.Serialize, HFSMAgent.OnAdded, HFSMAgent.OnRemoved, ComponentFlags.None)
          .Add<Quantum.HNSFStateAgent>(Quantum.HNSFStateAgent.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.HardTargetEntityDisabled>(Quantum.HardTargetEntityDisabled.Serialize, null, null,
            ComponentFlags.None)
          .Add<Quantum.Health>(Quantum.Health.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.Hitbox>(Quantum.Hitbox.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.Hurtbox>(Quantum.Hurtbox.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.InCounterhitState>(Quantum.InCounterhitState.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.InPunishCounterState>(Quantum.InPunishCounterState.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.IsChargingAttack>(Quantum.IsChargingAttack.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.IsDead>(Quantum.IsDead.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.IsInThrow>(Quantum.IsInThrow.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.IsInvisible>(Quantum.IsInvisible.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.IsThrowing>(Quantum.IsThrowing.Serialize, null, Quantum.IsThrowing.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.IsUntargetable>(Quantum.IsUntargetable.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.KCC>(Quantum.KCC.Serialize, null, Quantum.KCC.OnRemoved, ComponentFlags.None)
          .Add<Quantum.KCCProcessorLink>(Quantum.KCCProcessorLink.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.LocalDeltaTime>(Quantum.LocalDeltaTime.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.Parented>(Quantum.Parented.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.PlayerLink>(Quantum.PlayerLink.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.Projectile>(Quantum.Projectile.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.SyncedCutsceneTimer>(Quantum.SyncedCutsceneTimer.Serialize, null, null, ComponentFlags.None)
          .Add<Quantum.TeamVersusGlobals>(Quantum.TeamVersusGlobals.Serialize, null,
            Quantum.TeamVersusGlobals.OnRemoved,
            ComponentFlags.Singleton)
          .Add<Quantum.TrackedStates>(Quantum.TrackedStates.Serialize, null, Quantum.TrackedStates.OnRemoved,
            ComponentFlags.None)
          .Add<Quantum.TrainingGlobals>(Quantum.TrainingGlobals.Serialize, null, Quantum.TrainingGlobals.OnRemoved,
            ComponentFlags.Singleton)
          .Add<UTAgent>(UTAgent.Serialize, UTAgent.OnAdded, UTAgent.OnRemoved, ComponentFlags.None);

          InitModComponentTypeIdGen(ref componentTypeId);

          componentTypeId.Finish();*/
        }
    }

    public unsafe partial class Frame {
        /*partial void CustInitGen() {
            var tList = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .Where(t =>  t.IsClass && typeof(IModQuantumData).IsAssignableFrom(t));
          
            foreach (var t in tList) {
                var bClass = (IModQuantumData)Activator.CreateInstance(t);
                bClass.InitGen(this);
            }
        }*/
    }
}
