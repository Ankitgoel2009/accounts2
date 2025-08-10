using accounts2.MenuPanels;

namespace accounts2
{
    partial class HomeScreen
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
            this.mainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            // 🔴 REMOVED: this.rightCenteringLayout = new System.Windows.Forms.TableLayoutPanel();
            // 🔴 ADD: Simple Panel for content
            this.rightContentPanel = new System.Windows.Forms.Panel();
            this.leftPanel = new System.Windows.Forms.Panel(); // 🔴 ADD: Left panel placeholder

            this.mainTableLayout.SuspendLayout();
            // 🔴 REMOVED: this.rightCenteringLayout.SuspendLayout();
            this.rightContentPanel.SuspendLayout(); // 🔴 ADD
            this.SuspendLayout();

            // 
            // mainTableLayout
            // 
            this.mainTableLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.mainTableLayout.ColumnCount = 2;
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            // 🔴 CHANGED: Add leftPanel first, then rightContentPanel
            this.mainTableLayout.Controls.Add(this.leftPanel, 0, 0); // 🔴 ADD
            this.mainTableLayout.Controls.Add(this.rightContentPanel, 1, 0); // 🔴 CHANGED
            this.mainTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayout.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayout.Name = "mainTableLayout";
            this.mainTableLayout.RowCount = 1;
            this.mainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayout.Size = new System.Drawing.Size(1478, 690);
            this.mainTableLayout.TabIndex = 0;

            // 
            // 🔴 REMOVED ENTIRE rightCenteringLayout SECTION
            // 
            // 🔴 ADD: leftPanel
            // 
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftPanel.Location = new System.Drawing.Point(4, 4);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(731, 682);
            this.leftPanel.TabIndex = 0;

            // 
            // 🔴 ADD: rightContentPanel (replaces rightCenteringLayout)
            // 
            this.rightContentPanel.AutoScroll = true; // 🔴 KEY: Allows scrolling for large panels
            this.rightContentPanel.Controls.Add(this.gatewayoftallypanel); // 🔴 Will be added in code
            this.rightContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightContentPanel.Location = new System.Drawing.Point(742, 4);
            this.rightContentPanel.Name = "rightContentPanel";
            this.rightContentPanel.Size = new System.Drawing.Size(732, 682);
            this.rightContentPanel.TabIndex = 0;

            // 
            // HomeScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(217)))));
            this.Controls.Add(this.mainTableLayout);
            this.Name = "HomeScreen";
            this.Size = new System.Drawing.Size(1478, 690);
            this.mainTableLayout.ResumeLayout(false);
            // 🔴 REMOVED: this.rightCenteringLayout.ResumeLayout(false);
            this.rightContentPanel.ResumeLayout(false); // 🔴 ADD
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayout;
        // 🔴 REMOVED: private System.Windows.Forms.TableLayoutPanel rightCenteringLayout;
        // 🔴 ADD: New panel for content
        private System.Windows.Forms.Panel rightContentPanel;
        private System.Windows.Forms.Panel leftPanel; // 🔴 ADD
        private GatewayofTallypanel gatewayoftallypanel; // 🔴 ADD (move from code-behind)
    }
}