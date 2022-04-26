using System;
using System.Collections.Generic;

class Player
{
    public const int MapWidth = 17630;
    public const int HalfMapWidth = 8815;
    public const int QuarterMapWidth = 4408;

    public const int MapHeight = 9000;
    public const int HalfMapHeight = 4500;
    public const int QuarterMapHeight = 2250;

    public const int BaseAutoTarget = 5000;

    public const int BaseVisionRange = 6000;
    public const int HalfBaseVisionRange = 3000;
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
        //Debug(myBase);

        Point enemyBase = new Point
        {
            X = Math.Abs(myBase.X - MapWidth),
            Y = Math.Abs(myBase.Y - MapHeight)
        };
        //Debug(enemyBase);

        Point defPosition = new Point
        {
            X = Math.Abs(myBase.X - HalfBaseVisionRange),
            Y = Math.Abs(myBase.Y - HalfBaseVisionRange)
        };
        //Debug(defPosition);

        Point[] attackPositions = 
        {
            new Point
            {
                X = Math.Abs(myBase.X - QuarterMapWidth),
                Y = Math.Abs(myBase.Y - QuarterMapHeight)
            },
            new Point
            {
                X = Math.Abs(enemyBase.X - QuarterMapWidth),
                Y = Math.Abs(enemyBase.Y - QuarterMapHeight)
            }
        };
        Debug(attackPositions[0]);
        Debug(attackPositions[1]);

        Point targetAttackPosition = attackPositions[0];

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        // init
        bool firstMove = true;
        int attackHeroId = 0;
        int defendHeroId1 = 0;
        int defendHeroId2 = 0;

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

            List<Entity> myHeroes = new List<Entity>(heroesPerPlayer);
            List<Entity> oppHeroes = new List<Entity>(heroesPerPlayer);
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

            if(firstMove)
            {
                attackHeroId = myHeroes[0].Id;
                firstMove = false;
            }

            foreach (Entity hero in myHeroes)
            {
                if(hero.Id == attackHeroId)
                {
                    if(hero.Location.Equals(attackPositions[0]))
                    {
                        targetAttackPosition = attackPositions[1];
                    }
                    else if(hero.Location.Equals(attackPositions[1]))
                    {
                        targetAttackPosition = attackPositions[0];
                    }

                    (Entity nearestMonster, int distanceToMonster) = GetNearestEntity(monsters, hero.Location, TYPE_OP_HERO);
                    if (nearestMonster != null &&
                        distanceToMonster <= ControlCastRange &&
                        myMana >= ManaToCast &&
                        nearestMonster.Health >= 8 &&
                        !controlledMonsters.Contains(nearestMonster.Id))
                    {
                        Control(nearestMonster.Id, enemyBase, hero.Id);
                        controlledMonsters.Add(nearestMonster.Id);
                        myMana -= ManaToCast;
                        continue;
                    }

                    Move(targetAttackPosition, attackHeroId);
                    continue;
                }

                Entity targetMonster = null;
                int minDistance = Int32.MaxValue;

                for (int i = 0; i < monsters.Count; i++)
                {
                    var monster = monsters[i];

                    int distance = Distance(myBase, monster.NextLocation);
                    if (distance > BaseAutoTarget &&
                        monster.NearBase != TYPE_MY_HERO)
                    {
                        continue;
                    }

                    if (distance < minDistance)
                    {
                        targetMonster = monster;
                        minDistance = distance;
                    }
                }

                if (targetMonster == null)
                {
                    Point targetPosition = defPosition;

                    for (int i = 0; i < monsters.Count; i++)
                    {
                        var monster = monsters[i];

                        int distance = Distance(hero.Location, monster.NextLocation);
                        if (distance < minDistance)
                        {
                            targetPosition = monster.NextLocation;
                            minDistance = distance;
                        }
                    }

                    Move(targetPosition, hero.Id);
                    continue;
                }

                var distanceToTarget = Distance(hero.Location, targetMonster.Location);
                if (distanceToTarget <= ControlCastRange &&
                   minDistance <= BaseAutoTarget / 2 &&
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

    public static (Entity, int) GetNearestEntity(List<Entity> entities, Point target, int? ignoreThreatFor = null)
    {
        Entity nearestEntity = null;
        int minDistance = int.MaxValue;

        for (int i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];

            int distance = Distance(target, entity.Location);
            if (ignoreThreatFor != null &&
                entity.ThreatFor == ignoreThreatFor)
            {
                continue;
            }

            if (distance < minDistance)
            {
                nearestEntity = entity;
                minDistance = distance;
            }
        }

        return (nearestEntity, minDistance);  
    }

    public static int Distance(Point p1, Point p2) => (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

    public static int DistanceFast(Point p1, Point p2) => Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);

    public static void Debug(Point p) => Console.Error.WriteLine($"Debug point {p.X} {p.Y}");

    public static void Move(Point p, int hero) => Console.WriteLine($"MOVE {p.X} {p.Y} M{hero}");

    public static void Control(int id, Point p, int hero) => Console.WriteLine($"SPELL CONTROL {id} {p.X} {p.Y} C{hero}");
}