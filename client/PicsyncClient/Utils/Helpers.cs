using System.Diagnostics;
using System.Security.Cryptography;

namespace PicsyncClient.Utils;

public static class Helpers
{
    public static string HashUTF8toMD5HEX(string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static string BytesToHuman(ulong bytes)
    {
        string[] suffixes = ["Б", "КБ", "МБ", "ГБ", "ТБ", "ПБ", "ЭБ", "ЗБ", "ИБ"];
        int suffixIndex = 0;
        double count = bytes;

        while (count >= 1000 && suffixIndex < suffixes.Length - 1)
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

        return count.ToString(format) + " " + suffixes[suffixIndex];
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
            else
            {
                return false;
            }
        }

        if (input is IConvertible convertible)
        {
            try
            {
                result = convertible.ToDouble(null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return false;
    }
}