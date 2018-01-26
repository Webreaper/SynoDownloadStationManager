using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using SynoDSManager.SynologyService;
using static SynoDSManager.SynologyService.DSService;

namespace SynoDSManager
{
    class MainClass
    {
        protected static bool isDeletable(SynoTask task, IList<string> trackersToKeep )
        {
            bool canDelete = false;

            if (task.status == "completed" || task.status == "seeding")
            {
                canDelete = true;

                if (trackersToKeep.Any(term => task.additional.tracker.Any(tracker => tracker.url.ToLower().Contains(term))))
                    canDelete = false;
            }

            return canDelete;
        }

        public static void Main(string[] args)
        {
            var settingPath = args.Where(p => p.EndsWith(".json", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (string.IsNullOrEmpty(settingPath))
                settingPath = "Settings.json";

            try
            {
                string json = File.ReadAllText(settingPath);

                var settings = Utils.deserializeJSON<Settings>(json);

                Utils.logLocation = settings.logLocation;

                DSService service = new DSService(settings.synology);

                Utils.Log("Signing in to DownloadStation:");

                if (service.SignIn() )
                {
                    Utils.Log("Getting Task list...");
                    var tasks = service.GetTasks().OrderBy(x => x.title);

                    var tasksToDelete = tasks.Where(x => isDeletable(x, settings.synology.trackersToKeep));
                    var tasksToKeep = tasks.Except(tasksToDelete);

                    if (tasksToKeep.Any())
                    {
                        Utils.Log("Tasks to Keep:");
                        foreach (var task in tasksToKeep)
                            Utils.Log($" - {task.title}");
                    }

                    if (tasksToDelete.Any())
                    {
                        Utils.Log("Tasks to delete:");
                        foreach (var task in tasksToDelete)
                            Utils.Log($" - {task.title}");

                        service.DeleteTask(tasksToDelete.Select(x => x.id).ToArray());

                        Utils.SendAlertEmail(settings.email, tasksToDelete);
                    }
                    else
                        Utils.Log("No tasks to delete");
                }
                else
                    Utils.Log("Login failed.");
            }
            catch( Exception ex )
            {
                Utils.Log("Error initialising. {0}", ex);
            }
        }
    }
}
