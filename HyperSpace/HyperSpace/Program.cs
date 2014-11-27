using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HyperSpace
{
    class Program
    {
        static void Main(string[] args)
        {
            HyperSpace game = new HyperSpace();
            game.PlayGame();
        }
    }

    /// <summary>
    /// A single unit for a Hyperspace game.
    /// </summary>
    class Unit
    {
        public enum Direction { Up, Down }
        public enum Power { Ammo, Speed, Health, Random }


        // ---- Properties ----


        public int X { get; set; }
        public int Y { get; set; }
        public Direction Dir { get; set; }
        
        public ConsoleColor Color { get; set; }
        public string Symbol { get; set; }

        public bool IsPowerUp { get; set; }
        public Power PowerUpType { get; set; }


        // ---- Variables ---- 


        static List<string> ObstacleList = "! . : ; ' \"".Split().ToList(); //All of the possible obsticles.
        static Random rng = new Random();


        // ---- Constructors ---- 


        /// <summary>
        /// Sets this unit as an obsticle.
        /// </summary>
        /// <param name="y"></param>
        /// <param name="x"></param>
        /// <param name="dir"></param>
        public Unit(int x, int y, Direction dir)
        {
            this.X = x;
            this.Y = y;
            this.Dir = dir;

            this.Color = ConsoleColor.Cyan;
            this.IsPowerUp = false;

            if (dir == Direction.Up) //Player bullets travel upwards.
            {
                this.Symbol = "o";
                this.Color = ConsoleColor.Magenta;
            }
            else
                this.Symbol = ObstacleList[rng.Next(ObstacleList.Count)]; //Random symbol from the ObstacleList

            }

        /// <summary>
        /// Set this unit as a power up.
        /// </summary>
        /// <param name="x">The X position of the unit.</param>
        /// <param name="y">The Y position of the unit.</param>
        /// <param name="power">The type of power-up that this unit gives.</param>
        public Unit(int x, int y, Power power)
        {
            this.Y = y;
            this.X = x;
            this.Dir = Direction.Down; //Powers always travel toward the player.

            this.Symbol = "@";
            this.IsPowerUp = true;

            if (power == Power.Random)
                this.PowerUpType = (Power)rng.Next(4); //Number between 0 and 3 to set a power.

            switch (this.PowerUpType)
            {
                case Power.Ammo:
                    this.Color = ConsoleColor.White;
                    break;
                case Power.Speed:
                    this.Color = ConsoleColor.Green;
                    break;
                case Power.Health:
                    this.Color = ConsoleColor.Red;
                    break;
            }
        }

        /// <summary>
        /// Creates a custom unit.
        /// </summary>
        /// <param name="x">X position of the unit.</param>
        /// <param name="y">Y position of the unit.</param>
        /// <param name="color">Color of the unit.</param>
        /// <param name="symbol">Symbol that represents the unit.</param>
        public Unit(int x, int y, ConsoleColor color, string symbol)
        {
            this.X = x;
            this.Y = y;
            this.Color = color;
            this.Symbol = symbol;
        }


        // ---- Methods ----


        /// <summary>
        /// Make this unit print itself to the console.
        /// </summary>
        public void Draw()
        {
            Console.SetCursorPosition(this.X, this.Y);
            Console.ForegroundColor = this.Color;
            Console.Write(this.Symbol);
        }

        public void ShipCollision(Ship player)
        {
            if (this.IsPowerUp && this.Symbol != " ")
            {
                switch (PowerUpType)
                {
                    case Power.Ammo:
                        player.Ammo += 10;
                        break;
                    case Power.Speed:
                        player.Speed -= 50;
                        break;
                    case Power.Health:
                        player.Health += 20;
                        break;
                }
            }
            else
                player.Health -= 20;

            this.Symbol = " ";
        }
    }

    class Ship : Unit
    {
        //Properties
        public int Speed { get; set; }
        public int Health { get; set; }
        public int Ammo { get; set; }

        
        public bool IsAlive
        {
            get
            {
                if (this.Health > 0)
                    return true;

                return false;
            }
        }
        

        //---- Constructor ----


        public Ship(int x, int y, ConsoleColor color, string symbol) : base(x, y, color, symbol)
        {
            this.Speed = 0;
            this.Health = 100;
            this.Ammo = 20;
        }
    }

    
    class Explosion : Unit
    {
        public int Frame { get; set; }

        public Explosion(int x, int y) : base(x, y, ConsoleColor.White, "X")
        {
            this.Frame = 1;
            Console.Beep(800, 50);
        }

        public void NextFrame()
        {
            Frame++;

            switch (Frame)
            {
                //First frame is set in the constructor.
                case 2:
                    this.Color = ConsoleColor.Yellow;
                    this.Symbol = "#";
                    break;
                case 3:
                    this.Color = ConsoleColor.Red;
                    this.Symbol = "x";
                    break;
                case 4:
                    this.Symbol = " ";
                    break;
            }
        }
    }

    class HyperSpace
    {


        // ---- Properties ----


        int Score { get; set; }
        List<Unit> ObstacleList { get; set; }
        List<Unit> BulletList { get; set; }
        List<Explosion> ExplosionList { get; set; }
        Ship SpaceShip { get; set; }


        // ---- Variables ----


        private Random rng = new Random();


        // ---- Constructor ----


        /// <summary>
        /// Creates an instance of the game HyperSpace.
        /// </summary>
        public HyperSpace()
        {
            //Window settings
            Console.BufferHeight = Console.WindowHeight = 30;
            Console.BufferWidth = Console.WindowWidth = 60;

            //Player settings
            SpaceShip = new Ship((Console.WindowWidth / 2) - 1, Console.WindowHeight - 6, ConsoleColor.Red, "^");
            SpaceShip.Speed = 0;

            //Game settings
            ObstacleList = new List<Unit>();
            BulletList = new List<Unit>();
            ExplosionList = new List<Explosion>();
            this.Score = 0;
        }

        public void PlayGame()
        {
            bool playAgain = true;

            do
	        {
                //Set initial conditions.
                ConsoleKey input = ConsoleKey.L; //Unused key.
                SpaceShip.Health = 100;
                SpaceShip.Speed = 0;
                SpaceShip.Ammo = 20;
                SpaceShip.Symbol = "^";
                SpaceShip.X = Console.WindowWidth / 2 - 1;
                SpaceShip.Y = Console.WindowHeight - 6;
                Score = 0;
                int deathFrames = 50;

                do
                {
                    //10% chance to spawn a power-up.
                    if (rng.Next(11) < 9) //Not a power up.
                        this.ObstacleList.Add(new Unit(rng.Next(Console.WindowWidth - 2), 0, Unit.Direction.Down));
                    else //Is a power up.
                        this.ObstacleList.Add(new Unit(rng.Next(Console.WindowWidth - 2), 0, Unit.Power.Random));

                    MoveShip();
                    MoveObstacles();
                    DrawGame();

                    if (this.SpaceShip.Speed < 170)
                        this.SpaceShip.Speed++;

                    if (this.SpaceShip.Speed < 0)
                        this.SpaceShip.Speed = 0;

                    Thread.Sleep(190 - SpaceShip.Speed);
                
                } while (SpaceShip.IsAlive);

                do //Death animation.
                {
                    if(deathFrames > 10)
                        ExplosionList.Add(new Explosion(
                            rng.Next(SpaceShip.X - 1, SpaceShip.X + 2), rng.Next(SpaceShip.Y - 1, SpaceShip.Y + 2)));

                    deathFrames--;
                    MoveObstacles();
                    DrawGame();
                }while (deathFrames > 0);

                SpaceShip.Symbol = " ";
                SpaceShip.Draw();

                Console.ForegroundColor = ConsoleColor.Green;

                Console.SetCursorPosition(20, 10);
                Console.Write("--------------------");
                Console.SetCursorPosition(20, 11);
                Console.Write("|                  |");
                Console.SetCursorPosition(20, 12);
                Console.Write("|    Game Over!    |");
                Console.SetCursorPosition(20, 13);
                Console.Write("|    Try again?    |");
                Console.SetCursorPosition(20, 14);
                Console.Write("|       (Y\\N)      |");
                Console.SetCursorPosition(20, 15);
                Console.Write("|                  |");
                Console.SetCursorPosition(20, 16);
                Console.Write("--------------------");

                do
                {
                    input = Console.ReadKey(true).Key;
                    if(input == ConsoleKey.Y)
                        playAgain = true;
                    else
                        playAgain = false;
                } while (!(input == ConsoleKey.Y || input == ConsoleKey.N));

	        } while (playAgain == true);
        }

        public void MoveShip()
        {
            ConsoleKey keyPressed = ConsoleKey.L; //An unused key.

            if (Console.KeyAvailable)
                keyPressed = Console.ReadKey(true).Key; //Gets keypress if there is a key being pressed.

            while (Console.KeyAvailable) //Stops the system from queueing key presses.
                Console.ReadKey(true);

            switch(keyPressed)
            {
                case ConsoleKey.UpArrow:
                    if(this.SpaceShip.Y > 2)
                        this.SpaceShip.Y--;
                    break;

                case ConsoleKey.DownArrow:
                    if (this.SpaceShip.Y < Console.WindowHeight - 6)
                        this.SpaceShip.Y++;
                    break;
                
                case ConsoleKey.LeftArrow:
                    if (this.SpaceShip.X > 0)
                        this.SpaceShip.X--;
                    break;

                case ConsoleKey.RightArrow:
                    if (this.SpaceShip.X < Console.WindowWidth - 2)
                        this.SpaceShip.X++;
                    break;

                case ConsoleKey.Spacebar:
                    if (this.SpaceShip.Ammo > 0)
                    {
                        Console.Beep(5000, 50);
                        this.BulletList.Add(new Unit(SpaceShip.X, SpaceShip.Y - 1, Unit.Direction.Up));
                        this.SpaceShip.Ammo--;
                    }
                    break;
            }
        }

        public void MoveObstacles()
        {
            List<Unit> newObstacleList = new List<Unit>();
            List<Unit> newBulletList = new List<Unit>();
            List<Explosion> newExplosionList = new List<Explosion>();

            //Obstacles
            foreach(Unit i in this.ObstacleList)
            {
                i.Y++;

                if (i.X == this.SpaceShip.X && i.Y == this.SpaceShip.Y)//Something hit our ship.
                {
                    i.ShipCollision(this.SpaceShip); //Apply hit effect
                    if (!i.IsPowerUp)
                        newExplosionList.Add(new Explosion(i.X, i.Y - 1)); //Explosion here if not a power up.
                    else
                        Console.Beep(3000, 50);
                }

                if (i.Y < Console.WindowHeight - 5) //Add to list if we haven't passed the obstacle.
                    newObstacleList.Add(i);
                else
                    this.Score++; //Add to score if we have.
            }

            for (int i = 0; i < BulletList.Count; i++) //Foreach loops don't keep track of index position.
            {
                //Any obstacles that do or will match the position of the bullet?
                if(newObstacleList.Any(x => x.X == BulletList[i].X && (x.Y == BulletList[i].Y || x.Y == BulletList[i].Y -1))) 
                {
                    //Remove them.
                    newObstacleList.RemoveAll(x => x.X == BulletList[i].X && (x.Y == BulletList[i].Y || x.Y == BulletList[i].Y -1));
                    newExplosionList.Add(new Explosion(BulletList[i].X, BulletList[i].Y - 1)); //Explosion here.
                    BulletList.RemoveAt(i);

                    //Add to player score.
                    Score += 20;
                }
                else
                {
                    //Move the bullet.
                    BulletList[i].Y--;
                    if (BulletList[i].Y > 0)
                        newBulletList.Add(BulletList[i]);
                }
            }

            foreach(Explosion i in ExplosionList)
            {
                i.NextFrame();
                if (i.Frame < 4)
                    newExplosionList.Add(i);
            }

            ExplosionList = newExplosionList;
            ObstacleList = newObstacleList; //New list has no out of bounds units.
            BulletList = newBulletList;
        }

        public void DrawGame()
        {
            Console.Clear();
            SpaceShip.Draw();

            foreach (Unit i in ObstacleList)
                i.Draw();

            foreach (Unit i in BulletList)
                i.Draw();

            foreach (Explosion i in ExplosionList)
            {
                //Protect against player death explosions going outside the bounds of the window.
                if (i.X > 0 && i.X < Console.WindowWidth - 2 && i.Y > 0 && i.Y < Console.WindowHeight - 5)
                    i.Draw();
            }
            PrintAtPosition(0, Console.WindowHeight - 5, "------------------------------------------------------------", ConsoleColor.White);
            PrintAtPosition(10, Console.WindowHeight - 3, "Score: " + this.Score, ConsoleColor.Green);
            PrintAtPosition(10, Console.WindowHeight - 2, "Speed: " + this.SpaceShip.Speed, ConsoleColor.Green);
            PrintAtPosition(30, Console.WindowHeight - 3, "Health: " + this.SpaceShip.Health, ConsoleColor.Green);
            PrintAtPosition(30, Console.WindowHeight - 2, "Ammo: " + this.SpaceShip.Ammo, ConsoleColor.Green);
        }

        public void PrintAtPosition(int x, int y, string text, ConsoleColor color)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            Console.Write(text);
        }

    }
}