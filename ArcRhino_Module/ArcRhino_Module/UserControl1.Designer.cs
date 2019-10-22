using Grasshopper.Kernel;
using Rhino.PlugIns;
using Rhino.Runtime.InProcess;
using System;
using System.Windows.Forms;

namespace ArcRhino_Module
{
   partial class UserControl1 : UserControl
   {
      internal static RhinoCore rhinoCore;
      static readonly Guid GrasshopperGuid = new Guid(0xB45A29B1, 0x4343, 0x4035, 0x98, 0x9E, 0x04, 0x4E, 0x85, 0x80, 0xD9, 0xCF);
      public static GH_Document definition;
      /// <summary> 
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary> 
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.viewportControl1 = new RhinoWindows.Forms.Controls.ViewportControl();
         this.SuspendLayout();
         // 
         // viewportControl1
         // 
         this.viewportControl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.viewportControl1.Location = new System.Drawing.Point(0, 0);
         this.viewportControl1.Name = "viewportControl1";
         this.viewportControl1.Size = new System.Drawing.Size(670, 453);
         this.viewportControl1.TabIndex = 0;
         this.viewportControl1.Text = "viewportControl1";
         // 
         // UserControl1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.viewportControl1);
         this.Name = "UserControl1";
         this.Size = new System.Drawing.Size(670, 453);
         this.ResumeLayout(false);

      }

      #endregion

      private RhinoWindows.Forms.Controls.ViewportControl viewportControl1;

      [STAThread]
      protected override void OnHandleCreated(EventArgs e)
      {
         if (rhinoCore == null)
         {
            // rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden, Handle);
            rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Normal);
            // TODO: determine whether to load automatically or on-demand
            LoadGH();
         }
         base.OnHandleCreated(e);
      }
      protected override void OnHandleDestroyed(EventArgs e)
      {
         if (definition != null)
         {
            definition.Dispose();
            definition = null;
         }
         if (rhinoCore != null)
         {
            rhinoCore.Dispose();
            rhinoCore = null;
         }
         base.OnHandleDestroyed(e);
      }

      public void LoadGH()
      {
         if (!PlugIn.LoadPlugIn(GrasshopperGuid))
            return;

         var script = new Grasshopper.Plugin.GH_RhinoScriptInterface();

         if (!script.IsEditorLoaded())
            script.LoadEditor();

         script.ShowEditor();

         if (definition == null)
            Grasshopper.Instances.DocumentServer.DocumentAdded += DocumentServer_DocumentAdded;
      }

      private void DocumentServer_DocumentAdded(GH_DocumentServer sender, GH_Document doc)
      {
         doc.SolutionEnd += Definition_SolutionEnd;
         definition = doc;
      }

      private void Definition_SolutionEnd(object sender, GH_SolutionEventArgs e)
      {
         if (Tag is Dockpane1View dock && dock.autoupdate.IsChecked == true)
         {
            GhUtil.showDocumentPreview(dock.rhinoDoc);
            // TODO: Add code to harvest display meshes when the Grasshopper Definition solution completes solving.
         }
      }
   }
}
