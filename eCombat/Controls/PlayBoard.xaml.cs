using eCombat.Model;
using InteractiveEventTrigger = System.Windows.Interactivity.EventTrigger;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using eCombat.ViewModel;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using TriggerCollection = System.Windows.Interactivity.TriggerCollection;

namespace eCombat.Controls
{
    /// <summary>
    /// Interaction logic for PlayBoard.xaml
    /// </summary>
    public partial class PlayBoard : UserControl
    {
        /// <summary>
        /// Gets the view's ViewModel.
        /// </summary>
        private PlayBoardViewModel Vm => (PlayBoardViewModel)this.DataContext;

        private Grid CombateGrid { get; set; }
        private IEnumerable<UIElement> CombateGridChildren { get; set; }

        public PlayBoard()
        {
            InitializeComponent();

            Messenger.Default.Register<int[]>(this, "MoveBoardPiece", InvokeMoveAnimation);
        }

        public override void OnApplyTemplate()
        {
            this.CombateGrid = this.GetTemplateChild("CombateGrid") as Grid;
            if (this.CombateGrid == null) return;
            
            foreach (BoardField field in Board.Layout)
            {
                var rect = new Rectangle
                {
                    Stroke = Brushes.Black,
                };

                var rectStyle = new Style(typeof(Rectangle));

                var defaultFill = new Setter(Shape.FillProperty, Brushes.Transparent);
                var defaultStrokethickness = new Setter(Shape.StrokeThicknessProperty, 1.0);
                var defaultHitTest = new Setter(IsHitTestVisibleProperty, true);

                rectStyle.Setters.Add(defaultFill);
                rectStyle.Setters.Add(defaultStrokethickness);
                rectStyle.Setters.Add(defaultHitTest);

                var unwalkableFill = new Setter(Shape.FillProperty, Brushes.Blue);
                var unwalkableStrokeThickness = new Setter(Shape.StrokeThicknessProperty, 0.0);
                var unwalkableHitTest = new Setter(IsHitTestVisibleProperty, false);
                var unwalkableTrigger = new DataTrigger
                {
                    Binding = new Binding("IsWalkable") { Source = field },
                    Value = false
                };
                unwalkableTrigger.Setters.Add(unwalkableFill);
                unwalkableTrigger.Setters.Add(unwalkableStrokeThickness);
                unwalkableTrigger.Setters.Add(unwalkableHitTest);
                
                var attackableFill = new Setter
                {
                    Property = Shape.FillProperty,
                    Value = Brushes.Red
                };
                var atackableTrigger = new DataTrigger
                {
                    Binding = new Binding("IsAttackable") { Source = field },
                    Value = true
                };
                atackableTrigger.Setters.Add(attackableFill);

                var enabledFill = new Setter
                {
                    Property = Shape.FillProperty,
                    Value = Brushes.SpringGreen
                };
                var enabledTrigger = new DataTrigger
                {
                    Binding = new Binding("IsEnabled") { Source = field },
                    Value = true
                };
                enabledTrigger.Setters.Add(enabledFill);

                rectStyle.Triggers.Add(unwalkableTrigger);
                rectStyle.Triggers.Add(atackableTrigger);
                rectStyle.Triggers.Add(enabledTrigger);

                rect.Style = rectStyle;

                var clickDownEvent = new EventToCommand
                {
                    Command = this.Vm.FieldClickDownCommand,
                    CommandParameter = field
                };
                var triggerEvent = new InteractiveEventTrigger
                {
                    EventName = "MouseLeftButtonDown",
                };
                triggerEvent.Actions.Add(clickDownEvent);
                TriggerCollection triggers = Interaction.GetTriggers(rect);
                triggers.Add(triggerEvent);

                Grid.SetColumn(rect, field.Column);
                Grid.SetRow(rect, field.Row);
                Panel.SetZIndex(rect, 1);
                this.CombateGrid.Children.Add(rect);
            }

            foreach (BoardPiece piece in SelfPlayer.Army)
            {
                var button = new Button
                {
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    Command = this.Vm.PieceClickCommand,
                    CommandParameter = piece
                };

                var buttonStyle = new Style(typeof(Button))
                {
                    BasedOn = FindResource("MetroCircleButtonStyle") as Style
                };
                
                var defaultBackground = new Setter(BackgroundProperty, Brushes.Gray);
                var defaultVisibility = new Setter(VisibilityProperty, Visibility.Collapsed);
                var defaultEffect = new Setter(EffectProperty, null);
                var defaultForeground = new Setter(ForegroundProperty, Brushes.Black);
                var defaultFontSize = new Setter(FontSizeProperty, 12.0);

                buttonStyle.Setters.Add(defaultBackground);
                buttonStyle.Setters.Add(defaultVisibility);
                buttonStyle.Setters.Add(defaultEffect);
                buttonStyle.Setters.Add(defaultForeground);
                buttonStyle.Setters.Add(defaultFontSize);

                var player1Setter = new Setter(BackgroundProperty, TryFindResource("Player1Brush"));
                var player2Setter = new Setter(BackgroundProperty, TryFindResource("Player2Brush"));

                var isOpponentCondition = new Condition
                {
                    Binding = new Binding("IsOpponent") {Source = piece},
                    Value = true
                };
                var isSelfPlayerCondition = new Condition
                {
                    Binding = new Binding("IsOpponent") { Source = piece },
                    Value = false
                };
                var opponentIsPlayer1 = new Condition
                {
                    Binding = new Binding("IsPlayer2") {Source = this.Vm.Opponent},
                    Value = false
                };
                var selfPlayerIsPlayer1 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.SelfPlayer },
                    Value = false
                };
                var opponentIsPlayer2 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.Opponent },
                    Value = true
                };
                var selfPlayerIsPlayer2 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.SelfPlayer },
                    Value = true
                };

                var player1Trigger1 = new MultiDataTrigger();
                player1Trigger1.Conditions.Add(isOpponentCondition);
                player1Trigger1.Conditions.Add(opponentIsPlayer1);
                player1Trigger1.Setters.Add(player1Setter);

                var player1Trigger2 = new MultiDataTrigger();
                player1Trigger2.Conditions.Add(isSelfPlayerCondition);
                player1Trigger2.Conditions.Add(selfPlayerIsPlayer1);
                player1Trigger2.Setters.Add(player1Setter);

                var player2Trigger1 = new MultiDataTrigger();
                player2Trigger1.Conditions.Add(isOpponentCondition);
                player2Trigger1.Conditions.Add(opponentIsPlayer2);
                player2Trigger1.Setters.Add(player2Setter);

                var player2Trigger2 = new MultiDataTrigger();
                player2Trigger2.Conditions.Add(isSelfPlayerCondition);
                player2Trigger2.Conditions.Add(selfPlayerIsPlayer2);
                player2Trigger2.Setters.Add(player2Setter);

                var visibilitySetter = new Setter(VisibilityProperty, Visibility.Visible);
                var visibilityTrigger = new DataTrigger
                {
                    Binding = new Binding("IsAlive") {Source = piece},
                    Value = true
                };
                visibilityTrigger.Setters.Add(visibilitySetter);

                var effectSetter = new Setter(EffectProperty, new DropShadowEffect());
                var effectTrigger = new DataTrigger
                {
                    Binding = new Binding("IsSelected") {Source = piece},
                    Value = true
                };
                effectTrigger.Setters.Add(effectSetter);

                var onAttackFontSizeSetter = new Setter(FontSizeProperty, button.FontSize * 2.0);

                var winningForegroundSetter = new Setter(ForegroundProperty, Brushes.Green);
                var winningTrigger = new DataTrigger
                {
                    Binding = new Binding("IsDefeating") {Source = piece},
                    Value = true
                };
                winningTrigger.Setters.Add(onAttackFontSizeSetter);
                winningTrigger.Setters.Add(winningForegroundSetter);

                var defeatStoryboard = new Storyboard
                {
                    AccelerationRatio = 0.2
                };
                defeatStoryboard.Completed += (s, _) => DefeatStoryboard_Completed();
                var defeatAnimation = new DoubleAnimation(0.1, new TimeSpan(0, 0, 0, 2, 0));
                Storyboard.SetTarget(defeatAnimation, button);
                Storyboard.SetTargetProperty(defeatAnimation, new PropertyPath(FontSizeProperty));
                defeatStoryboard.Children.Add(defeatAnimation);
                var beginSb = new BeginStoryboard
                {
                    Storyboard = defeatStoryboard,
                    Name = "DefeatStoryboard"
                };
                var stopSb = new StopStoryboard
                {
                    BeginStoryboardName = beginSb.Name
                };
                var losingForegroundSetter = new Setter(ForegroundProperty, Brushes.Red);
                var losingTrigger = new DataTrigger
                {
                    Binding = new Binding("IsBeingDefeated") { Source = piece },
                    Value = true
                };
                losingTrigger.EnterActions.Add(beginSb);
                losingTrigger.ExitActions.Add(stopSb);
                losingTrigger.Setters.Add(onAttackFontSizeSetter);
                losingTrigger.Setters.Add(losingForegroundSetter);

                buttonStyle.Triggers.Add(player1Trigger1);
                buttonStyle.Triggers.Add(player1Trigger2);
                buttonStyle.Triggers.Add(player2Trigger1);
                buttonStyle.Triggers.Add(player2Trigger2);
                buttonStyle.Triggers.Add(visibilityTrigger);
                buttonStyle.Triggers.Add(effectTrigger);
                buttonStyle.Triggers.Add(winningTrigger);
                buttonStyle.Triggers.Add(losingTrigger);

                buttonStyle.RegisterName(beginSb.Name, beginSb);
                button.Style = buttonStyle;

                ApplyBinding("Column", piece, button, Grid.ColumnProperty);
                ApplyBinding("Row", piece, button, Grid.RowProperty);
                ApplyBinding("PowerLevel", piece, button, ContentProperty);

                Panel.SetZIndex(button, 2);
                this.CombateGrid.Children.Add(button);
            }

            foreach (BoardPiece piece in Opponent.Army)
            {
                var button = new Button
                {
                    Effect = null,
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false
                };

                var buttonStyle = new Style(typeof(Button))
                {
                    BasedOn = FindResource("MetroCircleButtonStyle") as Style
                };

                var defaultBackground = new Setter(BackgroundProperty, Brushes.Gray);
                var defaultVisibility = new Setter(VisibilityProperty, Visibility.Collapsed);
                var defaultForeground = new Setter(ForegroundProperty, Brushes.Black);
                var defaultFontSize = new Setter(FontSizeProperty, 12.0);

                buttonStyle.Setters.Add(defaultBackground);
                buttonStyle.Setters.Add(defaultVisibility);
                buttonStyle.Setters.Add(defaultForeground);
                buttonStyle.Setters.Add(defaultFontSize);

                var player1Setter = new Setter(BackgroundProperty, TryFindResource("Player1Brush"));
                var player2Setter = new Setter(BackgroundProperty, TryFindResource("Player2Brush"));
                
                var isOpponentCondition = new Condition
                {
                    Binding = new Binding("IsOpponent") { Source = piece },
                    Value = true
                };
                var isSelfPlayerCondition = new Condition
                {
                    Binding = new Binding("IsOpponent") { Source = piece },
                    Value = false
                };
                var opponentIsPlayer1 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.Opponent },
                    Value = false
                };
                var selfPlayerIsPlayer1 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.SelfPlayer },
                    Value = false
                };
                var opponentIsPlayer2 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.Opponent },
                    Value = true
                };
                var selfPlayerIsPlayer2 = new Condition
                {
                    Binding = new Binding("IsPlayer2") { Source = this.Vm.SelfPlayer },
                    Value = true
                };

                var player1Trigger1 = new MultiDataTrigger();
                player1Trigger1.Conditions.Add(isOpponentCondition);
                player1Trigger1.Conditions.Add(opponentIsPlayer1);
                player1Trigger1.Setters.Add(player1Setter);

                var player1Trigger2 = new MultiDataTrigger();
                player1Trigger2.Conditions.Add(isSelfPlayerCondition);
                player1Trigger2.Conditions.Add(selfPlayerIsPlayer1);
                player1Trigger2.Setters.Add(player1Setter);

                var player2Trigger1 = new MultiDataTrigger();
                player2Trigger1.Conditions.Add(isOpponentCondition);
                player2Trigger1.Conditions.Add(opponentIsPlayer2);
                player2Trigger1.Setters.Add(player2Setter);

                var player2Trigger2 = new MultiDataTrigger();
                player2Trigger2.Conditions.Add(isSelfPlayerCondition);
                player2Trigger2.Conditions.Add(selfPlayerIsPlayer2);
                player2Trigger2.Setters.Add(player2Setter);

                var visibilitySetter = new Setter(VisibilityProperty, Visibility.Visible);
                var visibilityTrigger = new DataTrigger
                {
                    Binding = new Binding("IsAlive") { Source = piece },
                    Value = true
                };
                visibilityTrigger.Setters.Add(visibilitySetter);

                var onAttackFontSizeSetter = new Setter(FontSizeProperty, button.FontSize * 2);

                var winningForegroundSetter = new Setter(ForegroundProperty, Brushes.Green);
                var winningTrigger = new DataTrigger
                {
                    Binding = new Binding("IsDefeating") { Source = piece },
                    Value = true
                };
                winningTrigger.Setters.Add(onAttackFontSizeSetter);
                winningTrigger.Setters.Add(winningForegroundSetter);

                var defeatStoryboard = new Storyboard
                {
                    AccelerationRatio = 0.2
                };
                defeatStoryboard.Completed += (s, _) => DefeatStoryboard_Completed();
                var defeatAnimation = new DoubleAnimation(0.1, new TimeSpan(0, 0, 0, 2, 0));
                Storyboard.SetTarget(defeatAnimation, button);
                Storyboard.SetTargetProperty(defeatAnimation, new PropertyPath(FontSizeProperty));
                defeatStoryboard.Children.Add(defeatAnimation);
                var beginSb = new BeginStoryboard
                {
                    Storyboard = defeatStoryboard,
                    Name = "DefeatStoryboard"
                };
                var stopSb = new StopStoryboard
                {
                    BeginStoryboardName = beginSb.Name
                };
                var losingForegroundSetter = new Setter(ForegroundProperty, Brushes.Red);
                var losingTrigger = new DataTrigger
                {
                    Binding = new Binding("IsBeingDefeated") { Source = piece },
                    Value = true
                };
                losingTrigger.EnterActions.Add(beginSb);
                losingTrigger.ExitActions.Add(stopSb);
                losingTrigger.Setters.Add(onAttackFontSizeSetter);
                losingTrigger.Setters.Add(losingForegroundSetter);

                buttonStyle.Triggers.Add(player1Trigger1);
                buttonStyle.Triggers.Add(player1Trigger2);
                buttonStyle.Triggers.Add(player2Trigger1);
                buttonStyle.Triggers.Add(player2Trigger2);
                buttonStyle.Triggers.Add(visibilityTrigger);
                buttonStyle.Triggers.Add(winningTrigger);
                buttonStyle.Triggers.Add(losingTrigger);

                buttonStyle.RegisterName(beginSb.Name, beginSb);
                button.Style = buttonStyle;

                ApplyBinding("Column", piece, button, Grid.ColumnProperty);
                ApplyBinding("Row", piece, button, Grid.RowProperty);
                ApplyBinding("PowerLevel", piece, button, ContentProperty);

                Panel.SetZIndex(button, 2);
                this.CombateGrid.Children.Add(button);
            }

            this.CombateGridChildren = this.CombateGrid.Children.Cast<UIElement>();

            base.OnApplyTemplate();
        }

        private static void ApplyBinding(
            string propertyPath, object src, DependencyObject dObject, DependencyProperty dProperty)
        {
            var binding = new Binding(propertyPath)
            {
                Source = src
            };
            BindingOperations.SetBinding(dObject, dProperty, binding);
        }

        private IEnumerable<UIElement> GetCombateGridChildren(int c, int r)
        {
            return this.CombateGridChildren.Where(e =>
                Grid.GetColumn(e) == c && Grid.GetRow(e) == r);
        }

        private void InvokeMoveAnimation(int[] values)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                AnimatePieceMove(values[0], values[1], values[2], values[3]));
        }

        private async void AnimatePieceMove(int srcX, int srcY, int destX, int destY)
        {
            foreach (UIElement elem in GetCombateGridChildren(srcX, srcY))
            {
                if (!(elem is Button)) continue;

                if (this.Vm.LastMovedPiece.IsDefeating)
                {
                    await Task.Delay(1500);
                }

                elem.RenderTransform = new TranslateTransform();

                if (destX == srcX)
                {
                    int steps = destY - srcY;
                    int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                    double moveHeight = this.CombateGrid.RenderSize.Height / 10;
                    var moveAnimation = new DoubleAnimation(moveHeight * steps, new TimeSpan(0, 0, 0, 0, tempo));
                    moveAnimation.Completed += (s, _) => MoveAnimation_Completed(elem);
                    elem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
                }
                else
                {
                    int steps = destX - srcX;
                    int tempo = (int)Math.Ceiling(0.1 + Math.Log(Math.Abs(steps))) * 500;
                    double moveWidth = this.CombateGrid.RenderSize.Width / 10;
                    var moveAnimation = new DoubleAnimation(moveWidth * steps, new TimeSpan(0, 0, 0, 0, tempo));
                    moveAnimation.Completed += (s, _) => MoveAnimation_Completed(elem);
                    elem.RenderTransform.BeginAnimation(TranslateTransform.XProperty, moveAnimation);
                }
            }
        }

        private static void MoveAnimation_Completed(UIElement element)
        {
            element.RenderTransform = null;

            Messenger.Default.Send(0, "MovePieceCompleted");
        }

        private static void DefeatStoryboard_Completed()
        {
            Messenger.Default.Send(0, "AttackPieceCompleted");
        }
    }
}
