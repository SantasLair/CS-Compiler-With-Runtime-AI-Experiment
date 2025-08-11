using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PixelManGame.Compiler
{
    public class CSharpCompiler
    {
        private Dictionary<string, ClassDefinition> classes = new Dictionary<string, ClassDefinition>();
        private List<Instruction> instructions = new List<Instruction>();
        private Dictionary<string, int> methodAddresses = new Dictionary<string, int>();
        private int currentAddress = 0;

        public string CompileToJson(string[] sourceFiles)
        {
            // Parse all source files first
            foreach (var file in sourceFiles)
            {
                ParseSourceFile(file);
            }

            // Generate bytecode for all methods
            foreach (var classKvp in classes)
            {
                foreach (var method in classKvp.Value.Methods)
                {
                    CompileMethod(classKvp.Key, method);
                }
            }

            // Create the final bytecode package
            var bytecode = new BytecodePackage
            {
                Classes = classes,
                Instructions = instructions,
                MethodAddresses = methodAddresses,
                EntryPoint = "PixelManGame.GameMain.StartGame"
            };

            return JsonSerializer.Serialize(bytecode, new JsonSerializerOptions { WriteIndented = true });
        }

        private void ParseSourceFile(string filePath)
        {
            var content = File.ReadAllText(filePath);
            var className = ExtractClassName(content);
            if (string.IsNullOrEmpty(className)) return;

            var classDef = new ClassDefinition
            {
                Name = className,
                BaseClass = ExtractBaseClass(content),
                Fields = ExtractFields(content),
                Methods = ExtractMethods(content),
                Properties = ExtractProperties(content)
            };

            classes[className] = classDef;
        }

        private string ExtractClassName(string content)
        {
            var match = Regex.Match(content, @"(?:public\s+)?(?:abstract\s+)?class\s+(\w+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private string ExtractBaseClass(string content)
        {
            var match = Regex.Match(content, @"class\s+\w+\s*:\s*(\w+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        private List<FieldDefinition> ExtractFields(string content)
        {
            var fields = new List<FieldDefinition>();
            var matches = Regex.Matches(content, @"(?:private|public|protected)\s+(?:static\s+)?(?:readonly\s+)?(\w+(?:<[^>]+>)?)\s+(\w+)(?:\s*=\s*([^;]+))?;");
            
            foreach (Match match in matches)
            {
                fields.Add(new FieldDefinition
                {
                    Type = match.Groups[1].Value,
                    Name = match.Groups[2].Value,
                    DefaultValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null,
                    IsStatic = match.Value.Contains("static"),
                    IsReadonly = match.Value.Contains("readonly")
                });
            }
            
            return fields;
        }

        private List<MethodDefinition> ExtractMethods(string content)
        {
            var methods = new List<MethodDefinition>();
            var pattern = @"(?:public|private|protected)\s+(?:static\s+)?(?:virtual\s+)?(?:override\s+)?(\w+(?:<[^>]+>)?)\s+(\w+)\s*\(([^)]*)\)\s*\{([^}]*(?:\{[^}]*\}[^}]*)*)\}";
            var matches = Regex.Matches(content, pattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var method = new MethodDefinition
                {
                    ReturnType = match.Groups[1].Value,
                    Name = match.Groups[2].Value,
                    Parameters = ParseParameters(match.Groups[3].Value),
                    Body = match.Groups[4].Value.Trim(),
                    IsStatic = match.Value.Contains("static"),
                    IsVirtual = match.Value.Contains("virtual"),
                    IsOverride = match.Value.Contains("override")
                };

                methods.Add(method);
            }

            return methods;
        }

        private List<PropertyDefinition> ExtractProperties(string content)
        {
            var properties = new List<PropertyDefinition>();
            var matches = Regex.Matches(content, @"(?:public|private|protected)\s+(\w+(?:<[^>]+>)?)\s+(\w+)\s*\{\s*get;\s*(?:(?:private\s+)?set;)?\s*\}(?:\s*=\s*([^;]+))?;");

            foreach (Match match in matches)
            {
                properties.Add(new PropertyDefinition
                {
                    Type = match.Groups[1].Value,
                    Name = match.Groups[2].Value,
                    DefaultValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null,
                    HasGetter = true,
                    HasSetter = match.Value.Contains("set;")
                });
            }

            return properties;
        }

        private List<ParameterDefinition> ParseParameters(string parametersString)
        {
            var parameters = new List<ParameterDefinition>();
            if (string.IsNullOrWhiteSpace(parametersString)) return parameters;

            var paramParts = parametersString.Split(',');
            foreach (var part in paramParts)
            {
                var trimmed = part.Trim();
                var match = Regex.Match(trimmed, @"(\w+(?:<[^>]+>)?)\s+(\w+)(?:\s*=\s*([^,]+))?");
                if (match.Success)
                {
                    parameters.Add(new ParameterDefinition
                    {
                        Type = match.Groups[1].Value,
                        Name = match.Groups[2].Value,
                        DefaultValue = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null
                    });
                }
            }

            return parameters;
        }

        private void CompileMethod(string className, MethodDefinition method)
        {
            var methodKey = $"{className}.{method.Name}";
            methodAddresses[methodKey] = currentAddress;

            // Simple bytecode generation - convert C# statements to bytecode instructions
            var statements = SplitStatements(method.Body);
            
            foreach (var statement in statements)
            {
                CompileStatement(statement.Trim());
            }

            // Add return instruction
            AddInstruction(OpCode.RETURN, null);
        }

        private List<string> SplitStatements(string body)
        {
            var statements = new List<string>();
            var current = new StringBuilder();
            int braceLevel = 0;
            bool inString = false;
            char stringChar = '\0';

            for (int i = 0; i < body.Length; i++)
            {
                char c = body[i];
                
                if (!inString)
                {
                    if (c == '"' || c == '\'')
                    {
                        inString = true;
                        stringChar = c;
                    }
                    else if (c == '{')
                    {
                        braceLevel++;
                    }
                    else if (c == '}')
                    {
                        braceLevel--;
                    }
                    else if (c == ';' && braceLevel == 0)
                    {
                        statements.Add(current.ToString());
                        current.Clear();
                        continue;
                    }
                }
                else if (c == stringChar && (i == 0 || body[i-1] != '\\'))
                {
                    inString = false;
                }

                current.Append(c);
            }

            if (current.Length > 0)
            {
                statements.Add(current.ToString());
            }

            return statements;
        }

        private void CompileStatement(string statement)
        {
            if (string.IsNullOrWhiteSpace(statement)) return;

            // Variable assignment
            if (Regex.IsMatch(statement, @"^\s*\w+\s*="))
            {
                var parts = statement.Split('=', 2);
                var variable = parts[0].Trim();
                var value = parts[1].Trim();
                
                AddInstruction(OpCode.LOAD_CONST, value);
                AddInstruction(OpCode.STORE_VAR, variable);
            }
            // Method call
            else if (Regex.IsMatch(statement, @"\w+\([^)]*\)"))
            {
                var match = Regex.Match(statement, @"(\w+(?:\.\w+)*)\(([^)]*)\)");
                if (match.Success)
                {
                    var methodName = match.Groups[1].Value;
                    var args = match.Groups[2].Value;
                    
                    // Load arguments
                    if (!string.IsNullOrWhiteSpace(args))
                    {
                        var argList = args.Split(',');
                        foreach (var arg in argList)
                        {
                            AddInstruction(OpCode.LOAD_CONST, arg.Trim());
                        }
                    }
                    
                    AddInstruction(OpCode.CALL_METHOD, methodName);
                }
            }
            // Property access
            else if (statement.Contains('.'))
            {
                AddInstruction(OpCode.LOAD_PROPERTY, statement);
            }
            // Console.WriteLine or other simple statements
            else
            {
                AddInstruction(OpCode.EXPRESSION, statement);
            }
        }

        private void AddInstruction(OpCode opCode, string operand)
        {
            instructions.Add(new Instruction
            {
                OpCode = opCode,
                Operand = operand,
                Address = currentAddress++
            });
        }
    }

    public enum OpCode
    {
        LOAD_CONST,
        STORE_VAR,
        LOAD_VAR,
        LOAD_PROPERTY,
        STORE_PROPERTY,
        CALL_METHOD,
        CALL_CONSTRUCTOR,
        RETURN,
        JUMP,
        JUMP_IF_FALSE,
        EXPRESSION
    }

    public class Instruction
    {
        public OpCode OpCode { get; set; }
        public string Operand { get; set; }
        public int Address { get; set; }
    }

    public class BytecodePackage
    {
        public Dictionary<string, ClassDefinition> Classes { get; set; }
        public List<Instruction> Instructions { get; set; }
        public Dictionary<string, int> MethodAddresses { get; set; }
        public string EntryPoint { get; set; }
    }

    public class ClassDefinition
    {
        public string Name { get; set; }
        public string BaseClass { get; set; }
        public List<FieldDefinition> Fields { get; set; } = new List<FieldDefinition>();
        public List<MethodDefinition> Methods { get; set; } = new List<MethodDefinition>();
        public List<PropertyDefinition> Properties { get; set; } = new List<PropertyDefinition>();
    }

    public class FieldDefinition
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadonly { get; set; }
    }

    public class MethodDefinition
    {
        public string ReturnType { get; set; }
        public string Name { get; set; }
        public List<ParameterDefinition> Parameters { get; set; } = new List<ParameterDefinition>();
        public string Body { get; set; }
        public bool IsStatic { get; set; }
        public bool IsVirtual { get; set; }
        public bool IsOverride { get; set; }
    }

    public class PropertyDefinition
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
        public bool HasGetter { get; set; }
        public bool HasSetter { get; set; }
    }

    public class ParameterDefinition
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string DefaultValue { get; set; }
    }
}
