namespace SchoolPC.Admin.Views
{
    partial class PcLogbookListView
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.ErrorLabel = new System.Windows.Forms.Label();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.FilterPanel = new System.Windows.Forms.Panel();
            this.ShowInactiveCheckbox = new System.Windows.Forms.CheckBox();
            this.PcGridView = new System.Windows.Forms.DataGridView();
            this.MainLayout.SuspendLayout();
            this.FilterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PcGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.BackColor = System.Drawing.Color.White;
            this.MainLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.MainLayout.ColumnCount = 1;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.ErrorLabel, 0, 0);
            this.MainLayout.Controls.Add(this.TitleLabel, 0, 1);
            this.MainLayout.Controls.Add(this.FilterPanel, 0, 2);
            this.MainLayout.Controls.Add(this.PcGridView, 0, 3);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.Padding = new System.Windows.Forms.Padding(15);
            this.MainLayout.RowCount = 4;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(891, 643);
            this.MainLayout.TabIndex = 0;
            // 
            // ErrorLabel
            // 
            this.ErrorLabel.AutoSize = false;
            this.ErrorLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.ErrorLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ErrorLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ErrorLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ErrorLabel.ForeColor = System.Drawing.Color.Red;
            this.ErrorLabel.Height = 60;
            this.ErrorLabel.Location = new System.Drawing.Point(15, 15);
            this.ErrorLabel.Name = "ErrorLabel";
            this.ErrorLabel.Padding = new System.Windows.Forms.Padding(5);
            this.ErrorLabel.Size = new System.Drawing.Size(861, 0);
            this.ErrorLabel.TabIndex = 0;
            this.ErrorLabel.Text = "";
            this.ErrorLabel.Visible = false;
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TitleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.TitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.TitleLabel.Height = 50;
            this.TitleLabel.Location = new System.Drawing.Point(15, 50);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(861, 50);
            this.TitleLabel.TabIndex = 1;
            this.TitleLabel.Text = "PC Logbook";
            // 
            // FilterPanel
            // 
            this.FilterPanel.AutoSize = false;
            this.FilterPanel.BackColor = System.Drawing.Color.White;
            this.FilterPanel.Controls.Add(this.ShowInactiveCheckbox);
            this.FilterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FilterPanel.Height = 35;
            this.FilterPanel.Location = new System.Drawing.Point(15, 100);
            this.FilterPanel.Name = "FilterPanel";
            this.FilterPanel.Size = new System.Drawing.Size(861, 35);
            this.FilterPanel.TabIndex = 2;
            // 
            // ShowInactiveCheckbox
            // 
            this.ShowInactiveCheckbox.AutoSize = true;
            this.ShowInactiveCheckbox.Checked = false;
            this.ShowInactiveCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ShowInactiveCheckbox.Location = new System.Drawing.Point(0, 8);
            this.ShowInactiveCheckbox.Name = "ShowInactiveCheckbox";
            this.ShowInactiveCheckbox.Size = new System.Drawing.Size(145, 19);
            this.ShowInactiveCheckbox.TabIndex = 0;
            this.ShowInactiveCheckbox.Text = "Show Inactive PCs";
            this.ShowInactiveCheckbox.UseVisualStyleBackColor = true;
            this.ShowInactiveCheckbox.CheckedChanged += new System.EventHandler(this.ShowInactiveCheckbox_CheckedChanged);
            // 
            // PcGridView
            // 
            this.PcGridView.AllowUserToAddRows = false;
            this.PcGridView.AllowUserToDeleteRows = false;
            this.PcGridView.AllowUserToOrderColumns = true;
            this.PcGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.PcGridView.BackgroundColor = System.Drawing.Color.White;
            this.PcGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.PcGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.PcGridView.ColumnHeadersHeight = 36;
            this.PcGridView.ColumnHeadersVisible = true;
            this.PcGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PcGridView.EnableHeadersVisualStyles = false;
            this.PcGridView.Location = new System.Drawing.Point(15, 135);
            this.PcGridView.MultiSelect = false;
            this.PcGridView.Name = "PcGridView";
            this.PcGridView.ReadOnly = true;
            this.PcGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.PcGridView.Size = new System.Drawing.Size(861, 493);
            this.PcGridView.TabIndex = 3;
            this.PcGridView.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.PcGridView_CellClick);
            this.PcGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.PcGridView_DoubleClick);
            // Grid columns will be added in code
            // 
            // PcLogbookListView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.MainLayout);
            this.Name = "PcLogbookListView";
            this.Size = new System.Drawing.Size(891, 643);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.FilterPanel.ResumeLayout(false);
            this.FilterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PcGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.Label ErrorLabel;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.Panel FilterPanel;
        private System.Windows.Forms.CheckBox ShowInactiveCheckbox;
        private System.Windows.Forms.DataGridView PcGridView;
    }
}
