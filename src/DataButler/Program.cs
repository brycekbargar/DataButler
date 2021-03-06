﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DataButler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetupRegistry();            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var startForm = GetStartupForm();
            if (startForm == null) return;
            Application.Run(startForm);
        }

        static Form GetStartupForm()
        {
            if (Environment.GetCommandLineArgs().Count() == 1) return new Backup();

            return AbortRestore() ? null : new Restore(GetCommandLineArgOrNull(1), GetCommandLineArgOrNull(2));
        }

        private static string GetCommandLineArgOrNull(int index)
        {
            var args = Environment.GetCommandLineArgs();
            return index < args.Length ? args[index] : null;
        }


        static bool AbortRestore()
        {
            if (Environment.GetCommandLineArgs().Count() < 2) return true;
            if (File.Exists(GetCommandLineArgOrNull(1))) return false;
            MessageBox.Show("Invalid database backup.", "DataButler");
            return true;
        }

        static void SetupRegistry()
        {
#if !DEBUG
            var dataButlerPath = string.Format(@"{0} ""%1""", Process.GetCurrentProcess().MainModule.FileName);
            SetupContextMenuAssociations(dataButlerPath);
#endif
        }

        static void SetupContextMenuAssociations(string dataButlerPath)
        {
            var bakHive = @"Software\Classes\.bak";
            if (!HiveExists(bakHive)) CreateHive(bakHive);
            SetDefaultHiveValue(bakHive, "DataButler");

            var commandHive = @"Software\Classes\DataButler\shell\Restore via DataButler\command";
            if (!HiveExists(commandHive)) CreateHive(commandHive);
            SetDefaultHiveValue(commandHive, dataButlerPath);

            var openCommandHive = @"Software\Classes\DataButler\shell\open\command";
            if (!HiveExists(openCommandHive)) CreateHive(openCommandHive);
            SetDefaultHiveValue(openCommandHive, dataButlerPath);
        }

        static bool HiveExists(string hive)
        {
            var softwareKey = Registry.CurrentUser.OpenSubKey(hive);
            return softwareKey != null;
        }

        static void CreateHive(string bakHive)
        {
            Registry.CurrentUser.CreateSubKey(bakHive);
        }

        static void SetDefaultHiveValue(string hive, string defaultValue)
        {
            var key = Registry.CurrentUser.OpenSubKey(hive, RegistryKeyPermissionCheck.ReadWriteSubTree);
            key.SetValue(null, defaultValue);
        }
    }
}
