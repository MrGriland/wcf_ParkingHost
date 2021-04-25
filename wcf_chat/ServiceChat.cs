using System;
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
        List<ServerUser> users = new List<ServerUser>();
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

        public void SendMsg(string login)
        {
            if (IsAdmin(login))
            {
                using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                {
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
                        string json = JsonConvert.SerializeObject(orderInfos);
                        user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    orderInfos.Clear();
                }
            }
            else
            {
                using (SqlConnection connection = new SqlConnection("Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=BGV_CP;Data Source=MRGRILAND"))
                {
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
                        string json = JsonConvert.SerializeObject(orderInfos);
                        user.operationContext.GetCallbackChannel<IServerChatCallback>().MsgCallback(json);
                    }
                    orderInfos.Clear();
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
                    Console.WriteLine("Login True");
                    return true;
                }
                else
                {
                    Console.WriteLine("Login False");
                    return false;
                }
            }
        }
    }
}
