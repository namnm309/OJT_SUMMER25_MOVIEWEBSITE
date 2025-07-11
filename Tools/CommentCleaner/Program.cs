// I will replace the entire file with the implementation
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommentCleaner;

class Program
{
    // Regex để nhận diện ký tự có dấu tiếng Việt
    private static readonly Regex VietnameseRegex = new(
        "[À-ỹ]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SingleLineCommentRegexCsJs = new(@"^\s*//", RegexOptions.Compiled);
    private static readonly Regex HtmlCommentRegex = new("<!--(.*?)-->", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex RazorCommentRegex = new(@"@\*(.*?)\*@", RegexOptions.Compiled | RegexOptions.Singleline);
    private static readonly Regex BlockCommentRegex = new(@"/\*(.*?)\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    static void Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : "UI";
        if (!Directory.Exists(root))
        {
            Console.WriteLine($"Không tìm thấy thư mục {root}");
            return;
        }

        var extensions = new[] { ".cs", ".cshtml", ".js", ".css" };
        var files = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                              .Where(f => extensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                              .ToList();

        Console.WriteLine($"Đang xử lý {files.Count} files ...");
        int changed = 0;

        foreach (var file in files)
        {
            var originalText = File.ReadAllText(file);
            var processed = ProcessContent(originalText, Path.GetExtension(file));
            if (processed != originalText)
            {
                File.WriteAllText(file, processed);
                changed++;
                Console.WriteLine($"Đã làm sạch: {file}");
            }
        }

        Console.WriteLine($"Hoàn thành. Đã cập nhật {changed}/{files.Count} files.");
    }

    static string ProcessContent(string content, string extension)
    {
        // Xử lý block comments trước
        content = BlockCommentRegex.Replace(content, m => VietnameseRegex.IsMatch(m.Value) ? m.Value : "");
        content = HtmlCommentRegex.Replace(content, m => VietnameseRegex.IsMatch(m.Value) ? m.Value : "");
        content = RazorCommentRegex.Replace(content, m => VietnameseRegex.IsMatch(m.Value) ? m.Value : "");

        // Xử lý single-line comments
        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (SingleLineCommentRegexCsJs.IsMatch(lines[i]) && !VietnameseRegex.IsMatch(lines[i]))
            {
                lines[i] = string.Empty;
            }
        }
        return string.Join('\n', lines);
    }
}
