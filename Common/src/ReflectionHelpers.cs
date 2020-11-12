using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Adriva.Common.Core
{
    [Flags]
    public enum ClrMemberFlags : long
    {
        None = 0,
        Class = 1,
        Primitive = 1 << 1,
        Concrete = 1 << 2,
        SpecialName = 1 << 3,
        NotSpecialName = 1 << 4,
        Static = 1 << 5,
        Public = 1 << 10,
        NonPublic = 1 << 11
    }

    public static class ReflectionHelpers
    {
        public static IEnumerable<Type> FindTypes(Func<Type, bool> predicate, AppDomain appDomain = null)
        {
            if (null == appDomain) appDomain = AppDomain.CurrentDomain;
            if (null == predicate) throw new ArgumentNullException(nameof(predicate));

            return from assembly in appDomain.GetAssemblies()
                   from type in assembly.GetTypes()
                   where predicate(type)
                   select type;
        }

        public static IEnumerable<Type> FindTypes<TBase>(string classNameSuffix = null, ClrMemberFlags typeFlags = ClrMemberFlags.Class)
        {
            Type typeOfTBase = typeof(TBase);

            return ReflectionHelpers.FindTypes(t =>
            {
                return
                    ClrMemberFlags.None == typeFlags ||
                        (typeFlags.HasFlag(ClrMemberFlags.Class) ? t.IsClass : true
                        && typeFlags.HasFlag(ClrMemberFlags.Concrete) ? !t.IsAbstract : true
                        && typeFlags.HasFlag(ClrMemberFlags.NonPublic) ? t.IsNotPublic : true
                        && typeFlags.HasFlag(ClrMemberFlags.Primitive) ? t.IsPrimitive : true
                        && typeFlags.HasFlag(ClrMemberFlags.Public) ? t.IsPublic : true
                        && typeFlags.HasFlag(ClrMemberFlags.SpecialName) ? t.IsSpecialName : true
                        && typeFlags.HasFlag(ClrMemberFlags.NotSpecialName) ? !t.IsSpecialName : true)
                && typeOfTBase.IsAssignableFrom(t)
                &&
                (
                    string.IsNullOrWhiteSpace(classNameSuffix)
                    || t.Name.EndsWith(classNameSuffix, StringComparison.OrdinalIgnoreCase)
                    );
            });
        }

        public static MethodInfo FindMethod(string methodMoniker, ClrMemberFlags methodFlags)
        {
#warning NOT CODE COMPLETE  
            var matches = Regex.Matches(methodMoniker, @"((^|\(|(\,\s*))(?<typeName>(\w+\.)*(\w+))|(\:{2})(?<methodName>\w+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            if (null != matches && 0 < matches.Count)
            {
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        if (match.Groups["typeName"].Success)
                        {
                            System.Console.WriteLine(match.Groups["typeName"].Value);
                            var t = Type.GetType(match.Groups["typeName"].Value, false);

                        }
                        if (match.Groups["methodName"].Success)
                        {
                            System.Console.WriteLine(match.Groups["methodName"].Value);
                        }
                    }
                }
            }

            return null;
        }

    }
}
