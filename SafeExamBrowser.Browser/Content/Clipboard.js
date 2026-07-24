/*
 * Copyright (c) 2026 ETH Z�rich, IT Services
 *
 * Audit build: use the native Windows clipboard.
 * Do not intercept copy, cut or paste events.
 */

if (typeof SafeExamBrowser !== 'undefined' && typeof SafeExamBrowser.clipboard === 'undefined') {
	SafeExamBrowser.clipboard = {
		id: '',
		ranges: [],
		text: '',
		clear: function () { },
		getContentEncoded: function () { return ''; },
		update: function () { }
	};
}
