// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// From: https://github.com/cake-build/cake-vs/tree/develop/src/Helpers

using System;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace Cake.VisualStudio.Helpers
{
    public static class Logger
    {
         public static void Log(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            TaskRunnerLogger.WriteLine(DateTime.Now.ToString () + ": " + message);
        }

        public static void Log(Exception ex)
        {
            if (ex != null)
            {
                Log(ex.ToString ());
            }
        }
    }
}