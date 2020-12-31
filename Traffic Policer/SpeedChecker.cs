using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using Rage.Native;
using System.Drawing;
using System.Diagnostics;
using Albo1125.Common.CommonLibrary;
using StopThePed;

namespace Traffic_Policer
{
    internal static class SpeedChecker
    {
        private static bool Blip;
        private static int CheckPoint;

        private static int clase;
        private static int pasageros;

        private static Vector3 CheckPointPosition;
        private static string TargetModel = "";
        public static string SpeedUnit = "KMH";
        private static int TargetSpeed = 0;
        private static string TargetFlag = "";
        //private static string TargetSpeedLimit = "";
        public static Keys SecondaryDisableKey = Keys.Back;
        public static Keys ToggleSpeedCheckerKey = Keys.F5;
        public static Keys ToggleSpeedCheckerModifierKey = Keys.None;
        public static Keys PositionResetKey = Keys.NumPad5;
        public static Keys PositionForwardKey = Keys.NumPad8;
        public static Keys PositionBackwardKey = Keys.NumPad2;
        public static Keys PositionRightKey = Keys.NumPad6;
        public static Keys PositionLeftKey = Keys.NumPad4;
        public static Keys PositionUpKey = Keys.NumPad9;

        public static Keys PositionDownKey = Keys.NumPad3;
        public static Keys MaxSpeedUpKey = Keys.PageUp;
        public static Keys MaxSpeedDownKey = Keys.PageDown;
        private static Color FlagsTextColour = Color.White;
        private static string TargetLicencePlate = "";
        private static List<Vehicle> VehiclesFlagged = new List<Vehicle>();
        private static List<Vehicle> VehiclesBlipPlayedFor = new List<Vehicle>();
        private static List<Ped> FlaggedDrivers = new List<Ped>();
        public static int FlagChance = 15;
        private static Color SpeedColour = Color.White;
        public static int SpeedToColourAt = 5;
        private static int xOffset = 0;
        private static int yOffset = 0;
        private static int zOffset = 0;
        private static System.Media.SoundPlayer FlagBlipPlayer = new System.Media.SoundPlayer("lspdfr/audio/scanner/midetector/FLAG_BLIP.wav");
        private static System.Media.SoundPlayer InfraccionDetectada = new System.Media.SoundPlayer("lspdfr/audio/scanner/midetector/infraccion.wav");
        public static bool PlayFlagBlip = true;
        private static List<Vehicle> VehiclesNotFlagged = new List<Vehicle>();

        enum SpeedCheckerStates { Average, FixedPoint, Speedgun, Off }
        private static SpeedCheckerStates CurrentSpeedCheckerState = SpeedCheckerStates.Off;

        private static Vector3 LastAverageSpeedCheckReferencePoint;
        private static Stopwatch AverageSpeedCheckStopwatch = new Stopwatch();
        private static float AverageSpeedCheckDistance = 0f;
        private static bool MeasuringAverageSpeed = false;
        private static float AverageSpeedCheckSecondsPassed = 0f;
        private static int AverageSpeedCheckCurrentSpeed = 0;
        private static float AverageSpeed = 0f;
        public static Keys StartStopAverageSpeedCheckKey = Keys.PageUp;
        public static Keys ResetAverageSpeedCheckKey = Keys.PageDown;
        private static Color AverageSpeedCheckerColor = Color.White;

        public enum estados
        {
            ALCOHOL,
            DROGAS,
            AMBOS
        }



        private static string[] infracciones = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE HABER BEBIDO ALGO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "NO TIENE EL CINTURON PUESTO",
                "TIENE EL CINTURON MAL PUESTO",
                "EL VEHICULO NO LLEVA LAS LUCES PUESTAS",
                "EL VEHICULO NO ESTA USANDO INTERMITENCIA",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCI",
                "EL CONDUCTOR TIENE LA MUSICA DEL VEHICULO MUY ALTA"
                };

        private static string[] infracciones2 = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE ALCOHOLISMO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCI"
                };

        private static string[] infracciones3 = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE ALCOHOLISMO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "NO TIENE EL CINTURON PUESTO",
                "TIENE EL CINTURON MAL PUESTO",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCI",
                "EL CONDUCTOR TIENE LA MUSICA DEL VEHICULO MUY ALTA",
                "EL CONDUCTOR ESTA MANUPULANDO ALBARANES O FACTURAS"
                };

        private static string[] infracciones4 = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE HABER BEBIDO ALGO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "EL CONDUCTOR NO TIENE EL CINTURON PUESTO",
                "EL CONDUCTOR TIENE EL CINTURON MAL PUESTO",
                "EL PASAJERO NO TIENE EL CINTURON PUESTO",
                "EL PASAJERO TIENE EL CINTURON MAL PUESTO",
                "EL PASAJERO DISTRAE AL CONDUCOR",
                "EL VEHICULO NO LLEVA LAS LUCES PUESTAS",
                "EL VEHICULO NO ESTA USANDO INTERMITENCIA",
                "PARECE EL PASAJERO Y EL CONDUCTOR ESTAN TENIENDO UNA FUERTE DISCUSION",
                "PARECE EL PASAJERO TIENE ALGO ILEGAL EN LAS MANOS",
                "PARECE EL PASAJERO ESCONDIO ALGO BAJO LAS PIERBAS",
                "PARECE EL PASAJERO SE ESTA HACIENDO UN PORRO",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCIA",
                "EL CONDUCTOR TIENE LA MUSICA DEL VEHICULO MUY ALTA"
                };
    
        private static string[] infracciones5 = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE ALCOHOLISMO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCI"
                };
       
        private static string[] infracciones6 = {
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "NO SE HA DETECTADO NINGUNA INFRACCION",
                "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA USANDO AURICULARES MIENTRAS CONDUCE",
                "EL CONDUCTOR DEL VEHICULO NO PRESTA LA ATENCION NECESARIA A LA VIA",
                "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE",
                "EL CONDUCTOR ESTA CONTROLANDO DOCUMENTACION MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE ALCOHOLISMO",
                "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS",
                "NO TIENE EL CINTURON PUESTO",
                "TIENE EL CINTURON MAL PUESTO",
                "EL PASAJERO NO TIENE EL CINTURON PUESTO",
                "EL PASAJERO TIENE EL CINTURON MAL PUESTO",
                "EL PASAJERO DISTRAE AL CONDUCOR",
                "PARECE EL PASAJERO Y EL CONDUCTOR ESTAN TENIENDO UNA FUERTE DISCUSION",
                "PARECE EL PASAJERO TIENE ALGO ILEGAL EN LAS MANOS",
                "PARECE EL PASAJERO ESCONDIO ALGO BAJO LAS PIERBAS",
                "PARECE EL PASAJERO SE ESTA HACIENDO UN PORRO",
                "EL VEHICULO PARECE TIENE LA PRESION DE NEUMATICOS MUY BAJOS",
                "EL CONDUCTOR ESTA COMIENDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET",
                "EL CONDUCTOR ESTA MANIPULANDO UN IPOD",
                "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV",
                "EL CONDUCTOR SE ESTA MAQUILLANDOSE MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA PEINANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR SE ESTA AFEITANDO MIENTRAS CONDUCE",
                "EL CONDUCTOR ESTA FUMANDO SIN MANTENER LA CONCENTRACION NECESARIA",
                "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA",
                "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCI",
                "EL CONDUCTOR TIENE LA MUSICA DEL VEHICULO MUY ALTA",
                "EL CONDUCTOR ESTA MANUPULANDO ALBARANES O FACTURAS"
                };
        
        private static Blip blipinfractor;

        public static void Main()
        {
            GameFiber.StartNew(delegate
            {
                Game.RawFrameRender += DrawVehicleInfo;
                LowPriority();
                Game.LogTrivial("Traffic Policer Speed Checker started.");
                try
                {
                    while (true)
                    {
                        GameFiber.Yield();
                        if (Game.LocalPlayer.Character.IsInAnyVehicle(false) && (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(ToggleSpeedCheckerModifierKey) || ToggleSpeedCheckerModifierKey == Keys.None))
                        {
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(ToggleSpeedCheckerKey))
                            {
                                if (CurrentSpeedCheckerState != SpeedCheckerStates.Off && CurrentSpeedCheckerState != SpeedCheckerStates.Speedgun)
                                {
                                    GameFiber.Wait(200);
                                    if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownRightNowComputerCheck(ToggleSpeedCheckerKey))
                                    {
                                        CurrentSpeedCheckerState = SpeedCheckerStates.Off;
                                        NativeFunction.Natives.DELETE_CHECKPOINT(CheckPoint);
                                        ResetAverageSpeedCheck();
                                        Game.HideHelp();
                                    }
                                    else
                                    {
                                        Game.DisplaySubtitle("~h~Hold Speed Checker toggle to disable.", 3000);
                                        if (CurrentSpeedCheckerState == SpeedCheckerStates.Off)
                                        {
                                            ResetAverageSpeedCheck();
                                            CurrentSpeedCheckerState = SpeedCheckerStates.FixedPoint;

                                        }
                                        else if (CurrentSpeedCheckerState == SpeedCheckerStates.FixedPoint)
                                        {
                                            NativeFunction.Natives.DELETE_CHECKPOINT(CheckPoint);
                                            CurrentSpeedCheckerState = SpeedCheckerStates.Off;

                                        }
                                    }
                                }
                                else if (Game.LocalPlayer.Character.IsInAnyVehicle(false))
                                {
                                    if (CurrentSpeedCheckerState == SpeedCheckerStates.Speedgun)
                                    {
                                        Game.DisplaySubtitle("Please unequip your speedgun first.");
                                    }
                                    else
                                    {
                                        if (Game.LocalPlayer.Character.CurrentVehicle.Speed > 6f)
                                        {
                                            CurrentSpeedCheckerState = SpeedCheckerStates.FixedPoint;
                                        }
                                        else
                                        {
                                            CurrentSpeedCheckerState = SpeedCheckerStates.FixedPoint;
                                        }
                                    }




                                }
                                CheckPointPosition = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0f, 8f, -1f));

                            }

                        }
                        if (CurrentSpeedCheckerState != SpeedCheckerStates.Off && CurrentSpeedCheckerState != SpeedCheckerStates.Speedgun)
                        {
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(SecondaryDisableKey) || !Game.LocalPlayer.Character.IsInAnyVehicle(false))
                            {
                                CurrentSpeedCheckerState = SpeedCheckerStates.Off;
                                NativeFunction.Natives.DELETE_CHECKPOINT(CheckPoint);
                                CheckPointPosition = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3(0f, 8f, -1f));

                                ResetAverageSpeedCheck();
                                Game.HideHelp();
                            }
                            //xOffset = 0;
                            //yOffset = 0;
                            //zOffset = 0;
                        }

                        if (CurrentSpeedCheckerState == SpeedCheckerStates.FixedPoint && Game.LocalPlayer.Character.IsInAnyVehicle(false))
                        {

                            CheckPointPosition = Game.LocalPlayer.Character.GetOffsetPosition(new Vector3((float)xOffset + 0.5f, (float)(yOffset + 8), (float)(-1 + zOffset)));

                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionResetKey))
                            {
                                xOffset = 0;
                                yOffset = 0;
                                zOffset = 0;

                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionForwardKey))
                            {
                                yOffset++;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionBackwardKey))
                            {
                                yOffset--;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionRightKey))
                            {
                                xOffset++;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionLeftKey))
                            {
                                xOffset--;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionUpKey))
                            {
                                zOffset++;
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(PositionDownKey))
                            {
                                zOffset--;
                            }
                            NativeFunction.Natives.DELETE_CHECKPOINT(CheckPoint);
                            CheckPoint = NativeFunction.Natives.CREATE_CHECKPOINT<int>(40, CheckPointPosition.X, CheckPointPosition.Y, CheckPointPosition.Z, CheckPointPosition.X, CheckPointPosition.Y, CheckPointPosition.Z, 3.5f, 255, 0, 0, 255, 0);
                            NativeFunction.Natives.SET_CHECKPOINT_CYLINDER_HEIGHT(CheckPoint, 2f, 2f, 2f);
                        }

                        /*if ((CurrentSpeedCheckerState == SpeedCheckerStates.FixedPoint && Game.LocalPlayer.Character.IsInAnyVehicle(false)))
                        {
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(MaxSpeedUpKey))
                            {
                                SpeedToColourAt += 5;
                                
                            }
                            if (Albo1125.Common.CommonLibrary.ExtensionMethods.IsKeyDownComputerCheck(MaxSpeedDownKey))
                            {
                                SpeedToColourAt -= 5;
                                if (SpeedToColourAt < 0) { SpeedToColourAt = 0; }
                                
                            }
                        }*/
                    }
                }
                catch (Exception e) { NativeFunction.Natives.DELETE_CHECKPOINT(CheckPoint); throw; }

            });
        }



        private static void LowPriority()
        {
            GameFiber.StartNew(delegate
            {
                while (true)
                {
                    GameFiber.Wait(100);
                    if (CurrentSpeedCheckerState == SpeedCheckerStates.FixedPoint && Game.LocalPlayer.Character.IsInAnyVehicle(false))
                    {
                        Entity[] WorldVehicles = World.GetEntities(CheckPointPosition, 7, GetEntitiesFlags.ConsiderAllVehicles | GetEntitiesFlags.ExcludePlayerVehicle);
                        foreach (Vehicle veh in WorldVehicles)
                        {
                            if (veh.Exists() && veh != Game.LocalPlayer.Character.CurrentVehicle && veh.DistanceTo(CheckPointPosition) <= 6.5f)
                            {
                                bool ShowVehicleNotification = false;
                                TargetModel = veh.Model.Name;
                                TargetModel = char.ToUpper(TargetModel[0]) + TargetModel.Substring(1).ToLower();
                                if (SpeedUnit == "MPH")
                                {
                                    TargetSpeed = (int)Math.Round(MathHelper.ConvertMetersPerSecondToMilesPerHour(veh.Speed));

                                }
                                else
                                {
                                    TargetSpeed = MathHelper.ConvertMetersPerSecondToKilometersPerHourRounded(veh.Speed);
                                }
                                if (TargetSpeed >= SpeedToColourAt)
                                {
                                    SpeedColour = Color.Red;
                                    if (PlayFlagBlip)
                                    {

                                        if (!VehiclesBlipPlayedFor.Contains(veh) && veh.IsEngineOn)
                                        {
                                            VehiclesBlipPlayedFor.Add(veh);

                                            ShowVehicleNotification = true;
                                        }

                                    }
                                }
                                else
                                {
                                    SpeedColour = Color.White;
                                }
                                //TargetSpeedLimit = GetSpeedLimit(veh.Position, SpeedUnit);

                                TargetFlag = "";
                                TargetLicencePlate = veh.LicensePlate;
                                FlagsTextColour = Color.White;




                                if (ShowVehicleNotification && veh.IsEngineOn)
                                {
                                    string dato = infracciones.PickRandom();

                                    string dato4 = infracciones4.PickRandom();

                                    Ped conductor = veh.Driver;

                                    if (dato != "NO SE HA DETECTADO NINGUNA INFRACCION" && dato4 != "NO SE HA DETECTADO NINGUNA INFRACCION")
                                    {

                                        if (veh.PassengerCount < 1)
                                        {


                                            blipinfractor = veh.AttachBlip();
                                            blipinfractor.Color = Color.DarkRed;

                                            FlagBlipPlayer.Play();
                                            VehiclesBlipPlayedFor.Add(veh);

                                            
  
                                            //MOVIL DETECTADO
                                            if (dato == "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE" || dato == "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE" || dato == "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE" || dato == "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET" || dato == "EL CONDUCTOR ESTA MANIPULANDO UN IPOD"
                                            || dato4 == "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE" || dato4 == "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE" || dato4 == "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE" || dato4 == "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET" || dato4 == "EL CONDUCTOR ESTA MANIPULANDO UN IPOD")
                                            {
                                                Rage.Native.NativeFunction.Natives.TASK_USE_MOBILE_PHONE_TIMED(conductor, 100000);
                                            }

                                            //REGISTRO DE ITV NO ESTA AL DIA
                                            if (dato == "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV" || dato4 == "EL CONDUCTOR NO TIENE PUESTA LA PEGATINA DE LA ITV") 
                                            {
                                                StopThePed.API.Functions.setVehicleRegistrationStatus(veh, StopThePed.API.STPVehicleStatus.Expired);
                                            }

                                            //NO LUCES DETECTADAS
                                            if (dato == "EL VEHICULO NO LLEVA LAS LUCES PUESTAS" || dato4 == "EL VEHICULO NO LLEVA LAS LUCES PUESTAS")
                                            {
                                                if (World.TimeOfDay.Hours < 7 || World.TimeOfDay.Hours > 20)
                                                {
                                                    Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(veh, 1);

                                                    if (Functions.IsPlayerPerformingPullover() && Vector3.Distance(Game.LocalPlayer.Character.Position, veh.Position) < 20f)
                                                    {

                                                        GameFiber.Wait(4000);

                                                        if (veh.Exists())
                                                        {
                                                            Rage.Native.NativeFunction.Natives.SET_VEHICLE_LIGHTS(veh, 0);
                                                        }
                                                        break;


                                                    }

                                                }
                                            }


                                            //CASO DROGAS
                                            

                                            if (dato == "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA" || dato == "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS" || dato == "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCIA" || dato == "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE HABER BEBIDO ALGO"
                                            ||  dato4 == "EL CONDUCTOR TIENE UNA ACTITUD AGRESIVA SOBRE LA VIA" || dato4 == "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE CONSUMIR DROGAS" || dato4 == "PARECE QUE EL CONDUCTOR ESTA CONDUCIENDO BAJO INDICIOS DE SOMNOLENCIA" || dato4 == "SE VE AL CONDUCTOR POCO CENTRADO Y POSIBLES SINTOMAS DE HABER BEBIDO ALGO") 
                                            {
                                                float speed = veh.Speed;
                                                if (speed <= 12f)
                                                {
                                                    speed = 12.1f;
                                                }


                                                estados d = (estados)(new Random()).Next(0, 4);
                                                switch (d) {
                                                    case estados.ALCOHOL:

                                                        StopThePed.API.Functions.setPedAlcoholOverLimit(conductor, true);
                                                        Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(conductor, 786603);
                                                        conductor.Tasks.CruiseWithVehicle(veh, speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
                                                        break;
                                                    case estados.DROGAS:
                                                        StopThePed.API.Functions.setPedUnderDrugsInfluence(conductor, true);
                                                        Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(conductor, 786603);
                                                        conductor.Tasks.CruiseWithVehicle(veh, speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
                                                        break;
                                                    case estados.AMBOS:
                                                        StopThePed.API.Functions.setPedUnderDrugsInfluence(conductor, true);
                                                        StopThePed.API.Functions.setPedAlcoholOverLimit(conductor, true);
                                                        Rage.Native.NativeFunction.Natives.SET_DRIVE_TASK_DRIVING_STYLE(conductor, 786603);
                                                        conductor.Tasks.CruiseWithVehicle(veh, speed, (VehicleDrivingFlags.FollowTraffic | VehicleDrivingFlags.YieldToCrossingPedestrians));
                                                        break;
                                                    default:
                                                        StopThePed.API.Functions.setPedAlcoholOverLimit(conductor, false);
                                                        StopThePed.API.Functions.setPedAlcoholOverLimit(conductor, false);
                                                        break;


                                                }




                                            }

                                            /*if (dato == "EL VEHICULO NO ESTA USANDO INTERMITENCIA" || dato4 == "EL VEHICULO NO ESTA USANDO INTERMITENCIA") 
                                            {
                                                while (Rage.Native.NativeFunction.Natives.SetVehicleIndicatorLights(veh, 1, true) || Rage.Native.NativeFunction.Natives.SetVehicleIndicatorLights(veh, 0, true)) {
                                                    Rage.Native.NativeFunction.Natives.SetVehicleIndicatorLights(veh, 1, false);
                                                    Rage.Native.NativeFunction.Natives.SetVehicleIndicatorLights(veh, 0, false);

                                                }
                                            }*/



                                            InfraccionDetectada.Play();
                                            Game.DisplayNotification("MATRICULA: ~b~" + TargetLicencePlate + "~n~~s~MARCA: ~b~" + TargetModel + "~n~~s~QUE VEO: " + dato);
                                            GameFiber.Wait(15000);
                                            if (blipinfractor.Exists()) { blipinfractor.Delete(); }





                                        }
                                        else
                                        {

                                            blipinfractor = veh.AttachBlip();
                                            blipinfractor.Color = Color.DarkRed;

                                            FlagBlipPlayer.Play();
                                            VehiclesBlipPlayedFor.Add(veh);

                                            if (dato == "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE" || dato == "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE" || dato == "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE" || dato == "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET" || dato == "EL CONDUCTOR ESTA MANIPULANDO UN IPOD"
                                           || dato4 == "EL CONDUCTOR DEL VEHICULO ESTA USANDO EL MOVIL MIENTRAS CONDUCE" || dato4 == "EL CONDUCTOR DEL VEHICULO NO TIENE AMBAS MANOS AL VOLANTE" || dato4 == "EL CONDUCTOR ESTA CONTROLANDO EL GPS MIENTRAS CONDUCE" || dato4 == "EL CONDUCTOR ESTA MANIPULANDO UNA TABLET" || dato4 == "EL CONDUCTOR ESTA MANIPULANDO UN IPOD")
                                            {
                                                Rage.Native.NativeFunction.Natives.TASK_USE_MOBILE_PHONE_TIMED(conductor, 100000);
                                            }

                                            InfraccionDetectada.Play();
                                            Game.DisplayNotification("MATRICULA: ~b~" + TargetLicencePlate + "~n~~s~MARCA: ~b~" + TargetModel + "~n~~s~QUE VEO: " + dato4);
                                            GameFiber.Wait(15000);
                                            if (blipinfractor.Exists()) { blipinfractor.Delete(); }


                                        }



                                    }
                                    if (dato == "NO SE HA DETECTADO NINGUNA INFRACCION" || dato4 == "NO SE HA DETECTADO NINGUNA INFRACCION")
                                    {
                                        Game.DisplayHelp("NO SE HA DETECTADO NINGUNA INFRACCION.");
                                    }

                                }


                            }
                        }
                    }

                }
            });
        }
        private static void ResetAverageSpeedCheck()
        {
            AverageSpeedCheckStopwatch.Reset();
            AverageSpeed = 0f;
            AverageSpeedCheckDistance = 0f;
            AverageSpeedCheckSecondsPassed = 0f;
            MeasuringAverageSpeed = false;
            AverageSpeedCheckerColor = Color.White;
            MeasuringAverageSpeed = false;
        }

        private static void StartAverageSpeedCheck()
        {
            AverageSpeedCheckStopwatch.Start();
            LastAverageSpeedCheckReferencePoint = Game.LocalPlayer.Character.CurrentVehicle.Position;
            MeasuringAverageSpeed = true;
            AverageSpeedCheckerColor = Color.Yellow;
        }
        private static void StopAverageSpeedCheck()
        {
            if (SpeedUnit == "MPH")
            {

                AverageSpeed = (AverageSpeedCheckDistance * 0.000621371f) / (AverageSpeedCheckSecondsPassed / 3600);

            }
            else
            {
                AverageSpeed = AverageSpeedCheckDistance / (AverageSpeedCheckSecondsPassed / 3600);
            }
            AverageSpeedCheckStopwatch.Stop();
            MeasuringAverageSpeed = false;
            AverageSpeedCheckerColor = Color.LightBlue;
        }

        private static void DrawVehicleInfo(object sender, GraphicsEventArgs e)
        {
            if (CurrentSpeedCheckerState == SpeedCheckerStates.FixedPoint)
            {


                Rectangle drawRect = new Rectangle(1, 250, 230, 117);
                e.Graphics.DrawRectangle(drawRect, Color.FromArgb(200, Color.Black));
                e.Graphics.DrawText("MATRICULA: " + TargetLicencePlate, "Arial Bold", 20.0f, new PointF(3f, 253f), Color.White, drawRect);
                e.Graphics.DrawText("MODELO: " + TargetModel, "Arial Bold", 20.0f, new PointF(3f, 278f), Color.White, drawRect);
                e.Graphics.DrawText("", "Arial Bold", 20.0f, new PointF(3f, 303f), SpeedColour, drawRect);
                //e.Graphics.DrawText("VELOCIAD FIJADA: " + SpeedToColourAt, "Arial Bold", 20.0f, new PointF(3f, 328f), FlagsTextColour, drawRect);



            }

        }
    }
}