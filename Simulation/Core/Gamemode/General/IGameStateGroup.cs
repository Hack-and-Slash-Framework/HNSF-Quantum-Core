namespace Quantum
{
    public interface IGameStateGroup { }
    public interface IGameStateGroupLobby : IGameStateGroup { }
    public interface IGameStateGroupPregame : IGameStateGroup { }
    public interface IGameStateGroupMapIntro : IGameStateGroup { }
    public interface IGameStateGroupCharacterIntro : IGameStateGroup { }
    public interface IGameStateGroupCountdown : IGameStateGroup { }
    public interface IGameStateGroupGame : IGameStateGroup { }
    public interface IGameStateGroupOutro : IGameStateGroup { }
    public interface IGameStateGroupVictory : IGameStateGroup { }
    public interface IGameStateGroupPostgame : IGameStateGroup { }
}