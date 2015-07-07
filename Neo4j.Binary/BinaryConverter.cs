using System;
using System.Collections.Generic;
using System.Linq;

public static class BinaryConverter
{
    public static byte[] IntToBinaryBytes(string number)
    {
        if (!IsHex(number))
            throw new ArgumentException("Value should be in hex form (0x)", nameof(number));

        var arrayOfArrays = number.Skip(2).Select(s => IntToBinaryBytes(s.GetNumericValue())).ToList();
        return arrayOfArrays.SelectMany(a => a).ToArray();
    }

    public static int GetNumericValue(this char c)
    {
        var def = (int) char.GetNumericValue(c);
        if (def != -1)
            return def;

        //Now we're in hex land.
        switch (c)
        {
            case 'a':
            case 'A':
                return 10;
            case 'b':
            case 'B':
                return 11;
            case 'c':
            case 'C':
                return 12;
            case 'd':
            case 'D':
                return 13;
            case 'e':
            case 'E':
                return 14;
            case 'f':
            case 'F':
                return 15;
            default:
                throw new ArgumentOutOfRangeException(nameof(c), c, "Value given isn't a valid Hex value.");
        }
    }

    public static byte[] IntToBinaryBytes(int number, int? padding = 4)
    {
        var binary = new List<byte>();
        while (number > 0)
        {
            var x = number & 1;
            binary.Insert(0, (byte) x);
            number = number >> 1;
        }

        if (padding.HasValue)
        {
            while (binary.Count < padding)
                binary.Insert(0, 0);
        }

        return binary.ToArray();
    }

    /// <summary>Checks if the <paramref name="value" /> represents a Hex value or not.</summary>
    /// <param name="value">The string to check hex-iness</param>
    /// <returns><c>true</c> if the <paramref name="value" /> represents a Hex value.</returns>
    /// <remarks>From: http://stackoverflow.com/questions/9192446/rewrite-ishexstring-method-with-regex </remarks>
    public static bool IsHex(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length < 3)
            return false;

        const byte stateZero = 0;
        const byte stateX = 1;
        const byte stateValue = 2;

        var state = stateZero;

        foreach (var c in value)
        {
            switch (c)
            {
                case '0':
                {
                    // Can be used in either Value or Zero.
                    switch (state)
                    {
                        case stateZero:
                            state = stateX;
                            break;
                        case stateX:
                            return false;
                        case stateValue:
                            break;
                    }
                }
                    break;
                case 'X':
                case 'x':
                {
                    // Only valid in X.
                    switch (state)
                    {
                        case stateZero:
                            return false;
                        case stateX:
                            state = stateValue;
                            break;
                        case stateValue:
                            return false;
                    }
                }
                    break;
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                {
                    // Only valid in Value.
                    switch (state)
                    {
                        case stateZero:
                            return false;
                        case stateX:
                            return false;
                        case stateValue:
                            break;
                    }
                }
                    break;
                default:
                    return false;
            }
        }

        return state == stateValue;
    }
}