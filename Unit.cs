namespace SimpleGame
{
    public class Unit
    {
        public const int MaxHP = 5;
        public Team Team;
        public int HP = MaxHP;
        public bool IsDead;
        public Vector2 Position;
        public Strategy Strategy;
        public bool IsShielded;
        public IUnitEventHandler EventHandler;

        public Unit(Team team, Vector2 position, Strategy strategy)
        {
            Team = team;
            Position = position;
            Strategy = strategy;
        }

        public void TakeDamage(int damage)
        {
            if (IsShielded)
            {
                IsShielded = false;
            }
            else
            {
                HP -= damage;
                IsDead = HP <= 0;
                if (IsDead)
                    EventHandler.OnUnitDied(this);
                else
                    EventHandler.OnUnitTookDamage(this);

            }
        }

        public void TakeHeal(int hp)
        {
            HP += hp;
            EventHandler.OnUnitWasHealed(this);
        }
        
        public void DoAction(Game game)
        {
            //Если не хочется менять код внутри if'ов, то можно заменить this на нужный вам объект
            var thisUnit = this;

            var Team = thisUnit.Team;
            var Position = thisUnit.Position;
            ref var IsShielded = ref thisUnit.IsShielded;

            if (Strategy == Strategy.Melee)
            {
                var nearestEnemy = game.FindNearest(Position, x => x.Team != Team);
                if (nearestEnemy is null) return;

                var distanceToEnemy = (Position - nearestEnemy.Position).Abs;
                var attackDistance = new Vector2(1, 1).Abs;
                if (distanceToEnemy <= attackDistance)
                {
                    nearestEnemy.TakeDamage(2);
                    if(nearestEnemy.IsDead) game.DeleteUnit(nearestEnemy);
                }
                else
                {
                    var minDistance = (Position - nearestEnemy.Position).Abs;
                    var bestPosition = Position;

                    var offsets = new Vector2[]
                    {
                        (0, 1), (1, 0),
                        (-1, 0), (0, -1),
                    };

                    foreach (var offset in offsets)
                    {
                        var pos = Position + offset;
                        if (game.PositionIsFree(pos))
                        {
                            var distance = (pos - nearestEnemy.Position).Abs;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestPosition = pos;
                            }
                        }
                    }

                    if (!Equals(bestPosition, Position))
                        game.MoveUnit(thisUnit, bestPosition);
                }

            }
            else if (Strategy == Strategy.Escape)
            {
                var nearestEnemy = game.FindNearest(Position, x => x.Team != Team);
                if (nearestEnemy is null) return;

                var maxDistance = (Position - nearestEnemy.Position).Abs;
                var bestPosition = Position;
                var offsets = new Vector2[]
                {
                    (1, 1), (1, 0), (1, -1),
                    (0, 1), (0, -1),
                    (-1, 1), (-1, 0), (-1, -1)
                };

                foreach (var offset in offsets)
                {
                    var pos = Position + offset;
                    if (game.PositionIsFree(pos))
                    {
                        var distance = (pos - nearestEnemy.Position).Abs;
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            bestPosition = pos;
                        }
                    }
                }

                if (!Equals(bestPosition, Position))
                    game.MoveUnit(thisUnit, bestPosition);
            }
            else if (Strategy == Strategy.Healing)
            {
                var nearestAlly = game.FindNearest(Position, x => x.Team == Team && x.HP < MaxHP);
                if (nearestAlly is null) return;

                var distanceToAlly = (Position - nearestAlly.Position).Abs;
                var healingDistance = new Vector2(2, 2).Abs;
                var offsets = new Vector2[]
                {
                    (1, 1), (1, 0), (1, -1),
                    (0, 1), (0, -1),
                    (-1, 1), (-1, 0), (-1, -1)
                };

                if (distanceToAlly <= healingDistance)
                {
                    nearestAlly.TakeHeal(1);
                }
                else
                {
                    var minDistance = (Position - nearestAlly.Position).Abs;
                    var bestPosition = Position;
                    foreach (var offset in offsets)
                    {
                        var pos = Position + offset;
                        if (game.PositionIsFree(pos))
                        {
                            var distance = (pos - nearestAlly.Position).Abs;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestPosition = pos;
                            }
                        }
                    }

                    if (!Equals(bestPosition, Position))
                        game.MoveUnit(thisUnit, bestPosition);
                }
            }
            else if (Strategy == Strategy.Tank)
            {
                var nearestEnemy = game.FindNearest(Position, x => x.Team != Team);
                if (nearestEnemy is null) return;

                var distanceToEnemy = (Position - nearestEnemy.Position).Abs;
                var attackDistance = new Vector2(1, 1).Abs;
                var offsets = new Vector2[]
                {
                    (0, 1), (1, 0),
                    (-1, 0), (0, -1),
                };

                if (distanceToEnemy <= attackDistance)
                {
                    IsShielded = true;
                }
                else
                {
                    var minDistance = (Position - nearestEnemy.Position).Abs;
                    var bestPosition = Position;
                    foreach (var offset in offsets)
                    {
                        var pos = Position + offset;
                        if (game.PositionIsFree(pos))
                        {
                            var distance = (pos - nearestEnemy.Position).Abs;
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                bestPosition = pos;
                            }
                        }
                    }

                    if (!Equals(bestPosition, Position))
                       game.MoveUnit(thisUnit, bestPosition);
                }
            }
        }

        public void ChangeStrategy(Strategy newStrategy)
        {
            Strategy = newStrategy;
            EventHandler.OnUnitChangedStrategy(this);
        }
    }

    public enum Strategy
    {
        Melee,
        Escape,
        Healing,
        Tank
    }
    
    public enum Team
    {
        Red,
        Blue
    }
    public interface IUnitEventHandler
    {
        void OnUnitTookDamage(Unit unit);
        void OnUnitDied(Unit unit);
        void OnUnitWasHealed(Unit unit);
        void OnUnitChangedStrategy(Unit unit);
        void OnUnitMoved(Unit unit, Vector2 oldPosition);
    }
}
