﻿using System;
using System.Collections.Generic;

namespace Liquid.NET.Constants
{
    public static class ArrayIndexer
    {
        public static IExpressionConstant ValueAt(IList<IExpressionConstant> array, int key)
        {
            if (key >= array.Count || key < -array.Count)
            {
                //return ConstantFactory.CreateNilValueOfType<StringValue>("index "+key+" is outside the bounds of the array.");
                // TODO: this should return an OPTION
                return new NilValue();
            }
            key = WrapMod(key, array.Count);
            
            Console.WriteLine("KEY IS "+ key);
            return array[key];

        }

        public static int WrapMod(int index, int length)
        {
            return (index % length + length) % length;
        }

    }
}
