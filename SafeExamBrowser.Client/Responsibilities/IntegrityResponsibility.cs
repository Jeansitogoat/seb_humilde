/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using SafeExamBrowser.Client.Contracts;
using SafeExamBrowser.I18n.Contracts;
using SafeExamBrowser.Integrity.Contracts;
using SafeExamBrowser.Logging.Contracts;
using SafeExamBrowser.UserInterface.Contracts.Windows.Data;

namespace SafeExamBrowser.Client.Responsibilities
{
	internal class IntegrityResponsibility : ClientResponsibility
	{
		private readonly ICoordinator coordinator;
		private readonly IText text;
		private readonly Timer timer;

		private IIntegrityModule IntegrityModule => Context.IntegrityModule;

		public IntegrityResponsibility(ClientContext context, ICoordinator coordinator, ILogger logger, IText text) : base(context, logger)
		{
			this.coordinator = coordinator;
			this.text = text;
			this.timer = new Timer();
		}

		public override void Assume(ClientTask task)
		{
			switch (task)
			{
				case ClientTask.PrepareShutdown_Wave2:
					StopIntegrityMonitoring();
					break;
				case ClientTask.ScheduleIntegrityVerification:
					ScheduleIntegrityVerification();
					break;
				case ClientTask.StartMonitoring:
					StartIntegrityMonitoring();
					break;
				case ClientTask.UpdateSessionIntegrity:
					UpdateSessionIntegrity();
					break;
				case ClientTask.VerifySessionIntegrity:
					VerifySessionIntegrity();
					break;
			}
		}

		private void ScheduleIntegrityVerification()
		{
		}

		private void StartIntegrityMonitoring()
		{
		}

		private void StopIntegrityMonitoring()
		{
			timer.Stop();
			timer.Elapsed -= Timer_Elapsed;

			Logger.Info("Stopped monitoring runtime integrity.");
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			Logger.Info("Attempting to verify runtime integrity...");

			if (IntegrityModule.TryVerifyRuntimeIntegrity(out var isValid))
			{
				HandleRuntimeIntegrityStatus(isValid);
			}
			else
			{
				Logger.Warn("Failed to verify runtime integrity!");
			}

			timer.Start();
		}

		private void UpdateSessionIntegrity()
		{
			IntegrityModule?.ClearSession(Settings?.Browser.ConfigurationKey);
		}

		private void VerifyApplicationIntegrity()
		{
		}

		private void VerifySessionIntegrity()
		{
			IntegrityModule?.ResetSessionCache();
		}

		private void HandleApplicationIntegrityStatus(bool isValid)
		{
		}

		private void HandleRuntimeIntegrityStatus(bool isValid)
		{
		}

		private void HandleSessionIntegrityStatus(bool isValid)
		{
			IntegrityModule?.ResetSessionCache();
		}
	}
}
