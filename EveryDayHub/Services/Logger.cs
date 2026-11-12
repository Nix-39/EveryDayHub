
using EveryDayHub.Models;
using System;
using System.IO;

namespace EveryDayHub.Services
{
	public static class Logger
	{
		// Store log files next to the application.
		private static readonly string _dataFolder =
			Path.Combine(AppContext.BaseDirectory, "Data");

		private static readonly string _filePath =
			Path.Combine(_dataFolder, "log.json");

		public static bool Log(string type, string message)
		{// Create the Data folder if it does not exist.
			Directory.CreateDirectory(_dataFolder);
			// Create a data store for log entries.
			var logStore = new DataStore<LogEntry>(_filePath);

			// Load existing logs if the file exists.
			if (File.Exists(_filePath) && !logStore.LoadFromJson())
			{
				return false;
			}

			// Create a new log entry with the current time.
			var entry = new LogEntry
			{
				Timestamp = DateTime.Now,
				Type = type,
				Message = message
			};

			// Add the entry and save all logs.
			logStore.AddItem(entry);
			return logStore.SaveToJson();
		}
	}
}
