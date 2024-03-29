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
        public static IEnumerable<Type> FindTypes(Func<Assembly, bool> assemblyPredicate, Func<Type, bool> typePredicate, AppDomain appDomain = null)
        {
            if (null == appDomain) appDomain = AppDomain.CurrentDomain;
            if (null == typePredicate) throw new ArgumentNullException(nameof(typePredicate));

            return from assembly in appDomain.GetAssemblies().Where(a => assemblyPredicate(a))
                   from type in assembly.GetTypes()
                   where typePredicate(type)
                   select type;
        }

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

        public static string GetNormalizedName(this Type type, bool ignoreNamespace = false)
        {
            string GetTypeName(Type type, bool ignoreNamespace = false)
            {
                if (ignoreNamespace) return type.Name;
                return type.FullName;
            }

            if (null == type) throw new ArgumentNullException(nameof(type));

            if (type.IsArray)
            {
                return string.Concat(ReflectionHelpers.GetNormalizedName(type.GetElementType(), ignoreNamespace), "[]");
            }

            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return string.Format(
                    "{0}<{1}>",
                    GetTypeName(genericTypeDefinition, ignoreNamespace).Substring(0, GetTypeName(genericTypeDefinition, ignoreNamespace).LastIndexOf("`", StringComparison.InvariantCulture)),
                    string.Join(",", type.GetGenericArguments().Select(t => GetNormalizedName(t, ignoreNamespace))));
            }

            return GetTypeName(type, ignoreNamespace);
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

            return from m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
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

        public static IEnumerable<MethodInfo> FindMethods(this Type type, Func<MethodInfo, bool> predicate, ClrMemberFlags methodFlags = ClrMemberFlags.None)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));
            if (null == predicate) throw new ArgumentNullException(nameof(predicate));

            return from m in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                   where (
                       ClrMemberFlags.None == methodFlags
                       || (
                           methodFlags.HasFlag(ClrMemberFlags.Instance) ? !m.IsStatic : true
                           && methodFlags.HasFlag(ClrMemberFlags.Static) ? m.IsStatic : true
                           && methodFlags.HasFlag(ClrMemberFlags.Public) ? m.IsPublic : true
                           && methodFlags.HasFlag(ClrMemberFlags.NonPublic) ? !m.IsPublic : true
                           && methodFlags.HasFlag(ClrMemberFlags.SpecialName) ? m.IsSpecialName : true
                       )
                   ) && predicate(m)
                   select m;
        }

        public static MethodInfo FindMethod(this Type ownerType, string methodName, ClrMemberFlags methodFlags, params Type[] argumentTypes)
        {
            if (null == ownerType) throw new ArgumentNullException(nameof(ownerType));
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentNullException(nameof(methodName));

            BindingFlags bindingFlags = BindingFlags.Default;

            if (methodFlags.HasFlag(ClrMemberFlags.Instance)) bindingFlags |= BindingFlags.Instance;
            if (methodFlags.HasFlag(ClrMemberFlags.Static)) bindingFlags |= BindingFlags.Static;
            if (methodFlags.HasFlag(ClrMemberFlags.Public)) bindingFlags |= BindingFlags.Public;
            if (methodFlags.HasFlag(ClrMemberFlags.NonPublic)) bindingFlags |= BindingFlags.NonPublic;

            return ownerType.GetMethods(bindingFlags)
                .Where(x => 0 == string.Compare(x.Name, methodName, StringComparison.OrdinalIgnoreCase))
                .Where(x =>
                {
                    var methodParameters = x.GetParameters();
                    if (null == argumentTypes || 0 == argumentTypes.Length) return 0 == methodParameters.Length;
                    else
                    {
                        if (argumentTypes.Length != methodParameters.Length) return false;

                        for (int loop = 0; loop < methodParameters.Length; loop++)
                        {
                            if (!methodParameters[loop].ParameterType.IsAssignableFrom(argumentTypes[loop]))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                })
                .FirstOrDefault();

        }

        // moniker = Name.Space.Type::MethodName
        public static MethodInfo FindMethod(string methodMoniker, ClrMemberFlags methodFlags, params Type[] argumentTypes)
        {
            if (string.IsNullOrWhiteSpace(methodMoniker)) throw new ArgumentNullException(nameof(methodMoniker));

            methodMoniker = methodMoniker.Split('(', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(methodMoniker)) throw new ArgumentException("Method moniker must be in the short format such as Namespace.TypeName::MethodName");

            StringBuilder buffer = new StringBuilder();
            buffer.Append(methodMoniker);
            buffer.Append("(");
            if (null != argumentTypes)
            {
                buffer.AppendJoin(",", argumentTypes.Select(x => x.GetNormalizedName()));
            }
            buffer.Append(")");
            return ReflectionHelpers.FindMethod(methodMoniker, methodFlags);

        }

        // moniker = Name.Space.Type::MethodName(@param1Type ...)
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

        public static IEnumerable<FieldInfo> FindFields(this Type type, Func<FieldInfo, bool> predicate, ClrMemberFlags fieldFlags = ClrMemberFlags.None)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));
            if (null == predicate) throw new ArgumentNullException(nameof(predicate));

            return from f in type.GetFields()
                   where (
                       ClrMemberFlags.None == fieldFlags
                       || (
                           fieldFlags.HasFlag(ClrMemberFlags.Instance) ? !f.IsStatic : true
                           && fieldFlags.HasFlag(ClrMemberFlags.Static) ? f.IsStatic : true
                           && fieldFlags.HasFlag(ClrMemberFlags.Public) ? f.IsPublic : true
                           && fieldFlags.HasFlag(ClrMemberFlags.NonPublic) ? !f.IsPublic : true
                           && fieldFlags.HasFlag(ClrMemberFlags.SpecialName) ? f.IsSpecialName : true
                           && fieldFlags.HasFlag(ClrMemberFlags.Constant) ? f.IsLiteral : true
                       )
                   ) && predicate(f)
                   select f;
        }

        public static IEnumerable<FieldInfo> FindFields<TFieldType>(this Type type, bool includeInheritedTypes = false, ClrMemberFlags fieldFlags = ClrMemberFlags.None)
        {
            Type typeOfField = typeof(TFieldType);
            return type.FindFields(f =>
            {
                if (includeInheritedTypes)
                {
                    return typeOfField.IsAssignableFrom(f.FieldType);
                }
                else
                {
                    return typeOfField.Equals(f.FieldType);
                }
            }, fieldFlags);
        }

        public static IEnumerable<PropertyInfo> FindProperties(this Type type, Func<PropertyInfo, bool> predicate, ClrMemberFlags propertyFlags = ClrMemberFlags.Public | ClrMemberFlags.Instance)
        {
            if (null == type) throw new ArgumentNullException(nameof(type));
            if (null == predicate) throw new ArgumentNullException(nameof(predicate));

            BindingFlags bindingFlags = BindingFlags.IgnoreCase;

            bindingFlags |= propertyFlags.HasFlag(ClrMemberFlags.Static) ? BindingFlags.Static : bindingFlags;
            bindingFlags |= propertyFlags.HasFlag(ClrMemberFlags.Instance) ? BindingFlags.Instance : bindingFlags;
            bindingFlags |= propertyFlags.HasFlag(ClrMemberFlags.NonPublic) ? BindingFlags.NonPublic : bindingFlags;
            bindingFlags |= propertyFlags.HasFlag(ClrMemberFlags.Public) ? BindingFlags.Public : bindingFlags;

            return type.GetProperties(bindingFlags).Where(predicate);
        }

        public static void InvokeMethods(IEnumerable<MethodInfo> methods, object instance, params object[] arguments)
        {
            if (null == methods) return;

            foreach (var method in methods)
            {
                method.Invoke(instance, arguments);
            }
        }
    }
}
