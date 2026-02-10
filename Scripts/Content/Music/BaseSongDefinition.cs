using Cysharp.Threading.Tasks;

namespace HnSF
{
    public abstract class BaseSongDefinition : IContentDefinition
    {
        public abstract SongAudio GetSong();
    }
}