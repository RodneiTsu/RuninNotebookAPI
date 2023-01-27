using System;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;


namespace RuninNotebookAPI.DB
{
    
    class ConexaoDB
    {
        private static readonly ConexaoDB Instancia = new ConexaoDB();
        private ConexaoDB() { }

        public static ConexaoDB PegarInstacia()
        {
            return Instancia;
        }

        public MySqlConnection PegarConexao()
        {
            string con = "Server=10.8.2.207; Port=3306;Database=EngTeste;Uid=engusr;Pwd=Eng$up";
            //string con = ConfigurationManager.ConnectionStrings["DBString"].ToString();
            return new MySqlConnection(con);
        }

        public static string TestConexao()
        {
            string resposta = "Conexao OK";
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                try
                {
                    conexao.Open();
                }
                catch (MySqlException e)
                {
                    conexao.Close();
                    resposta = e.ToString();
                }
            }
            return resposta;
        }


        public static DataTable Carrega_Tabela(string cmstr)
        {
            DataTable dt;
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                MySqlCommand cmd = new MySqlCommand(cmstr, conexao);
                conexao.Open();
                cmd.CommandType = CommandType.Text;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                dt = new DataTable();

                da.Fill(dt);
     
                conexao.Close();
                da.Dispose();
            }
            return dt ;
        }

        public static void CRUD_tabela(string strCRUD)
        {
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                MySqlCommand cmd = new MySqlCommand(strCRUD, conexao);
                conexao.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException)
                {
                    conexao.Close();
                    throw;
                }
                conexao.Close();
            }
        }


        public static Int32 CRUDValor_tabela(string strCRUD)
        {
            Int32 qtd;
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                MySqlCommand cmd = new MySqlCommand(strCRUD, conexao);
                conexao.Open();
                try
                {
                    qtd = Convert.ToInt32(cmd.ExecuteScalar());
                }
                catch (MySqlException)
                {
                    conexao.Close();
                    throw;
                }
                conexao.Close();
            }
            return qtd;
        }

        public static Int32 CRUDU_ID_tabela(string strCRUD)
        {
            Int32 qtd;
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                MySqlCommand cmd = new MySqlCommand(strCRUD, conexao);
                conexao.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                    qtd = Convert.ToInt32(cmd.LastInsertedId);
                }
                catch (MySqlException)
                {
                    conexao.Close();
                    qtd =0;
                }
                conexao.Close();
            }
            return qtd;
        }

        public static string CRUDCampo_tabela(string strCRUD)
        {
            string campo;
            using (MySqlConnection conexao = ConexaoDB.PegarInstacia().PegarConexao())
            {
                MySqlCommand cmd = new MySqlCommand(strCRUD, conexao);
                conexao.Open();
                try
                {
                    campo = cmd.ExecuteScalar().ToString();
                }
                catch (MySqlException)
                {
                    conexao.Close();
                    throw;
                }
                conexao.Close();
            }
            return campo;
        }
    }
}
