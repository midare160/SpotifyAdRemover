﻿using Microsoft.Win32;
using Spare.Helpers;
using Spare.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Spare.SpotifyTools
{
    public class Uninstaller
    {
        public static IEnumerable<ActionResult> DeleteDirectories()
        {
            foreach (var dir in new[] { Paths.Local, Paths.Roaming })
            {
                yield return PathHelper.DeleteDirectory(dir);
            }
        }

        public static void DeleteRegistryKeys()
        {
            const string softwareClasses = @"SOFTWARE\Classes\spotify";
            const bool throwOnMissing = false;

            Registry.LocalMachine.DeleteSubKeyTree(softwareClasses, throwOnMissing);
            Registry.Users.DeleteSubKeyTree($@"{WindowsIdentity.GetCurrent().User}_Classes\spotify", throwOnMissing);

            var currentUserKey = Registry.CurrentUser;
            currentUserKey.DeleteSubKeyTree(softwareClasses, throwOnMissing);
            currentUserKey.DeleteSubKeyTree($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Spotify", throwOnMissing);
            currentUserKey.DeleteSubKeyTree(@"SOFTWARE\Spotify", throwOnMissing);
        }

        public static async Task<bool> Uninstall()
        {
            if (File.Exists(Paths.SpotifyExe))
            {
                await Process.Start(Paths.SpotifyExe, "/uninstall /qn").WaitForExitAsync();
                await (ProcessHelper.GetProcessByName("SpotifyUninstall")?.WaitForExitAsync() ?? Task.CompletedTask);

                return !File.Exists(Paths.SpotifyExe);
            }

            return false;
        }
    }
}