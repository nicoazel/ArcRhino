using Rhino.Runtime.InProcess;
using System;
using System.Windows.Forms;

namespace ArcRhino_Module
{
   public partial class Form1 : Form
   {
      private RhinoCore rhinoCore;

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

      #region Windows Form Designer generated code

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
         this.viewportControl1.Size = new System.Drawing.Size(800, 450);
         this.viewportControl1.TabIndex = 0;
         this.viewportControl1.Text = "viewportControl1";
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(800, 450);
         this.Controls.Add(this.viewportControl1);
         this.Name = "Form1";
         this.Text = "Form1";
         this.ResumeLayout(false);

      }

      #endregion

      private RhinoWindows.Forms.Controls.ViewportControl viewportControl1;

      protected override void OnHandleCreated(EventArgs e)
      {
         rhinoCore = new Rhino.Runtime.InProcess.RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden, Handle);
         base.OnHandleCreated(e);
      }
      protected override void OnHandleDestroyed(EventArgs e)
      {
         rhinoCore.Dispose();
         rhinoCore = null;
         base.OnHandleDestroyed(e);
      }
   }
}