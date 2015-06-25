﻿using System;
using System.Collections.Generic;
using System.Linq;
using Liquid.NET.Constants;
using Liquid.NET.Symbols;
using Liquid.NET.Utils;

namespace Liquid.NET.Expressions
{
    public class NotEqualsExpression :ExpressionDescription
    {
        
        public override void Accept(IExpressionDescriptionVisitor expressionDescriptionVisitor)
        {
            expressionDescriptionVisitor.Visit(this);
        }

        //public IExpressionConstant Eval(SymbolTableStack symbolTableStack, IEnumerable<IExpressionConstant> expressions)
        public override LiquidExpressionResult Eval(SymbolTableStack symbolTableStack, IEnumerable<Option<IExpressionConstant>> expressions)
        {

            //IList<IExpressionConstant> exprList = expressions.ToList();
            IList<Option<IExpressionConstant>> exprList = expressions.ToList();

            //Console.WriteLine("EqualsExpression is Eval-ing expressions ");
            if (exprList.Count() != 2)
            {
                //return LiquidExpressionResult.Error("Equals is a binary expression but received " + exprList.Count() + ".");
                return LiquidExpressionResult.Error("Equals is a binary expression but received " + exprList.Count() + ".");
            }
//            if (exprList.Any(x => x.IsError))
//            {
//                return LiquidExpressionResult.Error(String.Join(";", exprList.Where(x => x.IsError)));
//            }
            
            //var successList = exprList.Select(x => x.SuccessResult).ToList();
            if (exprList.All(x => !x.HasValue)) // both equal
            {
                return LiquidExpressionResult.Success(new BooleanValue(true));
            }
            if (exprList.Any(x => !x.HasValue))
            {
                return LiquidExpressionResult.Success(new BooleanValue(false));
            }
           
//            if (exprList[0].IsUndefined)
//            {
//                return LiquidExpressionResult.Error(exprList[0].ErrorMessage);
//            }
//            if (exprList[1].IsUndefined)
//            {
//                return LiquidExpressionResult.Error(exprList[1].ErrorMessage);
//            }
            //if (exprList.)
            if (exprList[0].GetType() == exprList[1].GetType())
            {
                return LiquidExpressionResult.Success(new BooleanValue(! exprList[0].Value.Equals(exprList[1].Value)));

            }

            return LiquidExpressionResult.Error("\"Not Equals\" implementation can't cast yet"); 
        }


    }
}
