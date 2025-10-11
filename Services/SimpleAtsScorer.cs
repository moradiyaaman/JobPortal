using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using JobPortal.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace JobPortal.Services
{
    public sealed class SimpleAtsScorer : IAtsScorer
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<SimpleAtsScorer> _logger;

    private static readonly char[] SplitChars = " \r\n\t,.;:/\\|()-_[]{}'\"+*&^%$#@!~`?".ToCharArray();
    private static readonly ConcurrentDictionary<string, CacheEntry> ResumeCache = new ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<string> Stop = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a","an","the","and","or","for","to","of","in","on","with","by","at","as","is","are","was","were","be","been","being",
            "from","into","over","per","via","your","their","this","that","those","these","using","use","used","etc"
        };
        private static readonly Dictionary<string, string> Synonyms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "csharp", "c#" }, { "c#", "c#" },
            { "dotnet", ".net" }, { ".net", ".net" },
            { "aspnet", "asp.net" }, { "asp.net", "asp.net" },
            { "js", "javascript" }, { "ts", "typescript" },
            { "mssql", "sql" }, { "sqlserver", "sql" }, { "postgresql", "postgres" },
            { "py", "python" }, { "py3", "python" }, { "py2", "python" },
            { "jscript", "javascript" }, { "nodejs", "node" }, { "expressjs", "express" },
            { "reactjs", "react" }, { "angularjs", "angular" }, { "vuejs", "vue" },
            { "dockercompose", "docker" }, { "k8s", "kubernetes" },
            { "ci/cd", "cicd" }, { "ci", "cicd" }, { "cd", "cicd" },
            { "ml", "machinelearning" }, { "ai", "artificialintelligence" },
            { "oop", "objectoriented" }, { "mvc", "modelviewcontroller" },
            { "bachelors", "bachelor" }, { "masters", "master" }, { "phd", "doctorate" }
        };

        private static readonly Dictionary<string, string[]> SectionSynonyms = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "experience", new[] { "experience", "work experience", "professional experience", "work history", "employment history" } },
            { "education",  new[] { "education", "academic", "academics", "education & certifications", "academic background" } },
            { "skills",      new[] { "skills", "technical skills", "technologies", "tooling", "core skills", "key skills" } },
            { "projects",    new[] { "projects", "project work", "personal projects", "academic projects", "selected projects" } },
            { "summary",     new[] { "summary", "professional summary", "career summary", "objective", "about me" } },
            { "profile",     new[] { "profile", "professional profile", "summary profile" } }
        };

        private static readonly Dictionary<string, HashSet<string>> SectionSynonymsNormalized;

        private static readonly string[] ActionVerbs = { "built","designed","implemented","led","optimized","improved","launched","delivered","migrated","reduced","increased" };

        static SimpleAtsScorer()
        {
            SectionSynonymsNormalized = SectionSynonyms.ToDictionary(
                kvp => kvp.Key,
                kvp => new HashSet<string>(kvp.Value.Select(NormalizeHeading), StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }

        public SimpleAtsScorer(IWebHostEnvironment env, ILogger<SimpleAtsScorer> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<AtsResult> ScoreResumeAsync(ApplicationUser user)
        {
            var suggestions = new List<string>();
            var text = await ReadResumeTextAsync(user?.ResumeFileName);
            if (string.IsNullOrWhiteSpace(text))
            {
                return new AtsResult
                {
                    Score = 0,
                    MatchedKeywords = Array.Empty<string>(),
                    MissingKeywords = new[] { "Resume file missing or unreadable" },
                    Suggestions = new[] { "Upload a resume (DOCX or text-based PDF).", "Ensure the file opens and contains selectable text (not just images)." }
                };
            }

            if (text.Length < 80)
            {
                suggestions.Add("Your resume text seems very short. If it's a scanned PDF, export as text-based PDF or upload DOCX.");
            }

            _logger.LogDebug("ATS: extracted resume characters = {Len}", text.Length);

            var tokens = Tokenize(text);
            int score = 0;

            var foundSections = DetectSections(text);
            _logger.LogDebug("ATS: detected sections = {Sections}", string.Join(", ", foundSections));

            double structurePct = Math.Min(1.0, foundSections.Count / 3.0);
            if (foundSections.Count < 3) suggestions.Add("Add clear sections: Experience, Education, and Skills.");
            if (!foundSections.Contains("summary") && !foundSections.Contains("profile")) suggestions.Add("Add a brief Summary/Profile at the top to frame your experience.");
            score += (int)Math.Round(structurePct * 40);

            int words = text.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries).Length;
            double contentPct = words < 250 ? 0.4 : words > 1200 ? 0.6 : 1.0;
            if (words < 250) suggestions.Add("Increase content (aim for 1�2 pages with concrete details).");
            if (words > 1200) suggestions.Add("Trim content (keep it concise and focused).");
            score += (int)Math.Round(contentPct * 20);

            int uniqueTokens = tokens.Count();
            double skillsPct = Math.Min(1.0, uniqueTokens / 120.0);
            if (uniqueTokens < 80) suggestions.Add("Enrich your Skills and Experience sections with relevant keywords and tools.");
            score += (int)Math.Round(skillsPct * 30);

            bool hasActions = ActionVerbs.Any(v => tokens.Contains(v));
            if (!hasActions) suggestions.Add("Use action verbs and measurable results (e.g., �Improved X by Y%�).");
            score += hasActions ? 10 : 3;

            score = Math.Max(0, Math.Min(100, score));

            var allKeys = SectionSynonyms.Keys.ToArray();
            var matched = allKeys.Where(foundSections.Contains).ToArray();
            var missing = allKeys.Where(k => !foundSections.Contains(k)).ToArray();

            return new AtsResult
            {
                Score = score,
                MatchedKeywords = matched,
                MissingKeywords = missing,
                Suggestions = suggestions.Distinct().ToArray()
            };
        }

        public async Task<AtsRankResult> RankScoreAsync(ApplicationUser applicant, Job job, string coverLetterText)
        {
            try
            {
                var resumeText = await ReadResumeTextAsync(applicant?.ResumeFileName) ?? string.Empty;
                var coverText = coverLetterText ?? string.Empty;

                var resume = Tokenize(resumeText);
                var cover = Tokenize(coverText);
                var combined = new HashSet<string>(resume, StringComparer.OrdinalIgnoreCase);
                foreach (var t in cover) combined.Add(t);

                var jobSkills = Tokenize(job?.Skills ?? "");
                var jobTitle = Tokenize(job?.Title ?? "");
                var jobQuals = Tokenize(job?.Qualifications ?? "");
                var jobExp = Tokenize(job?.Experience ?? "");

                var jobKeywords = new HashSet<string>(jobSkills, StringComparer.OrdinalIgnoreCase);
                foreach (var token in jobTitle) jobKeywords.Add(token);
                foreach (var token in jobQuals) jobKeywords.Add(token);
                foreach (var token in jobExp) jobKeywords.Add(token);

                double fSkills = Coverage(jobSkills, combined);
                double fTitle = Coverage(jobTitle, combined);
                double fQuals = Coverage(jobQuals, combined);
                double fExp = Coverage(jobExp, combined);

                int score = (int)Math.Round(100.0 * (0.60 * fSkills + 0.15 * fTitle + 0.15 * fQuals + 0.10 * fExp));
                score = Math.Max(0, Math.Min(100, score));

                var matchedKeywords = jobKeywords
                    .Where(combined.Contains)
                    .OrderBy(k => k)
                    .Take(10)
                    .ToArray();

                var missingKeywords = jobKeywords
                    .Where(k => !combined.Contains(k))
                    .OrderBy(k => k)
                    .Take(10)
                    .ToArray();

                return new AtsRankResult
                {
                    Score = score,
                    MatchedKeywords = matchedKeywords,
                    MissingKeywords = missingKeywords
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ATS rank computation failed.");
                return new AtsRankResult
                {
                    Score = 0,
                    MatchedKeywords = Array.Empty<string>(),
                    MissingKeywords = Array.Empty<string>()
                };
            }
        }

        private async Task<string> ReadResumeTextAsync(string resumePath)
        {
            if (string.IsNullOrWhiteSpace(resumePath)) return null;

            // Resolve path robustly
            var candidates = new List<string>();
            try
            {
                if (Path.IsPathRooted(resumePath)) candidates.Add(resumePath);
            }
            catch { }

            if (resumePath.StartsWith("/"))
            {
                candidates.Add(Path.Combine(_env.WebRootPath, resumePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            }
            else
            {
                candidates.Add(Path.Combine(_env.WebRootPath, resumePath.Replace('/', Path.DirectorySeparatorChar)));
            }
            candidates.Add(Path.GetFullPath(resumePath));

            string physicalPath = candidates.FirstOrDefault(File.Exists);
            _logger.LogDebug("ATS: resume path '{Path}' resolved to '{Resolved}'", resumePath, physicalPath);
            if (string.IsNullOrEmpty(physicalPath)) return null;

            try
            {
                var info = new FileInfo(physicalPath);
                if (!info.Exists)
                {
                    return null;
                }

                var lastWrite = info.LastWriteTimeUtc.Ticks;
                var cacheKey = physicalPath.ToLowerInvariant();
                if (ResumeCache.TryGetValue(cacheKey, out var cached) && cached.LastWriteTicks == lastWrite)
                {
                    return cached.Text;
                }

                var ext = Path.GetExtension(physicalPath).ToLowerInvariant();
                string text;
                switch (ext)
                {
                    case ".docx":
                        text = await Task.Run(() =>
                        {
                            using var doc = WordprocessingDocument.Open(physicalPath, false);
                            return string.Join("\n", doc.MainDocumentPart.Document.Body
                                .Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>()
                                .Select(t => t.Text));
                        });
                        break;
                    case ".pdf":
                        text = await Task.Run(() => ExtractPdfText(physicalPath));
                        break;
                    default:
                        text = await File.ReadAllTextAsync(physicalPath);
                        break;
                }

                ResumeCache[cacheKey] = new CacheEntry
                {
                    LastWriteTicks = lastWrite,
                    Text = text
                };

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ATS: resume read failed for {Path}", resumePath);
                return null;
            }
        }

        private static string ExtractPdfText(string path)
        {
            try
            {
                using var reader = new PdfReader(path);
                using var pdf = new PdfDocument(reader);
                var sb = new StringBuilder();
                for (int page = 1; page <= pdf.GetNumberOfPages(); page++)
                {
                    var strategy = new LocationTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(pdf.GetPage(page), strategy);
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        sb.AppendLine(pageText);
                    }
                }
                return sb.ToString();
            }
            catch
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private static HashSet<string> DetectSections(string raw)
        {
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return found;
            }

            var lines = raw.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line?.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 120)
                {
                    continue;
                }

                var normalizedHeading = NormalizeHeading(trimmed);
                if (string.IsNullOrEmpty(normalizedHeading))
                {
                    continue;
                }

                foreach (var kv in SectionSynonymsNormalized)
                {
                    if (kv.Value.Contains(normalizedHeading))
                    {
                        found.Add(kv.Key);
                    }
                }
            }

            if (found.Count == 0)
            {
                var normalizedBody = NormalizeHeading(raw);
                foreach (var kv in SectionSynonymsNormalized)
                {
                    if (kv.Value.Any(n => normalizedBody.Contains(n)))
                    {
                        found.Add(kv.Key);
                    }
                }
            }

            return found;
        }

        private static string NormalizeHeading(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (char.IsWhiteSpace(ch) || ch == '/' || ch == '&' || ch == '-' )
                {
                    sb.Append(' ');
                }
            }

            var collapsed = Regex.Replace(sb.ToString(), "\\s+", " ").Trim();
            return collapsed;
        }

        private static IEnumerable<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return Enumerable.Empty<string>();
            var raw = text
                .ToLowerInvariant()
                .Replace("c#", "csharp")
                .Replace(".net", "dotnet");

            var tokens = raw.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => Regex.Replace(t, "[^a-z0-9.+#]", ""))
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(Normalize)
                .Where(t => !Stop.Contains(t));

            return new HashSet<string>(tokens, StringComparer.OrdinalIgnoreCase);
        }

        private static string Normalize(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return token;
            }

            if (Synonyms.TryGetValue(token, out var canon))
            {
                token = canon;
            }

            return StemToken(token);
        }

        private static string StemToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || token.Length < 4)
            {
                return token;
            }

            if (token.EndsWith("ing", StringComparison.OrdinalIgnoreCase) && token.Length > 5)
            {
                return token[..^3];
            }

            if (token.EndsWith("ies", StringComparison.OrdinalIgnoreCase) && token.Length > 5)
            {
                return token[..^3] + "y";
            }

            if (token.EndsWith("es", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            {
                return token[..^2];
            }

            if (token.EndsWith("s", StringComparison.OrdinalIgnoreCase) && token.Length > 4)
            {
                return token[..^1];
            }

            return token;
        }

        private sealed class CacheEntry
        {
            public long LastWriteTicks { get; set; }
            public string Text { get; set; }
        }

        private static double Coverage(IEnumerable<string> desired, HashSet<string> have)
        {
            var d = desired?.Where(x => !string.IsNullOrWhiteSpace(x))
                            .Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? new List<string>();
            if (d.Count == 0) return 1.0;
            var hit = d.Count(t => have.Contains(t));
            return (double)hit / d.Count;
        }
    }
}
