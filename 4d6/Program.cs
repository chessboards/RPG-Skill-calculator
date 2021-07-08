using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4d6 {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to the skill-level formula calculator! Enter q at any time to exit.");

            Console.WriteLine("Enter your dice and rolls (ex. 4d6, 3d12), or an initial roll value (4, 6, 24): ");
            string input = Console.ReadLine();
            if (input == "q") return; // exit program if q

            // if input a dice/roll pair, parse accordingly
            if (input.Contains("d"))
                ScoreFormula.ParseDiceText(input);
            // otherwise set a custom intial roll value
            else
                ScoreFormula.SetInitialRoll(input);

            Console.WriteLine("\nEnter a blank line to see the entire formula and skill level once you have finished.");

            // Continually loop asking for operation steps to take until user finishes with a blank line
            while (true) {
                Console.WriteLine("Enter an operation (ex. subtract 5, multiply 9, divide 3, etc.): ");
                string op = Console.ReadLine();
                ScoreFormula.ParseOperation(op);
                if (op == "q") return; // exit program if q
                if (op == "" || op == " ") break; // continue onward to PrintFormula if blank
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
        public static List<string> formula = new List<string>();

        // to implement: floating point nums, better error handling, better comments/docstrings, working program, push to github
        // both require int tests
        private static void RandomInitialRoll(int rolls, int sides) {
            int initialRoll = 0;
            for (int i = 0; i < rolls; i++) {
                initialRoll += rand.Next(1, sides + 1); // maximum value is exclusive
            }

            formula.Add( "r" + Convert.ToString(initialRoll) ); // add to formula ledger
        }
        public static void SetInitialRoll(string rollValue) {
            formula.Add( "r" + rollValue ); // manually set a roll value instead of it being random
        }
        ///////////////////////////////////////////////////

        public static void ParseOperation(string operation) {
            int endOfKeywordIndex = 0;
            endOfKeywordIndex = operation.IndexOf(" ") + 1;

            if (endOfKeywordIndex != 0) { // != null
                string amountToApply = operation.Substring(endOfKeywordIndex); // everything after "subtract ". i.e 45, 2, etc.
                try {
                    int numberAmount = Convert.ToInt32(amountToApply); // test if actually a number. Maybe implement a better test

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
                catch (Exception e) {
                    Console.WriteLine($"Error: Invalid operator format entered: \"{operation}\".");
                    throw e;
                }
            }
        }
        public static void ParseDiceText(string dice) {
            char[] charArray = dice.ToCharArray();
            string rolls = "";
            string dieSides = "";

            // find the rolls&sides variables from the user's input
            bool rollsSidesFlag = false; // false = rolls, sides = true
            foreach (char character in charArray) {
                var isNumeric = int.TryParse(character.ToString(), out _);
                if (isNumeric) {
                    if (!rollsSidesFlag)
                        rolls += character.ToString();
                    else if (rollsSidesFlag)
                        dieSides += character.ToString();
                } 
                else 
                    rollsSidesFlag = true; // we have hit 'd' of string "4d6" for example. numbers to the right of d are the amount of sides of the die
                
            }

            // now that we have the rolls&sides, make the inital roll(s)
            RandomInitialRoll(Convert.ToInt32(rolls), Convert.ToInt32(dieSides));
        }
        public static void PrintFormula() {
            double result = 0;

            for (int i = 0; i < formula.Count; i++) {
                string formulaStep = formula[i];
                string message = $"Step #{i+1}: ";
                double amountToApply = Convert.ToDouble(formulaStep.Substring(1)); // number after arithmetic code

                if (formulaStep.StartsWith("r")) { // initial roll
                    // substring retrieves all numbers after - in string "-131312314" for instance
                    message += "an initial roll of " + formulaStep.Substring(1);
                    result = amountToApply;
                }
                if (formulaStep.StartsWith("-")) {
                    message += "subtract " + formulaStep.Substring(1);
                    result -= amountToApply;
                }
                if (formulaStep.StartsWith("+")) { 
                    message += "add " + formulaStep.Substring(1);
                    result += amountToApply;
                }
                if (formulaStep.StartsWith("/")) { 
                    message += "divide " + formulaStep.Substring(1);
                    result /= amountToApply;
                }
                if (formulaStep.StartsWith("*")) {
                    message += "multiply " + formulaStep.Substring(1);
                    result *= amountToApply;
                }
                if (formulaStep.StartsWith("%")) {
                    message += "modulus " + formulaStep.Substring(1);
                    result %= amountToApply;
                }

                Console.WriteLine(message); // print step
            }
            
            Console.WriteLine("Final skill-level result: " + result + "!"); // Finally, print the calculated result
        }


    }
}
