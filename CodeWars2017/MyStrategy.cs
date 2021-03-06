using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk.Model;
using System.Linq;
using System.Timers;


namespace Com.CodeGame.CodeWars2017.DevKit.CSharpCgdk {
    public sealed class MyStrategy : IStrategy
    {
        public List<Vehicle> UnitsMy = new List<Vehicle>();
        public List<Vehicle> UnitsOpp = new List<Vehicle>();

        public static Universe Universe { get; internal set; } = new Universe();
        public static SquadCalculator SquadCalculator = new SquadCalculator();
        public static Predictor Predictor = new Predictor();
        public static BonusMapCalculator BonusCalculator = new BonusMapCalculator();

        private static Stopwatch MyStrategyTimer = new Stopwatch();
        public static int MaxActionBalance { get; internal set; }

        public void Move(Player me, World world, Game game, Move move)
        {
            
#if DEBUG
            RunTick(world, game, move, me);

            if (Universe.World.TickIndex == 0)
            {
                Visualizer.Visualizer.CreateForm();
                Visualizer.Visualizer.DrawSince = 5;
                Visualizer.Visualizer.LookAt(new Point(Universe.MapConerLeftUp.X, Universe.MapConerLeftUp.Y));
            }

            var timerVisualizer = new Stopwatch();
            timerVisualizer.Reset();
            timerVisualizer.Start();

            if (Universe.World.TickIndex % 1 == 0)
            {
                Visualizer.Visualizer.Draw();
                if (Universe.World.TickIndex >= Visualizer.Visualizer.DrawSince)
                {
                    var timer = new Stopwatch();
                    timer.Reset();
                    timer.Start();
                    while (!Visualizer.Visualizer.Done || timer.ElapsedMilliseconds < 13)
                    {
                        timerVisualizer.Stop();
                    }
                    timer.Stop();
                    timerVisualizer.Start();
                }
            }
            if (timerVisualizer.ElapsedMilliseconds > 1000)
               Universe.Print("Time for visualizer " + timerVisualizer.ElapsedMilliseconds);
            timerVisualizer.Stop();

            while (Visualizer.Visualizer.Pause && !Visualizer.Visualizer.RenderPressed)
            {
                // pause here
            }
            if (Visualizer.Visualizer.RenderPressed)
                Visualizer.Visualizer.RenderPressed = false;
#else
            try
            {
                RunTick(world, game, move, me);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

#endif
        }

        private void RunTick(World world, Game game, Move move, Player player)
        {
            if (player.IsStrategyCrashed)
                Universe.Print($"Crashed. Finish. Spent time is [{MyStrategyTimer.ElapsedMilliseconds}] ms");
            MyStrategyTimer.Start();
            var runTickTimer = new Stopwatch();
            runTickTimer.Reset();
            runTickTimer.Start();

            //var weather = world.WeatherByCellXY;
            //Universe.Print("Weather CellsAmount " +weather.Length);

            UpdateUnitsStatus(world);
            Universe.Update(world, game, UnitsMy, UnitsOpp, move, player);

            MaxActionBalance = CalculateActionBalance();

            Predictor.RunTick(Universe);
            BonusCalculator.RunTick(Universe);
            SquadCalculator.RunTick(Universe);
            ActionHandler.RunTick(Universe, SquadCalculator.ActionList, SquadCalculator.ImmediateActionList);

            runTickTimer.Stop();
            MyStrategyTimer.Stop();

            var duration = runTickTimer.ElapsedMilliseconds;
            if (duration > 300 || MyStrategyTimer.ElapsedMilliseconds > 18 * 20000)
                Universe.Print($"---StepTime [{duration:f2}] ms, total - [{MyStrategyTimer.ElapsedMilliseconds}/{20 * 20000 + 1000}] ms");
        }

        private static int CalculateActionBalance()
        {
            var controlCentersAmount = Universe.World.Facilities.Count(f => f.Type == FacilityType.ControlCenter && f.OwnerPlayerId == Universe.Player.Id);
            var reservedForEnemyNuke =
                Universe.World.GetOpponentPlayer().RemainingNuclearStrikeCooldownTicks < 100 ? 2 : 0;

            var reservedForMyNuke =
                Universe.World.GetMyPlayer().RemainingNuclearStrikeCooldownTicks < 100 ? 2 : 0;

            var maxActionBalance = Universe.Game.BaseActionCount + 3 * controlCentersAmount - reservedForEnemyNuke -
                                   reservedForMyNuke;
            return maxActionBalance;
        }

        private void UpdateUnitsStatus(World world)
        {
            var playerMy = world.GetMyPlayer();
            var playerOpp = world.GetOpponentPlayer();

            foreach (var venicle in world.NewVehicles)
            {
                var currentVenicleUpdate = world.VehicleUpdates.FirstOrDefault(u => u.Id == venicle.Id);
                if (currentVenicleUpdate == null)
                    currentVenicleUpdate = new VehicleUpdate(
                        venicle.PlayerId, 
                        venicle.X, 
                        venicle.Y, 
                        venicle.Durability, 
                        venicle.RemainingAttackCooldownTicks, 
                        venicle.IsSelected, 
                        venicle.Groups);

                if (venicle.PlayerId == playerMy.Id)
                    UnitsMy.Add(new Vehicle(venicle, currentVenicleUpdate));
                if (venicle.PlayerId == playerOpp.Id)
                    UnitsOpp.Add(new Vehicle(venicle, currentVenicleUpdate));
            }

            ReplaceUnitWithUpdate(world, UnitsMy);
            //foreach (var unit in UnitsMy)
            //{
            //    if (unit.X < 0.1) throw new Exception("0 coordinate! Dead warrior!");
            //}
            ReplaceUnitWithUpdate(world, UnitsOpp);
            //foreach (var unit in UnitsOpp)
            //{
            //    if (unit.X < 0.1) throw new Exception("0 coordinate! Dead enemy in the list.");
            //}
        }

        private void ReplaceUnitWithUpdate(World world, List<Vehicle> units)
        {
            foreach (var update in world.VehicleUpdates)
                foreach (var unit in units)
                    if (unit.Id == update.Id)
                    {
                        ReplaceWithUpdate(units, unit, update);
                        break;
                    }
        }

        private static void ReplaceWithUpdate(List<Vehicle> units, Vehicle unit, VehicleUpdate update)
        {
            var newUnit = new Vehicle(unit, update);
            units.Remove(unit);
            if (update.Durability != 0) //Note: Dead or not visible units are removed from the list! 
                units.Add(newUnit);

            // TODO if (UnitAliveButNotVisible) units.Add(newUnit);
            // TODO For dead units position is 0, for for the hidden ones?
        }
    }


}