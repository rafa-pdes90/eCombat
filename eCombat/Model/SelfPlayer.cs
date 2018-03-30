using System.Collections.ObjectModel;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class SelfPlayer : ObservableObject
    {
        public static readonly SelfPlayer Instance;

        public static ObservableCollection<BoardPiece> Army { get; set; }
        public static string Name { get; set; }
        public static string Id { get; set; }
        public static Brush Color { get; set; }

        static SelfPlayer()
        {
            Instance = new SelfPlayer();
            Army = BoardPiece.NewUnitlist();
        }
    }
}
