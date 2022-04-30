using System;
using System.Collections.Generic;
using System.Linq;

class Player
{
    public const int MapWidth = 17630;
    public const int ThreeQuartersMapWidth = 13222;
    public const int HalfMapWidth = 8815;
    public const int QuarterMapWidth = 4408;
    public const int EighthMapWidth = 2204;
    public const int TenMapWidth = 1763;

    public const int MapHeight = 9000;
    public const int ThreeQuartersMapHeight = 6750;
    public const int HalfMapHeight = 4500;
    public const int QuarterMapHeight = 2250;
    public const int EighthMapHeight = 1125;
    public const int TenMapHeight = 900;

    public const int BaseAutoTarget = 5000;

    public const int BaseVisionRange = 6000;
    public const int HeroVisionRange = 2200;

    public const int ManaToCast = 10;
    public const int ManaToControl = 50;
    public const int ManaToAttack = 80;
    public const int WindCastRange = 1280;
    public const int ControlCastRange = 2200;
    public const int ShieldCastRange = 2200;

    public const int MediumMonsterHealth = 16;
    public const int SmallMonsterHealth = 12;
    public const int VerySmallMonsterHealth = 2;

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
        public Point Prev2Location;
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
            this.Prev2Location = new Point(Math.Abs(x - vx), Math.Abs(y - vy));
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

        Point enemyBase = new Point
        {
            X = Math.Abs(myBase.X - MapWidth),
            Y = Math.Abs(myBase.Y - MapHeight)
        };

        Point[] attackPositions = 
        {
            new Point
            {
                X = Math.Abs(enemyBase.X - TenMapWidth),
                Y = Math.Abs(enemyBase.Y - HalfMapHeight)
            },
            new Point
            {
                X = Math.Abs(enemyBase.X - QuarterMapWidth),
                Y = Math.Abs(enemyBase.Y - TenMapHeight)
            }
        };

        Point[] defPositions1 = 
        {
            new Point
            {
                X = Math.Abs(myBase.X - QuarterMapWidth),
                Y = Math.Abs(myBase.Y - QuarterMapHeight)
            },            
            new Point
            {
                X = Math.Abs(myBase.X - HalfMapWidth),
                Y = Math.Abs(myBase.Y - QuarterMapHeight)
            },
        };

        Point[] defPositions2 = 
        {
            new Point
            {
                X = Math.Abs(myBase.X - EighthMapWidth),
                Y = Math.Abs(myBase.Y - HalfMapHeight)
            },
            new Point
            {
                X = Math.Abs(myBase.X - QuarterMapWidth),
                Y = Math.Abs(myBase.Y - ThreeQuartersMapHeight)
            }
        };

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());
                
        // init
        bool firstMove = true;
        int attackHeroId = 0;
        int defHeroId = 0;

        Point attackPosition = attackPositions[0];
        Point defPosition1 = defPositions1[0];
        Point defPosition2 = defPositions2[0];

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
                defHeroId = myHeroes[1].Id;
                firstMove = false;
            }

            Dictionary<Entity, double> inBaseMonsters = new Dictionary<Entity, double>();
            Dictionary<Entity, double> inAttackMonsters = new Dictionary<Entity, double>();
            Dictionary<Entity, double> allInAttackMonsters = new Dictionary<Entity, double>();
            foreach(Entity monster in monsters)
            {
                double distanceToBase = Distance(myBase, monster.Location);
                if(distanceToBase <= BaseVisionRange)
                {
                    inBaseMonsters.Add(monster, distanceToBase);
                }

                double distanceToAttack = Distance(enemyBase, monster.Location);
                if(distanceToAttack <= BaseVisionRange/2 && monster.Health > SmallMonsterHealth && monster.ShieldLife == 0)
                {
                    inAttackMonsters.Add(monster, distanceToAttack);
                }

                if(distanceToAttack <= BaseVisionRange)
                {
                    allInAttackMonsters.Add(monster, distanceToAttack);
                }          
            }

            foreach (Entity hero in myHeroes)
            {
                Entity nearestMonster = null;
                double distanceToMonster = double.MaxValue;

                // my base
                if(hero.Id != attackHeroId && inBaseMonsters.Count > 0)
                {
                    var inBaseMonsterHealth = inBaseMonsters.Sum(k => k.Key.Health);
                    var inBaseMonster = inBaseMonsters.OrderBy(k => k.Value).First().Key;
                    (Entity nearestHero, double distanceToHero) = GetNearestEntity(myHeroes, inBaseMonster.Location);

                    if (hero.Id == nearestHero.Id)
                    {
                        if(myMana >= ManaToCast &&
                           distanceToHero <= WindCastRange &&
                           inBaseMonster.ShieldLife == 0 &&
                           inBaseMonster.Health >= VerySmallMonsterHealth)
                        {
                            Wind(enemyBase, hero.Id);
                            myMana -= ManaToCast;
                        }
                        else
                        {
                            Move(inBaseMonster.NextLocation, hero.Id);
                        }
   
                        monsters.Remove(inBaseMonster);
                        continue;
                    }
                }

                // enemy base
                if(hero.Id == attackHeroId && myMana >= ManaToAttack && inAttackMonsters.Count > 0)
                {
                    var attackMonster = inAttackMonsters.OrderBy(k => k.Value).First().Key;
                    Shield(attackMonster.Id);
                    myMana -= ManaToCast;
                    continue;
                }

                if(hero.Id == attackHeroId && myMana >= ManaToAttack)
                {
                    (nearestMonster, distanceToMonster) = GetNearestEntity(monsters, enemyBase);
                    if(distanceToMonster <= BaseVisionRange)
                    {
                        var distanceToHero = Distance(hero.Location, nearestMonster.Location);
                        if(distanceToHero <= WindCastRange &&
                            nearestMonster.Health >= SmallMonsterHealth)
                        {
                            Wind(enemyBase, hero.Id);
                            myMana -= ManaToCast;
                        }
                        else
                        {
                            Move(nearestMonster.Location, hero.Id);
                        }

                        monsters.Remove(nearestMonster);
                        continue;
                    } 
                }  

                // control 
                (nearestMonster, distanceToMonster) = GetNearestEntity(monsters, hero.Location, TYPE_OP_HERO, TYPE_MONSTER);
                if (nearestMonster != null &&
                    distanceToMonster <= ControlCastRange &&
                    myMana >= ManaToControl &&
                    nearestMonster.Health >= SmallMonsterHealth)
                {
                    Control(nearestMonster.Id, enemyBase, hero.Id);
                    monsters.Remove(nearestMonster);
                    myMana -= ManaToCast;
                    continue;
                }

                // hunt
                (nearestMonster, distanceToMonster) = GetNearestEntity(monsters, hero.Location, TYPE_OP_HERO);
                if(hero.Id != attackHeroId &&
                   nearestMonster != null && 
                   distanceToMonster <= HeroVisionRange)
                {
                    Move(nearestMonster.NextLocation, hero.Id);
                    monsters.Remove(nearestMonster);
                    continue;
                }

                (nearestMonster, distanceToMonster) = GetNearestEntity(monsters, hero.Location);
                if(hero.Id == attackHeroId && myMana < ManaToAttack &&
                    nearestMonster != null && 
                    nearestMonster.ShieldLife == 0 &&
                   distanceToMonster <= HeroVisionRange)
                {
                    Move(nearestMonster.NextLocation, hero.Id);
                    monsters.Remove(nearestMonster);
                    continue;
                }                

                // patrol
                Point patrolPosition;
                if(hero.Id == attackHeroId)
                {
                    attackPosition = GetNextPosition(hero.Location, attackPositions) ?? attackPosition;
                    patrolPosition = attackPosition;
                }
                else if(hero.Id == defHeroId)
                {
                    defPosition1 = GetNextPosition(hero.Location, defPositions1) ?? defPosition1;
                    patrolPosition = defPosition1;
                }
                else
                {
                    defPosition2 = GetNextPosition(hero.Location, defPositions2) ?? defPosition2;
                    patrolPosition = defPosition2;
                }

                Move(patrolPosition, hero.Id);
            }
        }
    }

    public static Point? GetNextPosition(Point location, Point[] positions)
    {
        for(int i = 0; i < positions.Length; i++)
        {
            if(location.Equals(positions[i]))
            {
                if(i + 1 < positions.Length)
                {
                    return positions[i + 1];
                }

                return positions[0];
            }
        }

        return null;
    }

    public static (Entity, double) GetNearestEntity(List<Entity> entities, Point target, int? ignoreThreatFor = null, int? ignoreThreatFor2 = null)
    {
        Entity nearestEntity = null;
        double minDistance = double.MaxValue;

        foreach (var entity in entities)
        {
            double distance = Distance(target, entity.Location);
            if (ignoreThreatFor != null &&
                entity.ThreatFor == ignoreThreatFor)
            {
                continue;
            }

            if (ignoreThreatFor2 != null &&
                entity.ThreatFor == ignoreThreatFor2)
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

    public static double Distance(Point p1, Point p2) => Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));

    public static void Debug(Point p) => Console.Error.WriteLine($"Debug point {p.X} {p.Y}");

    public static void Move(Point p, int hero) => Console.WriteLine($"MOVE {p.X} {p.Y} M{hero}");

    public static void Control(int id, Point p, int hero) => Console.WriteLine($"SPELL CONTROL {id} {p.X} {p.Y} C{hero}");

    public static void Wind(Point p, int hero) => Console.WriteLine($"SPELL WIND {p.X} {p.Y} C{hero}");

    public static void Shield(int id) => Console.WriteLine($"SPELL SHIELD {id}");
}