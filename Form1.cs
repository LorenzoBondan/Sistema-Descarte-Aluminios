using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic;

namespace Descarte_Aluminios
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public int LimiteFinal; // delimitador de impressão

        private void Form1_Load(object sender, EventArgs e)
        {
            VerificarAdministrador();

            txtAno.Text = DateTime.Now.Year.ToString();
            txtData.Text = DateTime.Now.Day.ToString();

            lblUser.Text = Environment.UserName;

            cbMeses.SelectedIndex = DateTime.Now.Month - 1; // -1 pois o index começa em 0

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf"; // \db\
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

                comando.CommandText = "CREATE TABLE tabelaaluminios" + DateTime.Now.Month + DateTime.Now.Year + " (id INT NOT NULL PRIMARY KEY, codigo INT, descricao NVARCHAR(50), fornecedor NVARCHAR(50), npecas INT, tamanho DECIMAL(10,2), peso DECIMAL(10,2), data NVARCHAR(20), registro NVARCHAR(50))";
                comando.ExecuteNonQuery();

                label1.Text = "Tabela criada.";
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
            finally
            {
                conexao.Close();
            }

            // ADICIONANDO CÓDIGOS CADASTRADOS NA COMBOBOX
            try
            {
                conexao.Open();
                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                // combobox
                cbCodigo.Items.Clear();
                comando.CommandText = "SELECT codigo FROM tabelacadastroaluminios";
                comando.ExecuteNonQuery();

                DataTable dt = new DataTable();
                SqlCeDataAdapter da = new SqlCeDataAdapter(comando);
                da.Fill(dt);

                foreach (DataRow linha in dt.Rows)
                {
                    cbCodigo.Items.Add(linha["codigo"].ToString());
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

        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + ";Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                conexao.Open();

                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                Random random = new Random();
                int id = random.Next();
                int codigo = int.Parse(txtCodigo.Text);
                string descricao = txtDescricao.Text;
                string fornecedor = txtFornecedor.Text;
                int npecas = int.Parse(txtNPecas.Text);
                double tamanho = double.Parse(txtTamanho.Text,CultureInfo.InvariantCulture);
                double peso = double.Parse(txtPeso.Text,CultureInfo.InvariantCulture);
                string data = DateTime.Now.Day.ToString(); // caso deixe o programa aberto de um dia para o outro
                string registro = Environment.UserName.ToString(); // <<<<<<----

                comando.CommandText = "INSERT INTO tabelaaluminios" + DateTime.Now.Month + DateTime.Now.Year + " VALUES (" + id + " , '" + codigo + "', '" + descricao + "', '" + fornecedor + "' , '" + npecas + "' , '" + tamanho + "' , '" + peso + "' , '" + data + "' , '" + registro + "' )";
                comando.ExecuteNonQuery();

                label1.Text = "Registro inserido.";
                comando.Dispose();

                MessageBox.Show("Registro inserido com sucesso.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LimparCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conexao.Close();
                AtualizarTabela();
            }
        }

        private void btnSeparar_Click(object sender, EventArgs e)
        {
            //SOMASE (PESO TOTAL PARA CADA ITEM)

            if (lista.Columns[3].HeaderText == "Peso Total (kg)")
            {
                return;
            }

            LimiteFinal = 4;

            timer1.Enabled = false;

            Dictionary<int,double> dictionary = new Dictionary<int,double>();

            for (int i = 0; i < lista.Rows.Count; i++)
            {
                int codigo = int.Parse(lista.Rows[i].Cells[1].Value.ToString());
                //string descricao = lista.Rows[i].Cells[2].Value.ToString();
                double peso = double.Parse(lista.Rows[i].Cells[6].Value.ToString());

                if (dictionary.ContainsKey(codigo))
                {
                    dictionary[codigo] += peso;
                }
                else
                {
                    dictionary[codigo] = peso;
                }
            }

            lista.Rows.Clear();

            OcultarColunas();
            btnVoltar.Visible = true;

            foreach (var item in dictionary)
            {
                lista.Rows.Add( "" ,item.Key, "", item.Value);
            }

            // TRAZENDO A DESCRIÇÃO DO PERFIL
            TrazerDescricaoCodigos();

            // dividir peso / 1000
            foreach (DataGridViewRow line in lista.Rows)
            {
                double pesoConvertido = double.Parse(line.Cells[3].Value.ToString())/1000;
                line.Cells[3].Value = pesoConvertido;
            }
        }

        public void TrazerDescricaoCodigos()
        {
            dataAuxiliar.Rows.Clear();

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            foreach (DataGridViewRow linha in lista.Rows)
            {
                int codigo = (int)linha.Cells[1].Value;

                try
                {
                    string query = "SELECT descricao FROM tabelacadastroaluminios WHERE codigo LIKE '" + codigo + "'   "; // contains

                    DataTable dados = new DataTable();

                    SqlCeDataAdapter adaptador = new SqlCeDataAdapter(query, strConnection);

                    conexao.Open();

                    adaptador.Fill(dados);

                    foreach (DataRow l in dados.Rows)
                    {
                        dataAuxiliar.Rows.Add(l.ItemArray);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conexao.Close();
                }
            }

            int i = 0;
            foreach (DataGridViewRow row in dataAuxiliar.Rows)
            {
                try
                {
                    if (i == dataAuxiliar.Rows.Count -1)
                    {
                        break;
                    }
                    else 
                    { 
                        string descricao = row.Cells[0].Value.ToString();
                        lista.Rows[i].Cells[2].Value = descricao;
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
            }
        }

        private void cbMeses_SelectedIndexChanged(object sender, EventArgs e)
        {
            AtualizarTabela();

            // testar se pode acrescentar dados na tabela do mês escolhido ou não
            HabilitarCampos();
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                if (lista.Rows.Count > 0)
                {

                    Microsoft.Office.Interop.Excel.Application xcelApp = new Microsoft.Office.Interop.Excel.Application();
                    xcelApp.Application.Workbooks.Add(Type.Missing);

                    // CONFIGURAÇÃO PARA NAO IMPRIMIR A PRIMEIRA COLUNA
                    for (int i = 2; i < LimiteFinal + 1 ; i++)
                    {
                        xcelApp.Cells[1, i - 1 ] = lista.Columns[i - 1].HeaderText;
                    }

                    for (int i = 0; i < lista.Rows.Count; i++)
                    {
                        for (int j = 1; j < LimiteFinal; j++)
                        {
                            xcelApp.Cells[i + 2, j] = lista.Rows[i].Cells[j].Value;
                        }
                    }
                    xcelApp.Columns.AutoFit();
                    xcelApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AtualizarTabela()
        {
            LimiteFinal = 9;

            lista.Rows.Clear();

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                string query = "SELECT * FROM tabelaaluminios" + cbMeses.Text + txtAno.Text;

                DataTable dados = new DataTable();

                SqlCeDataAdapter adaptador = new SqlCeDataAdapter(query, strConnection);

                conexao.Open();

                adaptador.Fill(dados);

                foreach (DataRow linha in dados.Rows)
                {
                    lista.Rows.Add(linha.ItemArray);
                }
            }
            catch (Exception ex)
            {
                lista.Rows.Clear();
                //MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conexao.Close();
            }
        }

        private void btnVoltar_Click(object sender, EventArgs e)
        {
            MostrarColunas();
            cbMeses_SelectedIndexChanged(sender, e);
            timer1.Enabled = true;
            btnVoltar.Visible = false;
        }

        public void OcultarColunas()
        {
            lista.Columns[2].HeaderText = "Descrição";
            lista.Columns[3].HeaderText = "Peso Total (kg)";
            
            lista.Columns[4].Visible = false;
            lista.Columns[5].Visible = false;
            lista.Columns[6].Visible = false;
            lista.Columns[7].Visible = false;
            lista.Columns[8].Visible = false;
        }

        public void MostrarColunas()
        {
            lista.Columns[2].HeaderText = "Descrição";
            lista.Columns[3].HeaderText = "Fornecedor";
            lista.Columns[4].Visible = true;
            lista.Columns[5].Visible = true;
            lista.Columns[6].Visible = true;
            lista.Columns[7].Visible = true;
            lista.Columns[8].Visible = true;
        }

        public void HabilitarCampos()
        {
            if (cbMeses.SelectedIndex + 1 == DateTime.Now.Month)
            {
                btnCadastrar.Enabled = true;
            }
            else
            {
                btnCadastrar.Enabled = false;
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnCadastrarFornecedor_Click(object sender, EventArgs e)
        {
            frmFornecedores f = new frmFornecedores();
            f.Show();
        }

        private void cbCodigo_SelectedValueChanged(object sender, EventArgs e)
        {
            //string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            //string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            //SqlCeConnection conexao = new SqlCeConnection(strConnection);

            //// COMPLETANDO AS TEXTBOXES COM BASE NO CÓDIGO QUE FOI SELECIONADO (PROCV)
            //try
            //{
            //    conexao.Open();
            //    SqlCeCommand comando = new SqlCeCommand();
            //    comando.Connection = conexao;

            //    comando.CommandText = "SELECT descricao, fornecedor FROM tabelacadastroaluminios WHERE codigo LIKE '" + cbCodigo.Text + "' " ;
            //    comando.ExecuteNonQuery();

            //    SqlCeDataReader da = comando.ExecuteReader();
            //    while (da.Read())
            //    {
            //        txtDescricao.Text = da.GetValue(0).ToString();
            //        txtFornecedor.Text = da.GetValue(1).ToString();

            //    }

            //    comando.Dispose();

            //}
            //catch (Exception ex)
            //{
            //    label1.Text = ex.Message;
            //}
            //finally
            //{
            //    conexao.Close();
            //}
        }

        private void txtCodigo_KeyUp(object sender, KeyEventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            // COMPLETANDO AS TEXTBOXES COM BASE NO CÓDIGO QUE FOI SELECIONADO (PROCV)
            try
            {
                conexao.Open();
                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                comando.CommandText = "SELECT descricao, fornecedor FROM tabelacadastroaluminios WHERE codigo LIKE '" + txtCodigo.Text + "' ";
                comando.ExecuteNonQuery();

                SqlCeDataReader da = comando.ExecuteReader();
                while (da.Read())
                {
                    txtDescricao.Text = da.GetValue(0).ToString();
                    txtFornecedor.Text = da.GetValue(1).ToString();


                }

                comando.Dispose();

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

        private void txtTamanho_KeyUp(object sender, KeyEventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            // COMPLETANDO AS TEXTBOXES COM BASE NO CÓDIGO QUE FOI SELECIONADO (PROCV)
            try
            {
                conexao.Open();
                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                comando.CommandText = "SELECT pesoespecifico FROM tabelacadastroaluminios WHERE codigo LIKE '" + txtCodigo.Text + "' ";
                comando.ExecuteNonQuery();

                SqlCeDataReader da = comando.ExecuteReader();
                while (da.Read())
                {
                    double pesoespecifico = double.Parse(da.GetValue(0).ToString());
                    double peso = int.Parse(txtNPecas.Text) * double.Parse(txtTamanho.Text)/1000 * pesoespecifico;
                    txtPeso.Text = peso.ToString("F3",CultureInfo.InvariantCulture).Replace(".",",");

                }

                comando.Dispose();

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

        // COLOQUEI O MESMO CODIGO NOS DOIS CAMPOS, POIS DEPENDENDO DA ORDEM DE PREENCHIMENTO, ELE NÃO CALCULAVA
        private void txtNPecas_KeyUp(object sender, KeyEventArgs e)
        {
            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            // COMPLETANDO AS TEXTBOXES COM BASE NO CÓDIGO QUE FOI SELECIONADO (PROCV)
            try
            {
                conexao.Open();
                SqlCeCommand comando = new SqlCeCommand();
                comando.Connection = conexao;

                comando.CommandText = "SELECT pesoespecifico FROM tabelacadastroaluminios WHERE codigo LIKE '" + txtCodigo.Text + "' ";
                comando.ExecuteNonQuery();

                SqlCeDataReader da = comando.ExecuteReader();
                while (da.Read())
                {
                    double pesoespecifico = double.Parse(da.GetValue(0).ToString());
                    double peso = int.Parse(txtNPecas.Text) * double.Parse(txtTamanho.Text) / 1000 * pesoespecifico;
                    txtPeso.Text = peso.ToString("F3", CultureInfo.InvariantCulture).Replace(".", ",");

                }

                comando.Dispose();

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

                    int id = (int)lista.SelectedRows[0].Cells[0].Value;

                    comando.CommandText = "DELETE FROM tabelaaluminios" + cbMeses.Text + txtAno.Text + " WHERE id = '" + id + "' ";

                    comando.ExecuteNonQuery();
                    label1.Text = "Excluído.";
                    comando.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conexao.Close();
                    AtualizarTabela();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            AtualizarTabela();
        }

        public void LimparCampos()
        {
            txtCodigo.Text = "";
            txtDescricao.Text = "";
            txtFornecedor.Text = "";
            txtNPecas.Text = "";
            txtTamanho.Text = "";
            txtPeso.Text = "";

            txtCodigo.Focus();
        }

        public void VerificarAdministrador()
        {
            dataVerificaAdmin.Rows.Clear();

            string baseDados = Application.StartupPath + @"\DBSQLServer.sdf";
            string strConnection = @"DataSource = " + baseDados + "; Password = '1234'";

            SqlCeConnection conexao = new SqlCeConnection(strConnection);

            try
            {
                string query = "SELECT * FROM tabelaadministradores";

                DataTable dados = new DataTable();

                SqlCeDataAdapter adaptador = new SqlCeDataAdapter(query, strConnection);

                conexao.Open();

                adaptador.Fill(dados);

                foreach (DataRow linha in dados.Rows)
                {
                    dataVerificaAdmin.Rows.Add(linha.ItemArray);
                }

                bool achou = false;
                foreach (DataGridViewRow l in dataVerificaAdmin.Rows)
                {
                    string admin = l.Cells[0].Value.ToString();
                    if (admin == Environment.UserName)
                    {
                        achou = true;
                        break;
                    }
                }

                if (achou == false)
                {
                    btnExportar.Enabled = false;
                    btnExcluir.Enabled = false;
                    btnSeparar.Enabled = false;
                    //btnAdministradores.Enabled = false; // <<- Habilitar depois

                    //toolTip1.SetToolTip(btnExportar, "Você não tem acesso à esse comando. Consulte o facilitador da área.");
                }
            }
            catch (Exception ex)
            {
                //listaFornecedores.Rows.Clear();
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

        }

        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
            string senha = Interaction.InputBox("");
            if (senha == "aluminiosadministrador")
            {
                btnExcluir.Enabled = true;
                btnExportar.Enabled = true;
                btnSeparar.Enabled = true;
            }
            //frmAdministradores f = new frmAdministradores();
            //f.Show();
        }


    }
    
}
