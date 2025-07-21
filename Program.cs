Console.WriteLine("Hello, World!");

int a = 19;
int b = a & 7;

var word = new nes_csharp.primitives.Word();
word.Value = 5;
word.State = 2;
word.Switch1 = 0;
word.Switch2 = 1;

Console.WriteLine($"{word.InternalValue}, {word.State}, {word.Value}, {word.Switch1}, {word.Switch2} {1 << 5}"); // Outputs the internal value in hexadecimal format

