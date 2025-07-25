using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Adriva.Common.Core;

namespace Adriva.Extensions.Worker.Durable;

internal static partial class Helpers
{
    private const char TypeMethodSeperator = ':';
    private const char NamespaceSeperator = '.';

    [GeneratedRegex(@"^(?<classTypeName>\w+.*)::(?<methodName>\w+.*)\((?<parameters>.*)\)$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-TR")]
    private static partial Regex GetMonikerRegEx();

    public static List<Type> FindTypes(Func<Type, bool> predicate)
    {
        AppDomain appDomain = AppDomain.CurrentDomain;

        ArgumentNullException.ThrowIfNull(predicate);

        List<Type> types = [];

        foreach (var assembly in appDomain.GetAssemblies())
        {
            try
            {
                foreach (var type in assembly.GetExportedTypes())
                {
                    if (predicate(type))
                    {
                        types.Add(type);
                    }
                }
            }
            catch
            {
                System.Console.WriteLine("OK");
            }
        }

        return types;
    }

    public static bool TryParseNormalizedName(string normalizedName, [NotNullWhen(true)] out string? fullTypeName, out string? methodSignature)
    {
        fullTypeName = methodSignature = null;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return false;
        }

        if (1 < normalizedName.Count(x => Helpers.TypeMethodSeperator == x))
        {
            return false;
        }

        int methodNameStartIndex = normalizedName.IndexOf(Helpers.TypeMethodSeperator);

        if (0 < methodNameStartIndex)
        {
            fullTypeName = normalizedName[..methodNameStartIndex];
            methodSignature = normalizedName.Substring(1 + methodNameStartIndex);
        }
        else
        {
            fullTypeName = normalizedName;
            methodSignature = null;
        }

        return true;
    }

    public static bool TryResolveType(string normalizedName, [NotNullWhen(true)] out Type? type)
    {
        type = null;

        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            return false;
        }

        if (!Helpers.TryParseNormalizedName(normalizedName, out string? fullTypeName, out string? methodSignature))
        {
            return false;
        }

        var targetTypes = Helpers
                            .FindTypes(t => t.IsClass && !t.IsSpecialName && ReflectionHelpers.GetNormalizedName(t).Equals(fullTypeName, StringComparison.Ordinal));

        if (1 != targetTypes.Count)
        {
            return false;
        }

        type = targetTypes[0];
        return true;
    }

    // moniker = Name.Space.Type::MethodName(@param1Type ...)
    public static MethodInfo? FindMethod(string methodMoniker, ClrMemberFlags methodFlags)
    {
        if (string.IsNullOrWhiteSpace(methodMoniker)) throw new ArgumentNullException(nameof(methodMoniker));

        var matches = GetMonikerRegEx().Matches(methodMoniker);

        string? classTypeName = null, methodName = null;

        List<string> paramterTypes = [];

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

        ArgumentNullException.ThrowIfNullOrWhiteSpace(classTypeName);

        Type? classType = Type.GetType(classTypeName, false, true);

        classType ??= ReflectionHelpers.FindTypes(t => !t.IsSpecialName && t.IsClass && !string.IsNullOrWhiteSpace(t.FullName) && t.FullName.Equals(classTypeName, StringComparison.Ordinal)).FirstOrDefault();

        StringBuilder buffer = new();
        buffer.Append(methodName);
        buffer.Append('(');
        buffer.AppendJoin(",", paramterTypes);
        buffer.Append(')');

        if (null == classType) throw new TypeLoadException($"Could not find or load the specified type '{classTypeName}'.");

        try
        {
            return classType.FindMethods(methodFlags).Where(m =>
                paramterTypes.Count == m.GetParameters().Length
                && 0 == string.Compare(m.GetNormalizedName(), buffer.ToString(), StringComparison.OrdinalIgnoreCase)
            ).SingleOrDefault();
        }
        catch (InvalidOperationException innerException)
        {
            throw new InvalidOperationException($"Could not find method '{methodName}' matching the given signature. Have you forgotten to fully qualify all type names in the Namespace.TypeName format?", innerException);
        }
    }
}
