using System.Text;
using AgentsDashboard.Models;

namespace AgentsDashboard.Services
{
    public class AgentPrompts
    {
        private readonly string webRoot;

        public AgentPrompts(IWebHostEnvironment env)
        {
            webRoot = env.WebRootPath;
        }

        public string Build(Feature feature, AgentType type)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"# Feature: {feature.Title}");
            sb.AppendLine();
            sb.AppendLine($"Worktree: {feature.WorktreePath}");
            sb.AppendLine($"Branch: {feature.BranchName}");
            sb.AppendLine();
            sb.AppendLine("## Beschreibung");
            sb.AppendLine();
            sb.AppendLine(string.IsNullOrWhiteSpace(feature.Description) ? "(keine Beschreibung hinterlegt)" : feature.Description);
            sb.AppendLine();

            if (feature.Screenshots.Count > 0)
            {
                sb.AppendLine("## Screenshots");
                sb.AppendLine();
                foreach (var shot in feature.Screenshots)
                    sb.AppendLine("- " + Path.Combine(webRoot, shot.Replace('/', Path.DirectorySeparatorChar)));
                sb.AppendLine();
            }

            sb.AppendLine("## Aufgabe");
            sb.AppendLine();
            sb.AppendLine(TaskText(type));

            return sb.ToString();
        }

        private static string TaskText(AgentType type)
        {
            switch (type)
            {
                case AgentType.Planung:
                    return "Lies zuerst CLAUDE.md und HANDOFF.md im Projekt. Schau dir die Screenshots an, falls vorhanden.\n" +
                           "Analysiere den bestehenden Code, der für dieses Feature relevant ist, und erstelle einen konkreten\n" +
                           "Umsetzungsplan mit Etappen. Noch nichts implementieren. Stelle Rückfragen, wenn etwas unklar ist.";

                case AgentType.Implementierung:
                    return "Lies zuerst CLAUDE.md und HANDOFF.md im Projekt. Schau dir die Screenshots an, falls vorhanden.\n" +
                           "Setze das Feature vollständig um und halte dich dabei an die Regeln aus CLAUDE.md\n" +
                           "(i18n in allen vier Sprachen, keine UI-Libraries, bestehende Komponenten wiederverwenden).\n" +
                           "Arbeite in Etappen und committe nach jeder abgeschlossenen Etappe.\n" +
                           "Führe am Ende die Checks aus (npm run check:design, npm run check:i18n, npm run build,\n" +
                           "dotnet test wenn das Backend berührt wurde) und aktualisiere HANDOFF.md.";

                case AgentType.Review:
                    return "Du bist ein unabhängiger Reviewer in einer frischen Session. Prüfe die Änderungen dieses Features\n" +
                           "gegenüber master: erst `git fetch origin master`, dann `git diff origin/master...HEAD`.\n" +
                           "Bewerte: Erfüllt der Code die Beschreibung vollständig? Gibt es Bugs, fehlende Fehlerbehandlung,\n" +
                           "fehlende Übersetzungen (de/en/uk/ru), fehlende Tests, Verstöße gegen CLAUDE.md?\n" +
                           "Ändere keinen Code. Liste die Findings nach Schwere sortiert mit Datei und Zeile auf.";

                case AgentType.Security:
                    return "Du bist ein Security-Reviewer in einer frischen Session. Prüfe die Änderungen dieses Features\n" +
                           "gegenüber master: erst `git fetch origin master`, dann `git diff origin/master...HEAD`.\n" +
                           "Achte besonders auf: Tenant-Isolation (salonId nur aus dem Auth-Claim, Ownership-Checks für\n" +
                           "alle Fremdschlüssel), Auth/Autorisierung der Endpunkte, Input-Validierung, Injection, XSS,\n" +
                           "Rate-Limits, Secrets im Code, unsichere Uploads, Logging personenbezogener Daten.\n" +
                           "Ändere keinen Code. Liste die Findings nach Schwere sortiert mit Datei und Zeile auf.";

                default:
                    return "";
            }
        }
    }
}
