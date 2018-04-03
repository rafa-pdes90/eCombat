using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace eCombat.Model
{
    public sealed class BoardPiece : ObservableObject
    {
        /// <summary>
        /// The <see cref="Column" /> property's name.
        /// </summary>
        public const string ColumnPropertyName = "Column";

        private int _column;

        /// <summary>
        /// Sets and gets the Column property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Column
        {
            get => this._column;
            set => Set(() => this.Column, ref this._column, value);
        }

        /// <summary>
        /// The <see cref="Row" /> property's name.
        /// </summary>
        public const string RowPropertyName = "Row";

        private int _row;

        /// <summary>
        /// Sets and gets the Row property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int Row
        {
            get => this._row;
            set => Set(() => this.Row, ref this._row, value);
        }

        /// <summary>
        /// The <see cref="PowerLevel" /> property's name.
        /// </summary>
        public const string PowerLevelPropertyName = "PowerLevel";

        private string _powerLevel;

        /// <summary>
        /// Sets and gets the PowerLevel property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string PowerLevel
        {
            get => this._powerLevel;
            set => Set(() => this.PowerLevel, ref this._powerLevel, value);
        }

        /// <summary>
        /// The <see cref="IsAlive" /> property's name.
        /// </summary>
        public const string IsAlivePropertyName = "IsAlive";

        private bool _isAlive;

        /// <summary>
        /// Sets and gets the IsAlive property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsAlive
        {
            get => this._isAlive;
            set => Set(() => this.IsAlive, ref this._isAlive, value);
        }

        /// <summary>
        /// The <see cref="IsSelected" /> property's name.
        /// </summary>
        public const string IsSelectedPropertyName = "IsSelected";

        private bool _isSelected;

        /// <summary>
        /// Sets and gets the IsSelected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelected
        {
            get => this._isSelected;
            set => Set(() => this.IsSelected, ref this._isSelected, value);
        }

        /// <summary>
        /// The <see cref="IsMoving" /> property's name.
        /// </summary>
        public const string IsMovingPropertyName = "IsMoving";

        private bool _isMoving;

        /// <summary>
        /// Sets and gets the IsMoving property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMoving
        {
            get => this._isMoving;
            set => Set(() => this.IsMoving, ref this._isMoving, value);
        }

        /// <summary>
        /// The <see cref="IsDefeating" /> property's name.
        /// </summary>
        public const string IsDefeatingPropertyName = "IsDefeating";

        private bool _isDefeating;

        /// <summary>
        /// Sets and gets the IsDefeating property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsDefeating
        {
            get => this._isDefeating;
            set => Set(() => this.IsDefeating, ref this._isDefeating, value);
        }

        /// <summary>
        /// The <see cref="IsBeingDefeated" /> property's name.
        /// </summary>
        public const string IsBeingDefeatedPropertyName = "IsBeingDefeated";

        private bool _isBeingDefeated;

        /// <summary>
        /// Sets and gets the IsBeingDefeated property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsBeingDefeated
        {
            get => this._isBeingDefeated;
            set => Set(() => this.IsBeingDefeated, ref this._isBeingDefeated, value);
        }

        /// <summary>
        /// The <see cref="IsSelectable" /> property's name.
        /// </summary>
        public const string IsSelectablePropertyName = "IsSelectable";

        private bool _isSelectable;

        /// <summary>
        /// Sets and gets the IsSelectable property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelectable
        {
            get => this._isSelectable;
            set => Set(() => this.IsSelectable, ref this._isSelectable, value);
        }

        /// <summary>
        /// The <see cref="IsPowerLevelPublic" /> property's name.
        /// </summary>
        public const string IsPowerLevelPublicPropertyName = "IsPowerLevelPublic";

        private bool _isPowerLevelPublic;

        /// <summary>
        /// Sets and gets the IsPowerLevelPublic property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsPowerLevelPublic
        {
            get => this._isPowerLevelPublic;
            set => Set(() => this.IsPowerLevelPublic, ref this._isPowerLevelPublic, value);
        }

        public bool IsOpponent { get; }
        public int MoveLevel { get; }


        public BoardPiece(string powerLevel = null, int moveLevel = 1)
        {
            this.PowerLevel = powerLevel;
            this.MoveLevel = moveLevel;
            this.IsOpponent = moveLevel < 0;
        }

        public void Reset()
        {
            this.IsAlive = false;
            this.IsSelected = false;
            this.IsMoving = false;
            this.IsDefeating = false;
            this.IsBeingDefeated = false;
            this.IsSelectable = this.MoveLevel >= 0;
            this.IsPowerLevelPublic = !this.IsOpponent;
            if (!this.IsPowerLevelPublic)
            {
                this.PowerLevel = null;
            }
        }

        public static List<BoardPiece> NewOpponentList()
        {
            var opponentList = new List<BoardPiece>();

            for (int i = 0; i < 40; i++)
            {
                var newOpponent = new BoardPiece(moveLevel: -1);
                opponentList.Add(newOpponent);
            }

            return opponentList;
        }

        public static List<BoardPiece> NewUnitList()
        {
            var unitList = new List<BoardPiece>();

            for (int i = 0; i < 1; i++)
            {
                var novoPrisioneiro = new BoardPiece(powerLevel: "*", moveLevel: 0);
                unitList.Add(novoPrisioneiro);
            }

            for (int i = 0; i < 6; i++)
            {
                var novaBomba = new BoardPiece(powerLevel: "0", moveLevel: 0);
                unitList.Add(novaBomba);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoMarechal = new BoardPiece(powerLevel: "10");
                unitList.Add(novoMarechal);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoGeneral = new BoardPiece(powerLevel: "9");
                unitList.Add(novoGeneral);
            }

            for (int i = 0; i < 2; i++)
            {
                var novoCoronel = new BoardPiece(powerLevel: "8");
                unitList.Add(novoCoronel);
            }

            for (int i = 0; i < 3; i++)
            {
                var novoMajor = new BoardPiece(powerLevel: "7");
                unitList.Add(novoMajor);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoCapitao = new BoardPiece(powerLevel: "6");
                unitList.Add(novoCapitao);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoTenente = new BoardPiece(powerLevel: "5");
                unitList.Add(novoTenente);
            }

            for (int i = 0; i < 4; i++)
            {
                var novoSargento = new BoardPiece(powerLevel: "4");
                unitList.Add(novoSargento);
            }

            for (int i = 0; i < 5; i++)
            {
                var novoCaboArmeiro = new BoardPiece(powerLevel: "3");
                unitList.Add(novoCaboArmeiro);
            }

            for (int i = 0; i < 8; i++)
            {
                var novoSoldado = new BoardPiece(powerLevel: "2", moveLevel: 9);
                unitList.Add(novoSoldado);
            }

            for (int i = 0; i < 1; i++)
            {
                var novoEspiao = new BoardPiece(powerLevel: "1");
                unitList.Add(novoEspiao);
            }

            return unitList;
        }
    }
}
