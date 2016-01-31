using Mono.Unix;
using System;
using System.Collections.Generic;
using System.Configuration;

// xgettext --from-code=UTF-8 *.cs -jo nl.po
// msgfmt nl.po -o locale/nl/LC_MESSAGES/FxSchool.mo

// LANGUAGE=nl_BE:en_US; ./bin/Debug/FxSchoolCalculus.exe


namespace Tafels
{
	class MainClass
	{
		// Configuration
		static int attempts = 3;
		static bool allowAdd = true;
		static bool allowSubtract = true;
		static bool allowMultiply = true;
		static bool allowDivide = true;
		static int multiplicationMaximum = 10;
		static int addMaximum = 100;
		static int[] multiplicationTables = new int[] { 0, 1, 2, 3, 4, 5, /*6, 7, 8, 9,*/ 10 };
		static bool benfoldsDistribution = false;

		// State
		static Random random = new Random ();
		static int counterExercise = 0;
		static int counterRight = 0;
		static List < Tuple<string, int>> mistakes = new List<Tuple<string, int>> ();

		[Flags]
		enum ValidQuestions
		{
			None = 0,
			Number1 = 1,
			Number2 = 2,
			Result = 4,
			All = Number1 | Number2 | Result
		}

		delegate void GenerateExerciseHandler (out int number1, out char oper, out int number2, out int result,
			out ValidQuestions validQuestions);

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

		static void ReadConfiguration ()
		{
			attempts = ReadConfiguration ("attempts", attempts);
			allowAdd = ReadConfiguration ("allowAdd", allowAdd);
			allowSubtract = ReadConfiguration ("allowSubtract", allowSubtract);
			allowMultiply = ReadConfiguration ("allowMultiply", allowMultiply);
			allowDivide = ReadConfiguration ("allowDivide", allowDivide);
			multiplicationMaximum = ReadConfiguration ("multiplicationMaximum", multiplicationMaximum);
			addMaximum = ReadConfiguration ("addMaximum", addMaximum);
			multiplicationTables = ReadConfiguration ("multiplicationTables", multiplicationTables);
			benfoldsDistribution = ReadConfiguration ("benfoldsDistribution", benfoldsDistribution);
		}

		static int RandomNumber (int maximum)
		{
			if (benfoldsDistribution)
				return (int)Math.Floor (Math.Pow (10.0, random.NextDouble ()) * maximum / 10.0);
			else
				return random.Next (maximum);
		}

		private static void GenerateExerciseAdd (out int number1, out char oper, out int number2, out int result,
		                                         out ValidQuestions validQuestions)
		{
			oper = '+';
			validQuestions = ValidQuestions.All;

			number1 = RandomNumber (addMaximum + 1);
			number2 = RandomNumber (addMaximum + 1 - number1);
			result = number1 + number2;
		}

		private static void GenerateExerciseSubtract (out int number1, out char oper, out int number2, out int result,
		                                              out ValidQuestions validQuestions)
		{
			oper = '-';
			validQuestions = ValidQuestions.All;

			number1 = RandomNumber (addMaximum + 1);
			number2 = RandomNumber (number1 + 1);
			result = number1 - number2;
		}

		private static void GenerateExerciseMultiply (out int number1, out char oper, out int number2, out int result,
		                                              out ValidQuestions validQuestions)
		{
			oper = 'x';
			validQuestions = ValidQuestions.All;

			int len = multiplicationTables.Length;
			int number = RandomNumber (multiplicationMaximum + 1);
			int table = multiplicationTables [RandomNumber (len)];
			int product = number * table;

			switch (random.Next (2)) {
			case 0: // number * table = product
				number1 = number;
				number2 = table;
				result = product;
				break;
			case 1: // table * number = product
				number1 = table;
				number2 = number;
				result = product;
				break;
			default:
				throw new ApplicationException ();
			}

			if (number1 == 0)
				validQuestions &= ~ValidQuestions.Number2;
			if (number2 == 0)
				validQuestions &= ~ValidQuestions.Number1;
		}

		private static void GenerateExerciseDivide (out int number1, out char oper, out int number2, out int result,
		                                            out ValidQuestions validQuestions)
		{
			oper = ':';
			validQuestions = ValidQuestions.All;

			int len = multiplicationTables.Length;
			int number = RandomNumber (multiplicationMaximum + 1);
			int table = multiplicationTables [RandomNumber (len)];
			int product = number * table;

			switch (random.Next (2)) {
			case 0: // product : table = number
				number1 = product;
				number2 = table;
				result = number;
				break;
			case 1: // product : number = table
				number1 = product;
				number2 = number;
				result = table;
				break;
			default:
				throw new ApplicationException ();
			}

			if (number1 == 0)
				validQuestions &= ~ValidQuestions.Number2;
			if (number2 == 0)
				validQuestions = ValidQuestions.None;
		}

		public static void GenerateExercise (out string task, out int solution)
		{
			int number1, number2, result;
			char oper;
			ValidQuestions validQuestions;

			const int PossibilityAdd = 0;
			const int PossibilitySubtract = 1;
			const int PossibilityMultiply = 2;
			const int PossibilityDivide = 3;

			List<int> possibilities = new List<int> ();
			if (allowAdd)
				possibilities.Add (PossibilityAdd);
			if (allowSubtract)
				possibilities.Add (PossibilitySubtract);
			if (allowMultiply)
				possibilities.Add (PossibilityMultiply);
			if (allowDivide)
				possibilities.Add (PossibilityDivide);

			if (possibilities.Count == 0)
				throw new ApplicationException ();

			int possibility = possibilities [random.Next (possibilities.Count)];

			GenerateExerciseHandler generateExercise;
			switch (possibility) {
			case PossibilityAdd:
				generateExercise = GenerateExerciseAdd;
				break;
			case PossibilitySubtract:
				generateExercise = GenerateExerciseSubtract;
				break;
			case PossibilityMultiply:
				generateExercise = GenerateExerciseMultiply;
				break;
			case PossibilityDivide:
				generateExercise = GenerateExerciseDivide;
				break;
			default:
				throw new ApplicationException ();
			}
			do {
				generateExercise (out number1, out oper, out number2, out result, out validQuestions);
			} while (validQuestions == ValidQuestions.None);

			ValidQuestions question = ValidQuestions.None;
			do {
				switch (random.Next (3)) {
				case 0:
					if (validQuestions.HasFlag (ValidQuestions.Number1))
						question = ValidQuestions.Number1;
					break;
				case 1:
					if (validQuestions.HasFlag (ValidQuestions.Number2))
						question = ValidQuestions.Number2;
					break;
				case 2:
					if (validQuestions.HasFlag (ValidQuestions.Result))
						question = ValidQuestions.Result;
					break;
				default:
					throw new ApplicationException ();
				}
			} while (question == ValidQuestions.None);

			string textNumber1 = number1.ToString ();
			string textNumber2 = number2.ToString ();
			string textResult = result.ToString ();

			switch (question) {
			case ValidQuestions.Number1:
				solution = number1;
				textNumber1 = "?";
				break;
			case ValidQuestions.Number2:
				solution = number2;
				textNumber2 = "?";
				break;
			case ValidQuestions.Result:
				solution = result;
				textResult = "?";
				break;
			default:
				throw new ApplicationException ();
			}

			task = string.Format ("{0} {3} {1} = {2}", textNumber1, textNumber2, textResult, oper);
			task = string.Format (Catalog.GetString ("{0}  => Answer: "), task);
		}

		public static void DoSingleExercise (string task, int solution, out bool stop)
		{
			stop = false;
			bool right = false;

			for (int attempt = 0; attempt < attempts && !stop && !right; attempt++) {
				bool firstAttempt = attempt == 0;
				bool lastAttempt = attempt == attempts - 1;
				bool rightInput = false;
				do {
					Console.Write (task);
					string inputString = Console.ReadLine ();
					if (string.Compare (inputString, "stop", true) == 0) {
						stop = true;
					} else {
						int input;
						if (int.TryParse (inputString, out input)) {
							rightInput = true;
							if (input == solution)
								right = true;
							else if (firstAttempt)
								mistakes.Add (new Tuple<string, int> (task, solution));
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
							solution.ToString ());
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

			string yes = Catalog.GetString ("yes");
			string no = Catalog.GetString ("no");

			Console.WriteLine (Catalog.GetString ("FxSchoolCalculus 1.0"));
			Console.WriteLine ("(c) Copyright 2016 Fox Innovations / Wim Devos");
			Console.WriteLine ("==============================================================================");

			if ((allowAdd || allowSubtract || allowMultiply || allowDivide) == false) {
				Console.WriteLine (Catalog.GetString ("No exercise is allowed in the configuration.  Allowing all."));
				allowAdd = true;
				allowSubtract = true;
				allowMultiply = true;
				allowDivide = true;
			}
				
			Console.WriteLine (Catalog.GetString ("Configuration: "));
			Console.WriteLine (Catalog.GetString ("  * Attempts:                {0}"), attempts);
			Console.WriteLine (Catalog.GetString ("  * Add allowed:             {0}"), allowAdd ? yes : no);
			Console.WriteLine (Catalog.GetString ("  * Subtract allowed:        {0}"), allowSubtract ? yes : no);
			Console.WriteLine (Catalog.GetString ("  * Multiply allowed:        {0}"), allowMultiply ? yes : no);
			Console.WriteLine (Catalog.GetString ("  * Divide allowed:          {0}"), allowDivide ? yes : no);
			Console.WriteLine (Catalog.GetString ("  * Multiply/divide maximum: {0}"), multiplicationMaximum);
			Console.WriteLine (Catalog.GetString ("  * Multiply/divide tables:  {0}"), string.Join (", ", multiplicationTables));
			Console.WriteLine (Catalog.GetString ("  * Add/Subtract maximum:    {0}"), addMaximum);
			Console.WriteLine (Catalog.GetString ("  * Benfold's-distribution:  {0}"), benfoldsDistribution ? yes : no);
			Console.WriteLine ("==============================================================================");

			bool stop = false;
			do {
				string task;
				int answer;
				GenerateExercise (out task, out answer);

				DoSingleExercise (task, answer, out stop);
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

				List < Tuple<string, int>> mistakesCopy = mistakes;
				mistakes = new List<Tuple<string, int>> ();


				mistakesCopy.ForEach ((Tuple<string, int> fout) => {
					if (!stop)
						DoSingleExercise (fout.Item1, fout.Item2, out stop);
				});
			}

			if (!stop) {
				Console.WriteLine ();
				Console.WriteLine (Catalog.GetString ("No mistakes!"));
			}
		}
	}
}
