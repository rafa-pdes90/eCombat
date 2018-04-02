using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class Opponent : ObservableObject
    {
        public static readonly Opponent Instance;

        private static readonly List<BoardPiece> AssetList;

        public static IEnumerable<BoardPiece> Army
        {
            get
            {
                foreach (BoardPiece piece in AssetList)
                {
                    yield return piece;
                }
            }
        }

        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _name;

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get => this._name;
            set => Set(() => this.Name, ref this._name, value);
        }

        /// <summary>
        /// The <see cref="IsPlayer2" /> property's name.
        /// </summary>
        public const string IsPlayer2PropertyName = "IsPlayer2";

        private bool _isPlayer2;

        /// <summary>
        /// Sets and gets the IsPlayer2 property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPlayer2
        {
            get => this._isPlayer2;
            set => Set(() => this.IsPlayer2, ref this._isPlayer2, value);
        }

        static Opponent()
        {
            Instance = new Opponent();

            AssetList = BoardPiece.NewOpponentList();
        }
    }
}
