﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Localizy
{
    public class TokenScanner
    {
        public static IEnumerable<StringToken> GetAllTokens(Assembly[] assembly, Func<Type, bool> where)
        {
#if !DOTNET54
            assembly = (assembly == null || assembly.Length == 0) ? new[] { FindTheCallingAssembly() } : assembly;
#endif
            if (assembly == null)
                return Enumerable.Empty<StringToken>();

            var stringTokens = assembly.SelectMany(z => z.GetExportedTypes().Where(where).SelectMany(x => x.RecurseNestedTypes()).Distinct().SelectMany(ScanStringTokenType));

            return stringTokens;
        }

        private static IEnumerable<StringToken> ScanStringTokenType(Type type)
        {
            return type.GetFields(BindingFlags.Static | BindingFlags.Public).Where(field => field.FieldType.CanBeCastTo<StringToken>()).Select(field => field.GetValue(null).As<StringToken>());
        }

#if !DOTNET54
        public static Assembly FindTheCallingAssembly()
        {
            var trace = new StackTrace(false);

            var thisAssembly = Assembly.GetExecutingAssembly();
            var smAssembly = typeof(TokenScanner).Assembly;

            Assembly callingAssembly = null;
            for (var i = 0; i < trace.FrameCount; i++)
            {
                var frame = trace.GetFrame(i);
                var assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly && assembly != smAssembly)
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }
#endif
    }
}
