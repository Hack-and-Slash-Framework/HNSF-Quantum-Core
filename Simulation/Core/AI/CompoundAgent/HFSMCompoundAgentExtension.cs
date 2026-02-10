using System.Collections;
using System.Collections.Generic;
using Quantum.Core;

namespace Quantum
{
    public unsafe partial struct HFSMCompoundAgent : IBotSDKDebugInfoProvider
    {
        public DelegateGetDebugInfo GetDebugInfo()
        {
            return GetDebugInfoList;
        }

        private static IEnumerator<IBotSDKDebugInfo> GetDebugInfoList(
            FrameBase frame,
            EntityRef entity,
            void* ptr)
        {
            var compoundAgent = frame.Get<HFSMCompoundAgent>(entity);

            var iterator = new CompoundAgentsIterator();
            iterator.Initialize();
            iterator.Add(compoundAgent.Brain, compoundAgent.BrainBb);
            if (compoundAgent.Action.Data.Root.IsValid)
                iterator.Add(compoundAgent.Action, compoundAgent.ActionBb);
            return iterator;
        }
    }

    public class CompoundAgentsIterator : IEnumerator<IBotSDKDebugInfo>
    {
        private int _index = -1;
        private int _count;
        private HFSMAgent[] _agents;
        private AIBlackboardComponent[] _blackboards;

        public void Initialize()
        {
            Reset();
            _agents = new HFSMAgent[2];
            _blackboards = new AIBlackboardComponent[2];
            _count = 0;
        }

        public void Add(HFSMAgent hfsmAgent, AIBlackboardComponent blackboardComponent)
        {
            if (_count < _agents.Length)
            {
                _agents[_count] = hfsmAgent;
                _blackboards[_count] = blackboardComponent;
                _count++;
            }
        }
        

        public IBotSDKDebugInfo Current
        {
            get
            {
                if (_index >= 0 && _index < _count)
                    return new BotSDKDebugInfoHFSM
                    {
                        HFSMAgent = _agents[_index],
                        DebuggedBlackboardComponent = _blackboards[_index]
                    };
                return null;
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            ++_index;
            return _index < _count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
