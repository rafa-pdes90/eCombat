using System.Windows.Controls;

namespace eCombat.Model
{
    public class BoardPiece : Button
    {
        public string PowerLevel { get; set; }
        public int MoveLevel { get; set; }
        public bool PowerLevelIsPublic { get; set; }
        public bool IsEnemy { get; set; }

        public BoardPiece() { }

        public BoardPiece(string powerLevel, int moveLevel = 1, bool powerLevelIsPublic = true, bool isEnemy = false)
        {
            this.PowerLevel = powerLevel;
            this.MoveLevel = moveLevel;
            this.PowerLevelIsPublic = powerLevelIsPublic;
            this.IsEnemy = isEnemy;
        }
    }
}
