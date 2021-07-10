using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace _4d6 {
    class Program {
        // account for floating points, incorrect variables, different alphabetical characters, etc
        static void Main(string[] args) {
            Console.WriteLine("Welcome to the skill-level formula calculator! Enter q at any time to exit.");

            Console.WriteLine("Enter your dice notation (ex. 4d6, 3d12), or an initial roll value (4, 6, 24): ");
            string input = Console.ReadLine();
            if (input == "q") return; // exit program if q

            if (ScoreFormula.IsCustomRollInt(input)) // if a single initial roll integer, set the initial roll
                ScoreFormula.SetInitialRoll(input);
            else if (ScoreFormula.IsDiceNotation(input)) // else if dice notation, parse the dice text
                ScoreFormula.ParseDiceText(input);
            else { // otherwise if the input is not an integer nor dice notation,
                while (!ScoreFormula.IsDiceNotation(input) && !ScoreFormula.IsCustomRollInt(input)) {
                    Console.WriteLine("Please enter either an initial roll value integer or dice notation: ");
                    input = Console.ReadLine();
                    if (input == "q") return; // exit program if q
                }

                // now we can set the inital roll
                if (ScoreFormula.IsCustomRollInt(input)) // if a single initial roll integer, set the initial roll
                    ScoreFormula.SetInitialRoll(input);
                else if (ScoreFormula.IsDiceNotation(input)) // else if dice notation, parse the dice text
                    ScoreFormula.ParseDiceText(input);
            }

            Console.WriteLine("\nEnter a blank line to see the entire formula and skill level once you have finished.");

            // Continually loop asking for operation steps to take until user finishes with a blank line
            while (true) {
                Console.WriteLine("Enter an operation (ex. subtract 5, multiply 9, divide 3, etc.): ");
                string op = Console.ReadLine();
                if (op == "q") return; // exit program if q
                if (op == "" || op == " ") break; // continue onward to PrintFormula if blank
                ScoreFormula.ParseOperation(op);
            }

            // Print the steps and result to the user
            ScoreFormula.PrintFormula();

            // End the program
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }

    class ScoreFormula {
        private static Random rand = new Random();
        // "r6", "-52.2". keeps track of all steps of the formula for printing. Ex. Inital roll of 6, subtract 52.2
        private static List<string> formula = new List<string>();

        // to implement:
        // more standardization for parameters, return values, and print statements (easier to find where things are)
        // better error handling for regex
        // more regex (in PrintFormula() or ParseOperation()
        // more comments/docstrings
        // a cool repo

        public static void ParseOperation(string operation) {
            int endOfKeywordIndex = 0;
            endOfKeywordIndex = operation.IndexOf(" ") + 1;

            if (endOfKeywordIndex != 0) { // != null
                string amountToApply = operation.Substring(endOfKeywordIndex); // everything after "subtract ". i.e 45, 2, etc.
                var isInteger = int.TryParse(amountToApply, out _);
                var isDouble = double.TryParse(amountToApply, out _);
                if (isInteger || isDouble) {
                    if (operation.StartsWith("subtract"))
                        formula.Add("-" + amountToApply);
                    else if (operation.StartsWith("add"))
                        formula.Add("+" + amountToApply);
                    else if (operation.StartsWith("divide"))
                        formula.Add("/" + amountToApply);
                    else if (operation.StartsWith("multiply"))
                        formula.Add("*" + amountToApply);
                    else if (operation.StartsWith("modulus"))
                        formula.Add("%" + amountToApply);
                }
                else // amount to apply after operator is not a number? not an operation
                    Console.WriteLine($"\nError: Invalid operator format entered: \"{operation}\".");
            }
            else // no spaces? not an operation
                Console.WriteLine($"\nError: Invalid operator format entered: \"{operation}\".");
        }
        /// <summary>
        /// Detects whether or not the input is in dice notation, returning a bool if it is or not.
        /// </summary>
        /// <param name="input">The user's input</param>
        /// <returns>A bool representing whether or not the string is in dice notation</returns>
        public static bool IsDiceNotation(string input) {
            var results = Regex.Matches(input, @"\d+d\d+"); // find all occurances of dice notation
            var nums = Regex.Matches(results.ToString(), @"\d+"); // all numbers in the dice notation

            foreach (Match match in nums) {
                Console.WriteLine("DN: " + match.ToString());
                if (IsNumberTooBig(match.ToString())) {
                    return false; // TOO BIG. cannot use
                }
            }

            if (results.Count > 1 || results.Count == 0)
                return false;
            else
                return true;
        }
        /// <summary>
        /// Detects whether or not the input is an integer of any length, and returns if it is true or not.
        /// A string instead of an integer is sent, as it takes less memory to use a string, and it would be added to the ledger list anyway, so conversions are unneeded.
        /// </summary>
        /// <param name="input">A string representing the user's input</param>
        /// <returns>A bool indicating if the input is an integer or not</returns>
        public static bool IsCustomRollInt(string input) {
            var results = Regex.Matches(input, @"[0-9]+"); // find all occurances of an integer

            if (IsNumberTooBig(input))
                return false;
            else if (results.Count > 1 || results.Count == 0)
                return false;
            else
                return true;
        }
        public static bool IsNumberTooBig(string num) {
            try {
                bool isInfinite = double.IsInfinity(Convert.ToDouble(num));
                if (isInfinite) // TOO BIG!!
                    return true;
                else // not too big
                    return false;
            }
            catch (Exception e) {
                return true; // TOO BIG!!!!
            }
        }
        /// <summary>
        /// Parses the passed dice notation, and sets the intial roll as an additive of `rolls` rolls on a `dieSides` sided die
        /// </summary>
        /// <param name="diceNotation">The user's inputted dice notation </param>
        public static void ParseDiceText(string diceNotation) {
            var validDiceNotation = Regex.Match(diceNotation, @"[0-9]+[d]{1}[0-9]+"); // Match the first occurance of a valid dice notation.
                // why one might ask? sometimes the isDiceNotation regex fails, and a fully fledged error handler for imroper regex is kinda poop if the user
                // is intentionally trying to mess up the program. also less memory usage if you just take a piece of valid input from the garble they intend to send
            char[] charArray = validDiceNotation.ToString().ToCharArray();
            string rolls = "";
            string dieSides = "";
            
            // regex might improve readability and memory usage
            // Find the number of rolls and sides on the die from the user's input
            bool rollsSidesFlag = false; // false = rolls, true = sides
            foreach (char character in charArray) {
                var isNumericInt = int.TryParse(character.ToString(), out _);
                if (isNumericInt) {
                    if (!rollsSidesFlag)
                        rolls += character.ToString();
                    else if (rollsSidesFlag)
                        dieSides += character.ToString();
                } 
                else 
                    rollsSidesFlag = true; // we have hit 'd' of string "4d6" for example. now viewing sides
                
            }

            // if no rolls specified (like in string "d5"), then assume the user wishes for 1 roll to occur (dice notation)
            if (rolls == "")
                rolls = "1";
            // if no die sides specified (like in string "4d"), then assume the user wishes for a 6 sided die (dice notation)
            if (dieSides == "")
                dieSides = "6";

            // Now that we have the rolls&sides, make the inital roll(s)
            RandomInitialRoll(Convert.ToInt32(rolls), Convert.ToInt32(dieSides));
        }
        /// <summary>
        /// Uses rolls and sides to generate the inital roll value from dice notation. The additive result from each roll is set as the initial roll in the ledger.
        /// </summary>
        /// <param name="rolls">The number of times the die will be rolled. After each roll, the side landed on will be added to the initial roll.</param>
        /// <param name="sides">The number of sides the die contains.</param>
        private static void RandomInitialRoll(int rolls, int sides) {
            int initialRoll = 0;
            for (int i = 0; i < rolls; i++) {
                initialRoll += rand.Next(1, sides + 1); // maximum value is exclusive
            }

            formula.Add("r" + Convert.ToString(initialRoll)); // add to formula ledger
        }
        /// <summary>
        /// Set the inital roll value as a custom value instead of a random generation from dice notation
        /// </summary>
        /// <param name="rollValue">An integer as a string. A string value because it takes less memory to use a string than convert,
        /// and it's being added to the ledger as a string anyway.</param>
        public static void SetInitialRoll(string rollValue) {
            formula.Add("r" + rollValue); // manually set a roll value instead of it being random
        }
        /// <summary>
        /// Prints the skill formula steps within the var formula in a readable format, as well as the final result of those operations.
        /// </summary>
        public static void PrintFormula() {
            double result = 0;

            for (int i = 0; i < formula.Count; i++) {
                string formulaStep = formula[i]; // e.g step 1,2,3, etc.
                string message = $"Step #{i+1}: ";
                string amountToApply = formulaStep.Substring(1); // the amount to apply after the operation code. for instance "1441" in string "-1441"
                double numericalAmountToApply = Convert.ToDouble(amountToApply); // double, as may be floating point

                if (formulaStep.StartsWith("r")) { // initial roll
                    message += "an initial roll of " + amountToApply;
                    result = numericalAmountToApply;
                }
                if (formulaStep.StartsWith("-")) {
                    message += "subtract " + amountToApply;
                    result -= numericalAmountToApply;
                }
                if (formulaStep.StartsWith("+")) { 
                    message += "add " + amountToApply;
                    result += numericalAmountToApply;
                }
                if (formulaStep.StartsWith("/")) { 
                    message += "divide " + amountToApply;
                    result /= numericalAmountToApply;
                }
                if (formulaStep.StartsWith("*")) {
                    message += "multiply " + amountToApply;
                    result *= numericalAmountToApply;
                }
                if (formulaStep.StartsWith("%")) {
                    message += "modulus " + amountToApply;
                    result %= numericalAmountToApply;
                }

                Console.WriteLine(message); // Print the step information
            }
            
            Console.WriteLine("Final skill-level result: " + result + "!"); // Finally, print the calculated result
        }
    }
}
