﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Newtonsoft.Json;

namespace wcf_chat
{
  
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class ServiceChat : IServiceChat
    {
        List<OrderInfo> orderInfos = new List<OrderInfo>();
        List<string> marks = new List<string>();
        List<string> models = new List<string>();
        ServerUser user;
        public void Connect()
        {
             user = new ServerUser() {
                operationContext = OperationContext.Current
            };

            Console.WriteLine("Connected");
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnected");
            user = null;
        }

        public string SendDB(string login)
        {
            if (IsAdmin(login))
            {
                using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                {
                    orderInfos.Clear();
                    connection.Open();
                    string select = "select * from OrderInfo";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        OrderInfo orderInfo = new OrderInfo();
                        orderInfo.OrderInfo_Transport = Convert.ToInt32(dr[1]);
                        orderInfo.OrderInfo_Number = dr[2].ToString();
                        orderInfo.OrderInfo_Creator = Convert.ToInt32(dr[3]);
                        orderInfo.OrderInfo_CreationDate = dr[4].ToString();
                        orderInfo.OrderInfo_EndingDate = dr[5].ToString();
                        orderInfos.Add(orderInfo);
                        //string json = JsonConvert.SerializeObject(orderInfos);
                        //user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    return JsonConvert.SerializeObject(orderInfos);
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                {
                    orderInfos.Clear();
                    connection.Open();
                    string select = "select * from OrderInfo o join UserInfo u on o.OrderInfo_Creator=u.UserInfo_ID where u.UserInfo_Login=@login";
                    SqlCommand cmd = new SqlCommand(select, connection);
                    SqlParameter loginParam = new SqlParameter("@login", login);
                    cmd.Parameters.Add(loginParam);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        OrderInfo orderInfo = new OrderInfo();
                        orderInfo.OrderInfo_Transport = Convert.ToInt32(dr[1]);
                        orderInfo.OrderInfo_Number = dr[2].ToString();
                        orderInfo.OrderInfo_Creator = Convert.ToInt32(dr[3]);
                        orderInfo.OrderInfo_CreationDate = dr[4].ToString();
                        orderInfo.OrderInfo_EndingDate = dr[5].ToString();
                        orderInfos.Add(orderInfo);
                        //string json = JsonConvert.SerializeObject(orderInfos);
                        //user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    return JsonConvert.SerializeObject(orderInfos);
                }
            }
        }
        public bool IsAdmin(string login)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                connection.Open();
                string select = "select UserInfo_IsAdmin from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                while(dr.Read())
                {
                    if (dr[0].ToString() == "True")
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                return false;
            }
        }
        public bool TryLogin(string login, string password)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                connection.Open();
                string select = "select * from UserInfo where UserInfo_Login = @login and UserInfo_Password = @password";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                loginParam = new SqlParameter("@password", password);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    if(IsAdmin(login))
                        Console.WriteLine("Logged as admin");
                    else
                        Console.WriteLine("Logged as user");
                    return true;
                }
                else
                {
                    Console.WriteLine("Login failed");
                    return false;
                }
            }
        }

        public bool TryRegister(string login, string password, string name, string surname)
        {
            if (!SearchLogin(login))
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                    {
                        connection.Open();
                        string insert = "insert into UserInfo values (@login,@password,@name,@surname,'false')";
                        SqlCommand cmd = new SqlCommand(insert, connection);
                        SqlParameter loginParam = new SqlParameter("@login", login);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@password", password);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@name", name);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@surname", surname);
                        cmd.Parameters.Add(loginParam);
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (SearchLogin(login))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
                return false;
        }
        public bool SearchLogin(string login)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                connection.Open();
                string select = "select * from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string SendMarks()
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                marks.Clear();
                connection.Open();
                string select = "select TransportInfo_Mark from TransportInfo group by TransportInfo_Mark order by TransportInfo_Mark";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    marks.Add(dr[0].ToString());
                    //string json = JsonConvert.SerializeObject(marks);
                    //user.operationContext.GetCallbackChannel<IServerChatCallback>().MarksCallback(json);
                }
                return JsonConvert.SerializeObject(marks);
            }
        }
        public string SendModels(string mark)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                models.Clear();
                connection.Open();
                string select = "select TransportInfo_Model from TransportInfo where TransportInfo_Mark = @mark";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter Param = new SqlParameter("@mark", mark);
                cmd.Parameters.Add(Param);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    models.Add(dr[0].ToString());
                    //string json = JsonConvert.SerializeObject(models);
                    //user.operationContext.GetCallbackChannel<IServerChatCallback>().ModelsCallback(json);
                }
                return JsonConvert.SerializeObject(models);
            }
        }

        public bool TryOrder(int transport, string number, int creator, string creationdate, string endingdate)
        {

                try
                {
                    using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                    {
                        connection.Open();
                        string insert = "insert into OrderInfo values(@transport,@number,@creator,convert(datetime,@creationdate,20),convert(datetime,@endingdate,20))";
                        SqlCommand cmd = new SqlCommand(insert, connection);
                        SqlParameter loginParam = new SqlParameter("@transport", transport);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@number", number);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@creator", creator);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@creationdate", creationdate);
                        cmd.Parameters.Add(loginParam);
                        loginParam = new SqlParameter("@endingdate", endingdate);
                        cmd.Parameters.Add(loginParam);
                        SqlDataReader dr = cmd.ExecuteReader();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }

        }

        public int GetTransport(string mark, string model)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                connection.Open();
                string select = "select TransportInfo_ID from TransportInfo where TransportInfo_Mark = @mark and TransportInfo_Model = @model";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter Param = new SqlParameter("@mark", mark);
                cmd.Parameters.Add(Param);
                Param = new SqlParameter("@model", model);
                cmd.Parameters.Add(Param);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                return Convert.ToInt32(dr[0]);
            }
        }
        public int GetUserID(string login)
        {
            using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
            {
                connection.Open();
                string select = "select UserInfo_ID from UserInfo where UserInfo_Login = @login";
                SqlCommand cmd = new SqlCommand(select, connection);
                SqlParameter loginParam = new SqlParameter("@login", login);
                cmd.Parameters.Add(loginParam);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                return Convert.ToInt32(dr[0]);
            }
        }
    }
}
