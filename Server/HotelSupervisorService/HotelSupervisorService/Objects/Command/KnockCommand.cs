﻿using System;
using System.Drawing;
using System.Windows.Forms;
using HotelSupervisorService.Enums;
using HotelSupervisorService.Exceptions;
using HotelSupervisorService.Interfaces;
using HotelSupervisorService.Managers;
using HotelSupervisorService.Objects.Communication;
using HotelSupervisorService.Objects.Logs;

namespace HotelSupervisorService.Objects.Command
{
    public class KnockCommand : BaseCommand, ICommand
    {
        private CommunicationParameter communicationParameter = new CommunicationParameter(EncryptionManager.Decrypt(global::HotelSupervisorService.Properties.Resources.ReceiveMessageUserName), EncryptionManager.Decrypt(global::HotelSupervisorService.Properties.Resources.ReceiveMessagePassword));
        private string targetAddress = EncryptionManager.Decrypt(global::HotelSupervisorService.Properties.Resources.CommandKnockTargetAddress);

        public CommandType CommandType
        {
            get
            {
                return CommandType.UPDATE;
            }
        }

        #region ICommand 成员

        public string CommandName
        {
            get
            {
                return "探测";
            }
        }

        public int CommandSortID
        {
            get
            {
                return 0;
            }
        }

        public Image CommandIcon
        {
            get
            {
                return global::HotelSupervisorService.Properties.Resources.Knock;
            }
        }

        public string TargetAddress
        {
            get
            {
                return targetAddress;
            }
        }

        public CommunicationParameter CommunicationParameter
        {
            get
            {
                return communicationParameter;
            }
        }

        public void Execute()
        {
            try
            {
                DialogResult dr = MessageBox.Show("确认要发送探测命令吗？", "更新", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                {
                    return;
                }
                CommandObject commandObject = new CommandObject();
                commandObject.CommandType = CommandType.KNOCK;
                communicationManager.SendCommandMessage(this,commandObject, null, null);
                SystemLog systemLog = new SystemLog(SystemLogType.探测, "发送成功。");
                LogManager.GetLogManager().AddLog(systemLog);
                DataBaseManager.GlobalDataBaseManager.InsertCommand(commandObject);
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(ExceptionPlus))
                {
                    throw;
                }
                throw new ExceptionPlus("92。", e);
            }
        }

        #endregion
    }
}
