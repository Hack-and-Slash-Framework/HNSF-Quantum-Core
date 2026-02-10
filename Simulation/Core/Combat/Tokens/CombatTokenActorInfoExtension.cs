namespace Quantum
{
    public unsafe partial struct CombatTokenActorInfo
    {
        public int Score(Frame frame)
        {
            var tokenList = frame.ResolveList(currentTokens);
            int score = 0;

            for (int i = 0; i < tokenList.Count; i++)
            {
                if (!frame.Unsafe.TryGetPointer<AttackToken>(tokenList[i], out var token))
                {
                    score += token->score;
                }
            }
            return score;
        }

        public void CleanupInvalidTokens(Frame frame)
        {
            var tokenList = frame.ResolveList(currentTokens);

            for (int i = tokenList.Count - 1; i >= 0; i--)
            {
                if (!frame.Exists(tokenList[i]))
                {
                    tokenList.RemoveAt(i);
                }
            }
        }

        public void RemoveToken(Frame frame, EntityRef tokenEntityRef)
        {
            var tokenList = frame.ResolveList(currentTokens);

            for (int i = tokenList.Count - 1; i >= 0; i--)
            {
                if (tokenList[i] != tokenEntityRef) continue;
                tokenList.RemoveAt(i);
                break;
            }
            frame.Destroy(tokenEntityRef);
        }
    }
}
