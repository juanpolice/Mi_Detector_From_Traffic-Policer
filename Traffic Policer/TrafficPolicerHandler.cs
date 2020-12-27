using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;
using LSPD_First_Response.Mod.API;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Management;
using System.Net;
using System.Reflection;
using Rage.Native;

[assembly: Rage.Attributes.Plugin("Traffic Policer", Description = "INSTALL IN PLUGINS/LSPDFR. Enhances traffic policing in LSPDFR", Author = "Albo1125")]
namespace Traffic_Policer
{
    public class EntryPoint
    {
        public static void Main()
        {
            Game.DisplayNotification("You have installed Traffic Policer incorrectly and in the wrong folder: you must install it in Plugins/LSPDFR. It will then be automatically loaded when going on duty - you must NOT load it yourself via RAGEPluginHook. This is also explained in the Readme and Documentation. You will now be redirected to the installation tutorial.");
            GameFiber.Wait(5000);
            Process.Start("https://youtu.be/af434m72rIo");
            return;
        }
    }

    internal class TrafficPolicerHandler
    {


        public static List<GameFiber> AmbientEventGameFibersToAbort = new List<GameFiber>();

        public static List<Ped> PedsToChargeWithDrinkDriving = new List<Ped>();
        public static List<Ped> PedsToChargeWithDrugDriving = new List<Ped>();

        private static Random eventRnd = new Random();





        private static void loadValuesFromIniFile()
        {
            try
            {

                if (NumberOfAmbientEventsBeforeTimer < 1) { NumberOfAmbientEventsBeforeTimer = 1; }





                SpeedChecker.ToggleSpeedCheckerKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "ToggleSpeedCheckerKey"));
                SpeedChecker.ToggleSpeedCheckerModifierKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "ToggleSpeedCheckerModifierKey"));
                SpeedChecker.SpeedUnit = initialiseFile().ReadString("Speed Checker Settings", "SpeedUnit");
                if (SpeedChecker.SpeedUnit != "MPH" && SpeedChecker.SpeedUnit != "KMH")
                {
                    SpeedChecker.SpeedUnit = "KMH";
                }



                SpeedChecker.PositionUpKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionUpKey", "NumPad9"));
                SpeedChecker.PositionRightKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionRightKey", "NumPad6"));
                SpeedChecker.PositionResetKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionResetKey", "NumPad5"));
                SpeedChecker.PositionLeftKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionLeftKey", "NumPad4"));
                SpeedChecker.PositionForwardKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionForwardKey", "NumPad8"));
                SpeedChecker.PositionDownKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionDownKey", "NumPad3"));
                SpeedChecker.PositionBackwardKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "PositionBackwardKey", "NumPad2"));
                SpeedChecker.SecondaryDisableKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "SecondaryDisableKey", "Back"));
                SpeedChecker.MaxSpeedUpKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "MaxSpeedUpKey", "PageUp"));
                SpeedChecker.MaxSpeedDownKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "MaxSpeedDownKey", "PageDown"));
                SpeedChecker.FlagChance = initialiseFile().ReadInt32("Speed Checker Settings", "BringUpFlagChance");
                if (SpeedChecker.FlagChance < 1) { SpeedChecker.FlagChance = 1; }
                else if (SpeedChecker.FlagChance > 100) { SpeedChecker.FlagChance = 100; }
                SpeedChecker.SpeedToColourAt = initialiseFile().ReadInt32("Speed Checker Settings", "SpeedToColourAt");
                SpeedChecker.PlayFlagBlip = initialiseFile().ReadBoolean("Speed Checker Settings", "PlayFlagBlip");

                SpeedChecker.StartStopAverageSpeedCheckKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "StartStopAverageSpeedCheckKey", "PageUp"));
                SpeedChecker.ResetAverageSpeedCheckKey = (Keys)kc.ConvertFromString(initialiseFile().ReadString("Speed Checker Settings", "ResetAverageSpeedCheckKey", "PageDown"));




                getNextEventTimer();


            }
            catch (Exception e)
            {

                Game.LogTrivial(e.ToString());
                Game.LogTrivial("Loading default Traffic Policer INI file - Error detected in user's INI file.");
                Game.DisplayNotification("~r~~h~Error~s~ reading Traffic Policer ini file. Default values set; replace with default INI file!");
                Albo1125.Common.CommonLibrary.ExtensionMethods.DisplayPopupTextBoxWithConfirmation("Traffic Policer INI file", "Error reading Traffic Policer INI file. To fix this, replace your current INI file with the original one from the download. Loading default values...", true);

            }

        }



        public static KeysConverter kc = new KeysConverter();




        public static List<Ped> driversConsidered { get; set; }



        public static Random rnd = new Random();
        private static int NumberOfAmbientEventsBeforeTimer = 1;



        private static int nextEventTimer { get; set; }
        public static bool PerformingImpairmentTest = false;


        internal static void mainLoop()
        {
            Game.LogTrivial("Traffic Policer.Mainloop started");

            Game.LogTrivial("Loading Traffic Policer settings...");
            loadValuesFromIniFile();


            NextEventStopwatch.Start();
            driversConsidered = new List<Ped>();






            if (SpeedChecker.ToggleSpeedCheckerKey != Keys.None)
            {
                SpeedChecker.Main();
            }
            Game.LogTrivial("HA SIO CARGADO CORRECTAMENTE!");
            GameFiber.StartNew(delegate
            {
                GameFiber.Wait(12000);
                uint startnot = Game.DisplayNotification("~g~GUARDIA CIVIL TRAFICO ~b~INICIANDO SERVICIO!");
                GameFiber.Sleep(6000);
                Game.RemoveNotification(startnot);


                //Low priority loop
                while (true)
                {
                    GameFiber.Wait(1000);

                }

            });


        }
        public static bool IsLSPDFRPluginRunning(string Plugin, Version minversion = null)
        {
            foreach (Assembly assembly in Functions.GetAllUserPlugins())
            {
                AssemblyName an = assembly.GetName(); if (an.Name.ToLower() == Plugin.ToLower())
                {
                    if (minversion == null || an.Version.CompareTo(minversion) >= 0) { return true; }
                }
            }
            return false;
        }


        public static Assembly LSPDFRResolveEventHandler(object sender, ResolveEventArgs args) { foreach (Assembly assembly in Functions.GetAllUserPlugins()) { if (args.Name.ToLower().Contains(assembly.GetName().Name.ToLower())) { return assembly; } } return null; }



        private static int AmbientEventsPassed = 0;
        private static Stopwatch NextEventStopwatch = new Stopwatch();
        private static void SetNextEventStopwatch()
        {
            AmbientEventsPassed++;
            if (AmbientEventsPassed >= NumberOfAmbientEventsBeforeTimer)
            {
                NextEventStopwatch.Reset();
                NextEventStopwatch.Start();
            }
        }
        public static InitializationFile initialiseFile()
        {
            InitializationFile ini = new InitializationFile("Plugins/LSPDFR/midetector.ini");
            ini.Create();
            return ini;
        }



        private static string getShowIniMessage()
        {
            InitializationFile ini = initialiseFile();
            string show = ini.ReadString("General", "ShowStartupIniFileMessage", "true");
            return show;
        }



        private static void getNextEventTimer()
        {
            InitializationFile ini = initialiseFile();
            nextEventTimer = ini.ReadInt32("Ambient Event Chances", "NextEventTimer", 35);
            if (nextEventTimer < 5)
            {
                nextEventTimer = 5;
            }
            if (nextEventTimer > 200)
            {
                nextEventTimer = 200;
            }
            nextEventTimer *= 1000;
        }

        internal static void Initialise()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(LSPDFRResolveEventHandler);

            GameFiber.StartNew(delegate
            {
                Game.LogTrivial("midetector is not in beta.");
                mainLoop();
                Game.LogTrivial("midetector, BASED ON TRAFFIC POLICER, developed by Albo1125, has been loaded successfully!");
                GameFiber.Wait(6000);
                Game.DisplayNotification("~b~midetector, BASED ON TRAFFIC POLICER~s~, developed by ~b~Albo1125, And modify by juanpolice ~s~has been loaded ~g~successfully.");
            });
        }
    }
}
