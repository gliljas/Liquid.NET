﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Liquid.NET.Constants;
using Liquid.NET.Expressions;
using Liquid.NET.Rendering;
using Liquid.NET.Symbols;
using Liquid.NET.Tags;
using Liquid.NET.Tags.Custom;

namespace Liquid.NET
{

    /// <summary>
    /// Render the AST nodes as a String
    /// </summary>
    public class RenderingVisitor : IASTVisitor
    {
        private String _result = "";
        
        private readonly LiquidASTRenderer _astRenderer;
        private readonly SymbolTableStack _symbolTableStack;
        private readonly ConcurrentDictionary<String, int> _counters = new ConcurrentDictionary<string, int>();

        public readonly IList<LiquidError> Errors = new List<LiquidError>();

        public bool HasErrors { get { return Errors.Any();  } }

        public RenderingVisitor(LiquidASTRenderer astRenderer, SymbolTableStack symbolTableStack)
        {
            _astRenderer = astRenderer;
            _symbolTableStack = symbolTableStack;
        }

        public String Text
        {
            get { return _result; }
        }

        public void Visit(RawBlockTag rawBlockTag)
        {
            _result += rawBlockTag.Value;
        }


        public void Visit(CommentBlockTag commentBlockTag)
        {
            // do nothing
        }

        public void Visit(CustomTag customTag)
        {
            Console.WriteLine("Looking up Custom Tag " + customTag.TagName);
            var tagType = _symbolTableStack.LookupCustomTagRendererType(customTag.TagName);
            if (tagType != null)
            {
                _result += RenderCustomTag(customTag, tagType);
                return;
            }

            Console.WriteLine("Looking up Macro "+ customTag.TagName);
            var macroDescription = _symbolTableStack.LookupMacro(customTag.TagName);
            if (macroDescription != null)
            {
                Console.WriteLine("...");
                IEnumerable<IExpressionConstant> args =
                    customTag.LiquidExpressionTrees.Select(x => LiquidExpressionEvaluator.Eval(x, _symbolTableStack));

                _result += RenderMacro(macroDescription, args);
                return;
            }
            //_result += " ERROR: There is no macro or tag named "+  customTag.TagName+ " ";
            AddError("Liquid syntax error: Unknown tag '" + customTag.TagName + "'", customTag);
            //_result += "Liquid syntax error: Unknown tag '"+customTag.TagName+"'";
        }

        private void AddError(String message, IASTNode node)
        { 
            // TODO: pass the tag info in...
            Errors.Add(new LiquidError{Message = message});
        }

        private string RenderMacro(MacroBlockTag macroBlockTag, IEnumerable<IExpressionConstant> args)
        {
            var macroRenderer = new MacroRenderer();
            //var hiddenRenderer = new RenderingVisitor(_a)
            IList<LiquidError> macroErrors = new List<LiquidError>();
            var macro = ValueCaster.RenderAsString(macroRenderer.Render(macroBlockTag, _symbolTableStack, args.ToList(), macroErrors));
            foreach (var error in macroErrors)
            {
                Errors.Add(error);
            }
            return macro;
        }

        private string RenderCustomTag(CustomTag customTag, Type tagType)
        {
            var tagRenderer = CustomTagRendererFactory.Create(tagType);
            IEnumerable<IExpressionConstant> args =
                customTag.LiquidExpressionTrees.Select(x => LiquidExpressionEvaluator.Eval(x, _symbolTableStack));
            return tagRenderer.Render(_symbolTableStack, args.ToList()).StringVal;
        }

        public void Visit(CustomBlockTag customBlockTag)
        {
            var tagType = _symbolTableStack.LookupCustomBlockTagRendererType(customBlockTag.TagName);
            var tagRenderer = CustomBlockTagRendererFactory.Create(tagType);
            if (tagRenderer == null)
            {
                AddError("Liquid syntax error: Unknown tag '" + customBlockTag.TagName + "'", customBlockTag);
                //throw new Exception("Unregistered Tag: " + customBlockTag.TagName);
                return;
            }
            IEnumerable<IExpressionConstant> args =
                customBlockTag.LiquidExpressionTrees.Select(x => LiquidExpressionEvaluator.Eval(x, _symbolTableStack));
            _result += tagRenderer.Render(_symbolTableStack, customBlockTag.LiquidBlock, args.ToList()).StringVal;

            

        }

        public void Visit(CycleTag cycleTag)
        {
            
            _result += GetNextCycleText(cycleTag);
        }


        public void Visit(AssignTag assignTag)
        {
            var result = LiquidExpressionEvaluator.Eval(assignTag.LiquidExpressionTree, _symbolTableStack);
            _symbolTableStack.DefineGlobal(assignTag.VarName, result);
        }

        public void Visit(CaptureBlockTag captureBlockTag)
        {
            var hiddenVisitor = new RenderingVisitor(_astRenderer, _symbolTableStack);
            _astRenderer.StartVisiting(hiddenVisitor, captureBlockTag.RootContentNode);            
            _symbolTableStack.DefineGlobal(captureBlockTag.VarName, new StringValue(hiddenVisitor.Text) );
            foreach (var error in hiddenVisitor.Errors)
            {
                Errors.Add(error);
            }
        }

//        public void Visit(DecrementTag decrementTag)
//        {
//            var key = decrementTag.VarName;
//            AlterNumericvalue(key, -1, n => new NumericValue(n.IntValue - 1));
//            _result += ValueCaster.RenderAsString(_symbolTableStack.Reference(key));
//        }
        public void Visit(DecrementTag decrementTag)
        {
            int currentIndex;
            var key = decrementTag.VarName;

            while (true)
            {
                currentIndex = _counters.GetOrAdd(key, -1);
                var newindex = (currentIndex - 1);
                if (_counters.TryUpdate(key, newindex, currentIndex))
                {
                    break;
                }
            }

            _result += currentIndex;
        }
//        public void Visit(IncrementTag decrementTag)
//        {
//            var key = decrementTag.VarName;
//            AlterNumericvalue(key, 0, n => new NumericValue(n.IntValue + 1));
//            _result += ValueCaster.RenderAsString(_symbolTableStack.Reference(key));
//        }
        public void Visit(IncrementTag incrementTag)
        {
            int currentIndex;
            var key = incrementTag.VarName;

            while (true)
            {
                currentIndex = _counters.GetOrAdd(key, 0);
                var newindex = (currentIndex + 1);                
                if (_counters.TryUpdate(key, newindex, currentIndex))
                {
                    break;
                }
            }

            _result += currentIndex;
        }

        public void Visit(IncludeTag includeTag)
        {
            throw new NotImplementedException();
        }



        private void AlterNumericvalue(string key, int defaultValue, Func<NumericValue, NumericValue> newValueFunc)
        {
            _symbolTableStack.FindVariable(key,
                (st, foundExpression) =>
                {
                    var numref = foundExpression as NumericValue;
                    st.DefineVariable(key,
                        numref != null ? newValueFunc(numref) : new NumericValue(defaultValue));
                },
                () => _symbolTableStack.Define(key, new NumericValue(defaultValue)));
        }

        /// <summary>
        /// Side effect: state is managed in the _counters dictionary.
        /// </summary>
        /// <param name="cycleTag"></param>
        /// <returns></returns>
        private String GetNextCycleText(CycleTag cycleTag)
        {
            int currentIndex;
            var groupName = cycleTag.GroupNameExpressionTree==null ?
                null :
                LiquidExpressionEvaluator.Eval(cycleTag.GroupNameExpressionTree, _symbolTableStack);
            var groupNameAsString = groupName== null ? "" : ValueCaster.RenderAsString(groupName);
            Console.WriteLine("Evaluating " + groupName);
            //var key = "cycle_" + groupNameAsString + "_" + String.Join("|", cycleTag.CycleList.Select(x => x.Value.ToString()));
            var key = "cycle_" + groupNameAsString + "_" + String.Join("|", cycleTag.CycleList.Select(x => x.Data.Expression.ToString()));
            
            while (true)
            {                
                currentIndex = _counters.GetOrAdd(key, 0);
                var newindex = (currentIndex + 1) % cycleTag.Length;

                // fails if updated concurrently by someone else.
                if (_counters.TryUpdate(key, newindex, currentIndex))
                {
                    break;
                }
            }

            return ValueCaster.RenderAsString(LiquidExpressionEvaluator.Eval(cycleTag.ElementAt(currentIndex), _symbolTableStack));
            //return cycleTag.ElementAt(currentIndex).Value.ToString();

        }

        public void Visit(ForBlockTag forBlockTag)
        {
            new ForRenderer(this, _astRenderer).Render(forBlockTag, _symbolTableStack);
        }

        public void Visit(IfThenElseBlockTag ifThenElseBlockTag)
        {

            // find the first place where the expression tree evaluates to true (i.e. which of the if/elsif/else clauses)
            var match = ifThenElseBlockTag.IfElseClauses.FirstOrDefault(
                                expr => LiquidExpressionEvaluator.Eval(expr.LiquidExpressionTree, _symbolTableStack).IsTrue);
            if (match != null)
            {
                _astRenderer.StartVisiting(this, match.LiquidBlock); // then render the contents
            }
        }

        public void Visit(CaseWhenElseBlockTag caseWhenElseBlockTag)
        {
            var valueToMatch = LiquidExpressionEvaluator.Eval(caseWhenElseBlockTag.LiquidExpressionTree, _symbolTableStack);
            //Console.WriteLine("Value to Match: "+valueToMatch);

            var match =
                caseWhenElseBlockTag.WhenClauses.FirstOrDefault(
                    expr =>
                        // Take the valueToMatch "Case" expression result value
                        // and check if it's equal to the expr.GroupNameExpressionTree expression.
                        // THe "EasyValueComparer" is supposed to match stuff fairly liberally,
                        // though it doesn't cast values---TODO: probably it should.
                        new EasyValueComparer().Equals(valueToMatch,
                            LiquidExpressionEvaluator.Eval(expr.LiquidExpressionTree, _symbolTableStack)));

            if (match != null)
            {
                _astRenderer.StartVisiting(this, match.LiquidBlock); // then eval + render the HTML
            }
            else if (caseWhenElseBlockTag.HasElseClause)
            {
                _astRenderer.StartVisiting(this, caseWhenElseBlockTag.ElseClause.LiquidBlock);
            }
        }

        public void Visit(ContinueTag continueTag)
        {
            throw new ContinueException();
        }

        public void Visit(BreakTag breakTag)
        {
            throw new BreakException();
        }

        public void Visit(MacroBlockTag macroBlockTag)
        {
            // not implemented yet
            
            //Console.WriteLine("Creating a macro "+ macroBlockTag.Name);
            //Console.WriteLine("That takes args " + String.Join(",", macroBlockTag.Args));
            //Console.WriteLine("and has body " + macroBlockTag.LiquidBlock);
            _symbolTableStack.DefineMacro(macroBlockTag.Name, macroBlockTag);
        }

        public void Visit(ErrorNode errorNode)
        {
            //Console.WriteLine("TODO: Render error : " + errorNode.ToString());
            _result += errorNode.LiquidError.ToString();
        }

        public void Visit(RootDocumentNode rootDocumentNode)
        {
           // noop
        }

        public void Visit(VariableReference variableReference)
        {
            variableReference.Eval(_symbolTableStack, new List<IExpressionConstant>());
        }

        public void Visit(StringValue stringValue)
        {          
            _result += Render(stringValue); 
        }

        /// <summary>
        /// Process the object / filter chain
        /// </summary>
        /// <param name="liquidExpression"></param>
        public void Visit(LiquidExpression liquidExpression)
        {
            Console.WriteLine("Visiting Object Expression ");
            var constResult = LiquidExpressionEvaluator.Eval(liquidExpression, new List<IExpressionConstant>(),
                 _symbolTableStack);
            
            
            _result += Render(constResult); 

        }

        public void Visit(LiquidExpressionTree liquidExpressionTree)
        {
            Console.WriteLine("Visiting Object Expression Tree ");

            var constResult = LiquidExpressionEvaluator.Eval(liquidExpressionTree, _symbolTableStack);

            _result += Render(constResult); 
        }

        public String Render(IExpressionConstant result)
        {
            Console.WriteLine("Rendering IExpressionConstant " + result.Value);

//            if (result.HasError)
//            {
//                return result.ErrorMessage;
//            }

            return ValueCaster.RenderAsString(result);
        }


    }

    public class ContinueException : Exception { }

    public class BreakException : Exception { }
}
