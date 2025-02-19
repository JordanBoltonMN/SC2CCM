﻿using System.Collections.Generic;
using System.IO;

public static class StringExtensions
{
    public static IEnumerable<string> ReadLines(this string text)
    {
        string line;

        using (StringReader reader = new StringReader(text))
        {
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
