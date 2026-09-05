# AgentsDashboard

Kleines Blazor-Tool für die Salonary-Entwicklung. Zeigt alle Feature-Worktrees als Cards,
legt neue Worktrees vom `origin/master` an und startet VS Code, Claude/Codex-Terminals,
Review-/Security-/Planungs-/Implementierungs-Agenten und den Docker-Stack pro Feature.

## Starten

```
cd AgentsDashboard
dotnet run
```

Dann http://localhost:5157 öffnen.

## Konfiguration

Pfade zum Salonary-Repo und zum Worktree-Ordner stehen in `AgentsDashboard/appsettings.json`
unter `Salonary`.

Die Feature-Daten liegen in `AgentsDashboard/Data/features.json`, Screenshots unter
`wwwroot/screenshots`. Beides ist nicht im Git.

## Docker

Alle Worktrees nutzen dieselben Ports (Caddy 80/443 usw.). Beim Start eines Features werden
deshalb zuerst alle anderen laufenden Salonary-Compose-Projekte mit `docker compose down`
gestoppt, danach läuft `docker compose up -d --build` im Worktree.
