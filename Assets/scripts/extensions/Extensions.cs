using UnityEngine;
using System.Collections;
using System.Text;
using System;

public static class Extensions
{
    public static void Clear (this StringBuilder instance)
    {
        int cachedLength = instance.Length;

        for(int i = 0; i < cachedLength; ++i)
            instance.Append(" ");

        instance.Length = 0;
    }
}
