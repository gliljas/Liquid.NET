﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Liquid.NET.Constants;
using Liquid.NET.Symbols;
using Liquid.NET.Utils;

using ExpressionResult = Liquid.NET.Utils.Either<Liquid.NET.LiquidError, Liquid.NET.Utils.Option<Liquid.NET.Constants.IExpressionConstant>>;

namespace Liquid.NET.Expressions
{
    public class IsNullExpression : ExpressionDescription
    {
        public override void Accept(IExpressionDescriptionVisitor expressionDescriptionVisitor)
        {
            expressionDescriptionVisitor.Visit(this);
        }

        public override LiquidExpressionResult Eval(SymbolTableStack symbolTableStack, IEnumerable<Option<IExpressionConstant>> expressions)
        {
            var list = expressions.ToList();
            if (list.Count() != 1)
            {
                throw new Exception("Expected one variable to compare with \"null\""); // this will be obsolete when the lexer/parser is split
            }
            if (!list[0].HasValue)
            {
                return LiquidExpressionResult.Success(new BooleanValue(false));
            }
            return LiquidExpressionResult.Success(new BooleanValue(list[0].Value == null || list[0].Value.GetType() == typeof (NilValue)));

        }
    }
}
