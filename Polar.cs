namespace rpg
{
    public class Polar
    {
        public double Radius { get; set; }
        public double Angle { get; set; }

        public Polar()
        {
            Radius = Angle = 0;
        }

        public Polar(double radius, double angle)
        {
            Radius = radius;
            Angle = angle;
        }

        public Polar(double radius, double angle, char drg)
        {
            Radius = radius;
            Angle = angle;
            if (drg == 'D' || drg == 'd') Angle = Deg2Rad(angle);
            if (drg == 'G' || drg == 'g') Angle = Grad2Rad(angle);
            if (drg == 'I' || drg == 'i') Angle = Math.Atan2(radius, 1.0);
        }

        public Polar(Point rect)
        {
            Radius = Math.Sqrt(Math.Pow(rect.X, 2) + Math.Pow(rect.Y, 2));
            Angle = Math.Atan2(rect.Y, rect.X);
        }

        public Polar(PointF rect)
        {
            Radius = Math.Sqrt(Math.Pow(rect.X, 2) + Math.Pow(rect.Y, 2));
            Angle = Math.Atan2(rect.Y, rect.X);
        }

        public Polar ToDegrees()
        {
            return new Polar(Radius, Rad2Deg(Angle));
        }

        public Polar ToGradians()
        {
            return new Polar(Radius, Rad2Grad(Angle));
        }

        public Point ToPoint()
        {
            return new Point((int)(Radius * Math.Cos(Angle)), (int)(Radius * Math.Sin(Angle)));
        }

        public PointF ToPointF()
        {
            return new PointF((float)(Radius * Math.Cos(Angle)), (float)(Radius * Math.Sin(Angle)));
        }

        public double X() => Radius * Math.Cos(Angle);
        public double Y() => Radius * Math.Sin(Angle);
        public static double Deg2Rad(double angle) => angle / 180 * Math.PI;
        public static double Rad2Deg(double angle) => angle / Math.PI * 180;
        public static double Deg2Grad(double angle) => angle / 180 * 200;
        public static double Grad2Deg(double angle) => angle / 200 * 180;
        public static double Rad2Grad(double angle) => angle / Math.PI * 200;
        public static double Grad2Rad(double angle) => angle / 200 * Math.PI;
        public static double Normalize(double angle)
        {
            while (angle < 0) angle += Math.PI * 2;
            return angle % (Math.PI * 2);
        }

        public static double GetDistance(Polar p1, Polar p2)
        {
            return Math.Sqrt(Math.Pow(p2.X() - p1.X(), 2) + Math.Pow(p2.Y() - p1.Y(), 2));
        }
 
        public static Polar FromRect(double x, double y)
        {
            return new Polar(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)), Math.Atan2(y, x));
        }

        public static Polar Negate(Polar p)
        {
            return new Polar(p.Radius, Normalize(p.Angle + Math.PI));
        }

        public static Polar operator +(Polar c1, Polar c2)
        {
            return FromRect(c1.X() + c2.X(), c1.Y() + c2.Y());
        }

        public static Polar operator +(Polar c1, double thrust)
        {
            return new Polar(c1.Radius + thrust, c1.Angle);
        }

        public static Polar operator -(Polar c1, Polar c2)
        {
            return c1 + Negate(c2);
        }

        public static Polar operator -(Polar c1, double thrust)
        {
            return new Polar(c1.Radius - thrust, c1.Angle);
        }

        public static Polar operator *(Polar c, double scale)
        {
            return new Polar(c.Radius * scale, c.Angle);
        }

        public static Polar operator /(Polar c, double scale)
        {
            return new Polar(c.Radius / scale, c.Angle);
        }
    }
}