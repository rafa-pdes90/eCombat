using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class Board : ObservableObject
    {
        public static readonly Board Instance;

        public static BoardField[,] Layout { get; set; }

        static Board()
        {
            Instance = new Board();

            Layout = BoardField.New2DBoardFieldArray(10, 10);
        }
    }
}
