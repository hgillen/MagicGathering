namespace WebCrawler
{
   partial class Form1
   {
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
         this.urlText = new System.Windows.Forms.TextBox();
         this.alternateCheck = new System.Windows.Forms.CheckBox();
         this.bgCrawl = new System.ComponentModel.BackgroundWorker();
         this.goButton = new System.Windows.Forms.Button();
         this.progressBar1 = new System.Windows.Forms.ProgressBar();
         this.progressLabel = new System.Windows.Forms.Label();
         this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
         this.textSave = new System.Windows.Forms.TextBox();
         this.buttonSave = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // urlText
         // 
         this.urlText.Location = new System.Drawing.Point(12, 12);
         this.urlText.Name = "urlText";
         this.urlText.Size = new System.Drawing.Size(362, 20);
         this.urlText.TabIndex = 0;
         // 
         // alternateCheck
         // 
         this.alternateCheck.AutoSize = true;
         this.alternateCheck.Location = new System.Drawing.Point(380, 14);
         this.alternateCheck.Name = "alternateCheck";
         this.alternateCheck.Size = new System.Drawing.Size(115, 17);
         this.alternateCheck.TabIndex = 1;
         this.alternateCheck.Text = "Use Alternate URL";
         this.alternateCheck.UseVisualStyleBackColor = true;
         this.alternateCheck.CheckedChanged += new System.EventHandler(this.alternateCheck_CheckedChanged);
         // 
         // bgCrawl
         // 
         this.bgCrawl.WorkerReportsProgress = true;
         // 
         // goButton
         // 
         this.goButton.Enabled = false;
         this.goButton.Location = new System.Drawing.Point(12, 38);
         this.goButton.Name = "goButton";
         this.goButton.Size = new System.Drawing.Size(150, 52);
         this.goButton.TabIndex = 2;
         this.goButton.Text = "GO";
         this.goButton.UseVisualStyleBackColor = true;
         this.goButton.Click += new System.EventHandler(this.goButton_ClickAsync);
         // 
         // progressBar1
         // 
         this.progressBar1.Location = new System.Drawing.Point(12, 96);
         this.progressBar1.Maximum = 2;
         this.progressBar1.Minimum = 1;
         this.progressBar1.Name = "progressBar1";
         this.progressBar1.Size = new System.Drawing.Size(483, 27);
         this.progressBar1.TabIndex = 3;
         this.progressBar1.Value = 1;
         // 
         // progressLabel
         // 
         this.progressLabel.AutoSize = true;
         this.progressLabel.Location = new System.Drawing.Point(12, 126);
         this.progressLabel.Name = "progressLabel";
         this.progressLabel.Size = new System.Drawing.Size(0, 13);
         this.progressLabel.TabIndex = 4;
         // 
         // textSave
         // 
         this.textSave.Enabled = false;
         this.textSave.Location = new System.Drawing.Point(249, 40);
         this.textSave.Name = "textSave";
         this.textSave.Size = new System.Drawing.Size(244, 20);
         this.textSave.TabIndex = 5;
         // 
         // buttonSave
         // 
         this.buttonSave.Location = new System.Drawing.Point(168, 38);
         this.buttonSave.Name = "buttonSave";
         this.buttonSave.Size = new System.Drawing.Size(75, 23);
         this.buttonSave.TabIndex = 6;
         this.buttonSave.Text = "Save File";
         this.buttonSave.UseVisualStyleBackColor = true;
         this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(505, 146);
         this.Controls.Add(this.buttonSave);
         this.Controls.Add(this.textSave);
         this.Controls.Add(this.progressLabel);
         this.Controls.Add(this.progressBar1);
         this.Controls.Add(this.goButton);
         this.Controls.Add(this.alternateCheck);
         this.Controls.Add(this.urlText);
         this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
         this.Name = "Form1";
         this.Text = "MTG Card Searcher";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox urlText;
      private System.Windows.Forms.CheckBox alternateCheck;
      private System.ComponentModel.BackgroundWorker bgCrawl;
      private System.Windows.Forms.Button goButton;
      private System.Windows.Forms.ProgressBar progressBar1;
      private System.Windows.Forms.Label progressLabel;
      private System.Windows.Forms.SaveFileDialog saveFileDialog1;
      private System.Windows.Forms.TextBox textSave;
      private System.Windows.Forms.Button buttonSave;
   }
}

