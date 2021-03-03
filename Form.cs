using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleGame
{
    public partial class Form : System.Windows.Forms.Form, IGameEventHandler, IUnitEventHandler
    {
        private Game game;
        private SelectedUnitEventHandler selectedUnitEventHandler;
        private Color defaultButtonColor;
        private Button[,] field = new Button[Game.FieldSize, Game.FieldSize];
        public Form()
        {
            InitializeComponent();
            selectedUnitEventHandler = new SelectedUnitEventHandler(this);

            var i = 0;
            foreach (Button button in FieldPanel.Controls)
            {
                var x = i / Game.FieldSize;
                var y = i % Game.FieldSize;
                field[x, y] = button;
                i++;
            }

            defaultButtonColor = FieldPanel.Controls[1].BackColor;
            game = new Game(this);
        }

        #region IUnitEventHandler implementation

        public void OnUnitDied(Unit unit)
        {
            var button = field[unit.Position.X, unit.Position.Y];
            button.Text = "";
            button.BackColor = defaultButtonColor;
        }

        public void OnUnitWasHealed(Unit unit)
        {
            var button = field[unit.Position.X, unit.Position.Y];
            button.Text = $"{unit.Strategy}\n{unit.HP}";
        }

        public void OnUnitMoved(Unit unit, Vector2 oldPosition)
        {
            var oldButton = field[oldPosition.X, oldPosition.Y];
            var newButton = field[unit.Position.X, unit.Position.Y];

            newButton.BackColor = oldButton.BackColor;
            newButton.Text = oldButton.Text;

            oldButton.BackColor = defaultButtonColor;
            oldButton.Text = "";

        }

        public void OnUnitChangedStrategy(Unit unit)
        {
            var button = field[unit.Position.X, unit.Position.Y];
            button.Text = $"{unit.Strategy}\n{unit.HP}";
        }

        public void OnUnitTookDamage(Unit unit)
        {
            var button = field[unit.Position.X, unit.Position.Y];
            button.Text = $"{unit.Strategy}\n{unit.HP}";
        }

        public void OnUnitWasSpawned(Unit unit)
        {
            unit.EventHandler = this;

            var button = field[unit.Position.X, unit.Position.Y];
            button.BackColor = unit.Team switch
            {
                Team.Blue => Color.Blue,
                Team.Red => Color.Red
            };
            button.Text = $"{unit.Strategy}\n{unit.HP}";
        }

        #endregion

        #region Handling interface events

        private void OnStartButtonWasClicked(object sender, EventArgs e)
        {
            Timer.Start();
            StartButton.Enabled = false;
            PauseButton.Enabled = true;
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            game.DoStep();
        }

        private void OnPauseButtonWasClicked(object sender, EventArgs e)
        {
            Timer.Stop();
            StartButton.Enabled = true;
            PauseButton.Enabled = false;
        }

        private void OnCellWasClicked(object sender, EventArgs e)
        {
            var clickedButton = sender as Button;
            for (int i = 0; i < Game.FieldSize; i++)
            {
                for (int j = 0; j < Game.FieldSize; j++)
                {
                    if (clickedButton == field[i, j])
                    {
                        var selectedUnit = game.Field[i, j];
                        if (selectedUnit is null)
                        {
                            selectedUnitEventHandler.DeselectUnit();
                        }
                        else
                        {
                            selectedUnitEventHandler.SelectUnit(selectedUnit);
                        }

                        return;
                    }
                }
            }
        }

        #endregion

        class SelectedUnitEventHandler : IUnitEventHandler
        {
            private Form form;
            private Unit unit;

            public SelectedUnitEventHandler(Form form)
            {
                this.form = form;
                foreach (var value in Enum.GetValues(typeof(Strategy)))
                    form.SelectedUnitStrategyField.Items.Add(value);
            }

            public void SelectUnit(Unit unit)
            {
                this.unit = unit;
                unit.EventHandler = this;
                form.SelectedUnitTeamLabel.Text = $"Team: {unit.Team}";
                form.SelectedUnitHPLabel.Text = $"HP: {unit.HP}";

                var strategyField = form.SelectedUnitStrategyField;
                strategyField.Enabled = true;
                strategyField.SelectedItem = unit.Strategy;

            }

            public void DeselectUnit()
            {
                if(unit is { })
                    unit.EventHandler = form;
                form.SelectedUnitTeamLabel.Text = "Team:";
                form.SelectedUnitHPLabel.Text = "HP: ";

                var strategyField = form.SelectedUnitStrategyField;
                strategyField.Enabled = false;
                strategyField.SelectedItem = null;

            }

            public void OnSelectedUnitStrategyFieldWasChanged()
            {
                var strategy = form.SelectedUnitStrategyField.SelectedItem as Strategy?;
                unit.ChangeStrategy(strategy.Value);

            }

            public void OnUnitTookDamage(Unit unit)
            {
                form.SelectedUnitHPLabel.Text = $"HP: {unit.HP}";
                form.OnUnitTookDamage(unit);
            }

            public void OnUnitDied(Unit unit)
            {
                DeselectUnit();
                form.OnUnitDied(unit);
            }

            public void OnUnitWasHealed(Unit unit)
            {
                form.SelectedUnitHPLabel.Text = $"HP: {unit.HP}";
                form.OnUnitWasHealed(unit);
            }

            public void OnUnitChangedStrategy(Unit unit)
            {
                form.SelectedUnitStrategyField.SelectedItem = unit.Strategy;
                form.OnUnitChangedStrategy(unit);
            }

            public void OnUnitMoved(Unit unit, Vector2 oldPosition)
            {
                form.OnUnitMoved(unit, oldPosition);
            }
        }

        private void OnSelectedUnitStrategyFieldWasChanged(object sender, EventArgs e)
        {
            selectedUnitEventHandler.OnSelectedUnitStrategyFieldWasChanged();
        }
    }
}
