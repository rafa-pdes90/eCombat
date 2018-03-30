using System.Collections.ObjectModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class Opponent : ObservableObject
    {
        public static readonly Opponent Instance;

        public static ObservableCollection<BoardPiece> Army { get; set; }
        public static string Name { get; set; }
        public static string Id { get; set; }
        public static Brush Color { get; set; }

        static Opponent()
        {
            Instance = new Opponent();
            Army = BoardPiece.NewOpponentList();
        }
    }
}
