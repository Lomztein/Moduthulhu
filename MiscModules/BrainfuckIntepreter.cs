using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Brainfuck
{
    public class BrainfuckIntepreter {

        public BrainfuckIntepreter (Func<Task<byte>> _getInput) {
            getInput = _getInput;
        }

        public string program;
        public string result;

        private int counter = 0;
        private int pointer = 0;

        private Byte [ ] memory = new byte [ 30000 ];
        public Func<Task<byte>> getInput;

        public async Task<string> Interpret(string program) {

            string printout = string.Empty;

            try {
                for (counter = 0; counter < program.Length; counter++) {
                    switch (program [ counter ]) {
                        case '>':
                            MovePointer (1);
                            break;

                        case '<':
                            MovePointer (-1);
                            break;

                        case '+':
                            ChangeMemoryAtPointer (1);
                            break;

                        case '-':
                            ChangeMemoryAtPointer (-1);
                            break;

                        case '[':
                        case ']':
                            counter = LoopCounter (counter);
                            break;

                        case '.':
                            Printout (ref printout);
                            break;

                        case ',':
                            SetMemoryAtPointer (await getInput ());
                            break;
                    }
                }
            }catch (Exception e) {
                return e.Message;
            }

            result = printout;
            return printout;
        }

        // The > and < chars, changes memory pointer.
        private void MovePointer(int movement) {
            pointer += movement;
        }

        // The + and - chars, changes memory at pointer.
        private void ChangeMemoryAtPointer(int movement) {
            int curAsInt = memory [ pointer ];
            memory [ pointer ] = (byte)(curAsInt + movement);
        }

        // The , char, not sure how this is used.
        private void SetMemoryAtPointer (byte value) {
            memory [ pointer ] = value;
        }

        // The . char, prints out bytes.
        private void Printout(ref string printout) {
            printout += (char)memory[pointer];
        }

        // The [ and ] chars, functions as loops/pathers.
        private int LoopCounter(int position) {

            char starting = program [ position ];
            int movement = BracketToMovement (program [ position]);
            int balance = movement;

            int fit = position;

            while (true) {
                position += movement;
                balance += BracketToMovement (program [ position]);

                if (IsBracket (program[position])) {
                    bool isOpposite = program [ position ] == OppositeBracket (starting);

                    // Move forward if zero.
                    if (starting == '[' && isOpposite && balance == 0) {
                        if (memory [ pointer ] == 0)
                            fit = position;
                        break;
                    }

                    // Move backwards if non-zero.
                    if (starting == ']' && isOpposite && balance == 0) {
                        if (memory [ pointer ] != 0)
                            fit = position;
                        break;
                    }
                }
            }

            return fit;

            int BracketToMovement(char bracket) => bracket == '[' ? 1 : bracket == ']' ? -1 : 0;
            bool IsBracket(char bracket) => bracket == '[' || bracket == ']';
            int OppositeBracket (char bracket) => bracket == '[' ? ']' : '[';

        }
    }
}
