using Microsoft.EntityFrameworkCore;
using TeamCollabApp.TasksApi.Models;

namespace TeamCollabApp.TasksApi.Data
{
    public class TasksSeeder(TasksDbContext db)
    {
        public async Task SeedAsync()
        {
            if (await db.BoardColumns.AnyAsync()) return;

            // Resolve project IDs by querying the shared Projects table directly.
            // The MVC app owns that table but both apps share the same database.
            var projectIds = await db.Database
                .SqlQueryRaw<int>("SELECT Id FROM Projects ORDER BY Id")
                .ToListAsync();

            if (projectIds.Count == 0) return;

            // Seed columns and tasks for the first 5 projects.
            var projectsToSeed = projectIds.Take(5).ToList();

            var allAssignees = new List<TaskAssignee>();
            var allChecklistItems = new List<TaskChecklistItem>();

            // Fetch user IDs keyed by email prefix for explicit, order-independent lookup.
            var userIdsByEmail = await db.Database
                .SqlQueryRaw<UserEmailId>("SELECT CAST(Id AS NVARCHAR(450)) AS Id, Email FROM AspNetUsers")
                .ToListAsync();

            var userMap = userIdsByEmail.ToDictionary(
                u => u.Email.Split('@')[0].ToLower(),
                u => u.Id
            );

            // Helper to look up a user ID by first-name/email prefix.
            string User(string name) => userMap.TryGetValue(name.ToLower(), out var id) ? id : userMap.Values.First();

            var taskData = new (string ProjectName, (string Todo, string InProgress, string Done) Columns, (string Title, string? Desc, int Col, string[] Assignees, string[] Checklist)[] Tasks)[]
            {
                (
                    "Website Redesign",
                    ("To Do", "In Progress", "Done"),
                    [
                        ("Define colour palette",        "Pick brand colours and create a Figma swatch.",              0, [User("alex"), User("john")],  ["Gather existing brand assets", "Review competitor palettes"]),
                        ("Write homepage copy",          "Draft headline, subheadline and CTA text.",                  0, [User("jane")],                ["Interview stakeholders", "Three draft variants"]),
                        ("Set up CI pipeline",           "GitHub Actions workflow for build and deploy.",              1, [User("john")],                ["Create workflow file", "Configure secrets", "Test on PR"]),
                        ("Responsive nav prototype",     "Mobile-first navigation component in Figma.",                1, [User("alex"), User("jane")],  ["Wireframe", "Desktop breakpoint", "Accessibility pass"]),
                        ("Launch landing page",          "Push the redesigned homepage to production.",                2, [User("alex")],                ["Final QA", "Stakeholder sign-off", "Deploy"]),
                        ("SEO meta tags audit",          "Ensure all pages have correct titles and descriptions.",     2, [User("john")],                []),
                        ("Accessibility review",         "WCAG 2.1 AA compliance check across all pages.",            0, [User("jane")],                ["Screen reader test", "Colour contrast check"]),
                        ("Performance optimisation",     "Achieve Lighthouse score above 90 on all pages.",           1, [User("alex")],                ["Image compression", "Lazy loading", "Bundle analysis"]),
                        ("Analytics integration",        "Wire up GA4 events for all CTAs.",                          0, [User("john"), User("jane")],  ["Define event taxonomy", "Implement tags", "Verify in dashboard"]),
                        ("Redirect old URLs",            "301 redirects from legacy paths to new structure.",          2, [User("alex")],                ["Audit existing URLs", "Write redirect map"]),
                    ]
                ),
                (
                    "Mobile App v2",
                    ("Backlog", "In Progress", "Released"),
                    [
                        ("Onboarding flow redesign",     "New three-step onboarding with progress indicator.",         0, [User("alex"), User("joe")],   ["UX research", "Figma prototype", "Dev handoff"]),
                        ("Push notification service",    "Firebase Cloud Messaging integration.",                      1, [User("joe")],                 ["Configure FCM", "Handle foreground and background", "Deep link routing"]),
                        ("Offline mode for tasks",       "Cache task list locally for offline access.",                0, [User("alex")],                ["Choose storage strategy", "Sync on reconnect"]),
                        ("Dark mode support",            "System-aware dark mode across all screens.",                 1, [User("joe"), User("alex")],   ["Design tokens", "Platform-specific overrides"]),
                        ("App Store submission",         "Prepare build for iOS App Store review.",                    2, [User("alex")],                ["Screenshots", "Privacy policy", "Submit"]),
                        ("Crash reporting setup",        "Integrate Sentry for crash and error tracking.",             1, [User("joe")],                 ["Install SDK", "Configure DSN", "Test error capture"]),
                        ("Biometric authentication",     "Face ID and fingerprint login on supported devices.",        0, [User("alex")],                ["Research OS APIs", "Implement", "QA on devices"]),
                        ("Rate my app prompt",           "In-app review request after positive signal.",               0, [User("joe")],                 ["Define trigger logic", "Implement native prompt"]),
                        ("Localisation framework",       "i18n setup with English and French as first targets.",       1, [User("alex"), User("joe")],   ["Extract strings", "French translation", "RTL prep"]),
                        ("Beta TestFlight release",      "Distribute beta build to 50 internal testers.",              2, [User("joe")],                 ["Recruit testers", "Collect feedback"]),
                    ]
                ),
                (
                    "API Gateway Migration",
                    ("To Do", "In Progress", "Done"),
                    [
                        ("Inventory all endpoints",      "Document every service and route behind the old gateway.",   0, [User("john"), User("adam")],      ["Spreadsheet of routes", "Flag deprecated endpoints"]),
                        ("Set up Kong gateway",          "Install and configure Kong on the staging cluster.",         1, [User("adam")],                    ["Helm install", "Admin API smoke test"]),
                        ("Auth plugin configuration",    "JWT verification plugin on all authenticated routes.",       1, [User("john"), User("cassandra")],  ["Configure plugin", "Token rotation test"]),
                        ("Rate limiting rules",          "Apply per-consumer rate limits to prevent abuse.",           0, [User("adam")],                    ["Define limits per tier", "Configure plugin", "Load test"]),
                        ("Traffic cutover plan",         "Staged rollout from old to new gateway with rollback plan.", 0, [User("john")],                    ["Draft runbook", "Dry run on staging", "Schedule maintenance window"]),
                        ("Logging and tracing",          "Centralise request logs and add distributed tracing.",       1, [User("cassandra")],               ["OpenTelemetry setup", "Trace propagation headers"]),
                        ("Legacy gateway decommission",  "Shut down old gateway after 30 days of stable operation.",  2, [User("john")],                    ["Confirm zero traffic", "Terminate instances"]),
                        ("Developer portal update",      "Update internal docs with new base URLs and auth flow.",     0, [User("adam"), User("cassandra")],  ["Swagger update", "Changelog entry"]),
                        ("Load test new gateway",        "Simulate peak traffic and verify latency targets.",          2, [User("john")],                    ["k6 script", "Run 10k RPS test", "Review results"]),
                        ("Service mesh evaluation",      "Assess whether Istio is needed alongside the gateway.",      0, [User("cassandra")],               ["Compare options", "Decision record"]),
                    ]
                ),
                (
                    "Design System",
                    ("To Do", "In Progress", "Done"),
                    [
                        ("Token taxonomy decision",      "Agree on naming conventions for colour, spacing, type.",     2, [User("jane"), User("alex")],  ["Research conventions", "Workshop with team"]),
                        ("Button component",             "Primary, secondary, ghost, danger variants with states.",    2, [User("jane")],                ["Design", "Storybook story", "Accessibility"]),
                        ("Form input component",         "Text, number, select, textarea with validation states.",     1, [User("alex")],                ["Design all states", "Implement", "Document"]),
                        ("Icon library",                 "Source and integrate a consistent icon set.",                1, [User("jane"), User("sam")],   ["Evaluate libraries", "Select 80 core icons", "Publish"]),
                        ("Typography scale",             "Define heading and body type ramp for web and mobile.",      2, [User("jane")],                ["Type specimen", "Token definitions"]),
                        ("Colour palette",               "Full semantic palette including dark mode variants.",         1, [User("alex"), User("jane")],  ["Figma swatches", "CSS custom properties"]),
                        ("Spacing system",               "4px base grid, named scale tokens.",                         0, [User("jane")],                ["Define scale", "Document usage rules"]),
                        ("Card component",               "Content card with header, body, footer, and media slot.",    0, [User("alex")],                ["Design variants", "Implement", "Storybook"]),
                        ("Modal and drawer",             "Accessible overlay components with focus trapping.",          0, [User("jane"), User("alex")],  ["Design", "Implement", "ARIA roles"]),
                        ("Component documentation site", "Static site with live examples for every component.",        0, [User("sam")],                 ["Choose framework", "Write first five pages"]),
                    ]
                ),
                (
                    "Data Pipeline",
                    ("Backlog", "Building", "Shipped"),
                    [
                        ("Source connector for CRM",     "Ingest contact and deal data from Salesforce nightly.",      2, [User("karen"), User("kevin")],  ["Auth setup", "Field mapping", "Incremental sync"]),
                        ("Raw to staging transform",      "dbt models to clean and standardise raw ingestion tables.",  1, [User("kevin")],                ["Write models", "Add tests", "Schedule"]),
                        ("Staging to warehouse load",     "Load transformed data into BigQuery fact and dim tables.",   1, [User("karen"), User("kevin")],  ["Schema design", "Partition strategy", "Load job"]),
                        ("Data quality tests",           "Great Expectations suite for all critical tables.",          0, [User("karen")],                ["Define expectations", "CI integration"]),
                        ("Orchestration with Airflow",   "DAG for the full nightly pipeline with retries.",            1, [User("kevin")],                ["Write DAG", "Test backfill", "Set up alerts"]),
                        ("Executive KPI dashboard",      "Looker dashboard fed by the warehouse aggregates.",          2, [User("karen")],                ["Agree metrics", "Build explores", "Publish"]),
                        ("Real-time event streaming",     "Kafka topic for user activity events from the app.",         0, [User("kevin"), User("karen")],  ["Topic design", "Producer integration", "Consumer service"]),
                        ("Data retention policy",        "Archive and purge rules for GDPR compliance.",               0, [User("karen")],                ["Legal review", "Implement TTL rules"]),
                        ("Pipeline monitoring",          "Alerts for failed runs, stale data, and row count anomalies.",2, [User("kevin")],               ["Define SLOs", "Implement monitors"]),
                        ("Self-serve query layer",        "Expose curated datasets via a read-only Postgres replica.",  0, [User("karen"), User("kevin")],  ["Provision replica", "IAM setup", "Docs"]),
                    ]
                ),
            };

            for (int projectIndex = 0; projectIndex < Math.Min(taskData.Length, projectsToSeed.Count); projectIndex++)
            {
                var projectId = projectsToSeed[projectIndex];
                var data = taskData[projectIndex];

                var todo       = new BoardColumn { ProjectId = projectId, Title = data.Columns.Todo,       Position = 0 };
                var inProgress = new BoardColumn { ProjectId = projectId, Title = data.Columns.InProgress, Position = 1 };
                var done       = new BoardColumn { ProjectId = projectId, Title = data.Columns.Done,       Position = 2 };

                db.BoardColumns.AddRange(todo, inProgress, done);
            }

            await db.SaveChangesAsync();

            // Re-fetch so columns have their generated IDs
            var seededColumns = await db.BoardColumns
                .Where(c => projectsToSeed.Contains(c.ProjectId))
                .OrderBy(c => c.ProjectId).ThenBy(c => c.Position)
                .ToListAsync();

            for (int projectIndex = 0; projectIndex < Math.Min(taskData.Length, projectsToSeed.Count); projectIndex++)
            {
                var projectId = projectsToSeed[projectIndex];
                var data = taskData[projectIndex];
                var cols = seededColumns.Where(c => c.ProjectId == projectId).OrderBy(c => c.Position).ToList();
                if (cols.Count < 3) continue;

                var columnCounters = new int[3];

                foreach (var (title, desc, colIndex, assignees, checklist) in data.Tasks)
                {
                    var task = new BoardTask
                    {
                        ColumnId    = cols[colIndex].Id,
                        Title       = title,
                        Description = desc,
                        Position    = columnCounters[colIndex]++,
                        CreatedAt   = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 20))
                    };
                    db.BoardTasks.Add(task);
                }
            }

            await db.SaveChangesAsync();

            // Re-fetch tasks to get their IDs, then add assignees and checklist items
            var seededTasks = await db.BoardTasks
                .Include(t => t.Column)
                .Where(t => projectsToSeed.Contains(t.Column.ProjectId))
                .OrderBy(t => t.Column.ProjectId).ThenBy(t => t.ColumnId).ThenBy(t => t.Position)
                .ToListAsync();

            for (int projectIndex = 0; projectIndex < Math.Min(taskData.Length, projectsToSeed.Count); projectIndex++)
            {
                var data = taskData[projectIndex];
                var projectId = projectsToSeed[projectIndex];
                var projectTasks = seededTasks.Where(t => t.Column.ProjectId == projectId)
                    .OrderBy(t => t.ColumnId).ThenBy(t => t.Position).ToList();

                // Match task data entries to their saved tasks by position within each column
                var tasksByCol = new Dictionary<int, Queue<BoardTask>>();
                foreach (var t in projectTasks)
                {
                    if (!tasksByCol.ContainsKey(t.ColumnId))
                        tasksByCol[t.ColumnId] = new Queue<BoardTask>();
                    tasksByCol[t.ColumnId].Enqueue(t);
                }

                var cols = seededColumns.Where(c => c.ProjectId == projectId).OrderBy(c => c.Position).ToList();

                foreach (var (title, desc, colIndex, assignees, checklist) in data.Tasks)
                {
                    if (cols.Count <= colIndex) continue;
                    var colId = cols[colIndex].Id;
                    if (!tasksByCol.TryGetValue(colId, out var queue) || queue.Count == 0) continue;
                    var savedTask = queue.Dequeue();

                    foreach (var uid in assignees)
                        allAssignees.Add(new TaskAssignee { TaskId = savedTask.Id, UserId = uid });

                    for (int ci = 0; ci < checklist.Length; ci++)
                        allChecklistItems.Add(new TaskChecklistItem
                        {
                            TaskId     = savedTask.Id,
                            Text       = checklist[ci],
                            IsComplete = ci == 0 && colIndex == 2,
                            Position   = ci
                        });
                }
            }

            db.TaskAssignees.AddRange(allAssignees);
            db.TaskChecklistItems.AddRange(allChecklistItems);
            await db.SaveChangesAsync();
        }
    }

    file record UserEmailId(string Id, string Email);
}
