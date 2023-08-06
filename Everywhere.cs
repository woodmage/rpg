using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace rpg
{
    public class Room
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Wide { get; set; }
        public int High { get; set; }
        public Room(int x, int y, int wide, int high)
        {
            X = x;
            Y = y;
            Wide = wide;
            High = high;
        }
        public Point GetCenterLeft() => new(X, Y + High / 2);
        public Point GetCenterTop() => new(X + Wide / 2, Y);
        public Point GetCenterRight() => new(X + Wide - 1, Y + High / 2);
        public Point GetCenterBottom() => new(X + Wide / 2, Y + High - 1);
        public Point GetCenter() => new(X + Wide / 2, Y + High / 2);
        public bool IsInRoom(int x, int y, int wide, int high)
        {
            if (x > X + Wide) return false;
            if (x + wide < X) return false;
            if (y > Y + High) return false;
            if (y + high < Y) return false;
            return true;
        }
        public bool Touches(int x, int y)
        {
            if (((x == X) || (x == X + Wide - 1)) && (y >= Y) && (y < Y + High))
                return true;
            if (((y == Y) || (y == Y + High - 1)) && (x >= X) && (x < X + Wide))
                return true;
            return false;
        }
        public int DistanceX(int x, int y)
        {
            if (x < X) return x - X;
            if (x > X + Wide - 1) return X + Wide - 1 - x;
            return y - y; //this line is to keep Visual Studio from complaining about unused y parameter, returns 0
        }
        public int DistanceX(Room r)
        {
            if (r.X + r.Wide - 1 < X)
                return (r.X + r.Wide - 1) - X;
            if (r.X > X + Wide - 1)
                return (X + Wide - 1) - r.X;
            return 0;
        }
        public int DistanceY(int x, int y)
        {
            if (y < Y) return y - Y;
            if (y > Y + High - 1) return Y + High - 1 - y;
            return x - x; //this line is to keep Visual Studio from complaining about unused x parameter, returns 0
        }
        public int DistanceY(Room r)
        {
            if (r.Y + r.High - 1 < Y)
                return (r.Y + r.High - 1) - Y;
            if (r.Y > Y + High - 1)
                return (Y + High - 1) - r.Y;
            return 0;
        }
    }

    public class Door
    {
        public int X { get; set; }
        public int Y { get; set; }
        private void InitDoor(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
        public Door(int x, int y) => InitDoor(new Point(x, y));
        public Door(Point point) => InitDoor(point);
    }

    public class Passage
    {
        public List<Point> Points { get; set; }

        public Passage()
        {
            Points = new();
        }

        public void AddPoint(Point point)
        {
            if (!Points.Contains(point))
                Points.Add(point);
        }
        public void AddPoint(int x, int y) => AddPoint(new Point(x, y));
    }

    public class Generator
    {
        public List<Room> Rooms { get; set; }
        public List<Passage> Passages { get; set; }
        public List<Door> Doors { get; set; }
        public int MapWide { get; set; }
        public int MapHigh { get; set; }
        public int Count { get; set; }
        public int MinWide { get; set; }
        public int MaxWide { get; set; }
        public int MinHigh { get; set; }
        public int MaxHigh { get; set; }
        public int ChancePass { get; set; }
        private readonly Random random = new();


        //Constructors:
        public Generator()
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = 10;
            MinWide = 3;
            MinHigh = 3;
            MaxWide = 10;
            MaxHigh = 10;
            MapWide = MaxWide * Count;
            MapHigh = MaxHigh * Count;
            ChancePass = 20;
            DoGeneration();
        }
        public Generator(int count)
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = count;
            MinWide = 3;
            MinHigh = 3;
            MaxWide = 10;
            MaxHigh = 10;
            MapWide = MaxWide * count;
            MapHigh = MaxHigh * count;
            ChancePass = 20;
            DoGeneration();
        }
        public Generator(int count, int maxwide, int maxhigh)
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = count;
            MaxWide = maxwide;
            MaxHigh = maxhigh;
            MinWide = 3;
            MinHigh = 3;
            MapWide = maxwide * count;
            MapHigh = maxhigh * count;
            ChancePass = 20;
            DoGeneration();
        }
        public Generator(int count, int minwide, int minhigh, int maxwide, int maxhigh)
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = count;
            MaxWide = maxwide;
            MaxHigh = maxhigh;
            MinWide = minwide;
            MinHigh = minhigh;
            MapWide = maxwide * count;
            MapHigh = maxhigh * count;
            ChancePass = 20;
            DoGeneration();
        }
        public Generator(int count, int minwide, int minhigh, int maxwide, int maxhigh, int mapwide, int maphigh)
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = count;
            MaxWide = maxwide;
            MaxHigh = maxhigh;
            MinWide = minwide;
            MinHigh = minhigh;
            MapWide = mapwide;
            MapHigh = maphigh;
            ChancePass = 20;
            DoGeneration();
        }
        public Generator(int count, int minwide, int minhigh, int maxwide, int maxhigh, int mapwide, int maphigh, int chancepass)
        {
            Rooms = new();
            Passages = new();
            Doors = new();
            Count = count;
            MaxWide = maxwide;
            MaxHigh = maxhigh;
            MinWide = minwide;
            MinHigh = minhigh;
            MapWide = mapwide;
            MapHigh = maphigh;
            ChancePass = chancepass;
            DoGeneration();
        }


        //Get functions to get rooms, passages, doors
        public List<Door>? GetDoors()
        {
            return (Doors.Count == 0) ? null : Doors;
        }
        public List<Room>? GetRooms()
        {
            return (Rooms.Count == 0) ? null : Rooms;
        }
        public List<Passage>? GetPassages()
        {
            return (Passages.Count == 0) ? null : Passages;
        }



        private void DoGeneration()
        {
            GenerateRooms(); //generate rooms
            //There doesn't seem to be a need to sort the rooms anymore so...
            /*List<Room> sortedrooms = new(); //list of sorted rooms
            List<Room> unsortedrooms = new(); //list of unsorted rooms
            unsortedrooms.AddRange(Rooms); //copy all rooms into unsorted rooms
            int i = ClosestRoom(0, 0); //get index of closest room
            Room? room = Rooms[i]; //set room to index
            sortedrooms.Add(room); //add room to sorted rooms
            unsortedrooms.Remove(room); //remove room from unsorted rooms
            while (unsortedrooms.Count > 0) //while there are unsorted rooms
            {
                room = FindClosestRoom(room, unsortedrooms); //get closest room
                if (room != null) //if room exists
                {
                    sortedrooms.Add(room); //add room to sorted rooms
                    unsortedrooms.Remove(room); //remove room from unsorted rooms
                }
            }
            Rooms.Clear(); //clear rooms
            Rooms.AddRange(sortedrooms); //copy sorted rooms into Rooms*/
            MakePassages(); //This will also make doors while its at it
        }
        public List<Room> GenerateRooms()
        {
            Rooms = new(); //clear rooms
            for (int i = 0; i < Count; i++) //for each room we are supposed to create
            {
                int x = -1, y = -1, wide = -1, high = -1; //set up dummy values
                while (IsInRooms(Rooms, x, y, wide, high)) //while values are in a room
                {
                    wide = random.Next(MinWide, MaxWide); //get new width
                    high = random.Next(MinHigh, MaxHigh); //get new height
                    x = random.Next(0, MapWide - wide + 1); //get new starting x spot
                    y = random.Next(0, MapHigh - high + 1); //get new starting y spot
                }
                Rooms.Add(new(x, y, wide, high)); //make a new room with values
            }
            return Rooms; //return rooms
        }
        private static bool IsInRooms(List<Room> rooms, int x, int y, int wide, int high)
        {
            bool rval = false; //assume false
            if (x < 0) rval = true; //if dummy value, assume true
            foreach (Room room in rooms) //for each room
                if (room.IsInRoom(x - 1, y - 1, wide + 2, high + 2)) rval = true; //if in room, set to true
            return rval; //return value
        }
        /*private static Room? FindClosestRoom(Room? room, List<Room> rooms)
        {
            double dist = 99999; //set distance to an outrageous value
            int sel = -1; //set selection to -1
            if (room == null) return null; //if room is non-existent, return null (no room)
            for (int i = 0; i < rooms.Count; i++) //for each room in list
            {
                double d = Math.Sqrt(Math.Pow(room.DistanceX(rooms[i]), 2) + Math.Pow(room.DistanceY(rooms[i]), 2)); //compute distance
                if (d < dist) //if computed distance is less than distance
                {
                    dist = d; //set distance to computed distance
                    sel = i; //set selection to current room
                }
            }
            if (sel == -1) return null; //if selection is -1, return null (no room)
            return rooms[sel]; //return room that had smallest distance
        }*/
        /*private int ClosestRoom(int x, int y)
        {
            double dist = 99999; //set distance to an outrageous value
            int sel = -1; //selection is -1
            for (int i = 0; i < Rooms.Count; i++) //for each room
            {
                double d = Math.Sqrt(Math.Pow(Rooms[i].DistanceX(x, y), 2) + Math.Pow(Rooms[i].DistanceY(x, y), 2)); //get distance between rooms
                if (d < dist) //if this distance is less than distance
                {
                    dist = d; //set distance to this distance
                    sel = i; //set selection to current room
                }
            }
            return sel; //return selection
        }*/
        public void MakeMinPassages()
        {
            Passages = new(); //clear passages
            Doors = new(); //clear doors
            for (int i = 0; i < Rooms.Count - 1; i++) //for each room
                SetupPassages(Rooms[i], Rooms[i + 1]); //get passageway to next room
        }
        public void MakePassages()
        {
            MakeMinPassages(); //make the minimum passages
            if (ChancePass > 0) //if there is a chance of random passages
            {
                for (int i = 0; i < Rooms.Count; i++) //for each room
                {
                    if (random.Next(100) < ChancePass)  //if there is chance of a random passageway
                    {
                        int j = i; //set j = i
                        while (Math.Abs(i - j) < 2) //while j = i, i - 1, or i + 1
                            j = random.Next(Rooms.Count); //make j a random room
                        SetupPassages(Rooms[i], Rooms[j]); //get passageway
                    }
                }
            }
        }

        private void SetupPassages(Room r1, Room r2)
        {
            Passage passage = new(); //make a new passageway
            Point spoint, epoint; //starting and ending points
            int dx = Math.Abs(r2.DistanceX(r1)); //distance between rooms horizontally
            int dy = Math.Abs(r2.DistanceY(r1)); //distance between rooms vertically
            int xadj = 0, yadj = 0; //horizontal and vertical adjustments
            if (Math.Abs(dx) < Math.Abs(dy)) //shorter horizontally than vertically
            {
                if (r1.Y > r2.Y) //down to up
                {
                    spoint = r1.GetCenterTop(); //starting point is top of first room
                    epoint = r2.GetCenterBottom(); //ending point is bottom of second room
                    yadj = -1; //vertical adjustment is up
                }
                else //up to down
                {
                    spoint = r1.GetCenterBottom(); //starting point is bottom of first room
                    epoint = r2.GetCenterTop(); //ending point is top of second room
                    yadj = 1; //vertical adjustment is down
                }
            }
            else //shorter vertically than horizontally
            {
                if (r1.X > r2.X) //right to left
                {
                    spoint = r1.GetCenterLeft(); //starting point is left of first room
                    epoint = r2.GetCenterRight(); //ending point is right of second room
                    xadj = -1; //horizontal adjustment is left
                }
                else //left to right
                {
                    spoint = r1.GetCenterRight(); //starting point is right of first room
                    epoint = r2.GetCenterLeft(); //ending point is left of second room
                    xadj = 1; //horizontal adjustment is right
                }
            }
            AddPoints(spoint, epoint, passage); //add points to doors and passage
            spoint.X += xadj; //adjust starting point
            spoint.Y += yadj;
            epoint.X -= xadj; //adjust ending point
            epoint.Y -= yadj;
            Passages.Add(MakePassage(spoint, epoint, passage)); //make a passage between starting point to ending point
        }
        private void AddPoints(Point p1, Point p2, Passage pass)
        {
            Doors.Add(new(p1)); //add points to doors
            Doors.Add(new(p2));
            pass.AddPoint(p1); //add points to passage
            pass.AddPoint(p2);
        }

        private Passage MakePassage(Point start, Point end, Passage passage)
        {
            Point current = new(start.X, start.Y); //set current point to start point
            passage.AddPoint(current); //add current point to passage
            while (current != end) //while current point is not the same as ending point
            {
                Point npoint = new(-1, -1); //set up a (dummy) new point
                int ntry = 0; //set number tries to 0
                while (!IsPassageGood(npoint)) //while new point is not a good passage
                {
                    Point delta = new(end.X - current.X, end.Y - current.Y); //get delta point (used for chances, addition)
                    npoint.X = current.X; //copy current point to new point
                    npoint.Y = current.Y;
                    ntry++; //add 1 to number tries
                    float chanceX = (float)((Math.Abs(delta.X) + .1) / (Math.Abs(delta.X) + Math.Abs(delta.Y) + .1)); //compute chances of horizontal
                    if (random.NextDouble() < .05 / ntry) //decreasing chances of different move
                    {
                        delta = Negate(delta); //reverse directions
                        chanceX = 1 - chanceX; //reverse chances
                    }
                    if (random.NextDouble() < chanceX) //chances according to calculations
                        npoint.X += Math.Sign(delta.X); //go horizontal
                    else //otherwise
                        npoint.Y += Math.Sign(delta.Y); //go vertical
                    if ((npoint.X < 0) || (npoint.X > MapWide - 1) || (npoint.Y < 0) || (npoint.Y > MapHigh - 1)) //if out of bounds
                        npoint = new(current.X, current.Y); //reset to current point
                    if (ntry > 99) //if we have tried 100 times or more
                    {
                        delta.X = npoint.X - current.X; //get direction we last tried
                        delta.Y = npoint.Y - current.Y;
                        npoint.X = current.X; //get current point
                        npoint.Y = current.Y;
                        if (delta.X == 0) //if we moved in Y
                        {
                            if (npoint.X < end.X) npoint.X++; //if we need to move right, do it
                            else npoint.X--; //otherwise, move left
                        }
                        if (delta.Y == 0) //if we moved in X
                        {
                            if (npoint.Y < end.Y) npoint.Y++; //if we need to move down, do it
                            else npoint.Y--; //otherwise, move up
                        }
                        if (ntry > 150) //if we have tried more than 150 times
                        {
                            MessageBox.Show("Can't find way!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); //give an error message
                            return passage; //return what we've got so far
                        }
                    }
                }
                passage.AddPoint(npoint); //add new point to passage
                current.X = npoint.X; //set current point to new point
                current.Y = npoint.Y;
            }
            return passage; //return passageway
        }
        private static Point Negate(Point point)
        {
            point.X = -point.X; //make point into negative
            point.Y = -point.Y;
            return point; //return point
        }
        private bool IsPassageGood(Point p) => IsPassageGood(p.X, p.Y);
        private bool IsPassageGood(int x, int y)
        {
            bool retval = true; //assume passageway is good
            if (x == -1) return false; //if point is dummy point, return false
            foreach (Room room in Rooms) //for every room
                if (room.Touches(x, y)) //if room touches x,y point
                    retval = false; //return value is false (bad)
            return retval; //return value
        }
    }
}
