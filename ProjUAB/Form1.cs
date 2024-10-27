using System;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace ProjUAB
{
    public partial class Form1 : Form
    {
        private DataSet dataTramesa;

        private DataSet dataNotes;

        private DataSet dataActivitats;

        public Form1()
        {
            InitializeComponent();
            dataTramesa = new DataSet();
            dataNotes = new DataSet();
            dataActivitats = new DataSet();


            InitializeDataGridView(dataGridView1);
            InitializeDataGridView(dataGridView2);

            string tramesesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "trameses.csv");
            string notesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "notes.csv");
            string activitatsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "activitats.csv");

            LoadDataFromCSV(tramesesPath, dataTable, dataTramesa, ',');
            LoadDataFromCSV(notesPath, dataTableNotas, dataNotes, ';');
            LoadDataFromCSV(activitatsPath, dataTableActivitats, dataActivitats, ',');

            // Cargar los datos sin retorno booleano
            // LoadDataFromCSV(@"C:\Users\Alex\Downloads\arxiu\trameses.csv", dataTable, dataTramesa, ',');
            // LoadDataFromCSV(@"C:\Users\Alex\Downloads\arxiu\notes.csv", dataTableNotas, dataNotes, ';');
            // LoadDataFromCSV(@"C:\Users\Alex\Downloads\arxiu\activitats.csv", dataTableActivitats, dataActivitats, ',');

            DisplayDataSetContents(dataNotes);

            LoadColumnCheckBoxes(checkedListBox1, dataTramesa, "UserData");
            LoadColumnCheckBoxes(checkedListBox2, dataNotes, "UserNotas");
        }

        // Configurar el DataGridView y sus propiedades
        private void InitializeDataGridView(DataGridView dataGridView)
        {
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView.ReadOnly = true;
        }


        DataTable dataTable = new DataTable("UserData");
        DataTable dataTableNotas = new DataTable("UserNotas");
        DataTable dataTableActivitats = new DataTable("Activitats");



        // Cargar datos y manejar excepciones y columnas vacías
        private void LoadDataFromCSV(string filePath, DataTable dataTable, DataSet writeDataSet, char separatorCHAR)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length > 0)
            {
                // Split the header to get column names
                var header = lines[0].Split(separatorCHAR);

                // Add columns to the DataTable based on the header
                foreach (var column in header)
                {
                    dataTable.Columns.Add(column.Trim().Replace("\"", ""));
                }

                // Loop through the rest of the lines in the CSV
                for (int i = 1; i < lines.Length; i++)
                {
                    var data = lines[i].Split(separatorCHAR);
                    // Create an array to hold the row values
                    var rowValues = new object[dataTable.Columns.Count];

                    // Populate the row values with data
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        // Trim and replace quotes
                        string cellValue = (j < data.Length) ? data[j].Trim().Replace("\"", "") : string.Empty;

                        // Convert Unix timestamps to readable date format for specific columns
                        if (dataTable.Columns[j].ColumnName == "datesubmitted" || dataTable.Columns[j].ColumnName == "dategraded" || dataTable.Columns[j].ColumnName == "P_Grade_Date" || dataTable.Columns[j].ColumnName == "F_Grade_Date" || dataTable.Columns[j].ColumnName == "R_Grade_Date")
                        {
                            if (long.TryParse(cellValue, out long unixTime) && unixTime > 0)
                            {
                                rowValues[j] = ConvertUnixTimeToDateTime(unixTime).ToString("dd/MM/yyyy HH:mm:ss");
                            }
                            else
                            {
                                rowValues[j] = cellValue; // Keep the original value if parsing fails
                            }
                        }
                        else
                        {
                            rowValues[j] = cellValue; // For other columns, keep the original value
                        }
                    }

                    // Add the row values to the DataTable
                    dataTable.Rows.Add(rowValues);
                }
            }

            // Add the DataTable to the DataSet
            if (writeDataSet != null)
            {
                writeDataSet.Tables.Add(dataTable);
            }

            // Display the number of rows loaded
            MessageBox.Show($"Filas cargadas en DataTable: {dataTable.Rows.Count}");
        }

        // Método para buscar datos de un usuario por su UserID
        private DataRow[] GetUserData(int userId, DataSet data, string dataName)
        {
            DataTable table = data.Tables[dataName];
            Console.WriteLine($"Buscando registros de {data.DataSetName} para UserID: {userId}"); // Línea de depuracion
            return table.Select($"userid = '{userId}'"); // Usa comillas para asegurar que se trate como string
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            int userId;
            if (txtUserId.Text != "" && int.TryParse(txtUserId.Text.ToString(), out userId))
            {
                SearchInData(userId, dataTramesa, dataGridView1, "UserData");
                SearchInData(userId, dataNotes, dataGridView2, "UserNotas");
            }
            else
            {
                MessageBox.Show("Inser an ID");
            }


        }

        private void SearchInData(int userId, DataSet dataSet, DataGridView dataGridView, string dataName)
        {
            //int userId;
            if (int.TryParse(userId.ToString(), out userId))
            {
                var userData = GetUserData(userId, dataSet, dataName);

                if (userData.Length > 0)
                {
                    // Crear un DataTable temporal para mostrar los resultados
                    DataTable userTable = userData.CopyToDataTable();
                    dataGridView.DataSource = userTable;
                }
                else
                {
                    MessageBox.Show("No se encontraron registros para este usuario.");
                }
            }
            else
            {
                MessageBox.Show("Por favor, ingrese un User ID válido.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //textBox1.Text = dataGridView1.SelectedCells.ToString();
        }
        private void LoadColumnCheckBoxes(CheckedListBox checkedListBox, DataSet dataSet, string dataTableName)
        {
            // Limpiar el CheckedListBox
            checkedListBox.Items.Clear();

            // Verificar que el DataSet y la tabla existan
            if (dataSet == null || !dataSet.Tables.Contains(dataTableName))
            {
                MessageBox.Show($"El DataSet no contiene la tabla {dataTableName}.");
                return;
            }

            // Obtener la tabla de datos
            DataTable table = dataSet.Tables[dataTableName];
            foreach (DataColumn column in table.Columns)
            {
                checkedListBox.Items.Add(column.ColumnName, true); // Agregar todas las columnas como seleccionadas
            }
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }


        private void btnShow_Click(object sender, EventArgs e)
        {
            ShowCheckedList(txtUserId, dataTramesa, dataGridView1, checkedListBox1, "UserData");
            ShowCheckedList(txtUserId, dataNotes, dataGridView2, checkedListBox2, "UserNotas");
            SearchInData(int.Parse(txtUserId.Text), dataTramesa, dataGridView1, "UserData");
            SearchInData(int.Parse(txtUserId.Text), dataNotes, dataGridView2, "UserNotas");

            List<decimal> valoresP_Grade = Sigma(dataNotes, "UserNotas", "P_Grade");
            List<decimal> valoresF_Grade = Sigma(dataNotes, "UserNotas", "P_Grade");
            List<decimal> valoresR_Grade = Sigma(dataNotes, "UserNotas", "P_Grade");

            List<decimal> valoresPracticas = Sigma(dataTramesa, "UserData", "grade");

            processSigma(valoresP_Grade, valoresF_Grade, valoresR_Grade);

            alphaLabel.Text = alpha.ToString();

            /*var (partialMean, finalMean, sumOfMeans) = CalculateGrades(valoresP_Grade, valoresF_Grade);

            Console.WriteLine($"Partial Mean: {partialMean}");
            Console.WriteLine($"Final Mean: {finalMean}");
            Console.WriteLine($"Sum of Means: {sumOfMeans}");
            */

        }

        private void ShowCheckedList(TextBox txtUserId, DataSet dataSet, DataGridView dataGridView, CheckedListBox checkedListBox, string dataName)
        {
            if (txtUserId.Text == "")
            {
                MessageBox.Show("Enter a userId before showing new columns");
            }

            else if (dataGridView.RowCount == 0)
            {
                MessageBox.Show("Search before showing");
            }
            else
            {
                // Obtener la tabla original
                DataTable originalTable = dataSet.Tables[dataName];

                // Limpiar las columnas del DataGridView
                foreach (DataGridViewColumn column in dataGridView.Columns)
                {
                    column.Visible = false; // Ocultar todas las columnas
                }

                // Mostrar solo las columnas seleccionadas
                foreach (var item in checkedListBox.CheckedItems)
                {
                    string columnName = item.ToString();
                    if (originalTable.Columns.Contains(columnName))
                    {
                        dataGridView.Columns[columnName].Visible = true; // Mostrar la columna seleccionada
                    }
                }

                // Reasignar el DataSource al DataGridView para refrescar la vista
                dataGridView.DataSource = originalTable;
            }
        }


        // Unix Time to readable date (aueaueeee)
        private DateTime ConvertUnixTimeToDateTime(long unixTime)
        {
            // Epoch de Unix es 1 de enero de 1970, 00:00:00 UTC
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime).ToLocalTime();
        }




        // DEBUG PRINT

        private void DisplayDataSetContents(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
            {
                Console.WriteLine("The DataSet is empty or null.");
                return;
            }

            // Loop through each DataTable in the DataSet
            foreach (DataTable table in dataSet.Tables)
            {
                Console.WriteLine($"Table: {table.TableName}");

                // Print the header
                foreach (DataColumn column in table.Columns)
                {
                    Console.Write($"{column.ColumnName}\t");
                }
                Console.WriteLine(); // New line after header

                // Loop through each DataRow in the DataTable
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        Console.Write($"{row[i]}\t");
                    }
                    Console.WriteLine(); // New line after each row
                }
            }
        }


        //Gets info from specific column and puts it on a list (NESTING TO BE SOLVED WITH INVERSION OF CONDITIONALS)
        private List<decimal> Sigma(DataSet dataSet, string tableName, string columnName)
        {
            List<decimal> valuesList = new List<decimal>();

            // Verificar si el DataSet contiene la tabla especificada
            if (dataSet.Tables.Contains(tableName))
            {
                DataTable table = dataSet.Tables[tableName];

                // Verificar si la tabla contiene la columna especificada
                if (table.Columns.Contains(columnName))
                {
                    // Recorrer todas las filas de la tabla y añadir los valores de la columna a la lista
                    foreach (DataRow row in table.Rows)
                    {
                        object cellValue = row[columnName];

                        // Comprobar que el valor no sea nulo ni vacío antes de intentar convertirlo
                        if (cellValue != DBNull.Value && !string.IsNullOrWhiteSpace(cellValue.ToString()))
                        {
                            // Intentar convertir el valor a decimal
                            if (decimal.TryParse(cellValue.ToString(), out decimal parsedValue))
                            {
                                valuesList.Add(parsedValue);
                            }
                            else
                            {
                                Console.WriteLine($"Valor no numérico o inválido encontrado en '{columnName}': {cellValue}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Valor nulo encontrado en '{columnName}' y será omitido.");
                        }
                    }
                    Console.WriteLine("Lista de valores procesados: " + string.Join(", ", valuesList)); // Imprimir valores de la lista
                }
                else
                {
                    MessageBox.Show($"La columna '{columnName}' no existe en la tabla '{tableName}'.");
                }
            }
            else
            {
                MessageBox.Show($"La tabla '{tableName}' no existe en el DataSet.");
            }

            return valuesList;
        }

        public decimal alpha = 1;

        public void processSigma(List<decimal> valuesP, List<decimal> valuesF, List<decimal> valuesR)
        {
            if (valuesR.Count != 0)
            {
                alpha = alpha - 0.05m;
            }

            //if (valuesP.Count == 0 || valuesF.Count == 0) {

            if (valuesP.Count != 0 || valuesF.Count != 0)
            {
                alpha = alpha + ((valuesF[valuesF.Count-1] - valuesP[valuesP.Count - 1]) / 10) * 0.75m;
            }

        }

        public static (decimal? PartialMean, decimal? FinalMean, decimal? SumOfMeans) CalculateGrades(List<decimal?> partialGrades, List<decimal?> finalGrades)
        {
            // Filter out null values and select the decimal values
            var validPartialGrades = partialGrades.Where(grade => grade.HasValue).Select(grade => grade.Value);
            var validFinalGrades = finalGrades.Where(grade => grade.HasValue).Select(grade => grade.Value);

            // Calculate the mean for both lists
            decimal? partialMean = validPartialGrades.Any() ? validPartialGrades.Average() : (decimal?)null;
            decimal? finalMean = validFinalGrades.Any() ? validFinalGrades.Average() : (decimal?)null;

            // Calculate the sum of both means, ensuring both are not null
            decimal? sumOfMeans = (partialMean.HasValue && finalMean.HasValue)
                ? (partialMean.Value + finalMean.Value)
                : (decimal?)null;

            return (partialMean, finalMean, sumOfMeans);
        }
    }
}


