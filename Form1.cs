using System.Numerics;
using System.Windows.Forms;

namespace rpg
{
    public partial class Form1 : Form
    {
        readonly PictureBox screen1 = new(); //this will be used as our main screen
        readonly RichTextBox screen2 = new(); //this will be an information area
        int bpu = 25; //bits per unit, we do not want to make this readonly as it will be our zoom level
        Bitmap canvas = new(1, 1); //we will use this to do our graphics
        Bitmap cornerul = new(1, 1); //our individual pictures for drawing the level
        Bitmap cornerur = new(1, 1);
        Bitmap cornerll = new(1, 1);
        Bitmap cornerlr = new(1, 1);
        Bitmap wallhorz = new(1, 1);
        Bitmap wallvert = new(1, 1);
        Bitmap door = new(1, 1);
        Bitmap passage = new(1, 1);
        Bitmap monsterbad = new(1, 1); //our individual pictures for drawing the creatures
        Bitmap monstergood = new(1, 1);
        Bitmap monsterneut = new(1, 1);
        Bitmap playerpic = new(1, 1);
        readonly Color backcolor = Color.Silver; //The colors used
        readonly Color backroom = Color.White;
        readonly Color foreroom = Color.Black;
        readonly Color passcolor = Color.Gray;
        int level = 1; //keep track of current level, we don't want to make this readonly as we will be adding stairs
        Size client = new(); //this is storage for client size
        bool sizeSet = false; //used to keep window size at maximum
        bool isClosing = false; //used to keep from spamming with the verification of closing
        readonly bool isDebug = false; //used to show / hide debugging messages
        readonly Character player = new();
        public Level currentlevel = new();
        readonly List<Level> levels = new();
        public Form1()
        {
            InitializeComponent(); //windows requires this
        }

        private void RPGload(object sender, EventArgs e)
        {
            Size = Screen.PrimaryScreen.Bounds.Size; //set size of window to full screen size
            Location = new(0, 0); //move to upper left corner
            client = ClientSize; //get client size
            screen1.Size = new(client.Width, client.Height - 300); //set size to maximum but leave 300 for info area
            screen1.Location = new(0, 0); //we will have no margins
            screen2.Size = new(client.Width, 300); //and set info area size to maximum
            screen2.Location = new(0, client.Height - 300); //position info area
            Controls.Add(screen1); //add screen1
            Controls.Add(screen2); //add info area
            canvas.Dispose(); //get rid of canvas
            canvas = new(screen1.Width, screen1.Height); //make a new canvas with full size of screen1
            screen1.MouseClick += Mouse; //set up handlers for mouse
            screen1.MouseMove += Mouse;
            sizeSet = true; //set flag for size being set
            SetupPlayer();
            SetupMonsters();
            //MakePictures(); //make pictures for drawing stuff  (this gets done in DisplayIt where they are actually used)
            MakeLevel(); //make level
            ActionLog.OutputBox = screen2; //set up our richtextbox for output from routines in BattleSystem and Creature, etc.
            if (isDebug)
            {
                ActionLog.Debug($"Player is at ({player.X}, {player.Y}).\n");
                ActionLog.Debug($"Canvas is {canvas.Width} by {canvas.Height}.\n");
                ActionLog.Debug($"Player's real position: ({player.X * bpu}, {player.Y * bpu}).\n");
                ActionLog.Debug($"Screen1 size is {screen1.Width} by {screen1.Height}.\n");
            }
            //
            //TODO: Here would be perhaps more start-up code
            //
        }

        private void Mouse(object? sender, MouseEventArgs e)
        {
            //
            //TODO: this will be used for any mouse interactions
            //
            //call GameLogic if we did any input
        }

        private void RPGkey(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Escape:
                    Close();
                    return; //since this shouldn't use a turn, we'll exit here
                case Keys.Up:
                case Keys.W:
                    GameSession.MoveUp = true;
                    break;
                case Keys.Left:
                case Keys.A:
                    GameSession.MoveLeft = true;
                    break;
                case Keys.Down:
                case Keys.S:
                    GameSession.MoveDown = true;
                    break;
                case Keys.Right:
                case Keys.D:
                    GameSession.MoveRight = true;
                    break;
                case Keys.Home:
                case Keys.Q:
                    GameSession.MoveUp = true;
                    GameSession.MoveLeft = true;
                    break;
                case Keys.PageUp:
                case Keys.E:
                    GameSession.MoveUp = true;
                    GameSession.MoveRight = true;
                    break;
                case Keys.End:
                case Keys.Z:
                    GameSession.MoveDown = true;
                    GameSession.MoveLeft = true;
                    break;
                case Keys.PageDown:
                case Keys.C:
                    GameSession.MoveDown = true;
                    GameSession.MoveRight = true;
                    break;
                default:
                    return; //since we didn't do anything using a turn, exit stage left
            }
            GameLogic(); //we got some input, so let's move to GameLogic
            //
            //TODO: build other keyboard commands
            //
        }

        private void RPGclosing(object sender, FormClosingEventArgs e)
        {
            if (isClosing) //if we have already responded to question with a yes
                Application.Exit(); //then exit the program
            else //otherwise
            {
                //ask the user, if user says yes
                if (MessageBox.Show("Are you sure?", "Quit?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    isClosing = true; //set flag
                    Close(); //close window
                }
                else //otherwise (user said no)
                    e.Cancel = true; //set flag for cancelling closing
            }
        }

        private void RPGresize(object sender, EventArgs e)
        {
            if (sizeSet) ClientSize = client; //refuse resize requests
        }

        private void MakePictures()
        {
            int halfbpu = bpu / 2; //set up half size (duh!)
            int penthick = bpu / 5 + 1; //set pen thickness
            Pen bpen = new(foreroom, penthick); //set up pen
            SolidBrush wbrush = new(backroom); //set up brush
            Rectangle drect = new(halfbpu / 2, halfbpu / 2, halfbpu, halfbpu); //set up door rectangle
            cornerul = MakeNewBitmap(cornerul, bpu);
            using Graphics g1 = Graphics.FromImage(cornerul); //get graphics object
            {
                g1.DrawLine(bpen, halfbpu, bpu, halfbpu, halfbpu); //draw upper left corner
                g1.DrawLine(bpen, halfbpu, halfbpu, bpu, halfbpu);
            }
            cornerur = MakeNewBitmap(cornerur, bpu);
            using Graphics g2 = Graphics.FromImage(cornerur);
            {
                g2.DrawLine(bpen, 0, halfbpu, halfbpu, halfbpu);
                g2.DrawLine(bpen, halfbpu, halfbpu, halfbpu, bpu);
            }
            cornerll = MakeNewBitmap(cornerll, bpu);
            using Graphics g3 = Graphics.FromImage(cornerll);
            {
                g3.DrawLine(bpen, halfbpu, 0, halfbpu, halfbpu);
                g3.DrawLine(bpen, halfbpu, halfbpu, bpu, halfbpu);
            }
            cornerlr = MakeNewBitmap(cornerlr, bpu);
            using Graphics g4 = Graphics.FromImage(cornerlr);
            {
                g4.DrawLine(bpen, halfbpu, 0, halfbpu, halfbpu);
                g4.DrawLine(bpen, halfbpu, halfbpu, 0, halfbpu);
            }
            wallhorz = MakeNewBitmap(wallhorz, bpu);
            using Graphics g5 = Graphics.FromImage(wallhorz);
            g5.DrawLine(bpen, 0, halfbpu, bpu, halfbpu);
            wallvert = MakeNewBitmap(wallvert, bpu);
            using Graphics g6 = Graphics.FromImage(wallvert);
            g6.DrawLine(bpen, halfbpu, 0, halfbpu, bpu);
            door = MakeNewBitmap(door, bpu);
            using Graphics g7 = Graphics.FromImage(door);
            {
                g7.FillRectangle(wbrush, drect);
                g7.DrawRectangle(bpen, drect);
            }
            passage = MakeNewBitmap(passage, bpu);
            using Graphics g8 = Graphics.FromImage(passage);
            g8.Clear(passcolor);
            monsterbad = MakeNewBitmap(monsterbad, bpu);
            using Graphics ga = Graphics.FromImage(monsterbad);
            {
                ga.FillEllipse(Brushes.Red, 0, 0, bpu, bpu);
                ga.DrawEllipse(Pens.Black, 0, 0, bpu, bpu);
            }
            monstergood = MakeNewBitmap(monstergood, bpu);
            using Graphics gb = Graphics.FromImage(monstergood);
            {
                gb.FillEllipse(Brushes.Green, 0, 0, bpu, bpu);
                gb.DrawEllipse(Pens.Black, 0, 0, bpu, bpu);
            }
            monsterneut = MakeNewBitmap(monsterneut, bpu);
            using Graphics gc = Graphics.FromImage(monsterneut);
            {
                gc.FillEllipse(Brushes.Blue, 0, 0, bpu, bpu);
                gc.DrawEllipse(Pens.Black, 0, 0, bpu, bpu);
            }
            playerpic = MakeNewBitmap(playerpic, bpu);
            using Graphics gd = Graphics.FromImage(playerpic);
            {
                Pen ppen = new(Color.Magenta, penthick);
                gd.DrawLine(ppen, 0, 0, bpu - 1, bpu - 1);
                gd.DrawLine(ppen, 0, bpu - 1, bpu - 1, 0);
            }
        }
        private static Bitmap MakeNewBitmap(Bitmap bmp, int bpusize)
        {
            bmp.Dispose();
            bmp = new(bpusize, bpusize);
            return bmp;
        }
        private void MakeLevel()
        {
            //
            //Technically, the first level is made by the time the code reaches here, however there are still things we can do here.
            //
            if (level == 1) //if we are still on the first level
            {
                levels.Clear(); //clear the levels archive
                levels.Add(currentlevel); //archive current level
            }
            else //otherwise it is time to get a new level
            {
                currentlevel = new(); //get a new level
                levels.Add(currentlevel); //go ahead and archive it
            }
            GameSession.CurrentLevel = currentlevel; //make a reference for GameSession
            DisplayIt(); //display the level
        }

        private void GameLogic()
        {
            player.TakeTurn(); //give player's turn
            foreach (Creature c in currentlevel.creatures) //for every creature
            {
                if (GameSession.InCombat && BattleSystem.Combatants.Contains(c)) //if we are currently in combat and we are in it
                {
                        ;//do we have to do anything?
                }
                else //otherwise (not in combat)
                {
                    if (c is Monster m) //if creature is a monster
                        m.WanderTurn(); //let it wander
                    else //otherwise
                        c.TakeTurn(); //let it take its turn
                }
            }
            DisplayIt(); //update the display
        }

        private void SetupPlayer()
        {
            //
            //TODO: Right now, we are just putting some values in here.  This should be giving the player a chance to decide things and
            //      set up their character the way they want to.
            //
            if (currentlevel.rooms == null) return; //we depend on rooms to have a value, so...
            Point playerpos = currentlevel.rooms[0].GetCenter(); //get center position of first room
            player.X = (double)playerpos.X; //set player there
            player.Y = (double)playerpos.Y;
            player.XP = 0; //player has no experience
            player.Alignment = "Chaotic Good"; //this should be selected maybe from a drop down list
            player.Experience = 0; //player is not worth any xp to others
            player.HPmax = player.HP = 15; //give player some hit points (should be calculated)
            player.MinimumCrit = 20; //player crits if he rolls a 20
            player.Level = 1; //player is level 1
            player.Name = "Player"; //we should let the user enter a name here
            player.NumberAttacks = 1; //this is most likely correct for a level 1 player
            player.STR = 16; //these should be randomized but adjustable by player
            player.DEX = 14;
            player.CON = 18;
            player.INT = 13;
            player.WIS = 12;
            player.CHA = 9;
            Melee m = new() { Name = "Sword", Damage = "1d10+3", DamageType = "slashing", ToHit = 5 }; //give player a sword
            Ranged rw = new() { Name = "Bow", Damage = "1d6+3", DamageType = "piercing", ToHit = 4, Range = 300 }; //and a bow
            player.MeleeAttacks.Add(m); //add sword to melee attacks
            player.RangedAttacks.Add(rw); //add bow to ranged attacks
            player.AC = 18; //this should be calculated after player gets a chance to buy armor
        }

        private void SetupMonsters()
        {
            //
            //TODO: Monsters need to be read in from a text file but for now, we'll just create them
            //
            Monster mg = new()
            {
                Name = "Fairy",
                Alignment = "Chaotic Good",
                AC = 12
            };
            mg.HPmax = mg.HP = 15;
            mg.Experience = 100;
            mg.STR = mg.DEX = mg.CON = mg.INT = mg.WIS = mg.CHA = 13;
            mg.NumberAttacks = 1;
            mg.MinimumCrit = 10;
            Melee m0 = new() { Name = "Fairy Fire", Damage = "1d20+10", DamageType = "fire", ToHit = 5 };
            Ranged r0 = new() { Name = "Fairy Shot", Damage = "1d20+10", DamageType = "force", ToHit = 5, Range = 300 };
            mg.MeleeAttacks.Add(m0);
            mg.RangedAttacks.Add(r0);
            Monster mn = new()
            {
                Name = "Sloth",
                Alignment = "True Neutral",
                AC = 9
            };
            mn.HPmax = mn.HP = 10;
            mn.Experience = 100;
            mn.STR = mn.DEX = mn.CON = mn.INT = mn.WIS = mn.CHA = 10;
            mn.NumberAttacks = 1;
            mn.MinimumCrit = 20;
            Melee m1 = new() { Name = "Claws", Damage = "1d4+1", DamageType = "slashing", ToHit = 1 };
            mn.MeleeAttacks.Add(m1);
            Monster me = new()
            {
                Name = "Orc",
                Alignment = "Chaotic Evil",
                AC = 13
            };
            me.HPmax = me.HP = 15;
            me.Experience = 100;
            me.STR = me.DEX = me.CON = me.INT = me.WIS = me.CHA = 15;
            me.NumberAttacks = 1;
            me.MinimumCrit = 20;
            Melee m2 = new() { Name = "Sword", Damage = "1d8+5", DamageType = "slashing", ToHit = 3 };
            me.MeleeAttacks.Add(m2);
            if (currentlevel.rooms == null) return; //we need rooms, so...
            foreach (Room room in currentlevel.rooms) //we will probably edit this later, but for now, we put one monster in each room
            {
                double x = Roll.DoDice(room.Wide) + room.X; //put monster somewhere in the room
                double y = Roll.DoDice(room.High) + room.Y;
                Monster m = new(); //new monster assignment
                int w = Roll.DoDice(3); //this part will be replaced when we read our monsters from text file
                if (w == 1) m = new(mg); //had to edit Monster class to allow this, but totally worth it!
                if (w == 2) m = new(mn);
                if (w == 3) m = new(me);
                m.X = x; //set position of monster
                m.Y = y;
                currentlevel.AddCreature(m); //add monster to our creatures list
            }
        }

        private void DisplayIt()
        {
            MakePictures(); //make pictures
            canvas.Dispose(); //get rid of old canvas
            canvas = new(currentlevel.maxwide * bpu, currentlevel.maxhigh * bpu); //make bitmap
            if (currentlevel.rooms == null || currentlevel.doors == null || currentlevel.passages == null) return; //if we are missing rooms, doors, or passages, return
            using Graphics gb = Graphics.FromImage(canvas); //get graphics object for bitmap
            {
                gb.Clear(backcolor); //clear to background color
                foreach (Passage pass in currentlevel.passages) //for each passage
                    foreach (Point p in pass.Points) //for each point in passage
                        gb.DrawImage(passage, p.X * bpu, p.Y * bpu, bpu, bpu); //draw the passage
                foreach (Room r in currentlevel.rooms) //for each room
                {
                    SolidBrush rbb = new(backroom); //set brush for room insides
                    gb.FillRectangle(rbb, r.X * bpu + bpu / 2, r.Y * bpu + bpu / 2, (r.Wide - 1) * bpu, (r.High - 1) * bpu); //clear the inside of room
                    for (int y = r.Y + 1; y < r.Y + r.High - 1; y++) //for each y value
                    {
                        gb.DrawImage(wallvert, r.X * bpu, y * bpu, bpu, bpu); //draw left wall
                        gb.DrawImage(wallvert, (r.X + r.Wide - 1) * bpu, y * bpu, bpu, bpu); //draw right wall
                    }
                    for (int x = r.X + 1; x < r.X + r.Wide - 1; x++) //for each x value
                    {
                        gb.DrawImage(wallhorz, x * bpu, r.Y * bpu, bpu, bpu); //draw top wall
                        gb.DrawImage(wallhorz, x * bpu, (r.Y + r.High - 1) * bpu, bpu, bpu); //draw bottom wall
                    }
                    gb.DrawImage(cornerul, r.X * bpu, r.Y * bpu, bpu, bpu); //draw corners
                    gb.DrawImage(cornerur, (r.X + r.Wide - 1) * bpu, r.Y * bpu, bpu, bpu);
                    gb.DrawImage(cornerll, r.X * bpu, (r.Y + r.High - 1) * bpu, bpu, bpu);
                    gb.DrawImage(cornerlr, (r.X + r.Wide - 1) * bpu, (r.Y + r.High - 1) * bpu, bpu, bpu);
                }
                foreach (Door d in currentlevel.doors) //for each door
                    gb.DrawImage(door, d.X * bpu, d.Y * bpu, bpu, bpu); //draw door
                foreach (Creature c in currentlevel.creatures) //for each monster
                {
                    if (c.Alignment.Contains("vil")) //bad monster
                        gb.DrawImage(monsterbad, (float)c.X * bpu, (float)c.Y * bpu, bpu, bpu); //draw it
                    if (c.Alignment.Contains("eutral")) //neutral monster
                        gb.DrawImage(monsterneut, (float)c.X * bpu, (float)c.Y * bpu, bpu, bpu); //draw it
                    if (c.Alignment.Contains("ood")) //good monster
                        gb.DrawImage(monstergood, (float)c.X * bpu, (float)c.Y * bpu, bpu, bpu); //draw it
                }
                gb.DrawImage(playerpic, (float)player.X * bpu, (float)player.Y * bpu, bpu, bpu); //draw player
            }
            Bitmap viewport = new(screen1.Width, screen1.Height); //set up viewport
            using Graphics vg = Graphics.FromImage(viewport); //using graphics object for viewport
            {
                //This should (hopefully) center the player on the screen
                Rectangle src = new((int)player.X * bpu - screen1.Width / 2, (int)player.Y * bpu - screen1.Height / 2, screen1.Width, screen1.Height);
                Rectangle dst = new(0, 0, screen1.Width, screen1.Height);
                vg.DrawImage(canvas, dst, src, GraphicsUnit.Pixel);
            }
            screen1.Image = viewport; //set picturebox to use bitmap as its image
            screen1.Invalidate(); //tell windows to refresh the picturebox
            MessageBox.Show("Done", "Done", MessageBoxButtons.OK);
        }
    }
}
