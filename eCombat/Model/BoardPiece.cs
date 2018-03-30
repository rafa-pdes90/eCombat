using System.Windows.Controls;

namespace eCombat.Model
{
    public class BoardPiece : Button
    {
        public string PowerLevel { get; set; }
        public int MoveLevel { get; set; }
        public bool PowerLevelIsPublic { get; set; }
        public bool IsOpponent { get; }

        public BoardPiece() { }

        public BoardPiece(string powerLevel, int moveLevel = 1, bool isOpponent = false)
        {
            this.PowerLevel = powerLevel;
            this.MoveLevel = moveLevel;
            this.PowerLevelIsPublic = !isOpponent;
            this.IsOpponent = isOpponent;
        }

        public void Reset()
        {
            if (this.IsOpponent)
            {
                this.PowerLevelIsPublic = false;
            }
        }
    }
}
