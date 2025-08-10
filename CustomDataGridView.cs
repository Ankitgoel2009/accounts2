using System;
using System.ComponentModel;
using System.Windows.Forms;

[ToolboxItem(true)]
[Description("Custom DataGridView with enhanced Tab/Enter navigation")]
public class CustomDataGridView : DataGridView
{
    public CustomDataGridView()
    {
        // Set default properties for better user experience
        this.AllowUserToAddRows = true;
        this.AllowUserToDeleteRows = true;
        this.SelectionMode = DataGridViewSelectionMode.CellSelect;
        this.MultiSelect = false;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // Handle Tab key
        if (keyData == Keys.Tab)
        {
            return HandleTabNavigation();
        }

        // Handle Enter key
        if (keyData == Keys.Enter)
        {
            return HandleEnterNavigation();
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private bool HandleTabNavigation()
    {
        if (this.CurrentCell == null) return false;

        int currentRow = this.CurrentCell.RowIndex;
        int currentCol = this.CurrentCell.ColumnIndex;

        // Move to next column in same row
        if (currentCol < this.Columns.Count - 1)
        {
            // Find next visible and non-readonly column
            for (int col = currentCol + 1; col < this.Columns.Count; col++)
            {
                if (this.Columns[col].Visible && !this.Columns[col].ReadOnly)
                {
                    this.CurrentCell = this.Rows[currentRow].Cells[col];
                    return true;
                }
            }
        }

        // If we reach here, we're at the last column or no more editable columns
        // Tab behavior: stay in current cell (standard DataGridView behavior)
        return true;
    }

    private bool HandleEnterNavigation()
    {
        if (this.CurrentCell == null) return false;

        int currentRow = this.CurrentCell.RowIndex;
        int currentCol = this.CurrentCell.ColumnIndex;

        // If we're at the last column, move to next row, first column
        if (currentCol == this.Columns.Count - 1 || IsLastEditableColumn(currentCol))
        {
            // Move to next row
            if (currentRow < this.Rows.Count - 1)
            {
                // Find first editable column in next row
                for (int col = 0; col < this.Columns.Count; col++)
                {
                    if (this.Columns[col].Visible && !this.Columns[col].ReadOnly)
                    {
                        this.CurrentCell = this.Rows[currentRow + 1].Cells[col];
                        return true;
                    }
                }
            }
            else if (this.AllowUserToAddRows)
            {
                // Add new row if allowed
                int newRowIndex = this.Rows.Add();
                for (int col = 0; col < this.Columns.Count; col++)
                {
                    if (this.Columns[col].Visible && !this.Columns[col].ReadOnly)
                    {
                        this.CurrentCell = this.Rows[newRowIndex].Cells[col];
                        return true;
                    }
                }
            }
        }
        else
        {
            // Move to next column in same row (same as Tab)
            return HandleTabNavigation();
        }

        return true;
    }

    private bool IsLastEditableColumn(int currentCol)
    {
        for (int col = currentCol + 1; col < this.Columns.Count; col++)
        {
            if (this.Columns[col].Visible && !this.Columns[col].ReadOnly)
            {
                return false;
            }
        }
        return true;
    }

    // Optional: Add custom properties for additional customization
    [Category("Custom Navigation")]
    [Description("Enable automatic row creation when Enter is pressed on last row")]
    [DefaultValue(true)]
    public bool AutoCreateRowOnEnter { get; set; } = true;

    [Category("Custom Navigation")]
    [Description("Skip read-only columns during navigation")]
    [DefaultValue(true)]
    public bool SkipReadOnlyColumns { get; set; } = true;
}

// Optional: Custom designer for additional design-time features
#if DESIGN_TIME
using System.ComponentModel.Design;

public class CustomDataGridViewDesigner : System.Windows.Forms.Design.ControlDesigner
{
    public override void Initialize(IComponent component)
    {
        base.Initialize(component);
        
        // Add any custom design-time behavior here
        if (component is CustomDataGridView grid)
        {
            // Set some default properties at design time
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
#endif