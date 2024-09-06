using System.Linq;

public static class StringExtension
{
    public static string GetCamelCase(this string value)
    {
        string camelCase = value.First().ToString().ToLower();
        for (int i = 1; i < value.Length; i++)
        {
            if (value[i] == ' ')
            {
                camelCase += value[i + 1].ToString().ToUpper();
                ++i;
            }
            else
            {
                camelCase += value[i];
            }
        }
        return camelCase;
    }

    public static string GetPascalCase(this string value)
    {
        string PascalCase = value.First().ToString().ToUpper();
        for (int i = 1; i < value.Length; i++)
        {
            if (value[i] == ' ')
            {
                PascalCase += value[i + 1].ToString().ToUpper();
                ++i;
            }
            else
            {
                PascalCase += value[i];
            }
        }
        return PascalCase;
    }

    public static string SeparateAllCase(this string value)
    {
        string separateWord = value.First().ToString().ToUpper();
        for (int i = 1; i < value.Length; i++)
        {
            if (value[i].ToString() == value[i].ToString().ToUpper())
            {
                separateWord += $" {value[i].ToString().ToLower()}";
            }
            else
            {
                separateWord += value[i];
            }
        }
        return separateWord;
    }
}
