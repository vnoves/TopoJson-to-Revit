using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Timers;

namespace RoomsToTopoJson
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        static AddInId appId = new AddInId(new Guid("3256F49C-7F77-4737-8992-3F1CF468BE9B"));

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {



                Autodesk.Revit.UI.UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Document doc = uidoc.Document;
                MainForm mainForm = new MainForm(commandData);
                Process process = Process.GetCurrentProcess();
                IntPtr h = process.MainWindowHandle;
                mainForm.Topmost = true;
                mainForm.ShowDialog();
                return Result.Succeeded;
            

        }


    }
}