using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StageMapper
{
    public class Position
    {
        public double X, Y, Z;

        public Position()
        {
        }

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(Position p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public double GetDistance(double? x = null, double? y = null, double? z = null)
        {
            if (x == null) x = X;
            if (y == null) y = Y;
            if (z == null) z = Z;

            return Math.Sqrt((X - x.Value) * (X - x.Value)+ (Y - y.Value) * (Y - y.Value)+ (Z - z.Value) * (Z - z.Value));

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feedrate">mm/minute</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns>sec</returns>
        public double GetDelay(double feedrate,double? x = null, double? y = null, double? z = null)
        {
            var distance = GetDistance(x, y, z);
            return distance / feedrate * 60;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Position))
                return false;
            var p = (Position)obj;
            return X == p.X && Y == p.Y && Z == p.Z;
        }
        public override string ToString()
        {
            return string.Format("X{0} Y{1} Z{2}", X, Y, Z);
        }
    }
}
