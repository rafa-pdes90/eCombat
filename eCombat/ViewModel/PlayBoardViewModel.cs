using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using eCombat.Extensions;
using eCombat.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;

namespace eCombat.ViewModel
{
    public class PlayBoardViewModel : ViewModelBase
    {
        public SelfPlayer SelfPlayer => SelfPlayer.Instance;

        public Opponent Opponent => Opponent.Instance;

        /// <summary>
        /// The <see cref="IsOpponentTurn" /> property's name.
        /// </summary>
        public const string IsOpponentTurnPropertyName = "IsOpponentTurn";

        private bool _isOpponentTurn;

        /// <summary>
        /// Sets and gets the IsOpponentTurn property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsOpponentTurn
        {
            get => this._isOpponentTurn;
            set => Set(() => this.IsOpponentTurn, ref this._isOpponentTurn, value);
        }

        /// <summary>
        /// The <see cref="IsMatchOn" /> property's name.
        /// </summary>
        public const string IsMatchOnPropertyName = "IsMatchOn";

        private bool _isMatchOn;

        /// <summary>
        /// Sets and gets the IsMatchOn property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsMatchOn
        {
            get => this._isMatchOn;
            set => Set(() => this.IsMatchOn, ref this._isMatchOn, value);
        }

        private RelayCommand _randomOrStartCommand;

        /// <summary>
        /// Gets the RandomOrStartCommand.
        /// </summary>
        public RelayCommand RandomOrStartCommand =>
            this._randomOrStartCommand ??
            (this._randomOrStartCommand = new RelayCommand(RandomOrStartMethod,
                () => !this.IsMatchOn));

        private RelayCommand<BoardPiece> _pieceClickCommand;

        /// <summary>
        /// Gets the PieceClickCommand.
        /// </summary>
        public RelayCommand<BoardPiece> PieceClickCommand =>
            this._pieceClickCommand ??
            (this._pieceClickCommand = new RelayCommand<BoardPiece>(PieceClickMethod,
                piece => piece != null && !piece.IsSelected && piece.IsSelectable));

        private RelayCommand<BoardField> _fieldClickDownCommand;

        /// <summary>
        /// Gets the FieldClickDownCommand.
        /// </summary>
        public RelayCommand<BoardField> FieldClickDownCommand =>
            this._fieldClickDownCommand ??
            (this._fieldClickDownCommand = new RelayCommand<BoardField>(FieldClickDownMethod,
                field => field != null && (field.IsEnabled || field.IsAttackable)));

        public ObservableCollection<BoardPiece> SelfArmy { get; set; }

        public BoardPiece LastSelectedPiece { get; private set; }
        public BoardPiece LastMovedPiece { get; private set; }
        public BoardField LastMovedToField { get; private set; }

        private Queue<BoardPiece> LastFighters { get; }
        private Queue<Tuple<int, int>> MoveLog { get; }
        private List<BoardField> EnabledFields { get; }


        public PlayBoardViewModel()
        {
            Messenger.Default.Register<int>(this, "LoadBoard", token => LoadBoard());
            Messenger.Default.Register<bool>(this, "SetMatchTurn", SetMatchTurn);
            Messenger.Default.Register<int[]>(this, "MoveBoardPiece", InvokePieceMoveMethod);
            Messenger.Default.Register<int>(this, "MovePieceCompleted", token => CompletePieceMove());
            Messenger.Default.Register<object[]>(this, "AttackBoardPiece", InvokePieceAttackMethod);
            Messenger.Default.Register<int>(this, "AttackPieceCompleted", token => CompletePieceAttack());
            Messenger.Default.Register<int>(this, "SoftReset", token => SoftReset());
            
            this.EnabledFields = new List<BoardField>();
            this.LastFighters = new Queue<BoardPiece>(2);
            this.MoveLog = new Queue<Tuple<int, int>>(3);

            SoftReset();
        }

        private void SoftReset()
        {
            ResetLayout();

            this.SelfArmy = new ObservableCollection<BoardPiece>(SelfPlayer.Army);
            
            this.IsMatchOn = false;
            this.LastSelectedPiece = null;
            this.LastMovedPiece = null;
            this.LastMovedToField = null;
            this.EnabledFields.Clear();
            this.LastFighters.Clear();
            this.MoveLog.Clear();
        }

        private static void ResetLayout()
        {
            foreach (BoardField field in Board.Layout)
            {
                field.Reset();
            }

            Board.Layout[2, 4].IsWalkable = false;
            Board.Layout[3, 4].IsWalkable = false;
            Board.Layout[6, 4].IsWalkable = false;
            Board.Layout[7, 4].IsWalkable = false;
            Board.Layout[2, 5].IsWalkable = false;
            Board.Layout[3, 5].IsWalkable = false;
            Board.Layout[6, 5].IsWalkable = false;
            Board.Layout[7, 5].IsWalkable = false;

            foreach (BoardPiece piece in SelfPlayer.Army)
            {
                piece.Reset();
            }
        }

        private static void LoadBoard()
        {
            int c = 0;
            int r = 0;

            foreach (BoardPiece piece in Opponent.Army)
            {
                BoardField field = Board.Layout[c, r];

                piece.Column = c;
                piece.Row = r;
                piece.IsAlive = true;
                field.PieceOnTop = piece;

                if (c == 9)
                {
                    c = 0;
                    r++;
                }
                else
                {
                    c++;
                }
            }
        }

        private void SetMatchTurn(bool isOpponentTurn)
        {
            this.IsOpponentTurn = isOpponentTurn;
        }

        private void RandomOrStartMethod()
        {
            if (this.SelfArmy.Count == 0)
            {
                foreach (BoardPiece piece in SelfPlayer.SpecialUnits)
                {
                    piece.IsSelectable = false;
                }

                this.IsMatchOn = true;
            }
            else
            {
                this.SelfArmy.Shuffle();

                int c = 0;
                int r = 9;
                foreach (BoardPiece piece in this.SelfArmy)
                {
                    for (int i = c; i < 10; i++)
                    {
                        for (int j = r; j > 5; j--)
                        {
                            BoardField field = Board.Layout[i, j];

                            if (field.PieceOnTop != null) continue;

                            piece.Column = i;
                            piece.Row = j;
                            piece.IsAlive = true;
                            field.PieceOnTop = piece;

                            c = i;
                            r = j - 1;

                            i = 10;
                            break;
                        }

                        if (i < 10)
                        {
                            r = 9;
                        }
                    }
                }

                this.SelfArmy.Clear();
            }
        }

        private void ClearLastSelection()
        {
            if (this.LastSelectedPiece != null)
            {
                this.LastSelectedPiece.IsSelected = false;
            }

            if (this.EnabledFields.Count == 0) return;

            foreach (BoardField field in this.EnabledFields)
            {
                field.IsEnabled = false;
                field.IsAttackable = false;
            }

            this.EnabledFields.Clear();
        }

        private void PieceClickMethod(BoardPiece piece)
        {
            ClearLastSelection();

            var log = new List<Tuple<int, int>>();

            if (piece.Equals(this.LastMovedPiece))
            {
                if (this.MoveLog.Count == 3)
                {
                    List<Tuple<int, int>> distinctMoveLog = this.MoveLog.Distinct().ToList();
                    if (distinctMoveLog.Count < 3)
                    {
                        log = distinctMoveLog;
                    }
                }
            }

            EvalMoveField(piece.Column, piece.Row, 1, 0, piece.MoveLevel, log);
            EvalMoveField(piece.Column, piece.Row, -1, 0, piece.MoveLevel, log);
            EvalMoveField(piece.Column, piece.Row, 0, 1, piece.MoveLevel, log);
            EvalMoveField(piece.Column, piece.Row, 0, -1, piece.MoveLevel, log);

            piece.IsSelected = true;
            this.LastSelectedPiece = piece;
        }

        private void EvalMoveField(int srcC, int srcR, int incC, int incR, int moveLevelOriginal, ICollection<Tuple<int, int>> tempMoveLog)
        {
            int moveLevel = moveLevelOriginal;
            srcC += incC;
            srcR += incR;

            while (srcC >= 0 && srcC < 10 && srcR >= 0 && srcR < 10)
            {
                BoardField field = Board.Layout[srcC, srcR];

                if (!field.IsWalkable) return;

                if (field.PieceOnTop == null)
                {
                    moveLevel -= 1;
                    srcC += incC;
                    srcR += incR;

                    var coords = new Tuple<int, int>(srcC, srcR);

                    if (tempMoveLog.Contains(coords))
                    {
                        if (moveLevel <= 0) return;
                        continue;
                    }

                    field.IsEnabled = true;
                }
                else
                {
                    if (!field.PieceOnTop.IsOpponent || (moveLevelOriginal > 1 && moveLevel < 9)) return;

                    moveLevel = 0;
                    field.IsAttackable = true;
                }

                this.EnabledFields.Add(field);

                if (moveLevel <= 0) return;
            }
        }

        private void FieldClickDownMethod(BoardField field)
        {
            this.IsOpponentTurn = true;

            ClearLastSelection();

            int unitColumn = this.LastSelectedPiece.Column;
            int unitRow = this.LastSelectedPiece.Row;

            if (field.PieceOnTop != null)
            {
                GameMaster.Client.AttackBoardPieceAsync(unitColumn, unitRow, field.Column, field.Row,
                    this.LastSelectedPiece.PowerLevel);
            }
            else
            {
                GameMaster.Client.MoveBoardPieceAsync(unitColumn, unitRow, field.Column, field.Row);
            }

            var movingToField = new Tuple<int, int>(field.Column, field.Row);

            if (!this.LastSelectedPiece.Equals(this.LastMovedPiece))
            {
                this.MoveLog.Clear();
            }
            else
            {
                if (this.MoveLog.Count == 3)
                    this.MoveLog.Dequeue();
            }

            this.MoveLog.Enqueue(movingToField);

            this.LastMovedPiece = this.LastSelectedPiece;
        }

        private void InvokePieceMoveMethod(int[] values)
        {
            StartPieceMove(values[0], values[1], values[2], values[3]);
        }

        private void StartPieceMove(int srcX, int srcY, int destX, int destY)
        {
            BoardField oldField = Board.Layout[srcX, srcY];
            BoardField newField = Board.Layout[destX, destY];
            BoardPiece piece = oldField.PieceOnTop;

            oldField.PieceOnTop = null;
            this.LastMovedToField = newField;

            piece.IsMoving = true;
            this.LastMovedPiece = piece;
        }

        private void CompletePieceMove()
        {
            this.LastMovedPiece.IsMoving = false;
            this.LastMovedPiece.Column = this.LastMovedToField.Column;
            this.LastMovedPiece.Row = this.LastMovedToField.Row;
            this.LastMovedToField.PieceOnTop = this.LastMovedPiece;
        }

        private void InvokePieceAttackMethod(object[] values)
        {
            StartPieceAttack((int)values[0], (int)values[1], (int)values[2], (int)values[3],
                (string)values[4], (string)values[5]);
        }

        private void StartPieceAttack(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel, string defenderPowerLevel)
        {
            BoardPiece attacker = Board.Layout[srcX, srcY].PieceOnTop;
            BoardPiece defender = Board.Layout[destX, destY].PieceOnTop;

            this.LastFighters.Enqueue(attacker);
            this.LastFighters.Enqueue(defender);

            attacker.PowerLevel = attackerPowerLevel;
            defender.PowerLevel = defenderPowerLevel;
            
            int.TryParse(attacker.PowerLevel, out int attackerPower);
            bool isBandeira = !int.TryParse(defender.PowerLevel, out int defenderPower);

            if (defenderPower == attackerPower || defenderPower == 0 && attackerPower != 3)
            {
                attacker.IsBeingDefeated = true;
                defender.IsBeingDefeated = true;
            }
            else
            {
                BoardPiece winner;
                BoardPiece loser;

                if (defenderPower == 10 && attackerPower == 1 ||
                    defenderPower == 0 && attackerPower == 3 ||
                    attackerPower > defenderPower)
                {
                    winner = attacker;
                    loser = defender;
                }
                else
                {
                    winner = defender;
                    loser = attacker;
                }

                winner.IsDefeating = true;
                loser.IsBeingDefeated = true;

                if (winner.Equals(attacker))
                {
                    Messenger.Default.Send(new[] { srcX, srcY, destX, destY }, "MoveBoardPiece");
                }
            }

            if (!isBandeira) return;

            string finResult = this.IsOpponentTurn ? "Victory" : "Defeat";
            Messenger.Default.Send(finResult, "EndMatchResult");
        }

        private void CompletePieceAttack()
        {
            while (this.LastFighters.Count > 0)
            {
                BoardPiece piece = this.LastFighters.Dequeue();

                if (!piece.IsPowerLevelPublic)
                {
                    piece.PowerLevel = null;
                }

                if (piece.IsBeingDefeated)
                {
                    piece.IsBeingDefeated = false;
                    piece.IsAlive = false;
                    piece.IsSelectable = false;
                    Board.Layout[piece.Column, piece.Row].PieceOnTop = null;
                }
                else
                {
                    piece.IsDefeating = false;
                }
            }
        }
    }
}
