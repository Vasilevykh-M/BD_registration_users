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
using System.Globalization;

namespace Аутентификация
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

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

                if(login == "")
                {
                    lblStatus.Text = "Введите логин";
                    return ;
                }

                using (var sqlCommand = new NpgsqlCommand
                {
                    Connection = conn,
                    CommandText = @"SELECT COUNT(*)
                                    FROM users
                                    WHERE login = @userLogin"
                })
                {
                    sqlCommand.Parameters.AddWithValue("@userLogin", login);

                    if ((long)sqlCommand.ExecuteScalar() > 0)
                    {
                        lblStatus.Text = "Логин уже занят";
                        return;
                    }

                    lblStatus.Text = "";
                }

                var password = tbPassword.Text;

                var legal_elem1 = "qwertyuiopasdfghjklzxcvbnm";
                var legal_elem2 = "QWERTYUIOPASDFGHJKLZXCVBNM";
                var legal_elem3 = "1234567890";
                var legal_elem4 = "/|!@#$%^&*(){}[]<>.,`~;:'+-_";

                var a = legal_elem1.Except(password);
                var b = legal_elem2.Except(password);
                var c = legal_elem3.Except(password);
                var d = legal_elem4.Except(password);


                if (!(a.Count() < legal_elem1.Count() && b.Count() < legal_elem2.Count() && c.Count() < legal_elem3.Count()
                    && d.Count() < legal_elem4.Count()))
                {
                    lblStatus.Text = "В пароле должны присутствовать прописные и заглавные латинские буквы цифры и знаки  /|!@#$%^&*(){}[]<>.,`~;:'+-_";
                    return;
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                using (var sqlCommand = new NpgsqlCommand
                {
                    Connection = conn,
                    CommandText = @"INSERT INTO users (login, password)
                                    VALUES (@login, @passwordHash)"
                })
                {
                    sqlCommand.Parameters.AddWithValue("@login", login);
                    sqlCommand.Parameters.AddWithValue("@passwordHash", passwordHash);


                    if (sqlCommand.ExecuteNonQuery() == 1)
                    {
                        lblStatus.Text = "Пользователь успешно зарегистрирован";
                    }
                    else
                    {
                        lblStatus.Text = "Ошибка регистрации. Попробуйте позже";
                    }
                }
                conn.Close();
            }
        }
    }
}
