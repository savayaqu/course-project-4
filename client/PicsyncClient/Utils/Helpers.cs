using System.Security.Cryptography;
using static System.Math;

namespace PicsyncClient.Utils;

public static class Helpers
{
    public static string HashUTF8toMD5HEX(string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static readonly string[] BytesSuffixes = ["Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЭБ", "ЗБ", "ИБ"];
    
    public static string BytesToHuman(ulong bytes)
    {
        int suffixIndex = 0;
        double count = bytes;

        while (count >= 1000 && suffixIndex < BytesSuffixes.Length - 1)
        {
            count /= 1024;
            suffixIndex++;
        }

        string format = (
            count >= 100 || 
            suffixIndex == 0
        ) 
        ? "0" 
        : count >= 10 
            ? "0.0" 
            : "0.00";

        return count.ToString(format) + " " + BytesSuffixes[suffixIndex];
    }
    
    public static readonly string[] CountSuffixes = ["", "K", "M", "B", "T", "Q", "Qt", "Sx"];
    
    public static string CountToHuman(ulong number)
    {
        int pow = (int)Floor((number > 0 ? Log(number) : 0) / Log(1000));
        pow = Min(pow, CountSuffixes.Length - 1);

        double value = number / Pow(1000, pow);

        string formatted = value % 1 == 0 
            ? value.ToString("F0")
            : value.ToString("F" + 
                             (3 - Floor(Log10(value) + 1)).ToString()
            );

        return formatted + CountSuffixes[pow];
    }

    public static bool SafeGetDouble(object? input, out double result)
    {
        result = 0;
        if (input == null)
            return false;

        if (input is double)
        {
            result = (double)input;
            return true;
        }

        if (input is string str)
        {
            if (double.TryParse(str, out double parsedResult))
            {
                result = parsedResult;
                return true;
            }
            else return false;
        }

        if (input is IConvertible convertible)
        {
            try
            {
                result = convertible.ToDouble(null);
                return true;
            }
            catch { return false; }
        }
        return false;
    }
}