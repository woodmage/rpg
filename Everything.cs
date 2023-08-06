using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace rpg
{
    public static class ActionLog
    {
        public static RichTextBox? OutputBox { get; set; }

        public static void Debug(string message)
        {
            OutputBox?.AppendText($"DEBUG: {message}\n");
        }

        public static void AddMessage(string message)
        {
            OutputBox?.AppendText(message);
        }

        public static void Clear()
        {
            OutputBox?.Clear();
        }

        public static void LogCombat(Creature attacker, Creature defender, int damage)
        {
            AddMessage($"{attacker.Name} hits {defender.Name} for {damage} damage!\n");
        }
    }

    public static class GameSession
    {
        public static Level? CurrentLevel { get; set; }
        public static Creature? Player { get; set; }

        public static Creature? SelectedTarget { get; set; }

        public static bool InCombat { get; set; }

        public static bool InventoryOpen { get; set; }
        public static bool MoveUp { get; set; }
        public static bool MoveDown { get; set; }
        public static bool MoveLeft { get; set; }
        public static bool MoveRight { get; set; }
        
        public static Vector2 GetMovementVector()
        {
            return new Vector2(
                (MoveRight ? 1 : 0) - (MoveLeft ? 1 : 0),
                (MoveDown ? 1 : 0) - (MoveUp ? 1 : 0)
                );
        }

        public static void ClearMovement()
        {
            MoveUp = MoveDown = MoveLeft = MoveRight = false;
        }

        public static void SelectTarget(Creature target)
        {
            SelectedTarget = target;
        }

        public static void ToggleInventory()
        {
            InventoryOpen = !InventoryOpen;
        }

        public static bool IsEnemy(Creature creature)
        {
            // Check if creature is an enemy of player
            if (creature.Alignment.ToLower().Contains("evil"))
                return true;
            else
                return false;
        }
    }
    public abstract class Attack //base attack class
    {
        public string Name 
        {   get { return _name; }
            set { _name = value; }
        }
        public int ToHit
        {
            get { return _tohit; }
            set { _tohit = value; }
        }
        public string Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }
        public string DamageType
        {
            get { return _damagetype; }
            set { _damagetype = value; }
        }
        private string _name = string.Empty; //name of attack
        private int _tohit; //plus to hit
        private string _damage = string.Empty; //string for dice rolls
        private string _damagetype = string.Empty; //type of damage
        public Attack() { }
    }
    public class Melee : Attack //simple melee attack is equivalent to attack
    {
        public Melee() { }
    }
    public class Ranged : Attack
    {
        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }
        private int _range; //ranged attacks need a range (duh!)
        public Ranged() { }
    }
    public class ComplexMelee
    {
        public List<Melee> MeleeAttack
        {
            get { return _melee; }
            set { _melee = value; }
        }
        private List<Melee> _melee = new();
        public ComplexMelee() { }
    }
    public class ComplexRanged
    {
        public List<Ranged> RangedAttack
        {
            get { return _ranged; }
            set { _ranged = value; }
        }
        private List<Ranged> _ranged = new();
        public ComplexRanged() { }
    }
    public abstract class Everything //Our base class
    {
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _name = string.Empty; //what this thing is called
        public double X
        { 
            get { return _x; }
            set { _x = value; }
        }
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }
        private double _x, _y; //position or -1 and -1 if inside something, held, worn, etc.
        public Everything() { } //default constructor
    }

    public enum ItemType
    {
        Gold,
        Armor,
        Weapon,
        Potion,
        Scroll, //etc
    };

    public class GameItem : Everything //for any type of object
    {
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private int _value = 0; //gold piece value of object
        public ItemType Type { get; set; } //to keep track of what type item is the gameitem
        public GameItem() { } //default constructor
    }
    
    public class Armor : GameItem
    {
        public int Bonus
        {
            get { return _bonus; }
            set { _bonus = value; }
        }
        public int AC { get; set; } //this value will adjust character's AC
        private int _bonus = 0; //this value will add to the AC
        public Armor() { } //default constructor
    }

    public class Weapon : GameItem
    {
        public string Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }
        public int Bonus
        {
            get { return _bonus; }
            set { _bonus = value; }
        }
        public string DamageType
        {
            get { return _damagetype; }
            set { _damagetype = value; }
        }
        public int Range
        {
            get { return _range; }
            set { _range = value; }
        }
        private string _damage = string.Empty; //this value will be used for the "#d#" part of the damage string
        private int _bonus = 0; //this value will be added to any strength or dexterity bonuses that apply to the weapon
        private string _damagetype = string.Empty; //this will be the type of damage it does (slashing, piercing, fire, etc)
        private int _range = 0; //this value will be used for ranged weapons and right now as 0 indicates a melee weapon
        public Weapon() { }
        public string GetDamage(int ability)
        {
            string buf = _damage;
            int plusamt = Roll.AbilityBonus(ability) + _bonus;
            buf += plusamt.ToString();
            return buf;
        }
    }

    public class Chest
    {
        public List<GameItem> Items { get { return _gameitem; } set { _gameitem = value; } }
        private List<GameItem> _gameitem = new();
        public Chest() { }
    }

    public abstract class Creature : Everything
    {
        public int Experience { get { return _experience; } set { _experience = value; } }
        private int _experience = 0; //experience from killing monster
        public int HP { get { return _hp; } set { _hp = value; } }
        public int HPmax { get { return _hpmax; } set { _hpmax = value; } }
        public int AC { get { return _ac; } set { _ac = value; } }
        public int STR { get { return _STR; } set { _STR = value; } }
        public int DEX { get { return _DEX; } set { _DEX = value; } }
        public int CON { get { return _CON; } set { _CON = value; } }
        public int INT { get { return _INT; } set { _INT = value; } }
        public int WIS { get { return _WIS; } set { _WIS = value; } }
        public int CHA { get { return _CHA; } set { _CHA = value; } }
        public string Alignment { get { return _alignment; } set { _alignment = value; } }
        public int NumberAttacks { get { return _numberattacks; } set { _numberattacks = value; } }
        public int MinimumCrit { get { return _minimumcrit; } set { _minimumcrit = value; } }
        private int _hp, _hpmax, _ac; //creatures have hp and ac
        private int _STR, _DEX, _CON, _INT, _WIS, _CHA; //creatures have ability scores
        private string _alignment = string.Empty; //creatures (may) have an alignment
        private int _numberattacks = 1; //number of attacks creature gets
        private int _minimumcrit = 20; //minimum d20 roll to get a crit
        public int TeamID { get; set; } // New property for teams of creatures
        public List<Ranged> RangedAttacks
        {
            get { return _ranged; }
            private set { _ranged = value; }
        }
        public List<Melee> MeleeAttacks
        {
            get { return _melee; }
            private set { _melee = value; }
        }
        private List<Ranged> _ranged = new(); //list of ranged attacks
        private List<Melee> _melee = new(); //list of melee attacks
        public Creature() { } //default constructor
        public Attack ChooseAttack(Creature target) //here we have our choose attack method that returns an attack
        {
            //
            //TODO: Still need to verify target is within range of ranged attack
            //
            Polar distance = Polar.FromRect(target.X - X, target.Y - Y); //get a polar from target position
            if (distance.Radius <= 1) //if it is within 5'
            {
                int attackindex = Roll.DoDice(MeleeAttacks.Count); //get a random melee attack
                return MeleeAttacks[attackindex]; //return that attack
            }
            return RangedAttacks[Roll.DoDice(RangedAttacks.Count)]; //return a random ranged attack
        }
        public (int, List<int>) DamageRoll(Creature target) //damage roll method
        {
            List<int> rolls = new(); //list of dice rolls
            string damageresult = Roll.ParseDamageRoll(ChooseAttack(target).Damage, rolls); //get damage result from rolling for damage
            _ = int.TryParse(damageresult, out int result); //turn result into an integer
            return (result, rolls); //return result and list of dice rolls
        }
        public bool TakeDamage(int damage)
        {
            HP -= damage; //take damage to hit points
            return HP <= 0; //return true if dead/unconscious
        }
        public bool IsDead() => HP <= 0;
        public int RollInitiative()
        {
            return Roll.DoDice(20) + Roll.AbilityBonus(DEX); // Roll dice and return initiative
            //
            //TODO: note that some creatures could have advantage on initiative rolls, but I am quite willing to ignore that for now.
            //
        }
        public abstract void TakeTurn();
        public static void Defeated() //defeated method
        {
            //
            //TODO: we will fill this in later.  For now check that _hp > 0 for attackers before attacking.
            //
            //ActionLog.AddMessage(_name + " has been defeated!");
        }
        public void MoveCreature(Vector2 move)
        {
            if (GameSession.CurrentLevel == null) return; //we need currentlevel
            if (GameSession.CurrentLevel.ValidateMove(X, Y, move) == false) //if we don't run into anything
                Translate(move); //go ahead and move there
            Creature? creature = GameSession.CurrentLevel.CreatureThere(X, Y, move); //get creature there if there is one
            if (creature != null) //if there is a creature there
                BattleSystem.Attack(this, creature); //we will attack the creature there
            GameSession.ClearMovement();
            //otherwise we hit a wall and need to go elsewhere
        }
        public void Translate(Vector2 move)
        {
            X += (double)move.X; //move
            Y += (double)move.Y;
        }
    }

    public class Monster : Creature
    {
        public Chest? Inventory { get { return _inventory; } set { _inventory = value; } }
        private Chest? _inventory; //what the monster has on it (if anything)
        public Monster() { }
        public Monster(Monster m)
        {
            Name = m.Name; //make a copy of everything
            X = m.X;
            Y = m.Y;
            Experience = m.Experience;
            HP = m.HP;
            HPmax = m.HPmax;
            AC = m.AC;
            STR = m.STR;
            DEX = m.DEX;
            CON = m.CON;
            INT = m.INT;
            WIS = m.WIS;
            CHA = m.CHA;
            Alignment = m.Alignment;
            NumberAttacks = m.NumberAttacks;
            MinimumCrit = m.MinimumCrit;
            TeamID = m.TeamID;
            MeleeAttacks.Clear();
            foreach (Melee ma in m.MeleeAttacks)
                MeleeAttacks.Add(ma);
            RangedAttacks.Clear();
            foreach (Ranged ra in m.RangedAttacks)
                RangedAttacks.Add(ra);
            Inventory?.Items.Clear();
            if (m == null || m.Inventory == null || m.Inventory.Items == null) return; //we need Inventory.Items, so...
            foreach (GameItem gi in m.Inventory.Items)
                Inventory?.Items.Add(gi);
        }
        public override void TakeTurn()
        {
            // Get nearest target
            Creature target = GetNearestTarget();

            if (target == null)
            {
                // No targets in range

                // Get angle toward nearest target
                Polar angleToTarget = GetAngleToTarget();

                // 20% chance to move randomly instead
                if (Roll.DoDice(100) < 20) //I prefer using d100 if getting a percent chance
                {
                    angleToTarget.Angle = Roll.DoDice(360) * 2 * Math.PI / 360;  //changed this to allow for 360 degrees of movement
                }

                // Set orientation and move forward 1 space
                OrientAndMove(angleToTarget);

            }
            else
            {
                // Attack target
                BattleSystem.Attack(this, target);
            }
        }

        Polar GetAngleToTarget()
        {
            // Find nearest target
            Creature nearest = GetNearestTarget();

            // Get vector to nearest target
            Polar toTarget = Polar.FromRect(nearest.X - X, nearest.Y - Y);

            // Return angle to target
            return toTarget; //we need to return the entire polar coordinate
        }

        void OrientAndMove(Polar angle)
        {
            //I like this better anyway ;)
            angle.Radius = 1; //one space
            Vector2 move = new((float)angle.X(), (float)angle.Y()); //convert to a Vector2 for moving
            MoveCreature(move); //pass movement value to Creature.MoveCreature to execute move
            //X += angle.X(); //add X value
            //Y += angle.Y(); //add Y value
        }

        Creature GetNearestTarget()
        {
            Creature target = new Monster(); //had to do initialization to keep VS from complaining during the return in a bit
            Polar pos = new(99999, 0); //set up an enormous distance
            foreach (Creature creature in BattleSystem.Combatants) //go through the list of combatants
            {
                Polar newpos = Polar.FromRect(X - creature.X, Y - creature.Y); //make a polar from creature's position
                newpos.Radius = Math.Abs(newpos.Radius); //no negatives allowed
                if (newpos.Radius < pos.Radius) //if distance is less than recorded
                {
                    target = creature; //set target to this creature
                    pos = newpos; //set pos to new position
                }
            }
            return target; //return the target creature
        }

        public void WanderTurn()
        {
            //
            //TODO: Flesh this out so that creature can wander about, taking items off floor and dropping items, etc.
            //
        }

        public bool ShouldJoinCombat()
        {
            //
            //TODO: We need to have actual decision making here, though for now...
            //
            return true;
        }
    }

    public class Character : Creature
    {
        public string Class { get { return _class; } set { _class = value; } }
        public int XP { get { return _xp; } set { _xp = value; } }
        public int Level { get { return _level; } set { _level = value; } }
        public Chest? Inventory { get { return _inventory; } set { _inventory = value; } }
        private string _class = string.Empty; //characters get a class
        private int _xp, _level; //characters get xp and level instead of experience
        private Chest? _inventory; //characters get inventory too.
        //here are our XP requirements for levels
        private readonly int[] xpLevels = {      0,    300,    900,   2700,   6500,  14000,  23000,  34000,  48000,  64000,
                                             85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000 };
        public Character() { }
        public override void TakeTurn()
        {
            //
            //TODO: and we will need some way for the user's action choices to be used here, but we will get that later
            //
            if (GameSession.MoveUp || GameSession.MoveDown || GameSession.MoveLeft || GameSession.MoveRight)
                MoveCreature(GameSession.GetMovementVector());
        }

        public void AwardXP(int xp)
        {
            XP += xp;
            CheckForLevelUp();
        }

        private void CheckForLevelUp()
        {
            // Check if XP threshold reached for next level
            if (XP >= xpLevels[Level])
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            // Handles leveling up logic
            Level++;
            ActionLog.AddMessage($"{Name} reached level {Level}!\n");

            // Stat increases, etc
        }


        public void Equip(GameItem item)
        {
            if (item.Type == ItemType.Armor)
            {
                Armor armor = (Armor)item;
                AC = Roll.AbilityBonus(DEX) + armor.AC; //this needs improvement since if armor is heavy armor, no dex bonus and
                                                        //if armor is medium, dex bonus is limited to 2, but this will do for now.
                ActionLog.AddMessage(Name + " put on " + armor.Name + ".\n");
            }
            if (item.Type == ItemType.Weapon)
            {
                Weapon weapon = (Weapon)item;
                //The following definitely needs work!  For one thing, strength score may not be the relevant damage addition,
                //and dexterity may not be relevant to hitting, so...
                if (weapon.Range == 0) //if it is a melee weapon
                    MeleeAttacks.Add(new Melee() //add melee weapon
                    {
                        Name = item.Name,
                        DamageType = weapon.DamageType,
                        ToHit = Roll.AbilityBonus(DEX) + weapon.Bonus,
                        Damage = weapon.GetDamage(STR)
                    });
                else //otherwise
                    RangedAttacks.Add(new Ranged() //add ranged weapon
                    {
                        Name = item.Name,
                        DamageType = weapon.DamageType,
                        ToHit = Roll.AbilityBonus(DEX) + weapon.Bonus,
                        Damage = weapon.GetDamage(DEX),
                        Range = weapon.Range
                    });
                ActionLog.AddMessage(Name + " is now wielding " + item.Name + ".\n");
            }
        }
        public void Unequip(GameItem item)
        {
            if (item.Type == ItemType.Armor)
            {
                AC = 10 + Roll.AbilityBonus(DEX); //for now we are assuming there is only one armor being worn at a time and
                                                  //if you take it off, you are back to 10 + dexterity bonus for ac
                ActionLog.AddMessage(Name + " took off " + item.Name + ".\n");
            }
            if (item.Type == ItemType.Weapon)
            {
                Weapon weapon = (Weapon)item;
                if (weapon.Range == 0) //if it is a melee weapon
                {
                    Melee? melee = MeleeAttacks.Find(x => string.Equals(x.Name, item.Name)); //Find MeleeAttacks item that matches name of weapon
                    if (melee == null) //if it didn't exist, tell the user
                        MessageBox.Show(item.Name + " was not equipped!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else //otherwise
                    {
                        MeleeAttacks.Remove(melee); //remove that melee attack from the possible attacks list
                        ActionLog.AddMessage(Name + " is no longer wielding " + item.Name + ".\n");
                    }
                }
                else //otherwise (it is a ranged weapon
                {
                    Ranged? ranged = RangedAttacks.Find(x => string.Equals(x.Name, item.Name)); //find RangedAttacks item with same name
                    if (ranged == null) //if it didn't exist, tell the user
                        MessageBox.Show(item.Name + " was not equipped!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    else //otherwise
                    {
                        RangedAttacks.Remove(ranged); //remove that ranged attack from the possible attacks list
                        ActionLog.AddMessage(Name + " is no longer wielding " + item.Name + ".\n");
                    }
                }
            }
        }
    }

    public static class BattleSystem
    {
        private static int TotalXP = 0;
        public static List<Creature> Combatants { get { return combatants; } set { combatants = value; } }
        private static List<Creature> combatants = new();
        public static void StartCombat(List<Creature> team1, List<Creature> team2)
        {
            //List<Creature> combatants = new();
            GameSession.InCombat = true; //set flag to show combat has begun
            // Set up teams
            int teamId = 1;
            foreach (Creature c in team1)
            {
                c.TeamID = teamId;
            }

            teamId++;

            foreach (Creature c in team2)
            {
                c.TeamID = teamId;
            }

            // Add all creatures to master list
            combatants.Clear();
            combatants.AddRange(team1);
            combatants.AddRange(team2);

            // Sort by initiative - this is really cool LINQ code here that sorts combatants by what initiative rolls they make!  :)
            combatants = combatants.OrderBy(c => c.RollInitiative()).ToList();
            NextRound();
        }

        public static void EndCombat()
        {
            var characters = combatants.OfType<Character>();

            // Divide XP evenly
            int sharedXP = TotalXP / characters.Count();

            // Award each
            foreach (var ch in characters)
            {
                ch.AwardXP(sharedXP);
            }
            TotalXP = 0;
            GameSession.InCombat = false; //reset flag to show combat is over
        }

        public static void NextRound()
        {
            // Sort by initiative - this is really cool LINQ code here that sorts combatants by what initiative rolls they make!  :)
            combatants = combatants.OrderBy(c => c.RollInitiative()).ToList();
            foreach (Creature combatant in combatants) //for each combatant
                combatant.TakeTurn(); //have it take its turn
            int t1 = combatants.Count(c => c.TeamID == 1); //get # members of team 1
            int t2 = combatants.Count(c => c.TeamID == 2); //get # members of team 2
            if ((t1 == 0) || (t2 == 0)) //if no members of team 1 or no members of team 2
                EndCombat(); //end combat
            else //otherwise
                NextRound(); //move on to the next round
        }

        public static void AddCreature(Creature newcreature)
        {
            //TODO: Important!  We need to figure out which team to put newcreature on!
            combatants.Add(newcreature); //add new creature to combatants
        }

        public static int Attack(Creature attacker, Creature defender)
        {
            if (!GameSession.InCombat)
            {
                List<Creature> creatures1 = new() { attacker };
                List<Creature> creatures2 = new() { defender };
                StartCombat(creatures1, creatures2);
            }

            // Roll attack
            int actualRoll = Roll.DoDice(20);
            int attackRoll = actualRoll + attacker.MeleeAttacks[0].ToHit;
            ActionLog.AddMessage(attacker.Name + " attacks " + defender.Name + " ");

            // Check if hit
            if (attackRoll >= defender.AC)
            {
                // Roll damage
                int damageTotal;
                List<int> damageRolls;
                (damageTotal, damageRolls) = attacker.DamageRoll(defender);

                // Check if crit
                if (actualRoll >= attacker.MinimumCrit)
                {
                    damageTotal += damageRolls.Sum();  //this is all we need to do to add the rolls to the total again,
                                                       //thus giving us double dice for a crit
                }
                ActionLog.AddMessage("and hits for " + damageTotal.ToString() + " damage.\n");
                // Apply damage and deal with defeated defender if needed
                if (defender.TakeDamage(damageTotal))
                {
                    ActionLog.AddMessage(defender.Name + " has been defeated!\n");
                    TotalXP += defender.Experience;
                    combatants.Remove(defender);
                    Creature.Defeated();  //why is this not defender.Defeated()???  This makes no sense!
                }

                return damageTotal;
            }
            else
            {
                ActionLog.AddMessage("but missed!\n");
                // Attack missed
                return 0;
            }
        }
    }

    public static class Roll
    {
        //This is not the most optimal place for this, but I want to be able to access it from anywhere
        //and it doesn't seem worth making a new class for, so...
        public static int AbilityBonus(int ability) => (ability - 10) / 2;  //This should give us the bonuses for ability scores
        public static string ParseDamageRoll(string damageroll, List<int> rolls)
        {
            string buffer = damageroll.ToLower(); //make lowercase
            //get rid of whitespace and add a + before any - (since the - is part of the number)
            while (buffer.Contains(' ')) buffer = buffer[..buffer.IndexOf(' ')] + buffer[(buffer.IndexOf(' ') + 1)..];
            while (buffer.Contains('\t')) buffer = buffer[..buffer.IndexOf('\t')] + buffer[(buffer.IndexOf('\t') + 1)..];
            for (int pos = 1; pos < buffer.Length; pos++)
                if ((buffer[pos] == '-') && (buffer[pos - 1] != '+'))
                    buffer = buffer[..pos] + "+" + buffer[pos..];
            rolls.Clear(); //clear the rolls list
            while (buffer.Contains('d')) //first let's do all the dice rolls
                //we will need to edit this part to handle crits, but that's what our rolls is for.
            {
                int spos = FindBegin(buffer, buffer.IndexOf("d"));
                int epos = FindEnd(buffer, buffer.IndexOf("d"));
                int res = DoDice(buffer.Substring(spos, epos - spos + 1), rolls);
                buffer = buffer[..spos] + res + buffer[(epos + 1)..];
            }
            while (buffer.Contains('+')) //then we will do all the addition (and subtraction)
            {
                int spos = buffer.IndexOf("+");
                int epos = FindEnd(buffer, spos);
                _ = int.TryParse(buffer[..spos], out int v1);
                _ = int.TryParse(buffer.AsSpan(spos + 1, epos - spos), out int v2);
                int res = v1 + v2;
                buffer = res + buffer[(epos + 1)..];
            }
            return buffer; //and finally return the result
        }
        private static int FindBegin(string buffer, int pos) //My findbegin and findend routines are kinda clunky, but they work
        {
            while (true)
            {
                pos--;
                if (pos <= 0) return 0;
                if (buffer[pos] == '+') return pos + 1;
            }
        }

        private static int FindEnd(string buffer, int pos)
        {
            while (true)
            {
                pos++;
                if (pos >= buffer.Length - 1) return buffer.Length - 1;
                if (buffer[pos] == '+') return pos - 1;
            }
        }

        private static void SmallParse(string buffer, out int numdie, out int dietype)
        {
            string snumdie, sdietype; //strings to put numeric portions of buffer in
            if (buffer[0] == 'd') snumdie = "1"; //if we were passed "d6", we want to roll "1d6".
            else snumdie = buffer[..buffer.IndexOf('d')]; //otherwise parse the number of dice to roll
            sdietype = buffer[(buffer.IndexOf('d') + 1)..]; //parse the die type
            _ = int.TryParse(snumdie, out numdie); //get integers for both of them
            _ = int.TryParse(sdietype, out dietype);
        }

        public static int DoDice(string buffer, List<int> rolls) //this is the actual dice rolling part and where we store the rolls
        {
            SmallParse(buffer, out int numdie, out int dietype);
            return DoDice(numdie, dietype, rolls);
        }

        public static int DoDice(string buffer) //this is a version of DoDice that doesn't require a rolls
        {
            SmallParse(buffer, out int numdie, out int dietype);
            return DoDice(numdie, dietype);
        }

        public static int DoDice(int numdie, int dietype, List<int> rolls) //for original DoDice routine
        {
            int retval = 0;
            Random rand = new();
            for (int i = 0; i < numdie; i++) //roll those dice!
            {
                int res = rand.Next(dietype) + 1; //get die roll
                rolls.Add(res); //add it to list of rolls
                retval += res; //add it to return value
            }
            return retval; //return total
        }

        public static int DoDice(int numdie, int dietype) //for the one without rolls
        {
            int retval = 0;
            Random rand = new();
            for (int i = 0; i < numdie; i++) //roll those dice!
                retval += rand.Next(dietype) + 1; //get die roll and add it to return value
            return retval; //return total
        }

        public static int DoDice(int die)
        {
            Random rand = new();
            return rand.Next(die) + 1;
        }
    }

    public class Level
    {
        public List<Room>? rooms;
        public List<Passage>? passages;
        public List<Door>? doors;
        public Generator gen;
        public List<Creature> creatures = new();
        public int maxwide, maxhigh;
        public Level()
        {
            gen = new(15, 5, 5, 15, 15, 225, 225, 100); //generate a new level
            rooms = gen.GetRooms(); //get the rooms
            doors = gen.GetDoors(); //get the doors
            passages = gen.GetPassages(); //get the passages
            maxwide = gen.MapWide; //get the maximum width
            maxhigh = gen.MapHigh; //get the maximum height
            creatures.Clear(); //clear the creatures list
        }
        public bool ValidateMove(double X, double Y, Vector2 move) //returns true if running into wall or creature
        {
            double newx = X + move.X, newy = Y + move.Y; //get position
            return CheckIt(newx, newy); //return validation
        }
        public void AddCreature(Creature creature) => creatures.Add(creature); //simple utility
        public Creature? CreatureThere(double x, double y, Vector2 move) //returns creature at position or null if none there
        {
            double newx = x + move.X, newy = y + move.Y; //figure position
            foreach (Creature c in creatures) //for each creature
                if (Math.Abs(c.X - newx) + Math.Abs(c.Y - newy) < 2) return c; //if close enough, return that creature
            return null; //return null since we didn't find a creature close enough
        }
        private bool CheckIt(double x, double y)  //the actual validation
        {
            foreach (Creature c in creatures) //for each creature
                if (Math.Abs(c.X - x) + Math.Abs(c.Y - y) < 2) return true; //if we are very close return true
            if (rooms == null || doors == null || passages == null) return false; //we need rooms, doors, and passages for this
            foreach (Room r in rooms) //for every room
                if (r.X < x && x < r.X + r.Wide && r.Y < y && y < r.Y + r.High) return false; //if inside room, return false
            foreach (Door d in doors) //for every door
                if (d.X == (int)Math.Round(x) && d.Y == (int)Math.Round(y)) return false; //if in doorway, return false
            foreach (Passage pass in passages) //for every passageway
                foreach (Point p in pass.Points) //for each point along it
                    if (p.X == (int)Math.Round(x) && p.Y == (int)Math.Round(y)) return false; //if there, return false
            return true; //return true
        }
    }
}
