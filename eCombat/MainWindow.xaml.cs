using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using eCombat.Model;
using eCombat.View;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Messaging;

namespace eCombat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        private MainViewModel Vm => (MainViewModel)DataContext;

        public MetroWindow DialogWindow { get; set; }

        private bool KeepOn { get; set; }
        
        private BoardPiece LastMovedUnit { get; set; }
        private BoardPiece LastMovedPiece { get; set; }
        private BoardPiece LastSelectedUnit { get; set; }
        private Rectangle LastMovedCell { get; set; }
        private Queue<BoardPiece> Fighters { get; }
        private List<Rectangle> EnabledFields { get; }
        private Queue<Tuple<int, int>> MoveLog { get; }
        private IEnumerable<UIElement> CombateGridChildren { get; set; }

        public ICommand BoardPieceSelectCommand { get; }

        public MainWindow()
        {
            InitializeComponent();

            this.Closing += MainWindow_Closing;

            BoardPieceSelectCommand = new RelayCommand<BoardPiece>(BoardPieceSelectMethod);

            this.DialogWindow = null;
            this.KeepOn = false;

            this.Fighters = new Queue<BoardPiece>();
            this.EnabledFields = new List<Rectangle>();
            this.MoveLog = new Queue<Tuple<int, int>>(3);

            this.LogScrollViewer.ScrollToEnd();

            Messenger.Default.Register<int>(this, "ResetAll", token => ResetAll());
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.Vm.IsConnecting)
            {
                this.Vm.IsPlaying = false;
                GameMaster.Client.CancelMatch();
            }

            GameMaster.Client.LeaveGame();

            ViewModelLocator.Cleanup();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            LoadDialogWindow(new ConnectionWindow());

            if (!this.KeepOn) return;

            this.KeepOn = false;
            LoadCombateUIElements();
        }

        public void LoadDialogWindow(MetroWindow dialog)
        {
            this.DialogWindow = dialog;

            //this.Effect = new BlurEffect();

            dialog.Owner = this;
            dialog.ShowDialog();

            if (this.KeepOn)
            {
                this.Effect = null;
            }
            else
            {
                this.Close();
            }
        }

        public void StartNewMatch()
        {
            this.Vm.IsConnecting = false;
            this.Vm.IsPlaying = true;
            this.KeepOn = true;
            this.DialogWindow.Close();
        }

        // ReSharper disable once InconsistentNaming
        private void LoadCombateUIElements()
        {
            void AddNewCell(int r, int c)
            {
                var newCell = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent,
                };
                newCell.MouseLeftButtonDown += CellMouseLeftButtonDown;

                this.CombateGrid.Children.Add(newCell);
                Grid.SetRow(newCell, r);
                Grid.SetColumn(newCell, c);
                Panel.SetZIndex(newCell, 1);
            }

            void AddNewBoardPiece(int r, int c, Brush color, BoardPiece unit)
            {
                unit.Background = color;
                unit.BorderThickness = new Thickness(3);
                unit.BorderBrush = Brushes.Black;
                unit.FontWeight = FontWeights.Bold;
                unit.Style = FindResource("MetroCircleButtonStyle") as Style;

                var content = new Binding("PowerLevel")
                {
                    Source = unit
                };
                BindingOperations.SetBinding(unit, ContentProperty, content);

                if (unit.IsOpponent)
                {
                    unit.IsHitTestVisible = false;
                }

                if (unit.PowerLevel == "0" || unit.PowerLevel == "*")
                {
                    unit.IsEnabled = false;
                }
                else
                {
                    unit.Command = this.BoardPieceSelectCommand;
                    unit.CommandParameter = unit;
                }

                this.CombateGrid.Children.Add(unit);
                Grid.SetRow(unit, r);
                Grid.SetColumn(unit, c);
                Panel.SetZIndex(unit, 2);
            }

            List<BoardPiece> unitList = this.Vm.UnitList;
            List<BoardPiece> opponentList = this.Vm.OpponentList;

            int index = 0;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 9; c >= 0; c--)
                {
                    AddNewCell(r, c);
                    AddNewBoardPiece(r, c, this.Vm.OpponentColor, opponentList[index]);
                    index += 1;
                }
            }

            for (int r = 4; r < 6; r++)
            {
                for (int c = 0; c < 10; c++)
                {
                    AddNewCell(r, c);
                }
            }

            index = 0;
            for (int r = 9; r >= 6; r--)
            {
                for (int c = 0; c < 10; c++)
                {
                    AddNewCell(r, c);
                    AddNewBoardPiece(r, c, this.Vm.SelfColor, unitList[index]);
                    index += 1;
                }
            }

            this.CombateGridChildren = this.CombateGrid.Children.Cast<UIElement>();
        }

        private void ClearLastSelection()
        {
            foreach (Rectangle rect in this.EnabledFields)
            {
                rect.Fill = Brushes.Transparent;
            }

            this.EnabledFields.Clear();

            if (this.LastSelectedUnit != null)
                this.LastSelectedUnit.Effect = null;
        }

        private IEnumerable<UIElement> GetCombateGridChildren(int x, int y)
        {
            return this.CombateGridChildren.Where(e =>
                Grid.GetColumn(e) == x && Grid.GetRow(e) == y);
        }

        private void BoardPieceSelectMethod(BoardPiece unit)
        {
            void EvalMoveField(int r, int c, int incR, int incC, int moveLevelOriginal,
                ICollection<Tuple<int, int>> tempMoveLog)
            {
                int moveLevel = moveLevelOriginal;
                r += incR;
                c += incC;

                while (r >= 0 && r < 10 && c >= 0 && c < 10)
                {
                    var coords = new Tuple<int, int>(c, r);
                    if (tempMoveLog.Contains(coords))
                    {
                        moveLevel -= 1;
                        if (moveLevel == 0) break;
                        c += incC;
                        r += incR;
                        continue;
                    }

                    Rectangle moveRect = null;
                    bool hasOpponent = false;

                    foreach (UIElement element in GetCombateGridChildren(c, r))
                    {
                        switch (element)
                        {
                            case Rectangle rectangle:
                                moveRect = rectangle;
                                break;
                            case BoardPiece piece:
                                if (!piece.IsOpponent || (moveLevelOriginal > 1 && moveLevel < 9))
                                    return;
                                hasOpponent = true;
                                break;
                            default:
                                return;
                        }
                    }

                    if (moveRect == null) continue;
                    this.EnabledFields.Add(moveRect);
                    if (hasOpponent)
                    {
                        moveRect.Fill = Brushes.Red;
                        return;
                    }
                    else
                    {
                        moveRect.Fill = Brushes.SpringGreen;
                        moveLevel -= 1;
                        if (moveLevel <= 0) return;
                        r += incR;
                        c += incC;
                    }
                }
            }

            ClearLastSelection();

            var log = new List<Tuple<int, int>>();

            if (unit.Equals(this.LastMovedUnit))
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

            unit.Effect = new DropShadowEffect();

            int unitRow = Grid.GetRow(unit);
            int unitColumn = Grid.GetColumn(unit);

            EvalMoveField(unitRow, unitColumn, 1, 0, unit.MoveLevel, log);
            EvalMoveField(unitRow, unitColumn, -1, 0, unit.MoveLevel, log);
            EvalMoveField(unitRow, unitColumn, 0, 1, unit.MoveLevel, log);
            EvalMoveField(unitRow, unitColumn, 0, -1, unit.MoveLevel, log);

            this.LastSelectedUnit = unit;
        }

        private void CellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rect = (Rectangle)e.Source;
            if (!this.EnabledFields.Contains(rect)) return;

            this.Vm.IsOpponentTurn = true;

            int unitRow = Grid.GetRow(this.LastSelectedUnit);
            int unitColumn = Grid.GetColumn(this.LastSelectedUnit);
            int cellRow = Grid.GetRow(rect);
            int cellColumn = Grid.GetColumn(rect);

            if (rect.Fill.Equals(Brushes.Red))
            {
                GameMaster.Client.AttackBoardPieceAsync(unitColumn, unitRow, cellColumn, cellRow,
                    this.LastSelectedUnit.PowerLevel);
            }
            else
            {
                GameMaster.Client.MoveBoardPieceAsync(unitColumn, unitRow, cellColumn, cellRow);
            }

            ClearLastSelection();

            var movingCell = new Tuple<int, int>(cellColumn, cellRow);

            if (!this.LastSelectedUnit.Equals(this.LastMovedUnit))
            {
                this.MoveLog.Clear();
            }
            else
            {
                if (this.MoveLog.Count == 3)
                    this.MoveLog.Dequeue();
            }

            this.MoveLog.Enqueue(movingCell);

            this.LastMovedUnit = this.LastSelectedUnit;
        }

        private void MoveAnimationCompleted(object sender, EventArgs e)
        {
            int cellRow = Grid.GetRow(this.LastMovedCell);
            int cellColumn = Grid.GetColumn(this.LastMovedCell);

            this.LastMovedPiece.RenderTransform = null;
            Grid.SetRow(this.LastMovedPiece, cellRow);
            Grid.SetColumn(this.LastMovedPiece, cellColumn);
        }

        public void MoveBoardPiece(int srcX, int srcY, int destX, int destY)
        {
            this.LastMovedPiece = GetCombateGridChildren(srcX, srcY).FirstOrDefault(x =>
                                      x is BoardPiece) as BoardPiece ?? new BoardPiece();
            
            this.LastMovedCell = GetCombateGridChildren(destX, destY).FirstOrDefault(x =>
                                     x is Rectangle) as Rectangle ?? new Rectangle();

            this.LastMovedPiece.RenderTransform = new TranslateTransform();
            if (destX == srcX)
            {
                int steps = destY - srcY;
                int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                double moveHeight = CombateGrid.RenderSize.Height / 10;
                var moveAnimation = new DoubleAnimation(moveHeight * steps, new TimeSpan(0, 0, 0, 0, tempo));
                moveAnimation.Completed += MoveAnimationCompleted;
                this.LastMovedPiece.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
            }
            else
            {
                int steps = destX - srcX;
                int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                double moveWidth = CombateGrid.RenderSize.Width / 10;
                var moveAnimation = new DoubleAnimation(moveWidth * steps, new TimeSpan(0, 0, 0, 0, tempo));
                moveAnimation.Completed += MoveAnimationCompleted;
                this.LastMovedPiece.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
            }
        }

        private void LoserAnimationCompleted(object sender, EventArgs e)
        {
            while (Fighters.Count > 0)
            {
                BoardPiece piece = Fighters.Dequeue();
                if (piece.Foreground.Equals(Brushes.Red))
                {
                    this.CombateGrid.Children.Remove(piece);
                }
                else
                {
                    if (!piece.PowerLevelIsPublic)
                    {
                        piece.PowerLevel = null;
                        piece.GetBindingExpression(ContentProperty)?.UpdateTarget();
                    }

                    piece.FontSize = 12;
                    piece.Foreground = Brushes.Black;
                }
            }
        }

        public async void AttackBoardPiece(int srcX, int srcY, int destX, int destY,
            string attackerPowerLevel, string defenderPowerLevel)
        {
            BoardPiece attacker = GetCombateGridChildren(srcX, srcY).FirstOrDefault(x =>
                                      x is BoardPiece) as BoardPiece ?? new BoardPiece();

            BoardPiece defender = GetCombateGridChildren(destX, destY).FirstOrDefault(x =>
                                      x is BoardPiece) as BoardPiece ?? new BoardPiece();

            Fighters.Enqueue(attacker);
            Fighters.Enqueue(defender);

            attacker.PowerLevel = attackerPowerLevel;
            defender.PowerLevel = defenderPowerLevel;
            attacker.GetBindingExpression(ContentProperty)?.UpdateTarget();
            defender.GetBindingExpression(ContentProperty)?.UpdateTarget();

            attacker.FontSize *= 2;
            defender.FontSize *= 2;

            var loserAnimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 1, 0));
            loserAnimation.Completed += LoserAnimationCompleted;

            int.TryParse(attacker.PowerLevel, out int attackerPower);
            bool isBandeira = !int.TryParse(defender.PowerLevel, out int defenderPower);

            if (defenderPower == attackerPower || defenderPower == 0 && attackerPower != 3)
            {
                defender.IsEnabled = true;
                defender.Foreground = Brushes.Red;
                attacker.Foreground = Brushes.Red;
                await Task.Delay(1500);
                attacker.BeginAnimation(FontSizeProperty, loserAnimation);
                defender.BeginAnimation(FontSizeProperty, loserAnimation);
                await Task.Delay(500);
            }
            else
            {
                BoardPiece winner;
                BoardPiece loser;

                if (defenderPower == 10 && attackerPower == 1 ||
                    defenderPower == 0 && attackerPower == 3||
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

                winner.Foreground = Brushes.Green;
                loser.Foreground = Brushes.Red;
                await Task.Delay(1500);
                loser.BeginAnimation(FontSizeProperty, loserAnimation);
                await Task.Delay(500);

                if (winner.Equals(attacker))
                {
                    MoveBoardPiece(srcX, srcY, destX, destY);
                }
            }

            if (!isBandeira) return;
            
            LoadDialogWindow(this.Vm.IsOpponentTurn
                ? new Fin("You win! Congratulations!!")
                : new Fin("You lose. Better luck next time!"));
        }

        public string ShowPowerLevel(int srcX, int srcY)
        {
            BoardPiece piece = GetCombateGridChildren(srcX, srcY).FirstOrDefault(x =>
                                      x is BoardPiece) as BoardPiece ?? new BoardPiece();
            return piece.PowerLevel;
        }

        public void ChatScrollToEnd()
        {
            //double pseudoEnd = this.ChatScrollViewer.ExtentHeight;
            //this.ChatScrollViewer.ScrollToVerticalOffset(pseudoEnd);
        }

        public void EvalPrematureMatchEnd(bool isWorthPoints)
        {
            if (!this.Vm.IsPlaying) return;

            this.Vm.IsPlaying = false;

            Task.Run(() => Messenger.Default.Send(0, "ResetAll"));

            LoadDialogWindow(isWorthPoints
                ? new Fin("The opponent has given up! You win!")
                : new Fin("The match has been cancelled."));
        }

        private void ResetAll()
        {
            this.KeepOn = false;
        }
    }
}
