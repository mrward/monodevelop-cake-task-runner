﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// From: https://github.com/cake-build/cake-vs/tree/develop/src/TaskRunner

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using MonoDevelop.Core;

namespace Cake.VisualStudio.TaskRunner
{
    [TaskRunnerExport(Constants.ScriptFileName)]
    class TaskRunner : ITaskRunner
    {
        private static IconId _icon = new IconId("md-cake-task-runner");
        private List<ITaskRunnerOption> _options = null;
        private string _executablePath;

        private void InitializeCakeRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>
            {
            //    new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidCakePackageCmdSet, false,
            //        "-Verbosity=\"Diagnostic\""),
            //    new TaskRunnerOption("Debug", PackageIds.cmdDebug, PackageGuids.guidCakePackageCmdSet, false, "-debug"),
            //    new TaskRunnerOption("Dry Run", PackageIds.cmdDryRun, PackageGuids.guidCakePackageCmdSet, false, "-dryrun"),
            //    new TaskRunnerOption("Experimental", PackageIds.cmdExperimental, PackageGuids.guidCakePackageCmdSet, false, "-experimental")
            };
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeCakeRunnerOptions();
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            return await Task.Run(() =>
            {
                var hierarchy = LoadTasks(configPath);

                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadTasks(string configPath)
        {
            var cwd = Path.GetDirectoryName(configPath);
            _executablePath = GetCakePath(cwd);
            return string.IsNullOrWhiteSpace(_executablePath) ? NotFoundNode(configPath) : LoadHierarchy(configPath);
        }

        private ITaskRunnerNode NotFoundNode(string configPath)
        {
            var root = new TaskRunnerNode("Cake");

            var message = GettextCatalog.GetString("Could not find Cake.exe in local folder or in PATH");
            Logger.Log(message);
            //CakePackage.Dte.ShowStatusBarText(message);
            var node = new TaskRunnerNode(GettextCatalog.GetString("Cake.exe not found"), true)
            {
                Description = message,
                Command = new TaskRunnerCommand(Path.GetDirectoryName(configPath), "echo", message),
            };

            root.Children.Add(node);
            /*
             * return new TaskRunnerNode("Cake")
            {
                Children =
                {
                    new TaskRunnerNode("Cake.exe not found", true)
                    {
                        Description = message,
                        Command = new TaskRunnerCommand(Path.GetDirectoryName(configPath), "echo", message),
                    }
                }
            };
            */
            return root;
        }

        private ITaskRunnerNode LoadHierarchy (string configPath)
        {
            var configFileName = Path.GetFileName(configPath);
            var cwd = Path.GetDirectoryName(configPath);

            ITaskRunnerNode root = new TaskRunnerNode("Cake");

            // Build
            var buildDev = CreateTask(cwd, $"Default ({configFileName})", GettextCatalog.GetString("Runs 'cake build.cake'"), configFileName);
            var tasks = TaskParser.LoadTasks(configPath);
            var commands =
                tasks.Select(
                    t =>
                        CreateTask(cwd, t.Key, $"Runs {configFileName} with the \"{t.Key}\" target",
                            buildDev.Command.Args + $" {t.Value}"));
            var nodes = commands as IList<TaskRunnerNode> ?? commands.ToList();
            buildDev.Children.AddRange(nodes);
            root.Children.Add(buildDev);
            //CakePackage.Dte.ShowStatusBarText($"Loaded {nodes.Count} tasks from {configFileName}");
            return root;
        }

        private TaskRunnerNode CreateTask(string cwd, string name, string desc, string args)
        {
            var task = new TaskRunnerNode(name, true)
            {
                Description = desc,
                Command = GetCommand(cwd, args)
            };

            //ApplyOverrides(task);

            return task;
        }

        private ITaskRunnerCommand GetCommand(string cwd, string arguments)
        {
            ITaskRunnerCommand command = new TaskRunnerCommand(cwd, _executablePath, arguments);

            return command;
        }

        private static string GetCakePath(string cwd)
        {
            var knownPaths = new[] { "tools/Cake/Cake.exe", "Cake/Cake.exe", "Cake.exe" };
            foreach (var path in knownPaths)
            {
                var fullPath = Path.Combine(cwd, path);
                if (File.Exists(fullPath)) return fullPath;
            }
            if (PathHelpers.ExistsOnPath("cake.exe") || PathHelpers.ExistsOnPath("cake"))
            {
                return "cake"; // assume PATH
            }
            return null;
        }

        private static string GetExecutableFolder()
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(assembly);
        }
    }
}