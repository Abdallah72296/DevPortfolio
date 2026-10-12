namespace MyPortfolio.Application.Common.Helpers;

public static class SkillIconResolver
{
    private static readonly Dictionary<string, string> ExactMatches = new(StringComparer.OrdinalIgnoreCase)
    {
        { "c#", "devicon-csharp-plain" },
        { "csharp", "devicon-csharp-plain" },
        { ".net", "devicon-dotnetcore-plain" },
        { ".net core", "devicon-dotnetcore-plain" },
        { "asp.net", "devicon-dotnetcore-plain" },
        { "asp.net core", "devicon-dotnetcore-plain" },
        { "dotnet", "devicon-dotnetcore-plain" },
        { "entity framework", "devicon-dotnetcore-plain" },
        { "ef core", "devicon-dotnetcore-plain" },
        { "sql server", "devicon-microsoftsqlserver-plain" },
        { "mssql", "devicon-microsoftsqlserver-plain" },
        { "postgresql", "devicon-postgresql-plain" },
        { "postgres", "devicon-postgresql-plain" },
        { "mysql", "devicon-mysql-plain" },
        { "mongodb", "devicon-mongodb-plain" },
        { "mongo", "devicon-mongodb-plain" },
        { "redis", "devicon-redis-plain" },
        { "rabbitmq", "devicon-rabbitmq-plain" },
        { "kafka", "devicon-apachekafka-original" },
        { "docker", "devicon-docker-plain" },
        { "kubernetes", "devicon-kubernetes-plain" },
        { "k8s", "devicon-kubernetes-plain" },
        { "git", "devicon-git-plain" },
        { "github", "bi bi-github" },
        { "gitlab", "devicon-gitlab-plain" },
        { "angular", "devicon-angularjs-plain" },
        { "angularjs", "devicon-angularjs-plain" },
        { "react", "devicon-react-original" },
        { "react.js", "devicon-react-original" },
        { "reactjs", "devicon-react-original" },
        { "vue", "devicon-vuejs-plain" },
        { "vue.js", "devicon-vuejs-plain" },
        { "vuejs", "devicon-vuejs-plain" },
        { "typescript", "devicon-typescript-plain" },
        { "ts", "devicon-typescript-plain" },
        { "javascript", "devicon-javascript-plain" },
        { "js", "devicon-javascript-plain" },
        { "html", "devicon-html5-plain" },
        { "html5", "devicon-html5-plain" },
        { "css", "devicon-css3-plain" },
        { "css3", "devicon-css3-plain" },
        { "sass", "devicon-sass-original" },
        { "scss", "devicon-sass-original" },
        { "tailwind", "devicon-tailwindcss-plain" },
        { "tailwindcss", "devicon-tailwindcss-plain" },
        { "bootstrap", "devicon-bootstrap-plain" },
        { "node", "devicon-nodejs-plain" },
        { "node.js", "devicon-nodejs-plain" },
        { "nodejs", "devicon-nodejs-plain" },
        { "express", "devicon-express-original" },
        { "express.js", "devicon-express-original" },
        { "nest", "devicon-nestjs-plain" },
        { "nestjs", "devicon-nestjs-plain" },
        { "python", "devicon-python-plain" },
        { "django", "devicon-django-plain" },
        { "fastapi", "devicon-fastapi-plain" },
        { "flask", "devicon-flask-original" },
        { "java", "devicon-java-plain" },
        { "spring", "devicon-spring-plain" },
        { "spring boot", "devicon-spring-plain" },
        { "go", "devicon-go-original-wordmark" },
        { "golang", "devicon-go-original-wordmark" },
        { "rust", "devicon-rust-plain" },
        { "php", "devicon-php-plain" },
        { "laravel", "devicon-laravel-plain" },
        { "c++", "devicon-cplusplus-plain" },
        { "cpp", "devicon-cplusplus-plain" },
        { "c", "devicon-c-plain" },
        { "aws", "devicon-amazonwebservices-original" },
        { "amazon web services", "devicon-amazonwebservices-original" },
        { "azure", "devicon-azure-plain" },
        { "gcp", "devicon-googlecloud-plain" },
        { "google cloud", "devicon-googlecloud-plain" },
        { "linux", "devicon-linux-plain" },
        { "ubuntu", "devicon-ubuntu-plain" },
        { "nginx", "devicon-nginx-original" },
        { "apache", "devicon-apache-plain" },
        { "graphql", "devicon-graphql-plain" },
        { "next.js", "devicon-nextjs-plain" },
        { "nextjs", "devicon-nextjs-plain" },
        { "nuxt", "devicon-nuxtjs-plain" },
        { "nuxtjs", "devicon-nuxtjs-plain" },
        { "svelte", "devicon-svelte-plain" },
        { "figma", "devicon-figma-plain" },
        { "jira", "devicon-jira-plain" },
        { "trello", "devicon-trello-plain" },
        { "postman", "devicon-postman-plain" },
        { "swagger", "bi bi-braces-asterisk" },
        { "firebase", "devicon-firebase-plain" },
        { "supabase", "devicon-supabase-plain" },
        { "elasticsearch", "devicon-elasticsearch-plain" },
        { "jenkins", "devicon-jenkins-plain" },
        { "terraform", "devicon-terraform-plain" },
        { "ansible", "devicon-ansible-plain" },
        { "dart", "devicon-flutter-plain" },
        { "flutter", "devicon-flutter-plain" },
        { "android", "devicon-android-plain" },
        { "swift", "devicon-swift-plain" },
        { "ios", "devicon-apple-original" },
        { "sqlite", "devicon-sqlite-plain" },
        { "oracle", "devicon-oracle-original" },
        { "webpack", "devicon-webpack-plain" },
        { "vite", "devicon-vitejs-plain" },
        { "jest", "devicon-jest-plain" }
    };

    public static string Resolve(string? skillName, string? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(skillName))
            return fallback ?? "bi bi-code-slash";

        var clean = skillName.Trim().ToLowerInvariant();

        if (ExactMatches.TryGetValue(clean, out var exactIcon))
            return exactIcon;

        // Substring / keyword heuristics
        foreach (var kvp in ExactMatches)
        {
            if (clean.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        if (clean.Contains("sql") || clean.Contains("data") || clean.Contains("db"))
            return "bi bi-database";

        if (clean.Contains("cloud") || clean.Contains("deploy") || clean.Contains("devops") || clean.Contains("server"))
            return "bi bi-cloud-check";

        if (clean.Contains("api") || clean.Contains("microservice") || clean.Contains("rest"))
            return "bi bi-cpu";

        if (clean.Contains("test") || clean.Contains("qa") || clean.Contains("unit"))
            return "bi bi-check2-circle";

        if (clean.Contains("security") || clean.Contains("auth") || clean.Contains("jwt"))
            return "bi bi-shield-lock-fill";

        if (clean.Contains("design") || clean.Contains("ui") || clean.Contains("ux"))
            return "bi bi-palette";

        return !string.IsNullOrWhiteSpace(fallback) ? fallback : "bi bi-code-slash";
    }
}
