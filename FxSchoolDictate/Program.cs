using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

// xgettext --from-code=UTF-8 *.cs -jo nl.po
// msgfmt nl.po -o locale/nl/LC_MESSAGES/FxSchool.mo

// LANGUAGE=nl_BE:en_US; ./bin/Debug/FxSchoolCalculus.exe
using System.IO;
using System.Diagnostics;


namespace FxSchoolDictate
{
	class MainClass
	{
		// Configuration
		static int attempts = 3;
		static string language = "en";
		static string dictionaryFile = "dictionary.txt";

		// State
		static string[] dictionary;
		static Random random = new Random ();
		static int counterExercise = 0;
		static int counterRight = 0;
		static List <string> mistakes = new List<string> ();

		static int ReadConfiguration (string key, int def)
		{
			int val;
			if (int.TryParse (ConfigurationManager.AppSettings [key], out val))
				return val;
			return def;
		}

		static int[] ReadConfiguration (string key, int[] def)
		{
			List<int> val = new List<int> (); 
			foreach (string str in 
				ConfigurationManager.AppSettings [key].Split (new char[] {','}, 
					StringSplitOptions.RemoveEmptyEntries)) {
				int item;
				if (int.TryParse (str, out item))
					val.Add (item);
			}
			if (val.Count > 0)
				return val.ToArray ();
			else
				return def;
		}

		static bool ReadConfiguration (string key, bool def)
		{
			bool val;
			if (bool.TryParse (ConfigurationManager.AppSettings [key], out val))
				return val;
			return def;
		}

		static string ReadConfiguration (string key, string def)
		{
			string val = ConfigurationManager.AppSettings [key];
			if (!string.IsNullOrWhiteSpace (val))
				return val;
			return def;
		}

		static void ReadConfiguration ()
		{
			attempts = ReadConfiguration ("attempts", attempts);
			language = ReadConfiguration ("language", language);
			dictionaryFile = ReadConfiguration ("dictionary", dictionaryFile);
		}

		public static void GenerateExercise (out string task)
		{
			int pos = random.Next (dictionary.Length);
			task = dictionary [pos];
		}

		static void Dictate (string task)
		{
			string arg = string.Format ("-v{0}+f2 -s125 \"{1}\"", language, task);
			Process.Start ("espeak", arg);
			//Process process = Process.Start ("espeak", arg);
			//process.WaitForExit ();
		}

		public static void DoSingleExercise (string task, out bool stop)
		{
			stop = false;
			bool right = false;

			for (int attempt = 0; attempt < attempts && !stop && !right; attempt++) {
				bool firstAttempt = attempt == 0;
				bool lastAttempt = attempt == attempts - 1;
				bool rightInput = false;
				do {
					Dictate (task);
					Console.Write (Catalog.GetString ("Antwoord: "));
					string input = Console.ReadLine ();
					if (string.Compare (input, "stop", true) == 0) {
						stop = true;
					} else {
						if (!string.IsNullOrWhiteSpace (input)) {
							rightInput = true;	
							if (input.Equals (task, StringComparison.CurrentCultureIgnoreCase))
								right = true;
							else if (firstAttempt)
								mistakes.Add (task);
						}
					}
				} while (!rightInput && !stop);

				ConsoleColor color = ConsoleColor.Blue;
				string result;
				if (stop)
					result = Catalog.GetString ("STOP!");
				else {
					counterExercise++;
					if (right) {
						color = ConsoleColor.Green; 
						result = Catalog.GetString ("RIGHT!");
						counterRight++;
					} else if (lastAttempt) {
						color = ConsoleColor.Red; 
						result = string.Format (Catalog.GetString ("WRONG! The correct answer was {0}"), 
							task);
					} else {
						color = ConsoleColor.Red; 
						result = Catalog.GetString ("WRONG!");
					}
				}

				Console.Write ("*** ");
				Console.ForegroundColor = color;
				Console.Write (result);
				Console.ResetColor ();
				Console.Write (" *** ");
				Console.WriteLine (Catalog.GetString ("Your score is now {0} / {1}"), 
					counterRight, counterExercise); 
			}
		}

		public static void Main (string[] args)
		{
			ReadConfiguration ();

			Catalog.Init ("FxSchool", "./locale");

			Console.WriteLine (Catalog.GetString ("FxSchoolDictate 1.0"));
			Console.WriteLine ("(c) Copyright 2016 Fox Innovations / Wim Devos");
			Console.WriteLine ("==============================================================================");

			Console.WriteLine (Catalog.GetString ("Configuration: "));
			Console.WriteLine (Catalog.GetString ("  * Attempts:   {0}"), attempts);
			Console.WriteLine (Catalog.GetString ("  * Language:   {0}"), language);
			Console.WriteLine (Catalog.GetString ("  * Dictionary: {0}"), dictionaryFile);
			Console.WriteLine ("==============================================================================");

			dictionary = File.ReadAllLines (dictionaryFile).Select (l => l.Trim ()).
				Where (l => !string.IsNullOrWhiteSpace (l) && !l.StartsWith ("#")).ToArray ();

			bool stop = false;
			do {
				string task;
				GenerateExercise (out task);

				DoSingleExercise (task, out stop);
			} while (!stop);

			stop = false;
			while (mistakes.Count > 0 && !stop) {
				Console.WriteLine ();
				Console.WriteLine (
					Catalog.GetPluralString (
						"You made {0} mistake, which we will now repeat:",
						"You made {0} mistakes, which we will now repeat:",
						mistakes.Count), 
					mistakes.Count);

				List <string> mistakesCopy = mistakes;
				mistakes = new List<string> ();


				mistakesCopy.ForEach ((string fout) => {
					if (!stop)
						DoSingleExercise (fout, out stop);
				});
			}

			if (!stop) {
				Console.WriteLine ();
				Console.WriteLine (Catalog.GetString ("No mistakes!"));
			}
		}
	}
}
