using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleGame
{

    public class Game
    {
        public const int FieldSize = 8;
        private IGameEventHandler eventHandler;
        public Unit[,] Field = new Unit[FieldSize, FieldSize];
        private LinkedList<Unit> unitQueue = new LinkedList<Unit>();
        private LinkedListNode<Unit> currentQueueNode;

        public Game(IGameEventHandler eventHandler)
        {
            this.eventHandler = eventHandler;

            for (int i = 0; i < FieldSize; i++)
            {
                if (i % 2 == 0) 
                    SpawnUnit((i, 0), Team.Blue,  Strategy.Healing);
                SpawnUnit((i, 1), Team.Blue, Strategy.Melee);

                if (i % 2 == 0)
                    SpawnUnit((i, FieldSize - 1), Team.Red,  Strategy.Healing);
                SpawnUnit((i, FieldSize - 2), Team.Red,  Strategy.Melee);
            }
            MixQueue();
        }

        public void MixQueue()
        {
            var random = new Random();
            var newQueue = new LinkedList<Unit>();
            var newSequence = unitQueue.Select(x => (x, random.Next()))
                .OrderBy(x => x.Item2)
                .Select(x => x.Item1);
            foreach (var unit in newSequence)
            {
                newQueue.AddLast(unit);
            }

            unitQueue = newQueue;
            currentQueueNode = null;
        }

        public void SpawnUnit(Vector2 position, Team team, Strategy strategy)
        {
            var newUnit = new Unit(team, position, strategy);
            Field[position.X, position.Y] = newUnit;
            unitQueue.AddLast(newUnit);
            eventHandler.OnUnitWasSpawned(newUnit);
        }

        public void DoStep()
        {
            currentQueueNode ??= unitQueue.First;

            var currentUnit = currentQueueNode.Value;
            currentQueueNode = currentQueueNode.Next;

            currentUnit.DoAction(this);

        }

        public Unit FindNearest(Vector2 center, Predicate<Unit> predicate)
        {
            var motionDirections = new Vector2[]
            {
                (1, 1), (1, -1),
                (-1, -1), (-1, 1)
            };
            var pos = center;
            for (int radius = 1; radius < FieldSize; radius++)
            {
                pos += (-1, 0);
                for (int j = 0; j < 4; j++)
                {
                    var direction = motionDirections[j];
                    for (int i = 0; i < radius; i++)
                    {
                        if (pos.X >= 0 && pos.X < FieldSize
                            && pos.Y >= 0 && pos.Y < FieldSize
                            && Field[pos.X, pos.Y] is { } unit
                            && predicate(unit)) 
                            return unit;

                        pos += direction;
                    }
                }
            }
            
            return null;
        }

        public bool PositionIsFree(Vector2 pos)
        {
            return pos.X >= 0 && pos.X < FieldSize
                             && pos.Y >= 0 && pos.Y < FieldSize
                             && Field[pos.X, pos.Y] is null;
        }

        public void MoveUnit(Unit unit, Vector2 targetPosition)
        {
            var oldPosition = unit.Position;
            if (!Equals(oldPosition, targetPosition))
            {
                Field[oldPosition.X, oldPosition.Y] = null;
                Field[targetPosition.X, targetPosition.Y] = unit;
                unit.Position = targetPosition;
            }

            unit.EventHandler.OnUnitMoved(unit, oldPosition);
        }

        public void DeleteUnit(Unit unit)
        {
            Field[unit.Position.X, unit.Position.Y] = null;

            var node = unitQueue.Find(unit);
            if (currentQueueNode == node)
            {
                currentQueueNode = currentQueueNode.Next;
            }

            unitQueue.Remove(node);
        }
    }

    public interface IGameEventHandler
    {
        void OnUnitWasSpawned(Unit unit);
    }

    public struct Vector2
    {
        public int X;
        public int Y;

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2((int x, int y) value) => new Vector2 {X = value.x, Y = value.y};
        public static Vector2 operator -(Vector2 left, Vector2 right) => (left.X - right.X, left.Y - right.Y);
        public static Vector2 operator +(Vector2 left, Vector2 right) => (left.X + right.X, left.Y + right.Y);

        public double Abs => Math.Sqrt(Math.Pow(Math.Abs(X),2) + Math.Pow(Math.Abs(Y),2));
    }
}
