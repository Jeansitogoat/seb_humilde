/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SafeExamBrowser.Configuration.Contracts;

namespace SafeExamBrowser.Runtime
{
	public class App : Application
	{
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
			foreach (var process in Process.GetProcessesByName("SafeExamBrowser"))
			{
				if (process.Id == Process.GetCurrentProcess().Id)
				{
					continue;
				}

				process.Kill();
				process.WaitForExit(5000);
			}

			foreach (var process in Process.GetProcessesByName("SafeExamBrowser.Client"))
			{
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
			instanceMutex = new Mutex(false, AppConfig.RUNTIME_MUTEX_NAME);

			try
			{
				return instanceMutex.WaitOne(TimeSpan.Zero, true);
			}
			catch (AbandonedMutexException)
			{
				// A previous SEB instance was terminated without releasing the mutex.
				return true;
			}
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			ShutdownMode = ShutdownMode.OnExplicitShutdown;

			instances.BuildObjectGraph(Shutdown);
			instances.LogStartupInformation();

			Task.Run(new Action(TryStart));
		}

		private void TryStart()
		{
			var success = instances.RuntimeController.TryStart();

			if (!success)
			{
				Shutdown();
			}
		}

		public new void Shutdown()
		{
			Task.Run(new Action(ShutdownInternal));
		}

		private void ShutdownInternal()
		{
			instances.RuntimeController.Terminate();
			instances.LogShutdownInformation();

			Dispatcher.Invoke(base.Shutdown);
		}
	}
}
