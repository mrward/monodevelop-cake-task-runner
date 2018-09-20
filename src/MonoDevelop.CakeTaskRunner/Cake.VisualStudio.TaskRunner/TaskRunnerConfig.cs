﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// From: https://github.com/cake-build/cake-vs/tree/develop/src/TaskRunner

using System;
using System.IO;
using Cake.VisualStudio.Configuration;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using Constants = Cake.VisualStudio.Helpers.Constants;
using MonoDevelop.Core;

namespace Cake.VisualStudio.TaskRunner
{
    class TaskRunnerConfig : ITaskRunnerConfig
    {
        private ITaskRunnerCommandContext _context;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, IconId icon)
        {
            _context = context;
            TaskHierarchy = hierarchy;
            Icon = icon;
        }

        public IconId Icon { get; }

        public ITaskRunnerNode TaskHierarchy { get; }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public string LoadBindings(string configPath)
        {
            string bindingPath = GetBindingPath(configPath) ?? configPath + ".bindings";

            return File.Exists(bindingPath) ? new ConfigurationParser(bindingPath).LoadBinding().ToXml() : "<binding />";
        }

        private string GetBindingPath(string configPath, bool create = false)
        {
            string bindingPath;
            //var path = CakePackage.Dte.Solution?.FindProjectItem (Constants.ConfigFileName);
            //if (path != null && path.FileCount == 1)
            //{
            //    bindingPath = path.FileNames[1];
            //}
            //else
            {
                var cpath = Path.Combine(Path.GetDirectoryName (configPath), Constants.ConfigFileName);
                try
                {

                    if (!File.Exists(cpath) && create) File.Create(cpath).Close ();
                    //if (File.Exists(cpath)) ProjectHelpers.GetSolutionItemsProject(CakePackage.Dte).AddFileToProject(cpath);
                }
                catch
                {
                    // ignored
                }
                bindingPath = cpath;
            }
            return string.IsNullOrWhiteSpace(bindingPath) ? null : bindingPath; // remove the empty string scenario
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            string bindingPath = GetBindingPath(configPath, true) ?? configPath + ".bindings";
            var config = new ConfigurationParser(bindingPath);
            try
            {
                //ProjectHelpers.CheckFileOutOfSourceControl(bindingPath);

                if (bindingsXml == "<binding />" && File.Exists(bindingPath))
                {
                    config.RemoveBindings ();
                }
                else
                {
                    config.SaveBinding(TaskBinding.FromXml (bindingsXml));
                    //ProjectHelpers.AddNestedFile(configPath, bindingPath);
                }

                //IVsPersistDocData persistDocData;
                //if (!CakePackage.IsDocumentDirty(configPath, out persistDocData) && persistDocData != null)
                //{
                //    int cancelled;
                //    string newName;
                //    persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out newName, out cancelled);
                //} else if (persistDocData == null) {
                //    new FileInfo(configPath).LastWriteTime = DateTime.Now;
                //}

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log (ex);
                return false;
            }
        }
    }
}