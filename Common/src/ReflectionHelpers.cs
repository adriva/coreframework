using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Adriva.Common.Core
{
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

        public static string GetNormalizedName(this Type type)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));

            if (type.IsArray)
            {
                return string.Concat(ReflectionHelpers.GetNormalizedName(type.GetElementType()), "[]");
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return string.Format(
                    "{0}<{1}>",
                    genericTypeDefinition.FullName.Substring(0, genericTypeDefinition.FullName.LastIndexOf("`", StringComparison.InvariantCulture)),
                    string.Join(",", type.GetGenericArguments().Select(GetNormalizedName)));
            }

            return type.FullName ?? type.Name;
        }

        public static string GetNormalizedName(this MethodInfo methodInfo)
        {
            if (null == methodInfo) throw new ArgumentNullException(nameof(methodInfo));

            StringBuilder buffer = new StringBuilder();
            buffer.Append(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                buffer.Append("<");
                Type[] genericMethodArguments = methodInfo.GetGenericArguments();
                buffer.AppendJoin(",", genericMethodArguments.Select(x => GetNormalizedName(x)));
                buffer.Append(">");
            }

            buffer.Append("(");

            ParameterInfo[] parameters = methodInfo.GetParameters();

            if (null != parameters)
            {
                buffer.AppendJoin(",", parameters.Select(x => GetNormalizedName(x.ParameterType)));
            }

            buffer.Append(")");

            return buffer.ToString();
        }

        public static IEnumerable<MethodInfo> FindMethods(this Type type, ClrMemberFlags methodFlags = ClrMemberFlags.None)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));

            return from m in type.GetMethods()
                   where (
                       ClrMemberFlags.None == methodFlags
                       || (
                           methodFlags.HasFlag(ClrMemberFlags.Instance) ? !m.IsStatic : true
                           && methodFlags.HasFlag(ClrMemberFlags.Static) ? m.IsStatic : true
                           && methodFlags.HasFlag(ClrMemberFlags.Public) ? m.IsPublic : true
                           && methodFlags.HasFlag(ClrMemberFlags.NonPublic) ? !m.IsPublic : true
                           && methodFlags.HasFlag(ClrMemberFlags.SpecialName) ? m.IsSpecialName : true
                       )
                   )
                   select m;
        }

        public static MethodInfo FindMethod(string methodMoniker, ClrMemberFlags methodFlags)
        {
            if (string.IsNullOrWhiteSpace(methodMoniker)) throw new ArgumentNullException(nameof(methodMoniker));

            var matches = Regex.Matches(methodMoniker, @"^(?<classTypeName>\w+.*)::(?<methodName>\w+.*)\((?<parameters>.*)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string classTypeName = null, methodName = null;
            List<string> paramterTypes = new List<string>();

            if (null != matches && 0 < matches.Count)
            {
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        if (null == classTypeName)
                        {
                            var group = match.Groups["classTypeName"];
                            if (null != group && group.Success)
                            {
                                classTypeName = group.Value.Trim();
                            }
                        }

                        if (null == methodName)
                        {
                            var group = match.Groups["methodName"];
                            if (null != group && group.Success)
                            {
                                methodName = group.Value.Trim();
                            }
                        }

                        {
                            var group = match.Groups["parameters"];
                            if (null != group && group.Success)
                            {
                                paramterTypes.AddRange(group.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                            }
                        }
                    }
                }
            }

            Type classType = Type.GetType(classTypeName, false, true);

            StringBuilder buffer = new StringBuilder();
            buffer.Append(methodName);
            buffer.Append("(");
            buffer.AppendJoin(",", paramterTypes);
            buffer.Append(")");

            if (null == classType) throw new TypeLoadException($"Could not find or load the specified type '{classTypeName}'.");

            try
            {
                return classType.FindMethods(methodFlags).Where(m =>
                    paramterTypes.Count == m.GetParameters().Count()
                    && 0 == string.Compare(m.GetNormalizedName(), buffer.ToString(), StringComparison.OrdinalIgnoreCase)
                ).Single();
            }
            catch (InvalidOperationException innerException)
            {
                throw new InvalidOperationException($"Could not find method '{methodName}' matching the given signature. Have you forgotten to fully qualify all type names in the Namespace.TypeName format?", innerException);
            }
        }

    }
}
