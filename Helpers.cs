using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
namespace HungerGames.Helpers;

public static class Extensions
{
    public static IntEnumerator GetEnumerator(this Range range) => new (range);

    public static T SelectLower<T>(T num, T num2)
        where T : INumber<T> => num < num2 ? num : num2;

    public static T Choice<T>(this Random random, IList<T> list) => list[random.Next(list.Count)];
    public static T ChoiceNotVal<T>(this Random random, IList<T> list, T exclude)
    {
        if (list.Count == 1)
            throw new ArgumentException("List must have more than one element");
        T choice;
        do
        {
            choice = random.Choice(list);
        } while (choice?.Equals(exclude) ?? true);
        return choice;
    }
    public static string ToDisplayString<T>(this IList<T> list)
    {
        var str = new StringBuilder();
        str.Append("{ ");
        str.Append(string.Join(", ", list));
        str.Append(" }");
        return str.ToString();
    }
    public static void PrintList<T>(this IList<T> list)
    {
        var str = list.ToDisplayString();   
        Console.WriteLine(str);
    }
}

public ref struct IntEnumerator
{
    private int _current;
    private readonly int _end;
    public IntEnumerator(Range range)
    {
        if (range.End.IsFromEnd)
        {
            throw new ArgumentException("End cannot be from end");
        }
        _current = range.Start.Value - 1;
        _end = range.End.Value;
    }
    public int Current => _current;
    public bool MoveNext() => ++_current <= _end;
}