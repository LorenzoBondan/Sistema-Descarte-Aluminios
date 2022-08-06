using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace Descarte_Aluminios
{
    public partial class frmFornecedores : Form
    {
        public frmFornecedores()
        {
            InitializeComponent();
        }

        private void frmFornecedores_Load(object sender, EventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + ";Password = '1234'";
            SqlCeEngine db = new SqlCeEngine(strConnection);
            if (!File.Exists(baseDados))
            {
                db.CreateDatabase();
            }
            db.Dispose();
            SqlCeConnection conexao = new SqlCeConnection();
            conexao.ConnectionString = strConnection;
            try
            {
                conexao.Open();
                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                comando.CommandText = "CREATE TABLE tabelacadastroaluminios (codigo INT NOT NULL PRIMARY KEY, descricao NVARCHAR(100), fornecedor NVARCHAR(100), pesobarra NVARCHAR(20), pesoespecifico NVARCHAR(20))";
                comando.ExecuteNonQuery();

                label10.Text = "Tabela criada.";
            }
            catch (Exception ex)
            {

            }
            finally
            {
                conexao.Close();
            }

            AtualizarTabela();
        }

        private void btnCadastrarFornecedor_Click(object sender, EventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + ";Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                conexao.Open();

                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                //int id = new Random(DateTime.Now.Millisecond).Next(0, 1000);
                int codigo = int.Parse(txtCodigo.Text);
                string descricao = txtDescricao.Text;
                string fornecedor = txtFornecedor.Text;
                double pesobarra = double.Parse(txtPesoBarra.Text);
                double pesoespecifico = double.Parse(txtPesoEspecifico.Text);

                comando.CommandText = "INSERT INTO tabelacadastroaluminios VALUES ('" + codigo + "', '" + descricao + "', '" + fornecedor + "' , '" + pesobarra + "' , '" + pesoespecifico + "' )";
                comando.ExecuteNonQuery();

                label1.Text = "Registro inserido.";
                comando.Dispose();
                MessageBox.Show("Registro inserido com sucesso.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
                AtualizarTabela();
                
            }
        }

        public void AtualizarTabela()
        {
            listaFornecedores.Rows.Clear();

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                string query = "SELECT * FROM tabelacadastroaluminios";

                DataTable dados = new DataTable();

                SqlCeDataAdapter adaptador = new SqlCeDataAdapter(query, strConnection);

                conexao.Open();

                adaptador.Fill(dados);

                foreach (DataRow linha in dados.Rows)
                {
                    listaFornecedores.Rows.Add(linha.ItemArray);
                }
            }
            catch (Exception ex)
            {
                //listaFornecedores.Rows.Clear();
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
            }
        }

        private void btnCarregarCSV_Click(object sender, EventArgs e)
        {

            listaFornecedores.Rows.Clear();

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog1.FileName;
                BindDataCSV(filename);
            }
        }

        private void BindDataCSV(string filePath)
        {
            DataTable dt = new DataTable();
            string[] lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length > 0)
            {
                // first line to create header

                string firstLine = lines[0];

                string[] headerLabels = firstLine.Split(';');

                foreach (string headerWord in headerLabels)
                {
                    dt.Columns.Add(new DataColumn(headerWord));
                }

                //for data

                for (int r = 1; r < lines.Length; r++)
                {
                    string[] dataWords = lines[r].Split(';');
                    DataRow dr = dt.NewRow();
                    int columnIndex = 0;

                    foreach (string headerWord in headerLabels)
                    {
                        dr[headerWord] = dataWords[columnIndex++];
                    }

                    dt.Rows.Add(dr);
                }
            }

            if (dt.Rows.Count > 0)
            {
                listaFornecedores.DataSource = dt;
                
            }

        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + ";Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);
            var linenumber = 0;

            try
            {
                conexao.Open();

                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string filename = openFileDialog1.FileName;

                    using (StreamReader reader = new StreamReader(filename))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (linenumber != 0)
                            {
                                //int id = new Random((int)DateTime.Now.Ticks).Next(0,1000000) + 1;
                                var values = line.Split(';');
                                var sql = "INSERT INTO tabelacadastroaluminios VALUES (" + int.Parse(values[0]) + " , '" + values[1].ToString().Trim() + "' , '" + values[2].ToString().Trim() + "' , '" + values[3].ToString() + "' ,  '" + values[4].ToString() + "'  )";
                                //var sql = "INSERT INTO tabelacadastroaluminios VALUES ('" + int.Parse(values[0].ToString()) + "' , '" + values[1].ToString().Trim() + "' , '" + values[2].ToString().Trim() + "' , '" + double.Parse(values[3],CultureInfo.InvariantCulture) + "' ,  '" + double.Parse(values[4],CultureInfo.InvariantCulture) + "'  )";

                                var cmd = new SqlCeCommand();
                                cmd.CommandText = sql;
                                cmd.CommandType = System.Data.CommandType.Text;
                                cmd.Connection = conexao;

                                cmd.ExecuteNonQuery();

                            }
                            linenumber++;
                        }
                    }
                }

                conexao.Close();

                MessageBox.Show("Produtos importados com sucesso!");

            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
                AtualizarTabela();
            }
        }

        private void btnExcluir_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Deseja excluir o item selecionado?", "", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
                string strConnection = @"DataSource = " + baseDados + ";Password = '1234'";

                SqlCeConnection conexao = new SqlCeConnection(strConnection);

                try
                {
                    conexao.Open();

                    SqlCeCommand comando = new SqlCeCommand();
                    comando.Connection = conexao;

                    int codigo = (int)listaFornecedores.SelectedRows[0].Cells[0].Value;

                    comando.CommandText = "DELETE FROM tabelacadastroaluminios WHERE codigo = '" + codigo + "' ";
                    //comando.CommandText = "DELETE * FROM tabelaperfis";
                    comando.ExecuteNonQuery();
                    label1.Text = "Excluído.";
                    comando.Dispose();
                }
                catch (Exception ex)
                {
                    label1.Text = ex.Message;

                }
                finally
                {
                    conexao.Close();
                    AtualizarTabela();
                }
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // adicionando uma busca a uma variavel

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                string query = "SELECT descricao FROM tabelacadastroaluminios WHERE codigo = '" + 1024476 + "'  ";

                SqlCeCommand comando = new SqlCeCommand(query, conexao);

                conexao.Open();

                string descricao = (string)comando.ExecuteScalar();

                MessageBox.Show("a descrição é: " + descricao);

            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {

            // buscando todos e adicionando num datagrid

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);
            conexao.Open();

            try
            {
                foreach (DataGridViewRow linha in listaFornecedores.Rows)
                {
                    long codigo = long.Parse(linha.Cells[0].Value.ToString());

                    string query = "SELECT descricao FROM tabelacadastroaluminios WHERE codigo = '" + codigo + "'  ";

                    SqlCeCommand comando = new SqlCeCommand(query, conexao);


                    string descricao = (string)comando.ExecuteScalar();

                    datateste.Rows.Add(descricao);

                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // buscando todos, adicionando em uma lista e depois adicionando em um datagrid

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);
            conexao.Open();

            List<string> descricoes = new List<string>();

            try
            {
                foreach (DataGridViewRow linha in listaFornecedores.Rows)
                {
                    long codigo = long.Parse(linha.Cells[0].Value.ToString());

                    string query = "SELECT descricao FROM tabelacadastroaluminios WHERE codigo = '" + codigo + "'  ";

                    SqlCeCommand comando = new SqlCeCommand(query, conexao);


                    string descricao = (string)comando.ExecuteScalar();

                    descricoes.Add(descricao);
                }

                foreach (string d in descricoes)
                {
                    datateste.Rows.Add(d);
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
            }
        }
    }
}
