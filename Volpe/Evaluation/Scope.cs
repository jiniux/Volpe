using System;
using System.Collections.Generic;
using System.Linq;

namespace Volpe.Evaluation
{
    public class Scope
    {
        public Scope? Parent { get; }
        
        private readonly Dictionary<string, Value> _variables;
        private readonly Dictionary<string, Function> _functions;
        
        public Scope((string name, int parameterCount, Func<EvaluatorContext, Value[], Value> del)[] builtins)
        {
            _variables = new Dictionary<string, Value>();
            _functions = builtins.ToDictionary(b => b.name, b => (Function)new Function.Builtin(b.del, b.parameterCount));
        }

        public Scope() : this(Array.Empty<(string, int, Func<EvaluatorContext, Value[], Value>)>())
        {
        }
        
        public Scope(Scope? parent) : this()
        {
            Parent = parent;
        }
        
        public bool TryGetFunctionReference(string functionName, out Function? function)
        {
            if (_functions.TryGetValue(functionName, out function))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetFunctionReference(functionName, out function);

            return false;
        }
        
        public bool TrySetFunction(string functionName, Function.Standard function)
        {
            if (Parent?.HasFunction(functionName) ?? false)
                return false;

            if (this.HasFunction(functionName))
                return false;
            
            _functions.Add(functionName, function);
            
            return true;
        }
        
        public bool TryGetVariableValue(string variableName, out Value? value)
        {
            if (_variables.TryGetValue(variableName, out value))
                return true;
            
            if (Parent is not null)
                return Parent.TryGetVariableValue(variableName, out value);

            return false;
        }

        public bool HasVariable(string variableName) => _variables.ContainsKey(variableName);
        public bool HasFunction(string functionName) => _functions.ContainsKey(functionName);
        
        public void SetVariableValue(string variableName, Value value, bool shadowParentVariables = true)
        {
            if (!shadowParentVariables && Parent is not null && Parent.HasVariable(variableName))
            {
                Parent.SetVariableValue(variableName, value);
                return;
            }

            if (_variables.ContainsKey(variableName))
                _variables[variableName] = value;
            else
                _variables.Add(variableName, value);
        }
        
    }
}