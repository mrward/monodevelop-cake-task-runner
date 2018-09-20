// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// From: https://github.com/cake-build/cake-vs/tree/develop/src/TaskRunner

using Microsoft.VisualStudio.TaskRunnerExplorer;
using MonoDevelop.Core;

namespace Cake.VisualStudio.TaskRunner
{
    public class TaskRunnerOption : ITaskRunnerOption
    {
        public TaskRunnerOption(string name, bool isChecked, string command)
        {
            Name = name;
            Checked = isChecked;
            Command = command;
        }

        public bool Checked { get; set; }

        public string Name { get; }

        public string Command { get; set; }

        public string Description { get; set; }

        public IconId Icon { get; set; }
    }
}