using RedRust;
using System.Collections.ObjectModel;

Class printer = new(null, new(new Dictionary<string, Value>(){
        { "a", Values.i32 },
        { "add", new Function(new Argument("n",new RedRust.Nullable(Values.f32)))} }));

Function add = (Function)printer.GetValue("add");

bool ok = add.CanExecute(Values.i32);
bool ok2 = add.CanExecute(Values.f32);

Console.WriteLine("end");