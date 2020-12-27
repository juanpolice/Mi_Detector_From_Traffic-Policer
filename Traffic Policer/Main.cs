using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Reflection;

namespace Traffic_Policer
{

    using LSPD_First_Response.Mod.API;
    using Rage;


    /// <summary>
    /// Do not rename! Attributes or inheritance based plugins will follow when the API is more in depth.
    /// </summary>
    internal class Main : Plugin
    {
        /// <summary>
        /// Constructor for the main class, same as the class, do not rename.
        /// </summary>
        public Main()
        {
            Game.LogTrivial("Creating midetector.Main.");
            Functions.OnOnDutyStateChanged += Functions_OnOnDutyStateChanged;
            //Albo1125.Common.UpdateChecker.CheckForModificationUpdates("Traffic Policer", Assembly.GetExecutingAssembly().GetName().Version, new InitializationFile("Plugins/LSPDFR/Traffic Policer.ini"), VersionCheckURL, DownloadURL);
            Albo1125.Common.UpdateChecker.VerifyXmlNodeExists(PluginName, FileID, DownloadURL, Path);
            Albo1125.Common.DependencyChecker.RegisterPluginForDependencyChecks(PluginName);
            Game.LogTrivial("Done with midetector.Main.");
        }

        /// <summary>
        /// Called when the plugin ends or is terminated to cleanup
        /// </summary>
        public override void Finally()
        {
            return;
        }

        /// <summary>
        /// Called when the plugin is first loaded by LSPDFR
        /// </summary>
        public override void Initialize()
        {
            //Event handler for detecting if the player goes on duty


            Game.LogTrivial("midetector based on traffic policer " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + ", developed by Albo1125, Modify by Juanpolice, has been initialised.");
            Game.LogTrivial("Go on duty to start midetector - midetector.Initialise done.");


        }
        //Dependencies

        internal static string[] AudioFilesToCheckFor = new string[] { "lspdfr/audio/scanner/midetector/OTHER_UNIT_TAKING_CALL/OTHER_UNIT_TAKING_CALL_01.wav", "lspdfr/audio/scanner/midetector/Crimes/CRIME_DUI_01.wav" };


        internal static string FileID = "8303";
        internal static string DownloadURL = "https://www.lcpdfr.com/files/file/8303-traffic-policer-breathalyzer-traffic-offences-speed-detection-more/";
        internal static string PluginName = "Midetector";
        internal static string Path = "Plugins/LSPDFR/Midetector.dll";

        internal static string[] ConflictingFiles = new string[] { "Plugins/BreathalyzerRAGE.dll", "Plugins/LSPDFR/SpeedRadar.dll" };
        /// <summary>
        /// The event handler mentioned above,
        /// </summary>
        static void Functions_OnOnDutyStateChanged(bool onDuty)
        {
            Game.LogTrivial("In traffic policer duty event handler: " + onDuty.ToString());
            if (onDuty)
            {
                Albo1125.Common.UpdateChecker.InitialiseUpdateCheckingProcess();

                TrafficPolicerHandler.Initialise();

            }
        }
    }
}