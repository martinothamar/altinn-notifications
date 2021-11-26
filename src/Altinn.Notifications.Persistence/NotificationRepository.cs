﻿using Altinn.Notifications.Core;
using Altinn.Notifications.Core.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql;
using NpgsqlTypes;

namespace Altinn.Notifications.Persistence
{
    public class NotificationRepository : INotificationsRepository
    {
        private readonly string insertNotificationSql = "select * from notifications.insert_notification(@sendtime, @instanceid, @partyreference, @sender)";
        private readonly string insertTargetSql = "select * from notifications.insert_target(@notificationid, @channeltype, @address, @sent)";
        private readonly string insertMessageSql = "select * from notifications.insert_message(@notificationid, @emailsubject, @emailbody, @smstext, @language)";

        private readonly string getNotificationSql = "select * from notifications.get_notification(@_id)";

        private readonly string _connectionString;
        
        private readonly ILogger _logger;

        public NotificationRepository(IOptions<PostgreSQLSettings> postgresSettings, ILogger<NotificationRepository> logger)
        {
            _connectionString = string.Format(postgresSettings.Value.ConnectionString, postgresSettings.Value.EventsDbPwd);

            _logger = logger;
        }

        /// <summary>
        /// Remporarily created constructor to simplyfy testing.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="logger">Logger</param>
        public NotificationRepository(string connectionString, ILogger<NotificationRepository> logger)
        {
            _connectionString = connectionString;

            _logger = logger;
        }

        public async Task<Notification> AddNotification(Notification notification)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
            
            await conn.OpenAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(insertNotificationSql, conn);
            pgcom.Parameters.AddWithValue("sendtime", notification.SendTime);

            if (notification.InstanceId != null)
            {
                pgcom.Parameters.AddWithValue("instanceid", notification.InstanceId);
            }
            else
            {
                pgcom.Parameters.AddWithValue("instanceid", DBNull.Value);
            }

            if (notification.PartyReference != null)
            {
                pgcom.Parameters.AddWithValue("partyreference", notification.PartyReference);
            }
            else
            {
                pgcom.Parameters.AddWithValue("partyreference", DBNull.Value);
            }

            pgcom.Parameters.AddWithValue("sender", notification.Sender);

            using (NpgsqlDataReader reader = pgcom.ExecuteReader())
            {
                reader.Read();
                return ReadNotification(reader);
            }
        }

        /// <inheritdoc/>
        public async Task<Notification> GetNotification(int id)
        {
            Notification notification = null;

            using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(getNotificationSql, conn);
            pgcom.Parameters.AddWithValue("_id", NpgsqlDbType.Integer, id);

            using (NpgsqlDataReader reader = pgcom.ExecuteReader())
            {
                while (reader.Read())
                {
                    notification = ReadNotification(reader);
                }
            }

            return notification;
        }

        public async Task<Target> AddTarget(Target target)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);

            await conn.OpenAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(insertTargetSql, conn);
            pgcom.Parameters.AddWithValue("notificationid", target.NotificationId);
            pgcom.Parameters.AddWithValue("channeltype", target.ChannelType);

            if (target.Address != null)
            {
                pgcom.Parameters.AddWithValue("address", target.Address);
            }
            else
            {
                pgcom.Parameters.AddWithValue("address", DBNull.Value);
            }

            if (target.Sent != null)
            {
                pgcom.Parameters.AddWithValue("sent", target.Sent);
            }
            else
            {
                pgcom.Parameters.AddWithValue("sent", DBNull.Value);
            }

            using (NpgsqlDataReader reader = pgcom.ExecuteReader())
            {
                reader.Read();
                return ReadTarget(reader);
            }
        }

        public async Task<Message> AddMessage(Message message)
        {
            using NpgsqlConnection conn = new NpgsqlConnection(_connectionString);

            await conn.OpenAsync();

            NpgsqlCommand pgcom = new NpgsqlCommand(insertMessageSql, conn);
            pgcom.Parameters.AddWithValue("notificationid", message.NotificationId);

            if (message.EmailSubject != null)
            {
                pgcom.Parameters.AddWithValue("emailsubject", message.EmailSubject);
            }
            else
            {
                pgcom.Parameters.AddWithValue("emailsubject", DBNull.Value);
            }

            if (message.EmailBody != null)
            {
                pgcom.Parameters.AddWithValue("emailbody", message.EmailBody);
            }
            else
            {
                pgcom.Parameters.AddWithValue("emailbody", DBNull.Value);
            }

            if (message.SmsText != null)
            {
                pgcom.Parameters.AddWithValue("smstext", message.SmsText);
            }
            else
            {
                pgcom.Parameters.AddWithValue("smstext", DBNull.Value);
            }

            pgcom.Parameters.AddWithValue("language", message.Language);

            using (NpgsqlDataReader reader = pgcom.ExecuteReader())
            {
                reader.Read();
                return ReadMessage(reader);
            }
        }

        private static Notification ReadNotification(NpgsqlDataReader reader)
        {
            Notification notification = new Notification();
            notification.Id = reader.GetValue<int>("id");
            notification.SendTime = reader.GetValue<DateTime>("sendtime").ToUniversalTime();
            notification.InstanceId = reader.GetValue<string>("instanceid");
            notification.PartyReference = reader.GetValue<string>("partyreference");
            notification.Sender = reader.GetValue<string>("sender");
            return notification;
        }

        private static Target ReadTarget(NpgsqlDataReader reader)
        {
            Target target = new Target();
            target.Id = reader.GetValue<int>("id");
            target.NotificationId = reader.GetValue<int>("notificationid");
            target.ChannelType = reader.GetValue<string>("channeltype");
            target.Address = reader.GetValue<string>("address");
            target.Sent = reader.GetValue<DateTime>("sent").ToUniversalTime();
            return target;
        }

        private static Message ReadMessage(NpgsqlDataReader reader)
        {
            Message message = new Message();
            message.Id = reader.GetValue<int>("id");
            message.NotificationId = reader.GetValue<int>("notificationid");
            message.EmailSubject = reader.GetValue<string>("emailsubject");
            message.EmailBody = reader.GetValue<string>("emailbody");
            message.SmsText = reader.GetValue<string>("smstext");
            message.Language = reader.GetValue<string>("language");
            return message;
        }
    }
}