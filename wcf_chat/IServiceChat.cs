﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.SqlClient;

namespace wcf_chat
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IServiceChat" в коде и файле конфигурации.
    [ServiceContract(CallbackContract = typeof(IServerChatCallback))]
    public interface IServiceChat
    {
        [OperationContract]
        void Connect();

        [OperationContract]
        void Disconnect();

        [OperationContract]
        string SendDB(string login);
        [OperationContract]
        bool TryLogin(string login, string password);
        [OperationContract]
        bool TryRegister(string login, string password, string name, string surname);
        [OperationContract]
        string SendMarks();
        [OperationContract]
        string SendModels(string mark);
        [OperationContract]
        bool TryOrder(int transport, string number, int creator, string creationdate, string endingdate);
        [OperationContract]
        int GetTransport(string mark, string model);
        [OperationContract]
        int GetUserID(string login);
    }

    public interface IServerChatCallback
    {
        [OperationContract(IsOneWay = true)]
        void MsgCallback(string msg);
        [OperationContract(IsOneWay = true)]
        void MarksCallback(string msg);
        [OperationContract(IsOneWay = true)]
        void ModelsCallback(string msg);
    }
}
