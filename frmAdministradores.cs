using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Descarte_Aluminios
{
    public partial class frmAdministradores : Form
    {
        public frmAdministradores()
        {
            InitializeComponent();
        }

        private void frmAdministradores_Load(object sender, EventArgs e)
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

                comando.CommandText = "CREATE TABLE tabelaadministradores (usuario NVARCHAR(100) NOT NULL PRIMARY KEY)";
                comando.ExecuteNonQuery();

                //label10.Text = "Tabela criada.";
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
            finally
            {
                conexao.Close();
            }

            AtualizarTabela();
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

                string usuario = txtUsuario.Text;

                comando.CommandText = "INSERT INTO tabelaadministradores VALUES ('" + usuario + "')";
                comando.ExecuteNonQuery();

                //label1.Text = "Registro inserido.";
                comando.Dispose();
                MessageBox.Show("Registro inserido com sucesso.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexao.Close();
                AtualizarTabela();

            }
        }

        public void AtualizarTabela()
        {
            dataAdministradores.Rows.Clear();

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
                    dataAdministradores.Rows.Add(linha.ItemArray);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conexao.Close();
            }
        }

        private void btnSair_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
