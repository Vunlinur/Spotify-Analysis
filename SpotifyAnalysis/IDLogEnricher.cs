using System;
using System.Threading;
using Serilog.Core;
using Serilog.Events;

namespace SpotifyAnalysis {
	public class IDLogEnricher() : ILogEventEnricher {
		private readonly AsyncLocal<string?> sessionId = new();

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory) {
			if (string.IsNullOrWhiteSpace(sessionId.Value))
				sessionId.Value = Guid.NewGuid().ToString("n").Substring(0, 7);
			logEvent.AddPropertyIfAbsent(factory.CreateProperty("SessionID", sessionId.Value));
		}
	}
}
