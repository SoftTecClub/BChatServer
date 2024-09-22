using System.Text.RegularExpressions;

namespace BChatServer.Src.Common;

/// <summary>
/// 汎用的なセキュリティ関係の共通関数クラス
/// <author>Ryokugyoku</author>
/// <date>2024/09/22</date>
/// </summary>
public static class SecurityCommonFunc{

    /// <summary>
    /// 入力文字列から不要な文字や危険な文字を取り除きます。
    /// </summary>
    /// <param name="input">サニタイズする入力文字列</param>
    ///<author>Ryokugyoku</author>
    ///<date>2024/09/22</date>
    /// <returns>サニタイズされた文字列</returns>
    public static string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // HTMLタグを除去
        string sanitized = Regex.Replace(input, "<.*?>", string.Empty);

        // SQLインジェクション対策としてシングルクォートをエスケープ
        sanitized = sanitized.Replace("'", "''");

        // JavaScriptインジェクション対策として特定の文字をエスケープ
        sanitized = sanitized.Replace("<", "&lt;").Replace(">", "&gt;");
        sanitized = sanitized.Replace("(", "&#40;").Replace(")", "&#41;");
        sanitized = sanitized.Replace("\"", "&quot;").Replace("'", "&#x27;");
        sanitized = sanitized.Replace("/", "&#x2F;");

        // その他の不要な文字を除去（例: 制御文字）
        sanitized = Regex.Replace(sanitized, @"[\x00-\x1F\x7F]", string.Empty);

        return sanitized;
    }
}