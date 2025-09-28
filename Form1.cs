using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ToDoList_SQLv1
{
    public partial class Form1 : Form
    {
        private string connectionString =
                "Data Source=.\\SQLEXPRESS;Initial Catalog=ToDoDB;Integrated Security=True;";
       
        private DataTable taskTable;
        private bool isEditing = false;
        private int idTemp = 0;
        public Form1()
        {
            InitializeComponent();
            loadTask();
            dataGridTareas.ClearSelection();
            dataGridTareas.MultiSelect = false;
            dataGridTareas.DataBindingComplete += DataGridTareas_DataBindingComplete;
            dataGridTareas.Columns["Descripcion"].ReadOnly = true;
        }
        private void DataGridTareas_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridTareas.ClearSelection(); // quita selección al terminar el binding
        }

        private void loadTask()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT Id, Descripcion, IsCompleted FROM Tareas", conn);
                taskTable = new DataTable();
                da.Fill(taskTable);
                dataGridTareas.DataSource = taskTable;
                //dataGridTareas.Columns["Id"].Visible = false;
                dataGridTareas.AllowUserToAddRows = false;
                
            }
            if (dataGridTareas.Columns["IsCompleted"] is DataGridViewCheckBoxColumn == false) 
            {
                dataGridTareas.Columns["IsCompleted"].ReadOnly = false;
            }
        }
        private void btnBorrar_Click(object sender, EventArgs e)
        {
            txtTareas.Text = "";
            isEditing = false;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtTareas.Text) && isEditing == false)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("INSERT INTO Tareas (Descripcion) VALUES (@desc)", conn);
                    cmd.Parameters.AddWithValue("@desc", txtTareas.Text);
                    cmd.ExecuteNonQuery();
                }
                txtTareas.Clear();
                loadTask();
            }
            else if (isEditing == true)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE Tareas SET Descripcion=@desc WHERE Id=@id",conn);
                    cmd.Parameters.AddWithValue("@desc",txtTareas.Text);
                    cmd.Parameters.AddWithValue("@id", idTemp);
                    cmd.ExecuteNonQuery();
                }
                txtTareas.Clear();
                loadTask();
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dataGridTareas.CurrentRow != null) 
            {
                DialogResult result = MessageBox.Show(
                    "Esta seguro que desea elimiar?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes) 
                {
                    int id = (int)dataGridTareas.CurrentRow.Cells["Id"].Value;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Tareas where Id=@desc", conn);
                        cmd.Parameters.AddWithValue("@desc", id);
                        cmd.ExecuteNonQuery();
                    }
                    loadTask();
                }
            }
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (dataGridTareas.CurrentRow != null)
            {
                string tareaSeleccionada = (string)dataGridTareas.CurrentRow.Cells["Descripcion"].Value;
                txtTareas.Text = tareaSeleccionada;
                isEditing = true;
                idTemp = (int)dataGridTareas.CurrentRow.Cells["Id"].Value;
            }
        }

        private void dataGridTareas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0 && dataGridTareas.Columns[e.ColumnIndex].Name == "IsCompleted") 
            {
                DataGridViewRow row = dataGridTareas.Rows[e.RowIndex];
                bool currentValue = (bool)row.Cells["IsCompleted"].Value;

                DialogResult result = MessageBox.Show(!currentValue ? "Esta seguro que ha completado la tarea?" :
                    "Esta seguro que quiere desmarcar la tarea?", "Confirmar cambio",
                     MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes) 
                {
                    int id = (int)row.Cells["Id"].Value;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Tareas SET IsCompleted=@completada WHERE Id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@completada", !currentValue);
                        cmd.ExecuteNonQuery();
                    }
                    row.Cells["IsCompleted"].Value = currentValue;
                }
                else if (result == DialogResult.No) 
                {
                    loadTask();                   
                }
            }
        }
    }
}

