﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;

namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk
{
    public enum Squads
    {
        All,
        Arrvs,
        Fighters,
        Helicopters,
        Ifvs,
        Tanks,
        Mixture,
    }

    public class Universe
    {
        public Move Move { get; set; }
        public World World { get; }
        public Game Game { get; }
        public List<Vehicle> MyUnits { get; }
        public List<Vehicle> OppUnits { get; }
    
        public Universe(World world, Game game, List<Vehicle> myUnits, List<Vehicle> oppUnits, Move move)
        {
            World = world;
            Game = game;
            MyUnits = myUnits;
            OppUnits = oppUnits;
            Move = move;
        }
        public AbsolutePosition MapCenter => new AbsolutePosition(World.Width / 2.0D, World.Height / 2.0D);
        public AbsolutePosition MapConerLeftLower => new AbsolutePosition(0, World.Height);
        public AbsolutePosition MapConerRightUp => new AbsolutePosition(World.Width, 0);

    }
    
    public class AbsolutePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public AbsolutePosition(double x, double y)
        {
            X = x;
            Y = y;
        }
    }




}
