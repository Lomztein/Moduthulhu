using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lomztein.ModularDiscordBot.Modules.Misc.Brainfuck
{
    public class BrainfuckIntepreter {

        public BrainfuckIntepreter (Func<Task<byte>> getInput) {
            _getInput = getInput;
        }

        private string _program;

        private int _counter;
        private int _pointer;

        private readonly byte[ ] _memory = new byte [ 30000 ];
        private readonly Func<Task<byte>> _getInput;

        public async Task<string> Interpret(string program) {

            _program = program;
            string printout = string.Empty;

            try {
                while (_counter < _program.Length) {
                    switch (_program [_counter]) {
                        case '>':
                            MovePointer(1);
                            break;

                        case '<':
                            MovePointer(-1);
                            break;

                        case '+':
                            ChangeMemoryAtPointer(1);
                            break;

                        case '-':
                            ChangeMemoryAtPointer(-1);
                            break;

                        case '[':
                        case ']':
                            _counter = LoopCounter(_counter);
                            break;

                        case '.':
                            Printout(ref printout);
                            break;

                        case ',':
                            SetMemoryAtPointer(await _getInput());
                            break;

                        default:
                            break;
                    }

                    _counter++;
                }
            }catch (Exception e) {
                return e.Message;
            }

            return printout;
        }

        // The > and < chars, changes memory pointer.
        private void MovePointer(int movement) {
            _pointer += movement;
        }

        // The + and - chars, changes memory at pointer.
        private void ChangeMemoryAtPointer(int movement) {
            int curAsInt = _memory [ _pointer ];
            _memory [ _pointer ] = (byte)(curAsInt + movement);
        }

        // The , char, not sure how this is used.
        private void SetMemoryAtPointer (byte value) {
            _memory [ _pointer ] = value;
        }

        // The . char, prints out bytes.
        private void Printout(ref string printout) {
            printout += (char)_memory[_pointer];
        }

        // The [ and ] chars, functions as loops/pathers.
        private int LoopCounter(int position) {

            char starting = _program [ position ];
            int movement = BracketToMovement (_program [ position]);
            int balance = movement;

            int fit = position;

            while (true) {
                position += movement;
                balance += BracketToMovement (_program [ position]);

                if (IsBracket (_program[position])) {
                    bool isOpposite = _program [ position ] == OppositeBracket (starting);

                    // Move forward if zero.
                    if (starting == '[' && isOpposite && balance == 0) {
                        if (_memory [ _pointer ] == 0)
                        {
                            fit = position;
                        }
                        break;
                    }

                    // Move backwards if non-zero.
                    if (starting == ']' && isOpposite && balance == 0) {
                        if (_memory [ _pointer ] != 0)
                        {
                            fit = position;
                        }
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
