namespace Emergency_Strobes
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal static class Plugin
    {
        private const float ActionRadius = 225.0f; // the radius from the player where it will get the vehicles to make strobed
        private const float ActionRadiusSqr = ActionRadius * ActionRadius;

        public static readonly Random Random = new Random();

        public static List<StrobedVehicle> StrobedVehicles = new List<StrobedVehicle>();
        public static PlayerStrobedVehicle PlayerStrobedVehicle;

        public static uint GameTime;

        private static DateTime lastVehiclesCheckTime = DateTime.UtcNow;

        private static Ped playerPed;
        private static Vector3 playerPos;

        private static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Sleep(250);
            
            while (true)
            {
                GameFiber.Yield();

                GameTime = Game.GameTime;
                playerPed = Game.LocalPlayer.Character;
                playerPos = playerPed.Position;

                if (Settings.AIEnabled)
                {
                    if ((DateTime.UtcNow - lastVehiclesCheckTime).TotalSeconds > 1.5)
                    {
                        Entity[] vehs = World.GetEntities(playerPos, ActionRadius, GetEntitiesFlags.ConsiderAllVehicles);

                        for (int i = 0; i < vehs.Length; i++)
                        {
                            Vehicle v = (Vehicle)vehs[i];
                            if (CanBeStrobedVehicle(v))
                            {
                                StrobedVehicles.Add(new StrobedVehicle(v, Settings.Patterns[Random.Next(Settings.Patterns.Length)]));
                            }
                        }
                        lastVehiclesCheckTime = DateTime.UtcNow;
                    }

                    
                    for (int i = StrobedVehicles.Count - 1; i >= 0; i--)
                    {
                        StrobedVehicle v = StrobedVehicles[i];
                        if (v == null || !v.Vehicle || v.Vehicle == playerPed.CurrentVehicle || Vector3.DistanceSquared(v.Vehicle.Position, playerPos) > ActionRadiusSqr)
                        {
                            if (v != null)
                                v.Dispose();
                            StrobedVehicles.RemoveAt(i);
                            continue;
                        }

                        v.Update();
                    }
                }


                if (Settings.PlayerEnabled)
                {
                    if (PlayerStrobedVehicle == null)
                    {
                        TryCreatePlayerStrobedVehicle();
                    }
                    else if (!PlayerStrobedVehicle.Vehicle)
                    {
                        PlayerStrobedVehicle.Dispose();
                        PlayerStrobedVehicle = null;
                    }
                    else if (playerPed.IsInAnyVehicle(false) && PlayerStrobedVehicle.Vehicle != playerPed.CurrentVehicle)
                    {
                        PlayerStrobedVehicle.Dispose();
                        PlayerStrobedVehicle = null;
                        TryCreatePlayerStrobedVehicle();
                    }

                    PlayerStrobedVehicle?.Update();
                }
            }
        }


        private static void OnUnload(bool b)
        {
            if (StrobedVehicles != null)
            {
                foreach (StrobedVehicle v in StrobedVehicles)
                {
                    v?.Dispose();
                }
                StrobedVehicles.Clear();
                StrobedVehicles = null;
            }

            PlayerStrobedVehicle?.Dispose();
            PlayerStrobedVehicle = null;

            Settings.SwitchSound?.Dispose();
        }


        public static bool CanBeStrobedVehicle(Vehicle v)
        {
            return v && v.HasSiren && v != playerPed.CurrentVehicle && v != playerPed.LastVehicle && v != playerPed.VehicleTryingToEnter && !Settings.ExcludedVehicleModels.Any(m => v.Model == m) && StrobedVehicles.All(s => s.Vehicle != v);
        }

        public static bool TryCreatePlayerStrobedVehicle()
        {
            Vehicle[] possibleVehs = { playerPed.CurrentVehicle, playerPed.LastVehicle, playerPed.VehicleTryingToEnter };
            for (int i = 0; i < possibleVehs.Length; i++)
            {
                Vehicle v = possibleVehs[i];
                if (v && v.HasSiren && !Settings.ExcludedVehicleModels.Any(m => v.Model == m))
                {
                    PlayerStrobedVehicle = new PlayerStrobedVehicle(v, Settings.Patterns);
                    return true;
                }
            }
            possibleVehs = null;

            return false;
        }
    }
}
