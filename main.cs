using System;
using System.Collections.Generic;

class Player
{
    public const int MapWidth = 17630;
    public const int MapHeight = 9000;
    public const int BaseAutoTarget = 5000;

    public const int BaseVisionRange = 6000;
    public const int HeroVisionRange = 2200;

    public const int ManaToCast = 10;
    public const int ControlCastRange = 2200;

    public const int TYPE_MONSTER = 0;
    public const int TYPE_MY_HERO = 1;
    public const int TYPE_OP_HERO = 2;

    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Entity
    {
        public int Id;
        public int Type;
        public Point Location;
        public int ShieldLife;
        public int IsControlled;
        public int Health;
        public Point Trajectory;
        public Point NextLocation;
        public int NearBase;
        public int ThreatFor;

        public Entity(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor)
        {
            this.Id = id;
            this.Type = type;
            this.Location = new Point(x, y);
            this.ShieldLife = shieldLife;
            this.IsControlled = isControlled;
            this.Health = health;
            this.Trajectory = new Point(vx, vy);
            this.NextLocation = new Point(x + vx, y + vy);
            this.NearBase = nearBase;
            this.ThreatFor = threatFor;
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        Point myBase = new Point
        {
            X = int.Parse(inputs[0]),
            Y = int.Parse(inputs[1])
        };
        Debug(myBase);

        Point enemyBase = new Point
        {
            X = Math.Abs(myBase.X - MapWidth),
            Y = Math.Abs(myBase.Y - MapHeight)
        };
        Debug(enemyBase);

        Point defPosition = new Point
        {
            X = Math.Abs(myBase.X - BaseVisionRange / 2),
            Y = Math.Abs(myBase.Y - BaseVisionRange / 2)
        };
        Debug(defPosition);

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myHealth = int.Parse(inputs[0]); // Your base health
            int myMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            int oppHealth = int.Parse(inputs[0]);
            int oppMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            List<Entity> myHeroes = new List<Entity>(entityCount);
            List<Entity> oppHeroes = new List<Entity>(entityCount);
            List<Entity> monsters = new List<Entity>(entityCount);

            HashSet<int> controlledMonsters = new HashSet<int>();

            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                Entity entity = new Entity(
                    id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor
                );

                switch (type)
                {
                    case TYPE_MONSTER:
                        monsters.Add(entity);
                        break;
                    case TYPE_MY_HERO:
                        myHeroes.Add(entity);
                        break;
                    case TYPE_OP_HERO:
                        oppHeroes.Add(entity);
                        break;
                }
            }

            foreach (Entity hero in myHeroes)
            {
                Entity targetMonster = null;
                int minDistanceToBase = Int32.MaxValue;
                for (int i = 0; i < monsters.Count; i++)
                {
                    var monster = monsters[i];

                    int distance = Distance(myBase, monster.NextLocation);
                    if (distance > BaseAutoTarget &&
                        monster.NearBase != TYPE_MY_HERO)
                    {
                        continue;
                    }

                    if (distance < minDistanceToBase)
                    {
                        targetMonster = monster;
                        minDistanceToBase = distance;
                    }
                }

                if (targetMonster == null)
                {
                    Move(defPosition, hero.Id);
                    continue;
                }

                var distanceToTarget = Distance(hero.Location, targetMonster.Location);
                if (distanceToTarget <= ControlCastRange &&
                   minDistanceToBase <= BaseAutoTarget / 2 &&
                   myMana >= ManaToCast &&
                   targetMonster.Health >= 8 &&
                   !controlledMonsters.Contains(targetMonster.Id))
                {
                    Control(targetMonster.Id, enemyBase, hero.Id);
                    controlledMonsters.Add(targetMonster.Id);
                    myMana -= ManaToCast;
                    continue;
                }

                Move(targetMonster.NextLocation, hero.Id);
            }
        }
    }

    public static int Distance(Point p1, Point p2) => (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

    public static int DistanceFast(Point p1, Point p2) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    public static void Debug(Point p) => Console.Error.WriteLine($"Debug point {p.X} {p.Y}");

    public static void Move(Point p, int hero) => Console.WriteLine($"MOVE {p.X} {p.Y} M{hero}");

    public static void Control(int id, Point p, int hero) => Console.WriteLine($"SPELL CONTROL {id} {p.X} {p.Y} C{hero}");
}