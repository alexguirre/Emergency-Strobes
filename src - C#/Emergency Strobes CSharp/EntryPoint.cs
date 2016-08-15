namespace Emergency_Strobes
{
    // System
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // RPH
    using Rage;

    internal static class EntryPoint
    {
        public const float ActionRadius = 275.0f;

        public static readonly Random Random = new Random();

        public static List<StrobedVehicle> StrobedVehicles = new List<StrobedVehicle>();
        public static PlayerStrobedVehicle PlayerStrobedVehicle;

        private static DateTime lastVehiclesWithSirenCheck = DateTime.UtcNow;

        public static void Main()
        {
            while (Game.IsLoading)
                GameFiber.Yield();

            while (true)
            {
                GameFiber.Yield();

                if (Settings.AIEnabled)
                {
                    if ((DateTime.UtcNow - lastVehiclesWithSirenCheck).TotalSeconds > 1.25)
                    {
                        Vehicle[] veh = World.GetAllVehicles();

                        for (int i = 0; i < veh.Length; i++)
                        {
                            if (CanBeStrobedVehicle(veh[i]))
                            {
                                StrobedVehicles.Add(new StrobedVehicle(veh[i], Settings.Patterns[Random.Next(Settings.Patterns.Length)]));
                            }
                        }
                        lastVehiclesWithSirenCheck = DateTime.UtcNow;
                    }

                    for (int i = 0; i < StrobedVehicles.Count; i++)
                    {
                        StrobedVehicle v = StrobedVehicles[i];
                        if (v == null || !v.Vehicle || v.Vehicle == Game.LocalPlayer.Character.CurrentVehicle || v.Vehicle.DistanceTo2D(Game.LocalPlayer.Character) > ActionRadius)
                        {
                            if (v != null && v.Vehicle)
                                v.ResetVehicleLights();
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
                        CreatePlayerStrobedVehicle();
                    }
                    else if (!PlayerStrobedVehicle.Vehicle)
                    {
                        PlayerStrobedVehicle?.CleanUp();
                        PlayerStrobedVehicle = null;
                    }
                    else if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && PlayerStrobedVehicle.Vehicle != Game.LocalPlayer.Character.CurrentVehicle)
                    {
                        if (PlayerStrobedVehicle != null)
                        {
                            PlayerStrobedVehicle.CleanUp();
                            if (PlayerStrobedVehicle.Vehicle)
                                PlayerStrobedVehicle.ResetVehicleLights();
                        }
                        PlayerStrobedVehicle = null;
                        CreatePlayerStrobedVehicle();
                    }

                    PlayerStrobedVehicle?.Update();
                }
            }
        }

        public static bool CanBeStrobedVehicle(Vehicle v)
        {
            return v.HasSiren && v != Game.LocalPlayer.Character.CurrentVehicle && v != Game.LocalPlayer.Character.LastVehicle && !Settings.ExcludedVehicleModels.Any(m => v.Model == m) && StrobedVehicles.All(s => s.Vehicle != v);
        }

        public static bool CreatePlayerStrobedVehicle()
        {
            Ped playerPed = Game.LocalPlayer.Character;

            Vehicle current = playerPed.CurrentVehicle;
            Vehicle last = playerPed.LastVehicle;

            if (current && current.HasSiren && !Settings.ExcludedVehicleModels.Any(m => current.Model == m))
            {
                PlayerStrobedVehicle = new PlayerStrobedVehicle(current, 0);
                return true;
            }
            else if (last && last.HasSiren && !Settings.ExcludedVehicleModels.Any(m => last.Model == m))
            {
                PlayerStrobedVehicle = new PlayerStrobedVehicle(last, 0);
                return true;
            }
            return false;
        }


        public static void OnUnload(bool b)
        {
            foreach (StrobedVehicle v in StrobedVehicles)
            {
                if (v != null && v.Vehicle)
                    v.ResetVehicleLights();
            }
            StrobedVehicles.Clear();

            if (PlayerStrobedVehicle != null)
            {
                PlayerStrobedVehicle.CleanUp();
                if (PlayerStrobedVehicle.Vehicle)
                    PlayerStrobedVehicle.ResetVehicleLights();
            }

            Settings.SwitchSound.Dispose();
        }
    }
}
