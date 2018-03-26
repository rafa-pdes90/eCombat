using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro;
using MahApps.Metro.Controls;
using eCombat.Model;
using eCombat.View;
using eCombat.ViewModel;

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

        // ReSharper disable RedundantDefaultMemberInitializer
        private MetroWindow DialogWindow { get; set; } = null;

        private bool KeepOn { get; set; } = false;
        // ReSharper restore RedundantDefaultMemberInitializer

        private bool LastMoveAnimationEventFired { get; set; } = true;
        private bool LastLoserAnimationEventFired { get; set; } = true;
        private BoardPiece LastMovedUnit { get; set; }
        private BoardPiece LastMovedPiece { get; set; }
        private BoardPiece LastSelectedUnit { get; set; }
        private Queue<BoardPiece> Combatentes { get; set; }
        private Rectangle LastMovedCell { get; set; }
        private List<Rectangle> EnabledFields { get; set; }
        private Queue<Tuple<int, int>> MoveLog { get; set; }
        private IEnumerable<UIElement> CombateGridChildren { get; set; }

        public ICommand BoardPieceSelectCommand { get; }

        public MainWindow()
        {
            InitializeComponent();
            BoardPieceSelectCommand = new RelayCommand<BoardPiece>(BoardPieceSelectMethod);
            this.Combatentes = new Queue<BoardPiece>();
            this.EnabledFields = new List<Rectangle>();
            this.MoveLog = new Queue<Tuple<int, int>>(3);
            this.LogScrollViewer.ScrollToEnd();
        }

        private void MetroWindow_ContentRendered(object sender, EventArgs e)
        {
            LoadConnectionWindow();
            if (!this.KeepOn) return;
            this.KeepOn = false;
            LoadCombateUIElements();
        }

        public void LoadConnectionWindow()
        {
            this.Effect = new BlurEffect();
            this.DialogWindow = new ConnectionWindow
            {
                Owner = this
            };
            this.DialogWindow.ShowDialog();

            if (this.KeepOn)
            {
                this.Effect = null;
            }
            else
            {
                this.Close();
            }
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
                unit.Style = Application.Current.FindResource("MetroCircleButtonStyle") as Style;

                var content = new Binding("PowerLevel")
                {
                    Source = unit
                };
                BindingOperations.SetBinding(unit, ContentProperty, content);

                if (unit.IsEnemy)
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
            List<BoardPiece> enemyList = this.Vm.EnemyList;

            int index = 0;
            for (int r = 0; r < 4; r++)
            {
                for (int c = 9; c >= 0; c--)
                {
                    AddNewCell(r, c);
                    AddNewBoardPiece(r, c, this.Vm.OpponentColor, enemyList[index]);
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
                    AddNewBoardPiece(r, c, this.Vm.PlayerColor, unitList[index]);
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

        private IEnumerable<UIElement> GetCombateGridChildren(int r, int c)
        {
            return this.CombateGridChildren.Where(e =>
                Grid.GetColumn(e) == c && Grid.GetRow(e) == r);
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
                    var coords = new Tuple<int, int>(r, c);
                    if (tempMoveLog.Contains(coords))
                    {
                        moveLevel -= 1;
                        if (moveLevel == 0) break;
                        r += incR;
                        c += incC;
                        continue;
                    }

                    Rectangle moveRect = null;
                    bool hasEnemy = false;

                    foreach (UIElement element in GetCombateGridChildren(r, c))
                    {
                        switch (element)
                        {
                            case Rectangle rectangle:
                                moveRect = rectangle;
                                break;
                            case BoardPiece piece:
                                if (!piece.IsEnemy || (moveLevelOriginal > 1 && moveLevel < 9))
                                    return;
                                hasEnemy = true;
                                break;
                            default:
                                return;
                        }
                    }

                    if (moveRect == null) continue;
                    this.EnabledFields.Add(moveRect);
                    if (hasEnemy)
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

            if (!LastLoserAnimationEventFired) return;

            if (!this.LastMoveAnimationEventFired)
            {
                MoveAnimationClear();
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

        private void MoveAnimationClear()
        {
            this.LastMoveAnimationEventFired = true;
            int cellRow = Grid.GetRow(this.LastMovedCell);
            int cellColumn = Grid.GetColumn(this.LastMovedCell);

            this.LastMovedPiece.RenderTransform = null;
            Grid.SetRow(this.LastMovedPiece, cellRow);
            Grid.SetColumn(this.LastMovedPiece, cellColumn);
        }

        private void MoveAnimationCompleted(object sender, EventArgs e)
        {
            if (LastMoveAnimationEventFired) return;
            MoveAnimationClear();
        }

        private void LoserAnimationClear()
        {
            this.LastLoserAnimationEventFired = true;

            while (Combatentes.Count > 0)
            {
                BoardPiece piece = Combatentes.Dequeue();
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

        private void LoserAnimationCompleted(object sender, EventArgs e)
        {
            if (LastLoserAnimationEventFired) return;
            LoserAnimationClear();
        }

        private void CellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rect = (Rectangle)e.Source;
            if (!this.EnabledFields.Contains(rect)) return;

            int cellRow = Grid.GetRow(rect);
            int cellColumn = Grid.GetColumn(rect);
            int unitRow = Grid.GetRow(this.LastSelectedUnit);
            int unitColumn = Grid.GetColumn(this.LastSelectedUnit);

            this.Vm.IsOpponentTurn = true;
            this.Vm.FinishTurn(unitRow, unitColumn, cellRow, cellColumn, this.LastSelectedUnit.PowerLevel);

            MovePiece(this.LastSelectedUnit, rect, cellRow, cellColumn, unitRow, unitColumn);
        }

        private async void MovePiece(BoardPiece movingUnit, Rectangle rect, int cellRow, int cellColumn, int unitRow,
            int unitColumn,
            bool isEnemy = false, string powerLevel = null)
        {
            this.LastMoveAnimationEventFired = false;
            this.LastLoserAnimationEventFired = false;
            BoardPiece enemy = null;
            BoardPiece loser = null;

            if (isEnemy || rect.Fill.Equals(Brushes.Red))
            {
                foreach (UIElement element in GetCombateGridChildren(cellRow, cellColumn))
                {
                    if (!(element is BoardPiece piece)) continue;
                    enemy = piece;
                    break;
                }
            }

            if (!isEnemy)
            {
                ClearLastSelection();

                if (enemy != null)
                {
                    while (this.Vm.FeedbackReceived == null)
                    {
                    }

                    enemy.PowerLevel = this.Vm.FeedbackReceived;
                    this.Vm.FeedbackReceived = null;
                    enemy.GetBindingExpression(ContentProperty)?.UpdateTarget();
                }

                var movingCell = new Tuple<int, int>(cellRow, cellColumn);

                if (!movingUnit.Equals(this.LastMovedUnit))
                {
                    this.MoveLog.Clear();
                }
                else
                {
                    if (this.MoveLog.Count == 3)
                        this.MoveLog.Dequeue();
                }

                this.MoveLog.Enqueue(movingCell);
            }
            else if (enemy != null)
            {
                movingUnit.PowerLevel = powerLevel;
                movingUnit.GetBindingExpression(ContentProperty)?.UpdateTarget();
            }

            if (enemy != null)
            {
                Combatentes.Enqueue(movingUnit);
                Combatentes.Enqueue(enemy);

                movingUnit.FontSize *= 2;
                enemy.FontSize *= 2;

                var loserAnimation = new DoubleAnimation(0, new TimeSpan(0, 0, 0, 1, 0));
                loserAnimation.Completed += LoserAnimationCompleted;
                bool isBandeira = !int.TryParse(enemy.PowerLevel, out int enemyPower);
                int.TryParse(movingUnit.PowerLevel, out int unitPower);

                if (isBandeira && enemy.PowerLevel != null)
                {
                    if (isEnemy)
                    {
                        CallDefeat();
                    }
                    else
                    {
                        CallVictory();
                    }

                    return;
                }

                if (unitPower == enemyPower || (enemyPower == 0 && unitPower != 3))
                {
                    loser = movingUnit;
                    movingUnit.Foreground = Brushes.Red;
                    enemy.IsEnabled = true;
                    enemy.Foreground = Brushes.Red;
                    await Task.Delay(1500);
                    movingUnit.BeginAnimation(FontSizeProperty, loserAnimation);
                    enemy.BeginAnimation(FontSizeProperty, loserAnimation);
                    await Task.Delay(500);
                }
                else
                {
                    BoardPiece winner;

                    if ((enemyPower == 10 && unitPower == 1)
                        || (enemyPower == 0 && unitPower == 3)
                        || (unitPower > enemyPower))
                    {
                        winner = movingUnit;
                        loser = enemy;
                    }
                    else
                    {
                        winner = enemy;
                        loser = movingUnit;
                    }

                    winner.Foreground = Brushes.Green;
                    loser.Foreground = Brushes.Red;
                    await Task.Delay(1500);
                    loser.BeginAnimation(FontSizeProperty, loserAnimation);
                    await Task.Delay(500);
                }
            }
            else
            {
                this.LastLoserAnimationEventFired = true;
            }

            if (loser == null || !loser.Equals(movingUnit))
            {
                movingUnit.RenderTransform = new TranslateTransform();
                if (cellColumn == unitColumn)
                {
                    int steps = cellRow - unitRow;
                    int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                    double moveHeight = CombateGrid.RenderSize.Height / 10;
                    var moveAnimation = new DoubleAnimation(moveHeight * steps, new TimeSpan(0, 0, 0, 0, tempo));
                    moveAnimation.Completed += MoveAnimationCompleted;
                    movingUnit.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
                }
                else
                {
                    int steps = cellColumn - unitColumn;
                    int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                    double moveWidth = CombateGrid.RenderSize.Width / 10;
                    var moveAnimation = new DoubleAnimation(moveWidth * steps, new TimeSpan(0, 0, 0, 0, tempo));
                    moveAnimation.Completed += MoveAnimationCompleted;
                    movingUnit.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
                }

                this.LastMovedPiece = movingUnit;
                this.LastMovedCell = rect;
            }
            else
            {
                this.LastMoveAnimationEventFired = true;
            }

            if (!isEnemy)
            {
                this.LastMovedUnit = movingUnit;
            }

            await Task.Run(() =>
            {
                Thread.Sleep(1000);
                if (!LastLoserAnimationEventFired)
                {
                    this.Invoke(LoserAnimationClear);
                }
            });
        }

        public void MoveTheEnemy(int origemY, int origemX, int destY, int destX, string powerLevel)
        {
            origemY = Math.Abs(origemY - 9);
            origemX = Math.Abs(origemX - 9);
            destY = Math.Abs(destY - 9);
            destX = Math.Abs(destX - 9);
            BoardPiece enemy = null;
            Rectangle rect = null;

            foreach (UIElement element in GetCombateGridChildren(origemY, origemX))
            {
                switch (element)
                {
                    case BoardPiece piece:
                        enemy = piece;
                        break;
                }
            }

            if (enemy == null) return;
            foreach (UIElement element in GetCombateGridChildren(destY, destX))
            {
                switch (element)
                {
                    case Rectangle rectangle:
                        rect = rectangle;
                        break;
                    case BoardPiece piece:
                        this.Vm.SendDefenderFeedback(piece.PowerLevel);
                        break;
                }
            }

            MovePiece(enemy, rect, destY, destX, origemY, origemX, isEnemy: true, powerLevel: powerLevel);
        }

        private void CallVictory()
        {
            this.Vm.MensagemFinal = "Parabéns pela vitória!";
            CallDesistir();
        }

        private void CallDefeat()
        {
            this.Vm.MensagemFinal = "É, não foi dessa vez.";
            CallDesistir();
        }

        public void CallDesistir()
        {
            this.DialogWindow = new Fin
            {
                Owner = this
            };
            this.DialogWindow.ShowDialog();
        }

        public void StartNewMatch()
        {
            this.KeepOn = true;
            this.DialogWindow.Close();
        }

        public void ChatScrollToEnd()
        {
            double pseudoEnd = this.ChatScrollViewer.ExtentHeight;
            this.ChatScrollViewer.ScrollToVerticalOffset(pseudoEnd);
        }
    }
}
