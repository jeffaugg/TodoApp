using System.Text;

namespace TodoApp.Telemetry.Utils;

public static class NamingHelper
{

  public static string ToSnakeCase(string input)
  {
    if (string.IsNullOrEmpty(input))
      return string.Empty;

    var result = new StringBuilder();
    var chars = input.ToCharArray();

    for (int i = 0; i < chars.Length; i++)
    {
      var currentChar = chars[i];


      if (currentChar == '.')
      {
        result.Append('_');
        continue;
      }


      if (char.IsUpper(currentChar) && i > 0)
      {
        // Adiciona underscore antes se o caractere anterior não for underscore
        if (result.Length > 0 && result[result.Length - 1] != '_')
        {
          result.Append('_');
        }
        result.Append(char.ToLowerInvariant(currentChar));
      }
      else
      {
        result.Append(char.ToLowerInvariant(currentChar));
      }
    }

    return result.ToString();
  }

  public static string ToSnakeCaseStep(string className, string methodName)
  {
    var combined = $"{className}.{methodName}";
    return ToSnakeCase(combined);
  }
}