using System.Text.Json;
using AgentsDashboard.Models;

namespace AgentsDashboard.Services
{
    public class FeatureStore
    {
        private readonly string dataDir;
        private readonly string filePath;
        private readonly string screenshotDir;
        private List<Feature> features = new();

        public FeatureStore(IWebHostEnvironment env)
        {
            dataDir = Path.Combine(env.ContentRootPath, "Data");
            filePath = Path.Combine(dataDir, "features.json");
            screenshotDir = Path.Combine(env.WebRootPath, "screenshots");
            Load();
        }

        public List<Feature> GetAll()
        {
            return features.OrderByDescending(f => f.CreatedAt).ToList();
        }

        public Feature? GetById(string id)
        {
            return features.FirstOrDefault(f => f.Id == id);
        }

        public Feature? GetByWorktree(string worktreePath)
        {
            return features.FirstOrDefault(f => string.Equals(f.WorktreePath, worktreePath, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(Feature feature)
        {
            features.Add(feature);
            Save();
        }

        public void Remove(Feature feature)
        {
            features.Remove(feature);
            Save();
        }

        public void Save()
        {
            Directory.CreateDirectory(dataDir);
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(features, options));
        }

        public async Task<string> SaveScreenshot(string featureId, string fileName, Stream content)
        {
            var dir = Path.Combine(screenshotDir, featureId);
            Directory.CreateDirectory(dir);

            var safeName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(fileName)}";
            var target = Path.Combine(dir, safeName);

            await using var file = File.Create(target);
            await content.CopyToAsync(file);

            return $"screenshots/{featureId}/{safeName}";
        }

        private void Load()
        {
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            features = JsonSerializer.Deserialize<List<Feature>>(json) ?? new();
        }
    }
}
