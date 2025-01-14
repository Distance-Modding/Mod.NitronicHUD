using UnityEngine;

namespace Distance.NitronicHUD
{
    public static class Vehicle
    {
        public static CarStats CarStats => Utilities.FindLocalVehicleScreen().CarLogic_.CarStats_;

        private static CarLogic CarLogic { get; set; }

        public static float HeatLevel
        {
            get
            {
                UpdateObjectReferences();
                if (CarLogic)
                {
                    return CarLogic.Heat_;
                }
                return 0f;
            }
        }

        public static float VelocityKPH
        {
            get
            {
                UpdateObjectReferences();
                if (CarLogic)
                {
                    return CarLogic.CarStats_.GetKilometersPerHour();
                }
                return 0f;
            }
        }

        public static float VelocityMPH
        {
            get
            {
                UpdateObjectReferences();
                if (CarLogic)
                {
                    return CarLogic.CarStats_.GetMilesPerHour();
                }
                return 0f;
            }
        }

        private static void UpdateObjectReferences()
        {
            CarLogic = (Utilities.FindLocalCar()?.GetComponent<CarLogic>()) ?? Utilities.FindLocalCarLogic();
        }
    }
}
