using System.Text.RegularExpressions;

namespace Assets.Scripts.Utils {
  public static class StringUtils {
    /// <summary>
    /// LastUpdated --> Last Updated
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ToSentenceCase(this string str) {
      return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}");
    }
  }
}
