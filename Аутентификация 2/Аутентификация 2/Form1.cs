using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using Scrypt;


namespace Аутентификация_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var connStr = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Port = 5432,
                Database = "laba_2",
                Username = "postgres",
                Password = "postgres"
            }.ConnectionString;

            using (var conn = new NpgsqlConnection(connStr))
            {
                conn.Open();

                var login = tbLogin.Text;

                using (var sqlCommand = new NpgsqlCommand
                {
                    Connection = conn,
                    CommandText = @"SELECT COUNT(*)
                                    FROM users
                                    WHERE login = @userLogin"
                })
                {
                    sqlCommand.Parameters.AddWithValue("@userLogin", login);

                    if ((long)sqlCommand.ExecuteScalar() == 0)
                    {
                        lblStatus.Text = "Логина не существует";
                        return;
                    }

                    lblStatus.Text = "";
                }

                var password = tbPassword.Text;
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using (var sqlCommand = new NpgsqlCommand
                {
                    Connection = conn,
                    CommandText = @"SELECT *
                                    FROM users
                                    WHERE login = @userlogin"
                })
                {
                    sqlCommand.Parameters.AddWithValue("@userlogin", login);

                    var reder = sqlCommand.ExecuteReader();

                    reder.Read();
                    if (BCrypt.Net.BCrypt.Verify(password,(string)reder["password"]))
                    {
                        lblStatus.Text = "Добро пожаловать " + login;
                    }
                    else
                    {
                        lblStatus.Text = "Пароль неверный";
                    }

                }
                conn.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
