using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class BoardField : ObservableObject
    {
        /// <summary>
        /// The <see cref="IsEnabled" /> property's name.
        /// </summary>
        public const string IsEnabledPropertyName = "IsEnabled";

        private bool _isEnabled;

        /// <summary>
        /// Sets and gets the IsEnabled property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsEnabled
        {
            get => this._isEnabled;
            set => Set(() => this.IsEnabled, ref this._isEnabled, value);
        }

        /// <summary>
        /// The <see cref="IsAttackable" /> property's name.
        /// </summary>
        public const string IsAttackablePropertyName = "IsAttackable";

        private bool _isAttackable;

        /// <summary>
        /// Sets and gets the IsAttackable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAttackable
        {
            get => this._isAttackable;
            set => Set(() => this.IsAttackable, ref this._isAttackable, value);
        }

        /// <summary>
        /// The <see cref="IsWalkable" /> property's name.
        /// </summary>
        public const string IsWalkablePropertyName = "IsWalkable";

        private bool _isWalkable;

        /// <summary>
        /// Sets and gets the IsWalkable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsWalkable
        {
            get => this._isWalkable;
            set => Set(() => this.IsWalkable, ref this._isWalkable, value);
        }

        public BoardPiece PieceOnTop { get; set; }

        public int Column { get; }
        public int Row { get; }

        private BoardField(int c, int r)
        {
            this.Column = c;
            this.Row = r;
        }

        public void Reset()
        {
            this.IsEnabled = false;
            this.IsAttackable = false;
            this.IsWalkable = true;

            this.PieceOnTop = null;
        }

        public static BoardField[,] New2DBoardFieldArray(int c, int r)
        {
            var array = new BoardField[c, r];

            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    array[i, j] = new BoardField(i, j);
                }
            }

            return array;
        }
    }
}
