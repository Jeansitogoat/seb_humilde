/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Client
{
	public class App : Application
	{
		private const int ILMCM_CHECKLAYOUTANDTIPENABLED = 0x00001;
		private const int ILMCM_LANGUAGEBAROFF = 0x00002;

		private static Mutex instanceMutex;
		private readonly CompositionRoot instances = new CompositionRoot();

		[STAThread]
		public static void Main()
		{
			try
			{
				StartApplication();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n\n" + e.StackTrace, "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				instanceMutex?.Close();
			}
		}

		private static void StartApplication()
		{
			if (NoInstanceRunning())
			{
				new App().Run();
				return;
			}

#if SEB_DEVELOPMENT
			if (TryRestartForDevelopment())
			{
				new App().Run();
				return;
			}
#endif

			MessageBox.Show("You can only run one instance of SEB at a time.", "Startup Not Allowed", MessageBoxButton.OK, MessageBoxImage.Information);
		}

#if SEB_DEVELOPMENT
		private static bool TryRestartForDevelopment()
		{
			foreach (var process in Process.GetProcessesByName("SafeExamBrowser.Client"))
			{
				if (process.Id == Process.GetCurrentProcess().Id)
				{
					continue;
				}

				process.Kill();
				process.WaitForExit(5000);
			}

			Thread.Sleep(500);

			return TryAcquireInstanceLock();
		}
#endif

		private static bool NoInstanceRunning()
		{
			return TryAcquireInstanceLock();
		}

		private static bool TryAcquireInstanceLock()
		{
			instanceMutex?.Close();
			instanceMutex = new Mutex(false, AppConfig.CLIENT_MUTEX_NAME);

			try
			{
				return instanceMutex.WaitOne(TimeSpan.Zero, true);
			}
			catch (AbandonedMutexException)
			{
				return true;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			// We need to manually initialize a monitor in order to prevent Windows from automatically doing so and thus rendering an input lanuage
			// switch in the bottom right corner of the desktop. This must be done before any UI element is initialized or rendered on the screen.
			InitLocalMsCtfMonitor(ILMCM_CHECKLAYOUTANDTIPENABLED | ILMCM_LANGUAGEBAROFF);

			instances.BuildObjectGraph(Shutdown);
			instances.LogStartupInformation();

			var success = instances.ClientController.TryStart();

			if (!success)
			{
				Shutdown();
			}
		}

		public new void Shutdown()
		{
			void shutdown()
			{
				instances.ClientController.Terminate();
				instances.LogShutdownInformation();

				UninitLocalMsCtfMonitor();

				base.Shutdown();
			}

			Dispatcher.InvokeAsync(shutdown);
		}

		[DllImport("MsCtfMonitor.dll", SetLastError = true)]
		private static extern IntPtr InitLocalMsCtfMonitor(int dwFlags);

		[DllImport("MsCtfMonitor.dll", SetLastError = true)]
		private static extern IntPtr UninitLocalMsCtfMonitor();
	}
}
