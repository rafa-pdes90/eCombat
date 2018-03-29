using System.Windows.Controls;

namespace eCombat.Model
{
    public class BoardPiece : Button
    {
        public string PowerLevel { get; set; }
        public int MoveLevel { get; set; }
        public bool PowerLevelIsPublic { get; set; }
        public bool IsEnemy { get; }

        public BoardPiece() { }

        public BoardPiece(string powerLevel, int moveLevel = 1, bool isEnemy = false)
        {
            this.PowerLevel = powerLevel;
            this.MoveLevel = moveLevel;
            this.PowerLevelIsPublic = !isEnemy;
            this.IsEnemy = isEnemy;
        }
    }
}
