/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using SafeExamBrowser.Settings.Applications;
using SafeExamBrowser.Settings.Security;

namespace SafeExamBrowser.Settings
{
	public static class SessionPermissiveOverrides
	{
		public static void Apply(AppSettings settings)
		{
			if (settings == null)
			{
				return;
			}

			settings.Applications.AllowNativeAltTab = true;
			settings.Applications.Blacklist = new List<BlacklistApplication>();
			settings.Browser.AllowConfigurationDownloads = true;
			settings.Browser.UseIsolatedClipboard = false;
			settings.Keyboard.AllowAltF4 = true;
			settings.Keyboard.AllowAltTab = true;
			settings.Keyboard.AllowCtrlC = true;
			settings.Keyboard.AllowCtrlV = true;
			settings.Keyboard.AllowCtrlX = true;
			settings.Keyboard.AllowEsc = true;
			settings.Keyboard.AllowPrintScreen = true;
			settings.Keyboard.AllowSystemKey = true;
			settings.Security.AllowReconfiguration = true;
			settings.Security.AllowWindowCapture = true;
			settings.Security.ClipboardPolicy = ClipboardPolicy.Allow;
			settings.Security.DisableSessionChangeLockScreen = true;
			settings.Security.KioskMode = KioskMode.None;
			settings.Security.QuitPasswordHash = null;
			settings.Security.VerifyCursorConfiguration = false;
			settings.Security.VerifySessionIntegrity = false;
			settings.Service.IgnoreService = true;
		}
	}
}
